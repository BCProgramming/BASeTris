using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using BASeTris.AssetManager;
using BASeTris.BackgroundDrawers;
using BASeTris.GameStates;

namespace BASeTris.Rendering.GDIPlus
{
    [RenderingHandler(typeof(ViewScoreDetailsState), typeof(Graphics), typeof(GameStateDrawParameters))]
    public class ViewScoreDetailsStateHandler : StandardStateRenderingHandler<Graphics,ViewScoreDetailsState,GameStateDrawParameters>
    {
        private Pen Separator = new Pen(Color.Black, 3);
        public override void Render(IStateOwner pOwner, Graphics pRenderTarget, ViewScoreDetailsState Source, GameStateDrawParameters Element)
        {
            var g = pRenderTarget;
            var Bounds = Element.Bounds;
            RenderingProvider.Static.DrawElement(pOwner, pRenderTarget, Source.BG, new GDIBackgroundDrawData(Bounds));
            

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
                    DrawTetronimoDetails(Source,g, Bounds);
                    break;
                case ViewScoreDetailsState.ViewScoreDetailsType.Details_LevelTimes:
                    DrawLevelTimesDetails(Source,g, Bounds);
                    break;
            }
        }
        private void DrawTetronimoDetails(ViewScoreDetailsState Source,Graphics g, RectangleF Bounds)
        {
            //draws the tetronimo pictures, the tetronimo stats, and the numberof lines down the screen.
        }

        private void DrawLevelTimesDetails(ViewScoreDetailsState Source, Graphics g, RectangleF Bounds)
        {
            //draw the times each level was achieved.
            //(Possible feature: support paging if  we can't fit them on one screen?)
        }
        public override void RenderStats(IStateOwner pOwner, Graphics pRenderTarget, ViewScoreDetailsState Source, GameStateDrawParameters Element)
        {
            throw new NotImplementedException();
        }
    }
}