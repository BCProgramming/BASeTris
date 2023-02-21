using BASeCamp.BASeScores;
using BASeTris.AI;
using BASeTris.Choosers;
using BASeTris.Rendering.GDIPlus;
using BASeTris.Rendering.Skia;
using BASeTris.Tetrominoes;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.GameStates.GameHandlers
{
   

    /// <summary>
    /// ICustomizationHandler that handles the standard tetris game.
    /// </summary>
    [GameScoringHandler(typeof(StandardTetrisAIScoringHandler),typeof(StoredBoardState.TetrisScoringRuleData))]
    [HandlerTipText("Standard Tetris, Marathon Mode")]
    public class StandardTetrisHandler : IGameCustomizationHandler,IGameHandlerChooserInitializer
    {
        public bool ProgressiveMode = false;
        public virtual String Name { get { return "Tetris"; } }
        private int LastScoreCalc = 0;
        private int LastScoreLines = 0;
        public IList<HotLine> HotLines { get; set; } = new List<HotLine>();
        private Choosers.BlockGroupChooser _Chooser;
        public bool AllowFieldImageCache { get { return true; } }
        public TetrisStatistics Statistics { get; private set; } = new TetrisStatistics();
        BaseStatistics IGameCustomizationHandler.Statistics {  get { return this.Statistics; } }

        public virtual FieldCustomizationInfo GetFieldInfo()
        {
            return new FieldCustomizationInfo()
            {
                FieldRows = TetrisField.DEFAULT_ROWCOUNT + (ProgressiveMode ? 5 : 0),
                BottomHiddenFieldRows = 0,
                TopHiddenFieldRows = 2,
                FieldColumns = TetrisField.DEFAULT_COLCOUNT + (ProgressiveMode ? 4 : 0)
            };
        }

        /*public virtual int GetFieldRowHeight() { return TetrisField.DEFAULT_ROWCOUNT+ (ProgressiveMode ? 5 : 0); }
        public virtual int GetHiddenRowCount()
        {
            return 2;
        }
        public virtual int GetFieldColumnWidth()
        {
            return TetrisField.DEFAULT_COLCOUNT+(ProgressiveMode?4:0);
        }*/
        public GameOverStatistics GetGameOverStatistics(GameplayGameState state, IStateOwner pOwner)
        {

          


            Type[] TetTypes = new Type[] {typeof(Tetrominoes.Tetromino_I),
                            typeof(Tetrominoes.Tetromino_I) ,
                            typeof(Tetrominoes.Tetromino_O) ,
                            typeof(Tetrominoes.Tetromino_T) ,
                            typeof(Tetrominoes.Tetromino_J) ,
                            typeof(Tetrominoes.Tetromino_L) ,
                            typeof(Tetrominoes.Tetromino_S),
                        typeof(Tetrominoes.Tetromino_Z)};


            GameOverStatistic I_Stat = GetTetrominoStatistic(state, typeof(Tetrominoes.Tetromino_I));
            GameOverStatistic O_Stat = GetTetrominoStatistic(state, typeof(Tetrominoes.Tetromino_O));
            GameOverStatistic T_Stat = GetTetrominoStatistic(state, typeof(Tetrominoes.Tetromino_T));
            GameOverStatistic J_Stat = GetTetrominoStatistic(state, typeof(Tetrominoes.Tetromino_J));
            GameOverStatistic L_Stat = GetTetrominoStatistic(state, typeof(Tetrominoes.Tetromino_L));
            GameOverStatistic S_Stat = GetTetrominoStatistic(state, typeof(Tetrominoes.Tetromino_S));
            GameOverStatistic Z_Stat = GetTetrominoStatistic(state, typeof(Tetrominoes.Tetromino_Z));



            GameOverStatistics StatResult = new GameOverStatistics(
              new GameOverStatistic(new GameOverStatisticColumnData("---Line Clears---", Color.White, Color.Black, GameOverStatisticColumnData.HorizontalAlignment.Middle)),
              
              I_Stat,
              O_Stat,
              T_Stat,
              J_Stat,
              L_Stat,
              S_Stat,
              Z_Stat);

            return StatResult;



        }
        private GameOverStatistic GetTetrominoStatistic(GameplayGameState state,Type TetrominoType)
        {
            GameplayGameState standardgame = state;
            var useStats = standardgame.GameStats as TetrisStatistics;
            SKBitmap I_Tet = standardgame.GetTetrominoSKBitmap(TetrominoType);

            SKImage ski = SKImage.FromBitmap(I_Tet);

            GameOverStatistic result = new GameOverStatistic(
                new GameOverStatisticColumnData(ski, GameOverStatisticColumnData.HorizontalAlignment.Left, GameOverStatisticColumnData.VerticalAlignment.Top),
                new GameOverStatisticColumnData(Statistics.GetLineCount(TetrominoType).ToString(), SKColors.White, SKColors.Black, GameOverStatisticColumnData.HorizontalAlignment.Right, GameOverStatisticColumnData.VerticalAlignment.Middle));



            return result;
        }

        public TetrisGameOptions GameOptions { get;  } = new TetrisGameOptions();
        GameOptions IGameCustomizationHandler.GameOptions => this.GameOptions;
      
        public Nomino[] GetNominos()
        {
            Tetromino_I TetI = new Tetromino_I();
            Tetromino_J TetJ = new Tetromino_J();
            Tetromino_L TetL = new Tetromino_L();
            Tetromino_O TetO = new Tetromino_O();
            Tetromino_S TetS = new Tetromino_S();
            Tetromino_T TetT = new Tetromino_T();
            Tetromino_Z TetZ = new Tetromino_Z();
            return new Nomino[] { TetI, TetJ, TetL, TetO, TetS, TetT, TetZ};
        }

        public BlockGroupChooser CreateSupportedChooser(Type DesiredChooserType)
        {
            if (DesiredChooserType == typeof(BagChooser))
            {
                BagChooser bc = new BagChooser(Tetromino.StandardTetrominoFunctions);
                
                return bc;
            }
            else if (DesiredChooserType == typeof(NESChooser))
            {
                NESChooser nc = new NESChooser(Tetromino.StandardTetrominoFunctions);
                
                return nc;
            }
            else if (DesiredChooserType == typeof(GameBoyChooser))
            {
                GameBoyChooser gbc = new GameBoyChooser(Tetromino.StandardTetrominoFunctions);
                
                return gbc;
            }
            else if (DesiredChooserType == typeof(FullRandomChooser))
            {
                FullRandomChooser frc = new FullRandomChooser(Tetromino.StandardTetrominoFunctions);
                
                return frc;
            }
            return null;
        }

        private String CurrentChooserValue = null;

        public virtual Choosers.BlockGroupChooser GetChooser(IStateOwner pOwner)
        {
            //TODO: proper per-handler configs should include the chooser class to use.
            if (_Chooser == null || CurrentChooserValue != pOwner.Settings.std.Chooser)
            {
                CurrentChooserValue = pOwner.Settings.std.Chooser;
                Type createType = BlockGroupChooser.ChooserTypeFromString(CurrentChooserValue);
                if (createType == null)
                {
                    CurrentChooserValue = pOwner.Settings.std.Chooser;
                    createType = typeof(BagChooser);
                }
                _Chooser = CreateSupportedChooser(createType);
            }
            if (ProgressiveMode)
            {
                _Chooser = new CompositeChooser(new BlockGroupChooser[] { _Chooser, new NTrisChooser(5), new NTrisChooser(6) },
                    (bgc) =>
                    {
                        if (bgc is NTrisChooser ntc)
                        {
                            //NTrischooser weight is based on the current level.
                            if (pOwner.CurrentState is GameplayGameState ggs)
                            {
                                var lc = (ggs.GameStats as TetrisStatistics).LineCount;

                                //if (lc < 30) return 0; //no chance at all before level 3.


                                if (lc < 20 && ntc.BlockCount == 6) return 0; //no chance of sixers before level 2.
                                //we are higher than level 3. chance starts at 0.5% and goes up by .25% each level.
                                return 3*((float)lc) / 10 * 0.25f;


                            }
                            else
                            {
                                return 0f;
                            }
                        }
                        else
                        {
                            return 100.0f;
                        }
                    });
            }
            return _Chooser;
        }
        public virtual IHighScoreList GetHighScores() 
        {
            return (IHighScoreList)TetrisGame.ScoreMan["Standard"];
        }

        public IGameCustomizationHandler NewInstance()
        {
            return new StandardTetrisHandler();
        }
        private int GetScore(int LinesCleared, IList<HotLine> ClearedHotLines, GameplayGameState state, IStateOwner pOwner, Nomino Trigger)
        {
            var GameStats = Statistics;
            int result = LinesCleared;
            int AddScore = 0;
            if (result >= 1) AddScore += ((GameStats.LineCount / 10) + 1) * 15;
            if (result >= 2)
                AddScore += ((GameStats.LineCount / 10) + 2) * 30;
            if (result >= 3)
                AddScore += ((GameStats.LineCount / 10) + 3) * 45;
            if (result >= 4)
                AddScore += AddScore + ((GameStats.LineCount / 10) + 5) * 75;

            if (ClearedHotLines != null)
            {
                double SumMult = 00;
                foreach (var iterate in HotLines)
                {
                    SumMult += iterate.Multiplier;
                }
                AddScore = (int)((double)AddScore * SumMult);
            }

            LastScoreCalc = AddScore;

            if (LastScoreLines == result) //getting the same lines in a row gives added score.
            {
                AddScore *= 2;
            }

            LastScoreLines = result;
            
            return AddScore;
            

        }
        public FieldChangeResult ProcessFieldChange(GameplayGameState state, IStateOwner pOwner, Nomino Trigger)
        {
            var HotLines = new List<HotLine>();
            FieldChangeResult FCR = new FieldChangeResult();
            int rowsfound = 0;
            List<int> CompletedRows = new List<int>();
            List<Action> AfterClearActions = new List<Action>();
            var PlayField = state.PlayField;
            var Sounds = state.Sounds;
            var GameOptions = state.GameOptions;
            
            //checks the field contents for lines. If there are lines found, they are removed, and all rows above it are shifted down.
            for (int r = 0; r < PlayField.RowCount; r++)
            {
                if (PlayField.Contents[r].All((d) => d != null))
                {
                    Debug.Print("Found completed row at row " + r);
                    if (PlayField.Flags.HasFlag(TetrisField.GameFlags.Flags_Hotline) && PlayField.HotLines.ContainsKey(r))
                    {
                        Debug.Print("Found hotline row at row " + r);
                        HotLines.Add(PlayField.HotLines[r]);
                    }
                    CompletedRows.Add(r);
                    rowsfound++;
                    //enqueue an action to perform the clear. We'll be replacing the current state with a clear action state, so this should execute AFTER that state returns control.
                    var r1 = r;
                    AfterClearActions.Add
                    (() =>
                    {
                        for (int g = r1; g > 0; g--)
                        {
                            Debug.Print("Moving row " + (g - 1).ToString() + " to row " + g);

                            for (int i = 0; i < PlayField.ColCount; i++)
                            {
                                PlayField.Contents[g][i] = PlayField.Contents[g - 1][i];
                            }
                        }
                    });
                }
            }
            AfterClearActions.Add(() => { PlayField.HasChanged = true; });

            long PreviousLineCount = Statistics.LineCount;
            if (Trigger != null)
            {
                Statistics.AddLineCount(Trigger.GetType(), rowsfound);
            }

            if ((PreviousLineCount / 10) < (Statistics.LineCount / 10))
            {
                state.InvokePlayFieldLevelChanged(state, new TetrisField.LevelChangeEventArgs((int)Statistics.LineCount / 10));
                Statistics.SetLevelTime(pOwner.GetElapsedTime());

                state.Sounds.PlaySound(pOwner.AudioThemeMan.LevelUp.Key, pOwner.Settings.std.EffectVolume);
                PlayField.SetFieldColors(this);
                state.f_RedrawStatusBitmap = true;
            }

            if (rowsfound > 0 && rowsfound < 4)
            {
                Sounds.PlaySound(pOwner.AudioThemeMan.ClearLine.Key, pOwner.Settings.std.EffectVolume * 2);
            }
            else if (rowsfound >= 4)
            {
                Sounds.PlaySound(pOwner.AudioThemeMan.ClearTetris.Key, pOwner.Settings.std.EffectVolume * 2);
                if (rowsfound > 4)
                {
                    Sounds.PlaySound(pOwner.AudioThemeMan.ClearLine.Key, pOwner.Settings.std.EffectVolume * 2);
                    Sounds.PlaySound(pOwner.AudioThemeMan.ClearLine.Key, pOwner.Settings.std.EffectVolume * 2);
                    Sounds.PlaySound(pOwner.AudioThemeMan.ClearTetris.Key, pOwner.Settings.std.EffectVolume * 2);
                }
            }


            int topmost = PlayField.RowCount;
            //find the topmost row with any blocks.
            for (int i = 0; i < PlayField.RowCount; i++)
            {
                if (PlayField.Contents[i].Any((w) => w != null))
                {
                    topmost = i;
                    break;
                }
            }

            topmost = topmost + rowsfound; //subtract the rows that were cleared to get an accurate measurement.
            if (topmost < 9)
            {
                if (state.currenttempo == 1)
                {
                    state.currenttempo = 68;
                    if (GameOptions.MusicRestartsOnTempoChange || String.Equals(pOwner.Settings.std.MusicOption, "tetris_nes_theme", StringComparison.OrdinalIgnoreCase))
                    {
                        if (GameOptions.MusicEnabled) Sounds.PlayMusic(pOwner.Settings.std.MusicOption, pOwner.Settings.std.MusicVolume, true);
                    }

                    var grabbed = Sounds.GetPlayingMusic_Active();
                    if (grabbed != null)
                    {
                        Sounds.GetPlayingMusic_Active().Tempo = 75f;
                    }
                }
            }
            else
            {
                if (state.currenttempo != 1)
                {
                    state.currenttempo = 1;
                    if (GameOptions.MusicRestartsOnTempoChange || String.Equals(pOwner.Settings.std.MusicOption, "tetris_nes_theme", StringComparison.OrdinalIgnoreCase))
                        if (GameOptions.MusicEnabled)
                        {
                            if (pOwner.Settings.std.MusicOption == "<RANDOM>")
                                Sounds.PlayMusic(pOwner.AudioThemeMan.BackgroundMusic.Key, pOwner.Settings.std.MusicVolume, true);
                            else
                            {
                                Sounds.PlayMusic(pOwner.Settings.std.MusicOption, pOwner.Settings.std.MusicVolume, true);
                            }
                        }
                    var grabbed = Sounds.GetPlayingMusic_Active();
                    if (grabbed != null) grabbed.Tempo = 1f;
                }
            }

            PlayField.HasChanged |= rowsfound > 0;

            if (rowsfound > 0)
            {
                var ClearState = new FieldLineActionGameState(state, CompletedRows.ToArray(), AfterClearActions);
                ClearState.ClearStyle = TetrisGame.Choose((FieldLineActionGameState.LineClearStyle[])(Enum.GetValues(typeof(FieldLineActionGameState.LineClearStyle))));

                pOwner.CurrentState = ClearState;
            }

            //if(rowsfound > 0) pOwner.CurrentState = new FieldLineActionDissolve(this,CompletedRows.ToArray(),AfterClearActions);
            var scoreresult = GetScore(rowsfound, HotLines, state, pOwner, Trigger);
            pOwner.Feedback(0.9f * (float)scoreresult, scoreresult * 250);
            FCR.ScoreResult = rowsfound;
            return FCR;

        }
        public NominoTheme DefaultTheme { 

            get { 
                
                

                return new SNESTetrominoTheme(); 
            } }

        public void PrepareField(GameplayGameState state, IStateOwner pOwner)
        {
            //nothing needed here.
        }
        private StandardTetrisSkiaStatAreaRenderer StatRenderer = null;
        public IGameCustomizationStatAreaRenderer<TRenderTarget, GameplayGameState, TDataElement, IStateOwner> GetStatAreaRenderer<TRenderTarget, TDataElement>()
        {
            if (typeof(TRenderTarget) == typeof(SKCanvas))
            {
                if (StatRenderer == null)
                    StatRenderer = new StandardTetrisSkiaStatAreaRenderer();
                return (IGameCustomizationStatAreaRenderer < TRenderTarget, GameplayGameState, TDataElement, IStateOwner > )StatRenderer;
            }
            return null;
        }
    }
    public class StandardTetrisSkiaStatAreaRenderer : IGameCustomizationStatAreaRenderer<SKCanvas, GameplayGameState, GameStateSkiaDrawParameters, IStateOwner>
    {
        SKPaint BlackBrush = new SKPaint() { Color = SKColors.Black, Style = SKPaintStyle.StrokeAndFill };
        SKPaint WhiteBrush = new SKPaint() { Color = SKColors.White, Style = SKPaintStyle.StrokeAndFill };
        public void Render(IStateOwner pOwner, SKCanvas pRenderTarget, GameplayGameState Source, GameStateSkiaDrawParameters Element)
        {
            var g = pRenderTarget;
            SKTypeface standardFont = TetrisGame.RetroFontSK;
            Type[] useTypes = new Type[] { typeof(Tetromino_I), typeof(Tetromino_O), typeof(Tetromino_J), typeof(Tetromino_T), typeof(Tetromino_L), typeof(Tetromino_S), typeof(Tetromino_Z) };
            int[] PieceCounts = null;
            var useStats = Source.GameStats;
            var Bounds = Element.Bounds;
            var Factor = Bounds.Height / 280.28d;
            var DesiredFontPixelHeight = 22d; //  PixelsToPoints((int)(Bounds.Height * (30d / 644d)));
            float DesiredFontSize = (float)(DesiredFontPixelHeight * pOwner.ScaleFactor);
            if (useStats is TetrisStatistics ts)
            {
                PieceCounts = new int[] { ts.I_Piece_Count, ts.O_Piece_Count, ts.J_Piece_Count, ts.T_Piece_Count, ts.L_Piece_Count, ts.S_Piece_Count, ts.Z_Piece_Count };
            }
            else
            {
                PieceCounts = new int[] { 0, 0, 0, 0, 0, 0, 0 };
            }
            float StartYPos = Bounds.Top; // + (int)(140 * Factor);
            float useXPos = Bounds.Left;// + (int)(30 * Factor);
            //ImageAttributes ShadowTet = TetrisGame.GetShadowAttributes();
            for (int i = 0; i < useTypes.Length; i++)
            {
                if (Source.GameHandler is StandardTetrisHandler)
                {
                    BlackBrush.TextSize = DesiredFontSize;
                    WhiteBrush.TextSize = DesiredFontSize;
                    SKPoint BaseCoordinate = new SKPoint(useXPos, StartYPos + (int)((float)i * (40d * Factor)));
                    
                    
                    String StatText = "" + PieceCounts[i];
                    SKRect StatTextSize = new SKRect();
                    BlackBrush.MeasureText(StatText, ref StatTextSize);
                    SKPoint TextPos = new SKPoint(useXPos + (int)(100d * Factor), BaseCoordinate.Y + StatTextSize.Height);
                    //SizeF StatTextSize = g.MeasureString(StatText, standardFont);
                    SKBitmap TetrominoImage = Source.GetTetrominoSKBitmap(useTypes[i]);
                    PointF ImagePos = new PointF(BaseCoordinate.X, BaseCoordinate.Y + (StatTextSize.Height / 2 - TetrominoImage.Height / 2));
                    SKRect DrawRect = new SKRect(ImagePos.X, ImagePos.Y, ImagePos.X + TetrominoImage.Width * 1.5f, ImagePos.Y + TetrominoImage.Height * 1.5f);

                    g.DrawBitmap(TetrominoImage, DrawRect, null);

                    g.DrawTextSK(StatText, new SKPoint(Bounds.Left + TextPos.X + 4, Bounds.Top + TextPos.Y + 4), standardFont, SKColors.White, DesiredFontSize, pOwner.ScaleFactor);
                    g.DrawTextSK(StatText, TextPos, standardFont, SKColors.Black, DesiredFontSize, pOwner.ScaleFactor);
                }
                //g.DrawString(StatText, standardFont, Brushes.White, new PointF(TextPos.X + 4, TextPos.Y + 4));
                //g.DrawString(StatText, standardFont, Brushes.Black, TextPos);
            }
        }

        public void Render(IStateOwner pOwner, object pRenderTarget, object RenderSource, object Element)
        {
            if(pRenderTarget is SKCanvas && RenderSource is GameplayGameState && Element is GameStateSkiaDrawParameters)
            {
                Render(pOwner, pRenderTarget as SKCanvas, RenderSource as GameplayGameState, Element as GameStateSkiaDrawParameters);
            }
        }
    }
    [GameScoringHandler(typeof(StandardTetrisAIScoringHandler), typeof(StoredBoardState.TetrisScoringRuleData))]
    [HandlerTipText("Pentominoes and Hexominoes appear as you progress.")]
    public class ProgressiveTetrisHandler : StandardTetrisHandler
    {
        public ProgressiveTetrisHandler()
        {
            ProgressiveMode = true;
        }
        public override string Name { get { return "Progressive Tetris"; } }
        public override IHighScoreList GetHighScores() 
        {
            return TetrisGame.ScoreMan["Progressive"];
        }
    }
}
