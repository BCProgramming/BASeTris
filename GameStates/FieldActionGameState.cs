using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeTris.Choosers;
using BASeTris.Rendering;
using BASeTris.Rendering.Adapters;
using BASeTris.Particles;
using BASeTris.Rendering.GDIPlus;
using BASeTris.Blocks;
using SkiaSharp;
using BASeTris.AssetManager;

namespace BASeTris.GameStates
{
    //operates solely on a StandardTetrisGameState (or derived class- really anything with an appropriate TetrisField.
    //This is itself a base class for "Clear" actions. This would include actions like clearing a line in Tetris, clearing a set of blocks in Dr Mario or Tetris 2, etc.
    //also this could be used to ADD stuff to the field- so it's not strictly for CLEAR but "ActionGameState" seemed a bit silly as a name.
    
    public abstract class FieldActionGameState : GameState,ICompositeState<GameplayGameState>
    {
        public GameplayGameState _BaseState;

        public FieldActionGameState(GameplayGameState pBaseState)
        {
            _BaseState = pBaseState;
        }


        //we don't call the main State's GameProc here. We operate on the data (the field contents) to "clear" the given information but we operate on it separate from
        //the standard game state. For example we replace blocks with "intermediate" forms, or remove them altogether, then when finished return control and allow the game to continue.
        //The general approach ought to be to set this state, then enqueue a frameaction to be run for the next frame that "performs" the actual action, such as clearing rows or whatever.

        public override void GameProc(IStateOwner pOwner)
        {

            GameplayGameState desiredState = null;
            if (_BaseState is GameplayGameState)
                desiredState = _BaseState;
            else if(_BaseState is ICompositeState<GameplayGameState> comp)
            {
                desiredState = comp.GetComposite();
            }

            if (desiredState!=null)
            {
                desiredState.FrameUpdate();
                
            }
        }

        public GameplayGameState GetComposite()
        {
            return _BaseState;
        }

        public override void HandleGameKey(IStateOwner pOwner, GameKeys g)
        {
            //Should we allow Game keys here? probably. We'll block them for the moment though. There shouldn't be an active Nomino during this process
            //so really there shouldn't be anything to control- Aside, of course, from pause for example.
            //throw new NotImplementedException();
        }
    }
    //standard "line clear" which clears out a given set of line rows via animation, then calls a set of actions to perform afterwards.
    //This will use a "Tetris flash" when 4 or more rows are being cleared- this is a white overlay that is placed over top (well, for now- might be a gradient or some kind of pattern brush later!)

    public class FieldLineActionGameState : FieldActionGameState
    {
        public enum LineClearStyle
        {
            LineClear_Left_To_Right,
            LineClear_Right_To_Left,
            LineClear_Middle_Out,
            LineClear_Outside_In
        }
        private bool _Stagger = true;
        public bool Stagger {  get { return _Stagger; } set { _Stagger = value; } }
        protected LineClearStyle _ClearStyle = LineClearStyle.LineClear_Middle_Out;

        protected int MSBlockClearTime = 20; //number of ms between blocks being removed/cleared.
        public FieldBlockClearAction ClearAction { get; set; } = new FieldBlockClearShrinkAction(new TimeSpan(0,0,0,0,100));

        public LineClearStyle ClearStyle
        {
            get { return _ClearStyle; }
            set { _ClearStyle = value; }
        }


        private IEnumerable<Action> AfterClear = Enumerable.Empty<Action>();
        public bool FlashState = false;

        public Dictionary<int, NominoBlock[]> ClearRowInfo = null;
        public TetrisField PlayField { get; set; }

        public FieldLineActionGameState(GameplayGameState _BaseState, int[] ClearRows, IEnumerable<Action> pAfterClearActions) : base(_BaseState)
        {
            PlayField = _BaseState.PlayField;
            //prep the ClearRowInfo. take the row information from the state at the specified row indices, and plop them into ClearRowInfo at those keys.
            var ContentData = _BaseState.PlayField.Contents;
            ClearRowInfo = new Dictionary<int, NominoBlock[]>();
            foreach(var index in ClearRows)
            {
                NominoBlock[] thisrow = new NominoBlock[ContentData[index].Length];
                Array.Copy(ContentData[index], thisrow, ContentData[index].Length);
                //add to Dictionary.
                ClearRowInfo.Add(index, thisrow);
                //wipe out the original row to all be nulls.
                for(int i=0;i<ContentData[index].Length;i++)
                {
                    ContentData[index][i] = null;
                }
            }

            
            
            AfterClear = pAfterClearActions;
           // RowNumbers = ClearRows;
        }

        
        protected int CurrentClearIndex = 0;
        DateTime StartOperation = DateTime.MaxValue;
        DateTime LastOperation = DateTime.MaxValue;

