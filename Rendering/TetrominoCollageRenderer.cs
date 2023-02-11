using BASeCamp.BASeScores;
using BASeTris.Blocks;
using BASeTris.Choosers;
using BASeTris.GameStates;
using BASeTris.GameStates.GameHandlers;
using BASeTris.Rendering.Skia;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Rendering
{

    public class FakeRendererHandler : IGameCustomizationHandler
    {
        public int ColumnWidth { get; set; }
        public int ColumnHeight { get; set; }

        TetrisStatistics _Stats = null;
        public FakeRendererHandler(int pWidth, int pHeight, int pLevel)
        {
            _Stats = new TetrisStatistics() {I_Line_Count=pLevel*10 };
            
        }
        public string Name => "Fake Collage Hander";

        public NominoTheme DefaultTheme => null;

        public BaseStatistics Statistics { get { return _Stats; }set { } }

        public GameOptions GameOptions => null;

        public BlockGroupChooser GetChooser(IStateOwner pOwner)
        {
            throw new NotImplementedException();
        }
        public virtual FieldCustomizationInfo GetFieldInfo()
        {
            return new FieldCustomizationInfo()
            {
                FieldRows = ColumnHeight,
                BottomHiddenFieldRows = 0,
                TopHiddenFieldRows = 0,
                FieldColumns = ColumnWidth
            };
        }


      

        public GameOverStatistics GetGameOverStatistics(GameplayGameState state, IStateOwner pOwner)
        {
            return null;
        }

   
        public IHighScoreList<TetrisHighScoreData> GetHighScores()
        {
            return null;
        }

        public Nomino[] GetNominos()
        {
            return null;
        }

        public IGameCustomizationStatAreaRenderer<TRenderTarget, GameplayGameState, TDataElement, IStateOwner> GetStatAreaRenderer<TRenderTarget, TDataElement>()
        {
            return null;
        }

        public IGameCustomizationHandler NewInstance()
        {
            return null;
        }

        public void PrepareField(GameplayGameState state, IStateOwner pOwner)
        {
            
        }

        public FieldChangeResult ProcessFieldChange(GameplayGameState state, IStateOwner pOwner, Nomino Trigger)
        {
            return null;
        }
    }

    /// <summary>
    /// This class is responsible for effectively taking an arrangement of Nominos and a theme and some additional control properties, and generating a Bitmap out of it.
    /// The aim/purpose of this is for visual flourishes or elements on stuff like the title screen. Eg it would have corner/borders constructed out of tetrominoes- at least, that is the idea.
    /// </summary>
    public class TetrominoCollageRenderer
    {
        public static SKBitmap GetNominoBitmap(NominoTheme _theme)
        {
            //similar to "Collage", but we only want to use One Nomino, at random. (one of the tetrominoes, for the moment.)
            int generatedlevel = TetrisGame.rgen.Next(0, 21);
            //first, choose a random Tetromino Type.
            var buildNominoFunc = TetrisGame.Choose(Tetrominoes.Tetromino.StandardTetrominoFunctions);
            Nomino GenerateResult = buildNominoFunc();

            //with the generated Nomino in hand, construct a CollageRenderer instance the size of this Nomino.
            GenerateResult.X = 0;
            GenerateResult.Y = 0;
            GenerateResult.RecalcExtents();

            TetrominoCollageRenderer tcr = new TetrominoCollageRenderer( GenerateResult.GroupExtents.Right+1, GenerateResult.GroupExtents.Bottom+1, 500, 500, generatedlevel, _theme,SKColors.Transparent);
            _theme.ApplyTheme(GenerateResult, null, tcr._field, NominoTheme.ThemeApplicationReason.NewNomino);
            tcr.AddNomino(GenerateResult);

            SKBitmap CreateBitmap = tcr.Render();
            
            return CreateBitmap;
        }
        public static SKBitmap GetBackgroundCollage(NominoTheme _theme)
        {
            //this is a fun one. We've got a bitmap we use for backgrounds, Would be cool if we generated that "on the fly" using a theme, I think.
            //this collage has a number of Nomino blocks.
            int generatedlevel = TetrisGame.rgen.Next(0, 21);
            TetrominoCollageRenderer tcr = new TetrominoCollageRenderer(6, 6, 500, 500, generatedlevel,_theme,SKColors.Black);

            //two I Nominoes. I think 0,0 for each Nomino is their top-left corner. I really should know this, I wrote the thing but time is a cruel mistress. I'm into that shit though.
            
            Tetrominoes.Tetromino_I  IBlock = new Tetrominoes.Tetromino_I() {X=-1,Y=-3};
            Tetrominoes.Tetromino_I  IBlock2 = new Tetrominoes.Tetromino_I() { X = -1, Y = 3 };

            //both are up and down, so rotate the I blocks.
            IBlock.Rotate(false);
            IBlock2.Rotate(false);
            Tetrominoes.Tetromino_T TBlock1 = new Tetrominoes.Tetromino_T() {X=2,Y=1 };
            TBlock1.Rotate(false);
            TBlock1.Rotate(false);
            TBlock1.Rotate(false);
            Tetrominoes.Tetromino_T TBlock2 = new Tetrominoes.Tetromino_T() {X=3,Y=-1 };
            TBlock2.Rotate(false);
            Tetrominoes.Tetromino_T TBlock3 = new Tetrominoes.Tetromino_T() { X = 3, Y = 5 };
            TBlock3.Rotate(false);

            Tetrominoes.Tetromino_L LBlock1 = new Tetrominoes.Tetromino_L() { X = -1, Y = 0 };
            Tetrominoes.Tetromino_L LBlock2 = new Tetrominoes.Tetromino_L() { X = 1, Y = 3 };
            LBlock1.Rotate(false);
            LBlock2.Rotate(false);
            Tetrominoes.Tetromino_S SBlock = new Tetrominoes.Tetromino_S() { X = 1, Y = 0 };

            Tetrominoes.Tetromino_J JBlock1 = new Tetrominoes.Tetromino_J() { X = -1, Y = 3 };
            Tetrominoes.Tetromino_J JBlock2 = new Tetrominoes.Tetromino_J() { X = 5, Y = 3 };
            JBlock1.Rotate(true);
            JBlock2.Rotate(true);

            Tetrominoes.Tetromino_Z ZBlock1 = new Tetrominoes.Tetromino_Z() { X = 3, Y = 1 };
            ZBlock1.Rotate(false);

            Tetrominoes.Tetromino_L LBlock3 = new Tetrominoes.Tetromino_L() { X = 3, Y = 3 };

            //LBlock3.Rotate(true);
            bool doRandomize = TetrisGame.rgen.NextDouble() > 0.5;
            Nomino[][] AddBlocks = new Nomino[][]{ new Nomino[] { IBlock, IBlock2 } ,new Nomino[] { TBlock1, TBlock2, TBlock3 } , new Nomino[] { SBlock }, new Nomino[] { LBlock1, LBlock2 }, new Nomino[] { JBlock1, JBlock2 }, new Nomino[] { ZBlock1 }, new Nomino[] { LBlock3 } };
            foreach (var groupadd in AddBlocks)
            {
                foreach (var blockadd in groupadd)
                {
                    _theme.ApplyTheme(blockadd, null, tcr._field, NominoTheme.ThemeApplicationReason.FieldSet);
                    tcr.AddNomino(blockadd);
                }
                if (doRandomize) (tcr.DummyHandler.Statistics as TetrisStatistics).SetLineCount(typeof(Tetrominoes.Tetromino_I), TetrisGame.rgen.Next(0, 21));
            }

            SKBitmap CreateBitmap = tcr.Render();




            return CreateBitmap;


        }

        private SKColor ClearColor = SKColors.Transparent;
        private TetrisField _field = null;
        public NominoTheme Theme { get { return _field.Theme; } set { _field.Theme = value; } }
        public int ColumnCount { get; set; }
        public int RowCount { get; set; }
        public int ColumnWidth { get; set; }
        public int RowHeight { get; set; }

        public void AddNomino(Nomino Source)
        {

            _field.SetGroupToField(Source);

        }
        public IGameCustomizationHandler DummyHandler { get; }
        public TetrominoCollageRenderer(int pColumnCount, int pRowCount, int pColumnWidth, int pRowHeight,int pLevel,NominoTheme _theme,SKColor pClearColor)
        {
            //Note: We can get away with no CustomizationHandler here, as we aren't going to actually be having the Field "do" anything other than use it as a render source.
            ClearColor = pClearColor;
            ColumnCount = pColumnCount;
            RowCount = pRowCount;
            ColumnWidth = pColumnWidth;
            RowHeight = pRowHeight;
            DummyHandler = new FakeRendererHandler(pRowCount, pColumnCount, pLevel);
            _field = new TetrisField(_theme,DummyHandler, RowCount, ColumnCount);
        }
        public SKBitmap Render()
        {
            Size BitmapSize = new Size((int)(ColumnCount+1) * (ColumnWidth), (int)RowHeight* (RowCount+1));
            SKImageInfo info = new SKImageInfo(BitmapSize.Width, BitmapSize.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
            SKBitmap BuiltRepresentation = new SKBitmap(info, SKBitmapAllocFlags.ZeroPixels);

            using (SKCanvas DrawRep = new SKCanvas(BuiltRepresentation))
            {
                DrawRep.Clear(ClearColor);
                //var bg = _field.Theme.GetThemePlayFieldBackground(_field, _field.Handler);
                //bg.

                Object grabdata = RenderingProvider.Static.GetExtendedData(_field.GetType(), _field,
                    (o) =>
                        new TetrisFieldDrawSkiaParameters()
                        {
                            Bounds = new SKRect(0, 0, BitmapSize.Width, BitmapSize.Height),
                            COLCOUNT = ColumnCount,
                            ROWCOUNT = RowCount,
                            FieldBitmap = null,
                            LastFieldSave = SKRect.Empty,
                            VISIBLEROWS = RowCount
                        }) ;

                TetrisFieldDrawSkiaParameters parameters = (TetrisFieldDrawSkiaParameters)grabdata;
                parameters.LastFieldSave = parameters.Bounds;
                //if (!Source.GameHandler.AllowFieldImageCache) parameters.FieldBitmap = null;
                RenderingProvider.Static.DrawElement(null, DrawRep, _field, parameters);
            }


            return BuiltRepresentation;

        }

    }
}
