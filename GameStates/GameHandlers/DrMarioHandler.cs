using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BASeCamp.BASeScores;
using BASeTris.Blocks;
using BASeTris.Choosers;
using BASeTris.Theme.Block;
using SkiaSharp;

namespace BASeTris.GameStates.GameHandlers
{
    public class DrMarioHandler : IGameCustomizationHandler
    {
        public DrMarioStatistics Statistics { get; private set; } = new DrMarioStatistics();
        BaseStatistics IGameCustomizationHandler.Statistics { get { return this.Statistics; }  }
        public BlockGroupChooser Chooser  {
            get {
                var result = Duomino.Duomino.BagTetrominoChooser();
                result.ResultAffector = DrMarioNominoTweaker;
                return result;
    } }

        private void DrMarioNominoTweaker(Nomino Source)
        {
            foreach(var iterate in Source)
            {
                if (iterate.Block is LineSeriesBlock lsb)
                {
                    lsb.CombiningIndex = TetrisGame.Choose(new LineSeriesBlock.CombiningTypes[] { LineSeriesBlock.CombiningTypes.Yellow, LineSeriesBlock.CombiningTypes.Red, LineSeriesBlock.CombiningTypes.Blue });
                }
            }
            
            //tweak the nomino and set a random combining index.
        }
        public IHighScoreList<TetrisHighScoreData> GetHighScores()
        {
            return null;
        }

        public IGameCustomizationHandler NewInstance()
        {
            return new DrMarioHandler();
        }
    