        public override void GameProc(IStateOwner pOwner)
        {
            base.GameProc(pOwner);
            if (pOwner.CurrentState is GameplayGameState stgs1)
            {
                //pOwner.EnqueueAction(() => { stgs1.PlayField.HasChanged = true; });

            }
            if (StartOperation == DateTime.MaxValue)
            {
                StartOperation = LastOperation = DateTime.Now;
            }
            if(ClearActivities.Count > 0)
            {
                DateTime useNowTime = DateTime.Now;
                foreach(var ClearActivity in ClearActivities)
                {
                    var elapsed = useNowTime - ClearActivity.StartClearTime;
                    foreach(var block in ClearActivity.Blocks)
                    {
                        if(block!=null)
                            ClearActivity.ClearAction.ProcessBlock(pOwner,new BlockClearData(block,new BCPoint() { }),elapsed);
                    }
                }
            }
            //for now we clear horizontally across...
            if ((DateTime.Now - LastOperation).TotalMilliseconds > MSBlockClearTime)
            {
                //clear another block on each row.
                var ClearResult = ClearFrame(pOwner);
                if (ClearRowInfo.Keys.Count >= 4)
                {
                    FlashState = !FlashState;
                }

                if(ClearActivities.Count > 0)
                {
                    DateTime NowTime = DateTime.Now;
                    //if there are entries in ClearActivities, we want to make sure those are cleared as well. This is the case if the cleartime has elapsed for each.
                    ClearResult |= ClearActivities.All((kvp) => { return NowTime - kvp.StartClearTime > kvp.ClearAction.ClearTime; });
                }

                LastOperation = DateTime.Now;

                if (ClearResult)
                {
                    //reset the original State.
                    pOwner.CurrentState = _BaseState;
                   
                    foreach (var iterate in AfterClear)
                    {
                        pOwner.EnqueueAction(iterate);
                    }
                    if (pOwner.CurrentState is GameplayGameState stgs2)
                    {
                       // pOwner.EnqueueAction(() => { stgs2.PlayField.HasChanged = true; });
                        
                    }
                }
            }

        }
        bool SpawnedTextIndicator = false;
        //The actual "Frame" operation. This implementation clears one block and returns true when it clears all the blocks on each line.
        //derived classes can override pretty much just this one to clear the lines in different ways.        
        protected virtual bool ClearFrame(IStateOwner pOwner)
        {
            if(!SpawnedTextIndicator)
            {
                SpawnedTextIndicator = true;
                var getflag = (CharParticle.SpecialCharacterParticleFlags)TetrisGame.Choose<int>((int[])Enum.GetValues(typeof(CharParticle.SpecialCharacterParticleFlags)));
                foreach (var rowClear in ClearRowInfo)
                {


                    BCRect useBound = new BCRect(0, rowClear.Key-2, _BaseState.PlayField.ColCount - 1, 1);
                    //var UseRowBound = _BaseState.PlayField.GetBlockBounds(pOwner,(BCRect)pOwner.LastDrawBounds, new BCPointI(0, rowClear.Key), new BCPointI(_BaseState.PlayField.ColCount - 1, rowClear.Key));
                    /*UseRowBound = new BCRect((float)(
                        UseRowBound.Left + pOwner.ScaleFactor),
                        (float)(UseRowBound.Top * pOwner.ScaleFactor),
                        (float)(UseRowBound.Width * pOwner.ScaleFactor),
                        (float)(UseRowBound.Height * pOwner.ScaleFactor));*/
                    AddParticles_Row(pOwner, useBound, ClearRowInfo.Count,getflag);

                }

            }


            var useStyle = _ClearStyle;
            try
            {
                switch (useStyle)
                {
                    case LineClearStyle.LineClear_Left_To_Right:
                        foreach (var rowClear in ClearRowInfo)
                        {
                            if (_Stagger && rowClear.Key % 2 == 0) ClearFrame_Right_To_Left(pOwner, rowClear, ClearRowInfo.Count);
                            else
                                ClearFrame_Left_To_Right(pOwner, rowClear, ClearRowInfo.Count);
                        }

                        CurrentClearIndex++;
                        //_BaseState.PlayField.HasChanged = true;
                        return CurrentClearIndex >= _BaseState.PlayField.Contents[0].Length + 2;
                    case LineClearStyle.LineClear_Right_To_Left:
                        foreach (var rowClear in ClearRowInfo)
                        {
                            if (_Stagger && rowClear.Key % 2 == 0) ClearFrame_Left_To_Right(pOwner, rowClear, ClearRowInfo.Count);
                            else
                                ClearFrame_Right_To_Left(pOwner, rowClear, ClearRowInfo.Count);
                        }

                        CurrentClearIndex++;
                        //_BaseState.PlayField.HasChanged = true;
                        return CurrentClearIndex >= _BaseState.PlayField.Contents[0].Length + 2;
                    case LineClearStyle.LineClear_Middle_Out:
                        //middle out
                        foreach (var rowClear in ClearRowInfo)
                        {
                            if (_Stagger && rowClear.Key % 2 == 0) ClearFrame_Outside_In(pOwner, rowClear, ClearRowInfo.Count);
                            else
                                ClearFrame_Middle_Out(pOwner, rowClear, ClearRowInfo.Count);
                        }

                        CurrentClearIndex++;
                        //_BaseState.PlayField.HasChanged = true;
                        return CurrentClearIndex >= _BaseState.PlayField.Contents[0].Length + 2;
                    case LineClearStyle.LineClear_Outside_In:
                        {
                            //middle out
                            foreach (var rowClear in ClearRowInfo)
                            {
                                if (_Stagger && rowClear.Key % 2 == 0) ClearFrame_Outside_In(pOwner, rowClear, ClearRowInfo.Count);
                                else
                                    ClearFrame_Outside_In(pOwner, rowClear, ClearRowInfo.Count);
                            }

                            CurrentClearIndex++;
                            //_BaseState.PlayField.HasChanged = true;
                            return CurrentClearIndex >= _BaseState.PlayField.Contents[0].Length + 2;
                        }
                }
            }
            finally
            {
                
            }

            return true;
        }
        private BCColor[] ClearLineParticleColours = new BCColor[]
        {
            Color.Red,Color.White,Color.Yellow,Color.Orange,Color.Pink };

