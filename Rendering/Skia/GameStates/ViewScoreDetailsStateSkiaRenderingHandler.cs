using BASeCamp.BASeScores;
using BASeCamp.Rendering;
using BASeTris.BackgroundDrawers;
using BASeTris.GameStates;
using BASeTris.GameStates.GameHandlers;
using BASeTris.Tetrominoes;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Rendering.Skia.GameStates
{
    [RenderingHandler(typeof(ViewScoreDetailsState), typeof(SKCanvas), typeof(GameStateSkiaDrawParameters))]
    public class ViewScoreDetailsStateSkiaRenderingHandler : StandardStateRenderingHandler<SKCanvas, ViewScoreDetailsState, GameStateSkiaDrawParameters>
    {
        private SKPaint Separator = new SKPaint() { Color = SKColors.Black };
        private GameplayGameState Tet_State = null;
        TetrisField tf = null;
        public override void Render(IStateOwner pOwner, SKCanvas pRenderTarget, ViewScoreDetailsState Source, GameStateSkiaDrawParameters Element)
        {
            //TODO: finish this!
            var g = pRenderTarget;
            var Bounds = Element.Bounds;

            //Draw the background first.
            RenderingProvider.Static.DrawElement(pOwner, pRenderTarget, Source.BG, new SkiaBackgroundDrawData(Bounds));

            DrawTextInformationSkia skdraw = new DrawTextInformationSkia()
            {
                ForegroundPaint = new SKPaint()
            };

            float StartY = (Bounds.Height * 0.175f);
            var CurrentY = StartY;
            float MiddleX = Bounds.Width / 2;
            
            float TextSize = Bounds.Height / 30f;
            var ScoreFont = TetrisGame.RetroFontSK; //point size 24.
            SKPaint MainScoreFont = new SKPaint() { Typeface = ScoreFont, TextSize = (float)(24 * pOwner.ScaleFactor), Color = SKColors.Black };
            SKPaint ShadowScoreFont = new SKPaint() { Typeface = ScoreFont, TextSize = (float)(24 * pOwner.ScaleFactor), Color = SKColors.White };
            SKPaint ListingFont = new SKPaint() { Typeface = ScoreFont, TextSize = (float)(18 * pOwner.ScaleFactor), Color = SKColors.Black };
            float PercentThroughSecond = (float)DateTime.Now.Millisecond / 1000f;
            SKPaint ListingFontRainbow = new SKPaint() { Typeface = ScoreFont, TextSize = (float)(18 * pOwner.ScaleFactor), Color = SKColor.FromHsl(PercentThroughSecond * 240, 240, 120) };
            SKPaint ListingFontShadow = new SKPaint() { Typeface = ScoreFont, TextSize = (float)(18 * pOwner.ScaleFactor), Color = SKColors.White };
            SKPaint ListingFontArrow = new SKPaint() { Typeface = TetrisGame.ArialFontSK, TextSize = (float)(18 * pOwner.ScaleFactor), Color = SKColor.FromHsl(PercentThroughSecond * 240, 240, 120) };
            SKPaint ListingFontArrowShadow = new SKPaint() { Typeface = TetrisGame.ArialFontSK, TextSize = (float)(18 * pOwner.ScaleFactor), Color = SKColors.White };
            SKRect resultitem = new SKRect();
            float LineHeight = MainScoreFont.MeasureText("#", ref resultitem);
            var useShader = SKShader.CreateLinearGradient(new SKPoint(0, 0), new SKPoint(0, Bounds.Height), new SKColor[] { SKColors.Red, SKColors.Orange, SKColors.Yellow, SKColors.Green, SKColors.Blue, SKColors.Indigo, SKColors.Violet }, null, SKShaderTileMode.Mirror);

            SKPaint LinePaint = new SKPaint() { BlendMode = SKBlendMode.ColorBurn, StrokeWidth = 24, Shader = useShader };
            g.DrawRect(new SKRect(0, 0, Bounds.Width, Bounds.Height), LinePaint);
            //g.DrawRect(new SKRect((int)(Bounds.Width * (1 / 7)), CurrentY, (float)(Bounds.Width - (Bounds.Width * (1 / 7))), (float)(CurrentY + (LineHeight * 2.5) + (LineHeight * 3) * 12)),LinePaint);



            if (Source.IncrementedDrawState >= 0)
            {
                //draw "SCORE DETAILS"
                String sHeaderText = "SCORE DETAILS";
                SKRect MeasuredRect = new SKRect();
                MainScoreFont.MeasureText(sHeaderText, ref MeasuredRect);
                SKPoint DrawPosition = new SKPoint(MiddleX - (MeasuredRect.Width / 2), StartY);
                g.DrawText(sHeaderText, new SKPoint(DrawPosition.X + 2, DrawPosition.Y + 2), ShadowScoreFont);
                g.DrawText(sHeaderText, DrawPosition, MainScoreFont);
                CurrentY = StartY + MeasuredRect.Height + 10;
            }

            if (Source.IncrementedDrawState >= 1)
            {
                //maybe a line under the header.
            }

            if (Source.IncrementedDrawState >= 2)
            {
                switch (Source.CurrentView)
                {
                    case ViewScoreDetailsState.ViewScoreDetailsType.Details_Tetrominoes:
                        DrawTetronimoDetails(pOwner, Source, g, Bounds, CurrentY) ;
                        break;
                    case ViewScoreDetailsState.ViewScoreDetailsType.Details_LevelTimes:
                        DrawLevelTimesDetails(pOwner,Source, g, Bounds,CurrentY);
                        break;
                }





            }

            /*

            Font HeaderFont = TetrisGame.GetRetroFont(24, pOwner.ScaleFactor);
            Font PlacementFont = TetrisGame.GetRetroFont(10, pOwner.ScaleFactor);
            Font DetailFont = TetrisGame.GetRetroFont(8, pOwner.ScaleFactor);


            //One thing we draw in every case is the "--SCORE DETAILS--" header text. this is positioned at 5% from the top, centered in the middle of our bounds.
            float Millipercent = (float)DateTime.Now.Ticks / 5000f; //(float)DateTime.Now.Millisecond / 1000;

            var MeasuredHeader = g.MeasureString(Source._DetailHeader, HeaderFont);
            int RotateAmount = (int)(Millipercent * 240);
            Color UseColor1 = HSLColor.RotateHue(Color.Red, RotateAmount);
            Color UseColor2 = HSLColor.RotateHue(Color.LightPink, RotateAmount);
            PointF ScorePosition = new PointF((Bounds.Width / 2) - (MeasuredHeader.Width / 2), Bounds.Height * 0.05f);
            using (LinearGradientBrush lgb = new LinearGradientBrush(new Rectangle(0, 0, (int)MeasuredHeader.Width, (int)MeasuredHeader.Height), UseColor1, UseColor2, LinearGradientMode.Vertical))
            {
                using (GraphicsPath gp = new GraphicsPath())
                {
                    gp.AddString(Source._DetailHeader, HeaderFont, new Point((int)ScorePosition.X, (int)ScorePosition.Y), StringFormat.GenericDefault);
                    g.FillPath(lgb, gp);
                    g.DrawPath(Pens.White, gp);
                }
            }

            //we also show Xth Place - <NAME> centered below the header using the placementfont.
            String sPlacement = TetrisGame.FancyNumber(Source._Position) + " - " + Source.ShowEntry.Name + " - " + Source.ShowEntry.Score.ToString();

            var measureit = g.MeasureString(sPlacement, PlacementFont);

            PointF DrawPlacement = new PointF(Bounds.Width / 2 - measureit.Width / 2, (float)(ScorePosition.Y + MeasuredHeader.Height * 1.1f));

            g.DrawString(sPlacement, PlacementFont, Brushes.Black, DrawPlacement.X + 3, DrawPlacement.Y + 3);
            g.DrawString(sPlacement, PlacementFont, Brushes.White, DrawPlacement.X, DrawPlacement.Y);

            g.DrawLine(Separator, (float)(Bounds.Width * 0.05f), (float)(DrawPlacement.Y + measureit.Height + 5), (float)(Bounds.Width * 0.95), (float)(DrawPlacement.Y + measureit.Height + 5));


            switch (Source.CurrentView)
            {
                case ViewScoreDetailsState.ViewScoreDetailsType.Details_Tetrominoes:
                    DrawTetronimoDetails(Source, g, Bounds);
                    break;
                case ViewScoreDetailsState.ViewScoreDetailsType.Details_LevelTimes:
                    DrawLevelTimesDetails(Source, g, Bounds);
                    break;
            }
            */
        }
        private Type[] TetrominoTypes = new Type[] { typeof(Tetromino_F), typeof(Tetromino_G), typeof(Tetromino_I), typeof(Tetromino_J), typeof(Tetromino_L), typeof(Tetromino_O), typeof(Tetromino_S), typeof(Tetromino_T), typeof(Tetromino_Y), typeof(Tetromino_Z) };
        private Dictionary<string, Type> TetrominoTypeMap = null;
        private Dictionary<string, SKBitmap> TetrominoBitmaps = null;
        private StandardTetrisHandler stubHandler = null;
        private void DrawTetronimoDetails(IStateOwner pOwner,ViewScoreDetailsState Source, SKCanvas g, SKRect Bounds,float YStart)
        {
            
            //draws the tetronimo pictures, the tetronimo stats, and the numberof lines down the screen.
            var ScoreFont = TetrisGame.RetroFontSK; //point size 24.
            SKPaint MainScoreFont = new SKPaint() { Typeface = ScoreFont, TextSize = (float)(24 * pOwner.ScaleFactor), Color = SKColors.Black };
            SKPaint ShadowScoreFont = new SKPaint() { Typeface = ScoreFont, TextSize = (float)(24 * pOwner.ScaleFactor), Color = SKColors.White };
            SKPaint ListingFont = new SKPaint() { Typeface = ScoreFont, TextSize = (float)(18 * pOwner.ScaleFactor), Color = SKColors.Black };
            if (Source.ShowEntry is IHighScoreEntry<TetrisHighScoreData> hsd)
            {
              



            }
            else if (Source.ShowEntry is IHighScoreEntry<DrMarioHighScoreData> drhsd)
            {
                //no details For Dr.Mario yet.
            }
            
        }

        private void DrawLevelTimesDetails(IStateOwner pOwner,ViewScoreDetailsState Source, SKCanvas g, SKRect Bounds,float YStart)
        {
            //draw the times each level was achieved.
            //(Possible feature: support paging if  we can't fit them on one screen?)
            var ScoreFont = TetrisGame.RetroFontSK; //point size 24.
            SKPaint MainScoreFont = new SKPaint() { Typeface = ScoreFont, TextSize = (float)(24 * pOwner.ScaleFactor), Color = SKColors.Black };
            SKPaint ShadowScoreFont = new SKPaint() { Typeface = ScoreFont, TextSize = (float)(24 * pOwner.ScaleFactor), Color = SKColors.White };
            SKPaint ListingFont = new SKPaint() { Typeface = ScoreFont, TextSize = (float)(18 * pOwner.ScaleFactor), Color = SKColors.Black };

            if (Source.ShowEntry is IHighScoreEntry<TetrisHighScoreData> hsd)
            {
                TimeSpan[] ShowTimes = hsd.CustomData.LevelReachedTimes;
                SKRect MeasuredRect = new SKRect();
                MainScoreFont.MeasureText("##########", ref MeasuredRect);
                var ElementHeight = (MeasuredRect.Height + 10);
                for (int l=0;l<ShowTimes.Length;l++)
                {
                    String BuildLine = $"Level {l}:               " + ShowTimes[l].ToString("c");
                    var YOffset =YStart + ElementHeight * (l + 5);
                    SKPoint DrawPosition = new SKPoint(50, YOffset);
                    g.DrawText(BuildLine, new SKPoint(DrawPosition.X + 2, DrawPosition.Y + 2), ShadowScoreFont);
                    g.DrawText(BuildLine, DrawPosition, MainScoreFont);
                }


            }
            else if(Source.ShowEntry is IHighScoreEntry<DrMarioHighScoreData> drhsd)
            {
                //no details For Dr.Mario yet.
            }

            

        }
        public override void RenderStats(IStateOwner pOwner, SKCanvas pRenderTarget, ViewScoreDetailsState Source, GameStateSkiaDrawParameters Element)
        {
            throw new NotImplementedException();
        }
    }
}