        //finds critical mass excesses starting from the given position.
        //this basically only finds one specific "critical mass"
        private HashSet<Point> FindCriticalMassesHorizontal(GameplayGameState state, IStateOwner pOwner, Point StartPosition)
        {
            HashSet<Point> FoundPoints = new HashSet<Point>();
            var OurPos = state.PlayField.Contents[StartPosition.Y][StartPosition.X] as LineSeriesBlock;
            if (OurPos == null) return new HashSet<Point>();
            else
            {
               int CurrentMass = 0;
                int FirstCol = StartPosition.X;
               //first, we look to the left until we no longer find a matching block.

               for(int X=StartPosition.X-1;X > 0;X--)
                {
                    var CheckPos = state.PlayField.Contents[StartPosition.Y][X] as LineSeriesBlock;
                    if (CheckPos == null) break;
                    if (CheckPos.CombiningIndex == OurPos.CombiningIndex)
                        FirstCol = X;
                }

                //starting from FirstCol we will work through and find all matching combining indices....
                int HorizontalMass = 1; //start at one since we know we have one block.
                int MaxCriticalHorz = 4;
                List<Point> Horizontals = new List<Point>();
                Horizontals.Add(StartPosition);
            for(int X = FirstCol;X<state.PlayField.ColCount;X++)
                {
                    var CheckPos = state.PlayField.Contents[StartPosition.Y][X] as LineSeriesBlock;
                    if(CheckPos.CombiningIndex==OurPos.CombiningIndex)
                    {
                        MaxCriticalHorz = Math.Max(MaxCriticalHorz, CheckPos.CriticalMass);
                        Horizontals.Add(new Point(X, StartPosition.Y));
                        HorizontalMass++;

                    }
                }
            
                if(HorizontalMass > MaxCriticalHorz)
                {
                    foreach(var iterate in Horizontals)
                    {
                        if(!FoundPoints.Contains(iterate))
                        {
                            FoundPoints.Add(iterate);
                        }
                    }
                }
            }
            return FoundPoints;
        }
        private HashSet<Point> FindCriticalMassesVertical(GameplayGameState state, IStateOwner pOwner, Point StartPosition)
        {
            HashSet<Point> FoundPoints = new HashSet<Point>();
            var OurPos = state.PlayField.Contents[StartPosition.Y][StartPosition.X] as LineSeriesBlock;
            if (OurPos == null) return new HashSet<Point>();
            else
            {
                int CurrentMass = 0;
                int FirstRow = StartPosition.Y;
                //first, we look to the left until we no longer find a matching block.

                for (int Y = StartPosition.Y - 1; Y > 0; Y--)
                {
                    var CheckPos = state.PlayField.Contents[Y][StartPosition.X] as LineSeriesBlock;
                    if (CheckPos == null) break;
                    if (CheckPos.CombiningIndex == OurPos.CombiningIndex)
                        FirstRow = Y;
                }

                //starting from FirstCol we will work through and find all matching combining indices....
                int VerticalMass = 1; //start at one since we know we have one block.
                int MaxCriticalVert = 4;
                List<Point> Verticals = new List<Point>();
                Verticals.Add(StartPosition);
                for (int Y = FirstRow; Y < state.PlayField.RowCount; Y++)
                {
                    var CheckPos = state.PlayField.Contents[Y][StartPosition.X] as LineSeriesBlock;
                    if (CheckPos.CombiningIndex == OurPos.CombiningIndex)
                    {
                        MaxCriticalVert = Math.Max(MaxCriticalVert, CheckPos.CriticalMass);
                        Verticals.Add(new Point(StartPosition.X, Y));
                        VerticalMass++;

                    }
                }

                if (VerticalMass > MaxCriticalVert)
                {
                    foreach (var iterate in Verticals)
                    {
                        if (!FoundPoints.Contains(iterate))
                        {
                            FoundPoints.Add(iterate);
                        }
                    }
                }
            }
            return FoundPoints;
        }
        private HashSet<Point> FindCriticalMasses(GameplayGameState state, IStateOwner pOwner, Point StartPosition)
        {

            HashSet<Point> Horizontal = FindCriticalMassesHorizontal(state, pOwner, StartPosition);
            HashSet<Point> Vertical = FindCriticalMassesVertical(state, pOwner, StartPosition);
            foreach(Point verticalpoint in Vertical)
            {
                if (!Horizontal.Contains(verticalpoint)) Horizontal.Add(verticalpoint);
            }
            return Horizontal;

        }
        public FieldChangeResult ProcessFieldChange(GameplayGameState state, IStateOwner pOwner, Nomino Trigger)
        {

            //here we would go through the field and handle where the blocks line up to more than the required critical mass. 
            //Remove those blocks from the field.
            //then, reprocess the field: find any unsupported blocks, and generate new ActiveBlockGroups for them. Add them to the list of active block groups. Set the fallspeed appropriately.
            //if we found any unsupported blocks groups, change the state to the GroupFallState (not defined) which is a composite state that doesn't allow input, and waits for all active block groups to come to rest before
            //continuing.

            //once all block groups come to rest, ProcessFieldChange will be called again.

            //Note: for visual flair eventually we'll want to have a temporary state which does nothing but allow the blocks being destroyed to be indicated for perhaps 250ms, before advancing to the state where blocks
            //will fall



            return new FieldChangeResult() { ScoreResult = 5 };

        }
        public TetrominoTheme DefaultTheme { get { return new DrMarioTheme(); } }
        public void PrepareField(GameplayGameState state, IStateOwner pOwner)
        {
            //need to come up with a proper way of implementing progressive levels.
            //lkikely will need to have stats and stuff abstracted to each Handler.
            //for now, we'll generate say 25 random viruses and toss them in the lower half of the playfield.

            HashSet<SKPointI> usedPositions = new HashSet<SKPointI>();
            for(int i=0;i<25;i++)
            {
                //choose a random virus type.
                var chosentype = TetrisGame.Choose(new LineSeriesBlock.CombiningTypes[] { LineSeriesBlock.CombiningTypes.Yellow, LineSeriesBlock.CombiningTypes.Red, LineSeriesBlock.CombiningTypes.Blue });
                LineSeriesMasterBlock lsmb = new LineSeriesMasterBlock() { CombiningIndex = chosentype };
                var Dummino = new Nomino() { };
                Dummino.AddBlock(new Point[] { new Point(0, 0) }, lsmb);
                state.PlayField.Theme.ApplyTheme(Dummino,this, state.PlayField);
                lsmb.CriticalMass = 4;
                
                int RandomXPos = TetrisGame.rgen.Next(state.PlayField.ColCount);
                int RandomYPos = state.PlayField.RowCount - 1 -TetrisGame.rgen.Next(state.PlayField.RowCount / 2);
                SKPointI randomPos = new SKPointI(RandomXPos, RandomYPos);
                while(usedPositions.Contains(randomPos))
                {
                    int rndXPos = TetrisGame.rgen.Next(state.PlayField.ColCount);
                    int rndYPos = state.PlayField.RowCount - 1 - TetrisGame.rgen.Next(state.PlayField.RowCount / 2);
                     randomPos = new SKPointI(rndXPos, rndYPos);
                }
                state.PlayField.Contents[RandomYPos][RandomXPos] = lsmb;




            }


        }
    }
}