        Dictionary<int, String> RowClearText = new Dictionary<int, string>()
        {
            {1, "SINGLE " },
            {2, "DOUBLE " },
            {3, "TRIPLE " },
            {4, "TETRIS!" },
            {5, "PENTRIS!" },
            {6, "HEXTRIS!" },
            {7, "SEPTRIS!" },
            {8, "OCTRIS!" },
            {9, "NONTRIS!" },
            {10, "DECTRIS!" },
            {11, "UNODECTRIS!" },
            {12, "DUODECTRIS!" }
        };
        
        const int ParticleCountPerBlock = 15;
        private readonly long ClearParticleTTL = 300;
        private void AddParticles_Row(IStateOwner pOwner,BCRect RowBounds,int Lines=1, CharParticle.SpecialCharacterParticleFlags CharFlags = CharParticle.SpecialCharacterParticleFlags.Effect_Wave)
        {
            String UseText = RowClearText.ContainsKey(Lines)?(RowClearText?[Lines]) : $"{Lines}LINE";
            Func<BaseParticle, BCColor> TetrisColorFunc = BaseParticle.GetRainbowColorFunc(pOwner);
            Func<BaseParticle, BCColor> SingleLineColorFunc = (o) =>
            {
                int timebase = 2000;
                double DarknessValue = (Math.Sin((float)pOwner.GetElapsedTime().Ticks / 2000)/2)+1;

                BCColor usecolor = (Color)new HSLColor(0, 120, DarknessValue*120);

                return usecolor;
            };

            Func<BaseParticle, BCColor> DoubleLineColorFunc = (o) =>
            {
                int timebase = 2000;
                double DarknessValue = Math.Sin((float)pOwner.GetElapsedTime().Ticks/2000);

                BCColor usecolor = (Color)new HSLColor(75, 120, DarknessValue * 120);

                return usecolor;
            };
            Func<BaseParticle, BCColor> TripleLineColorFunc = (o) =>
            {
                int timebase = 2000;
                double DarknessValue = Math.Sin((float)pOwner.GetElapsedTime().Ticks / 2000);

                BCColor usecolor = (Color)new HSLColor(150, 120, DarknessValue * 120);

                return usecolor;
            };


            Func<BaseParticle, BCColor>[] LineFuncs = new Func<BaseParticle, BCColor>[]
            {
                SingleLineColorFunc,DoubleLineColorFunc,TripleLineColorFunc,TetrisColorFunc
            };
              //split the text into characters...

              char[] CharsToShow = UseText.ToCharArray();

            float XOffset = (int)((float)RowBounds.Width/2-((float)CharsToShow.Length/2)); //one character per block, ideally.
            var generatedflags = CharFlags;
            List<CharParticle> MakeParticles = new List<CharParticle>();
            for (int x = 0; x < CharsToShow.Length; x++)
            {
                int i = x % CharsToShow.Length;
                CharParticle makeparticle = new CharParticle(new BCPoint(RowBounds.Left +XOffset+ x, RowBounds.Top + RowBounds.Height / 2), new BCPoint(0, -0.05f), Color.Red, CharsToShow[i].ToString());
                makeparticle.Flags = generatedflags;
                makeparticle.TTL = 1500;
                //makeparticle.Decay = new BCPoint(0.5f, 0.5f);
                MakeParticles.Add(makeparticle);
                Lines = Lines > 4 ? Lines = 4:Lines;
                if(Lines >= 4)
                    makeparticle.ColorCalculatorFunction = LineFuncs[Lines-1];

                else
                {
                    makeparticle.Color = new Color[] { Color.Red, Color.Green, Color.Yellow }[Lines - 1];
                }


                
                lock (_BaseState.TopParticles)
                {
                    _BaseState.TopParticles.AddRange(MakeParticles);
                }
            }


        }



        private void AddParticles(IStateOwner pOwner,int BlockX,int BlockY,int DirectionMultiplier,int RowsCleared)
        {
            //the actual block we are clearing...
            var ClearingBlock = _BaseState.PlayField.Contents[BlockY][BlockX];
            SKColor baseColor = TetrisGame.Choose(ClearLineParticleColours);
            Bitmap sourcebitmap = null;
            if (ClearingBlock!=null && ClearingBlock is ImageBlock ib)
            {
                sourcebitmap = new Bitmap(ib._RotationImages[MathHelper.mod(ib.Rotation,ib._RotationImages.Length)]);
                
            }
            var ReductionFactor = 10f / (_BaseState.PlayField.ColCount);
            var blockWidth = _BaseState.PlayField.GetBlockWidth((SKRect)pOwner.LastDrawBounds);
            var blockHeight = _BaseState.PlayField.GetBlockHeight((SKRect)pOwner.LastDrawBounds);
            var CoordPos = new BCPoint( BlockX,
                BlockY-2);
            lock (_BaseState.Particles)
            {
                List<BaseParticle> ToAdd = new List<BaseParticle>((int)(ParticleCountPerBlock*ReductionFactor));
                for (int i = 0; i < (int)(ParticleCountPerBlock*ReductionFactor); i++)
                {
                    BCPoint ParticlePos = new BCPoint((float)TetrisGame.rgen.NextDouble(), (float)TetrisGame.rgen.NextDouble());
                    //choose a new random position within the block.
                    BCPoint NewParticlePoint = new BCPoint(CoordPos.X + ParticlePos.X, CoordPos.Y + ParticlePos.Y );
                    BCPoint Velocity = new BCPoint(

                        (float)(DirectionMultiplier * (TetrisGame.rgen.NextDouble() * 1 + (Math.Abs(BlockX - (_BaseState.PlayField.ColCount / 2))/5))), 0
                    
                        );
                    BCColor ChosenColor = baseColor;

                    if (sourcebitmap!=null)
                    {
                        Point TargetPixel = new Point((int)(ParticlePos.X*sourcebitmap.Width),(int)(ParticlePos.Y*sourcebitmap.Height));
                        ChosenColor = sourcebitmap.GetPixel(TargetPixel.X, TargetPixel.Y);    
                    }
                    

                    BaseParticle p = new BaseParticle(NewParticlePoint, Velocity, ChosenColor);
                    if (RowsCleared >= 4)
                    {
                        if(TetrisGame.rgen.NextDouble()>0.25d)
                        {
                            p.ColorCalculatorFunction = BaseParticle.GetRainbowColorFunc(pOwner, 500);
                        }
                    }
                    p.TTL = (uint)ClearParticleTTL;
                    ToAdd.Add(p);

                }
                _BaseState.Particles.AddRange(ToAdd);
            }

            
        }
        private void ClearFrame_Outside_In(IStateOwner pOwner, KeyValuePair<int, NominoBlock[]> row,int RowsCleared)
        {
            var GrabRow = row.Value;
            int processindex = GrabRow.Length - CurrentClearIndex;
            int i = GrabRow.Length >> 1;
            int useindex = i + ((processindex % 2 == 0) ? processindex / 2 : -(processindex / 2 + 1));
            
            if (useindex < GrabRow.Length && useindex >= 0)
            {
                int ParticleSign = -1;
                if (useindex < GrabRow.Length / 2)
                {
                    ParticleSign = 1;
                }
                AddParticles(pOwner, useindex, row.Key, ParticleSign,RowsCleared);
                PerformClearAct(GrabRow, useindex);
            }
        }

        private void ClearFrame_Middle_Out(IStateOwner pOwner, KeyValuePair<int, NominoBlock[]> row, int RowsCleared)
        {
            var GrabRow = row.Value;

            int i = GrabRow.Length >> 1;
            int useindex = i + ((CurrentClearIndex % 2 == 0) ? CurrentClearIndex / 2 : -(CurrentClearIndex / 2 + 1));
            //middle out will have particles flying outwards from the center in the direction they are being cleared, added each block clear.
            //decide which direction we want the particles to move.

            if (useindex < GrabRow.Length && useindex >= 0)
            {
                int ParticleSign = 1;
                if (useindex < GrabRow.Length / 2)
                {
                    ParticleSign = -1;
                }
                AddParticles(pOwner, useindex, row.Key, ParticleSign,RowsCleared);
                PerformClearAct(GrabRow, useindex);
            }
        }

        private void ClearFrame_Right_To_Left(IStateOwner pOwner, KeyValuePair<int, NominoBlock[]> row,int RowsCleared)
        {
            var GrabRow = row.Value;
            int useIndex = GrabRow.Length - CurrentClearIndex - 1;
            //find the block, and clear it out if needed.
            var FindClear = CurrentClearIndex >= GrabRow.Length ? null : GrabRow[useIndex];

            if (FindClear != null)
            {
                int ParticleSign = -1;
                
                AddParticles(pOwner, useIndex, row.Key, ParticleSign,RowsCleared);
                PerformClearAct(GrabRow, useIndex);
            }
        }

        private void ClearFrame_Left_To_Right(IStateOwner pOwner, KeyValuePair<int, NominoBlock[]> row,int RowsCleared)
        {
            var GrabRow = row.Value;
            //find the block, and clear it out if needed.
            var FindClear = CurrentClearIndex >= GrabRow.Length ? null : GrabRow[CurrentClearIndex];
            if (FindClear != null)
            {
                int ParticleSign = 1;
                
                AddParticles(pOwner, CurrentClearIndex, row.Key, ParticleSign,RowsCleared);
                PerformClearAct(GrabRow, CurrentClearIndex);
            }
        }

        protected void PerformClearAct(NominoBlock[] FullRow, int index)
        {
            if(ClearAction==null)
                FullRow[index] = null;
            else
            {
                //if we have a clear action, then we want to create a new instance and set it up here, then add it to the ClearActivities Dictionary.
                //Remember- this is separate from our line clear style, and is used to animate each block "clearing" (shrinking away, fading, etc)
                FieldBlockClearTask fbct = new FieldBlockClearTask(ClearAction, new NominoBlock[] { FullRow[index] });
                ClearActivities.Add(fbct);
            }
        }
        List<FieldBlockClearTask> ClearActivities = new List<FieldBlockClearTask>();
        
            
        

      
    }

    public class FieldBlockClearTask
    {
        //used by main line clear state when needed to track progress of each individual block being cleared.
        //since the FieldBlockClearAction is stateless (or at least tries to be) it has no block references, so we keep track of those here
        public DateTime StartClearTime;
        public FieldBlockClearAction ClearAction;
        public IEnumerable<NominoBlock> Blocks;
        public FieldBlockClearTask(FieldBlockClearAction pClearAction,IEnumerable<NominoBlock> pBlocks)
        {
            ClearAction = pClearAction;
            Blocks = pBlocks;
            StartClearTime = DateTime.Now;
        }
    }

    public class FieldLineActionDissolve : FieldLineActionGameState
    {
        Queue<Point> ClearBlockList = null;
        int RowClearCount = 0;

        public FieldLineActionDissolve(GameplayGameState _BaseState, int[] ClearRows, IEnumerable<Action> pAfterClearActions) : base(_BaseState, ClearRows, pAfterClearActions)
        {
            List<Point> AllBlockPositions = new List<Point>();
            RowClearCount = ClearRows.Length;
            foreach (int grabrow in ClearRows)
            {
                var rowcontent = _BaseState.PlayField.Contents[grabrow];
                for (int addblock = 0; addblock < rowcontent.Length; addblock++)
                {
                    if (rowcontent[addblock] != null)
                    {
                        AllBlockPositions.Add(new Point(grabrow, addblock));
                    }
                }
            }

            ClearBlockList = new Queue<Point>();
            foreach (var choosernd in BagChooser.Shuffle(new Random(), AllBlockPositions))
            {
                ClearBlockList.Enqueue(choosernd);
            }
        }

        protected override bool ClearFrame(IStateOwner pOwner)
        {
            for (int clearblock = 0; clearblock < RowClearCount; clearblock++)
            {
                if (ClearBlockList.Count == 0) return true;
                var grabnext = ClearBlockList.Dequeue();
                _BaseState.PlayField.Contents[grabnext.X][grabnext.Y] = null;
                //_BaseState.PlayField.HasChanged = true;
            }

            return false;
        }
    }

    public class InsertBlockRowsActionGameState : FieldActionGameState
    {
        private IEnumerable<Action> AfterClear = Enumerable.Empty<Action>();

        private Queue<NominoBlock[]> RowInsertions = null;

        //private TetrisBlock[][] InsertionData = null;
        private int InsertionRow;


        public InsertBlockRowsActionGameState(GameplayGameState pBaseState, int InsertRow, NominoBlock[][] RowData, IEnumerable<Action> pAfterClearActions) : base(pBaseState)
        {
            RowInsertions = new Queue<NominoBlock[]>();
            foreach (NominoBlock[] AddRow in RowData)
            {
                RowInsertions.Enqueue(AddRow);
            }

            InsertionRow = InsertRow;
            AfterClear = pAfterClearActions;
        }

        int MSLineAddTime = 30; //number of ms between blocks being removed/cleared.
        int CurrentClearIndex = 0;
        int InsertedCount = 0;
        DateTime StartOperation = DateTime.MaxValue;
        DateTime LastOperation = DateTime.MaxValue;

        public override void GameProc(IStateOwner pOwner)
        {
            base.GameProc(pOwner);
            if (StartOperation == DateTime.MaxValue)
            {
                StartOperation = LastOperation = DateTime.Now;
            }

            //for now we clear horizontally across...
            if ((DateTime.Now - LastOperation).TotalMilliseconds > MSLineAddTime)
            {
                int InsertionIndex = _BaseState.PlayField.Contents.Length - 1 - InsertionRow - InsertedCount; //since high rows have lower indices, we want to "reverse" it.
                //if the queue is empty, we are now finished.
                if (RowInsertions.Count == 0)
                {
                    //reset the original State.
                    _BaseState.PlayField.HasChanged = true;

                    pOwner.CurrentState = _BaseState;
                    foreach (var iterate in AfterClear)
                    {
                        pOwner.EnqueueAction(iterate);
                    }
                }
                else
                {
                    //dequeue the next line of blocks to insert.
                    NominoBlock[] NextRow = RowInsertions.Dequeue();

                    //insert into the playfield at InsertionIndex.
                    //This means moving All rows from InsertionIndex up one.
                    for (int moverow = 0; moverow < InsertionIndex; moverow++)
                    {
                        NominoBlock[] ThisRow = _BaseState.PlayField.Contents[moverow + 1];
                        NominoBlock[] TargetRow = _BaseState.PlayField.Contents[moverow];

                        for (int copyCol = 0; copyCol < ThisRow.Length; copyCol++)
                        {
                            TargetRow[copyCol] = ThisRow[copyCol];
                        }
                    }

                    NominoBlock[] InsertedRow = _BaseState.PlayField.Contents[InsertionIndex];
                    for (int i = 0; i < InsertedRow.Length; i++)
                    {
                        InsertedRow[i] = NextRow[i % NextRow.Length];
                    }

                    InsertedCount++;
                    LastOperation = DateTime.Now;
                }


                //another way of doing this: we can just use DrawProc and draw the background over top, I suppose?
            }
        }

       
    }
}