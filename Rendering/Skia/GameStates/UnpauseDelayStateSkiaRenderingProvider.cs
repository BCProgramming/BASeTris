using BASeCamp.Rendering;
using BASeTris.GameStates;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Rendering.Skia.GameStates
{

    [RenderingHandler(typeof(TemporaryInputPauseGameState), typeof(SKCanvas), typeof(GameStateSkiaDrawParameters))]
    public class TemporaryPauseGameStateSkiaRenderingHandler : StandardStateRenderingHandler<SKCanvas, TemporaryInputPauseGameState, GameStateSkiaDrawParameters>
    {
        public override void Render(IStateOwner pOwner, SKCanvas pRenderTarget, TemporaryInputPauseGameState Source, GameStateSkiaDrawParameters Element)
        {
            SKCanvas g = pRenderTarget;
            var Bounds = Element.Bounds;
            RenderingProvider.Static.DrawElement(pOwner, pRenderTarget, Source.GetComposite(), Element);
        }

        public override void RenderStats(IStateOwner pOwner, SKCanvas pRenderTarget, TemporaryInputPauseGameState Source, GameStateSkiaDrawParameters Element)
        {
            RenderingProvider.Static.DrawStateStats(pOwner, pRenderTarget, Source.GetComposite(), Element);
        }
    }


    [RenderingHandler(typeof(UnpauseDelayGameState), typeof(SKCanvas), typeof(GameStateSkiaDrawParameters))]

    public class UnpauseDelayStateSkiaRenderingHandler : StandardStateRenderingHandler<SKCanvas, UnpauseDelayGameState, GameStateSkiaDrawParameters>
    {
        private SKPaint SecondsPaint = null;
        private SKPaint MSPaint = null;
        private SKPaint MSPaintBG = null;
        public override void Render(IStateOwner pOwner, SKCanvas pRenderTarget, UnpauseDelayGameState Source, GameStateSkiaDrawParameters Element)
        {

            SKCanvas g = pRenderTarget;
            var Bounds = Element.Bounds;
            var usePaint = new SKPaint() { Color = SKColors.Yellow, StrokeWidth = 6};
            
            


            RenderingProvider.Static.DrawElement(pOwner, pRenderTarget, Source._ReturnState, Element);
            
            //Draw Faded out overlay to darken things up.
            DrawFadeOverlay(g, Bounds);
            //draw a centered Countdown

            if (Source.LastSecond != Source.timeremaining.Seconds)
            {
                //emit a sound.
                TetrisGame.Soundman.PlaySound(pOwner.AudioThemeMan.GameOverShade.Key, pOwner.Settings.std.EffectVolume);
                Source.LastSecond = Source.timeremaining.Seconds;
                Source.lastMillis = 1000;
            }

            double SecondsLeft = Math.Round(Source.timeremaining.TotalSeconds, 1);
            String sSecondsLeft = Source.timeremaining.ToString("%s");
            double Millis = (double)Source.timeremaining.Milliseconds / 1000d; //millis in percent. We will use this to animate the unpause time left.
            Millis = Math.Min(Millis, Source.lastMillis);
            float useSize = (float)(64f * (1 - (Millis)))*(float)pOwner.ScaleFactor;
            float FullSize = 1.33f*(float)(64f) * (float)pOwner.ScaleFactor;
            
            var CurrentColor = SKColors.White;
            if(SecondsPaint==null)
            {
                SecondsPaint = new SKPaint() { Typeface = TetrisGame.RetroFontSK, TextSize = useSize, Color = CurrentColor };
                MSPaint = new SKPaint() { Typeface = TetrisGame.RetroFontSK, TextSize = (float)(13*pOwner.ScaleFactor), Color = SKColors.Yellow };
                MSPaintBG = new SKPaint() { Typeface = TetrisGame.RetroFontSK, TextSize = MSPaint.TextSize, Color = SKColors.Navy };
            }
            SecondsPaint.TextSize = useSize;
            SKRect MeasureText = new SKRect();
            SecondsPaint.MeasureText(sSecondsLeft, ref MeasureText);
            SKRect SecondBound = new SKRect(Bounds.Width / 2 - FullSize / 2, Bounds.Height / 2 - FullSize / 2, Bounds.Width / 2 - FullSize / 2 + FullSize, Bounds.Height / 2 - FullSize / 2+FullSize);
            SKPoint DrawPosition = new SKPoint(Bounds.Width / 2 - MeasureText.Width / 2, Bounds.Height / 2 + MeasureText.Height / 2);

            //g.DrawOval(SecondBound, new SKPaint() { Color = SKColors.Red, StrokeWidth = 1,Style=SKPaintStyle.Stroke });
            //g.DrawArc(SecondBound, 0, (float)(360 * (1 - Millis)), true, new SKPaint() { StrokeWidth = 1f, StrokeCap=SKStrokeCap.Round,  Color = SKColors.Yellow, Style = SKPaintStyle.Stroke });
            float StartAngle = 0;
            float SweepAngle = (float)(360 * (1 - Millis));
            //path.AddOval(SecondBound, SKPathDirection.CounterClockwise);
            //path.AddArc(SecondBound, 0, SweepAngle);
            double MinAngle = Math.PI / 180;
            double XOffset = SecondBound.Left + SecondBound.Width / 2;
            double YOffset = SecondBound.Top + SecondBound.Height / 2;
            double CircleRadius = (SecondBound.Width / 2 * 5) * (1 - Millis);

            using (SKPath path = new SKPath() {    FillType = SKPathFillType.Winding, Convexity = SKPathConvexity.Concave })
            {
                

                for (double drawAngle = StartAngle; drawAngle < SweepAngle; drawAngle += MinAngle)
                {
                    double useAngle = drawAngle * (Math.PI / 180);
                    double XPos1 = Math.Sin(useAngle - MinAngle) * CircleRadius / 2 + XOffset;
                    double YPos1 = Math.Cos(useAngle - MinAngle) * CircleRadius / 2 + YOffset;
                    double XPos2 = Math.Sin(useAngle) * CircleRadius / 2 + XOffset;
                    double YPos2 = Math.Cos(useAngle) * CircleRadius / 2 + YOffset;
                    g.DrawLine((float)XPos1, (float)YPos1, (float)XPos2, (float)YPos2, usePaint);
                    
                }
                //g.DrawArc(SecondBound, StartAngle, SweepAngle, false, usePaint);
                path.AddArc(SecondBound, 0, SweepAngle);
                path.Close();
              
            }

            //lets draw a little millisecond display, too!
            double Angle = (Math.PI*2 * (1 - Millis));
            double XPosition = Math.Sin(Angle) * CircleRadius / 2 + XOffset;
            double YPosition = Math.Cos(Angle) * CircleRadius / 2 + YOffset;
            String useMS = String.Format("{0:0}", Millis*1000);
            var PosPaint = new SKPoint((float)XPosition, (float)YPosition);

            if (false && LastPaintMSPositions.Any())
            {
                foreach (var q in LastPaintMSPositions)
                {
                    var UsePos = q.Item2;
                    SKPaint skForeground = MSPaint.Clone();
                    SKPaint skBackground = MSPaintBG.Clone();


                    double CalcSize = skForeground.TextSize * 1-((Millis*1000 - q.Item1) / 200);
                    double CalcAlpha = 1-Math.Min(Math.Abs((Millis*1000 - q.Item1) / 200), 1);
                    skForeground.TextSize = skBackground.TextSize = (float)CalcSize;
                    skForeground.Color = skForeground.Color.WithAlpha((byte)(CalcAlpha * 255));
                    skBackground.Color = skBackground.Color.WithAlpha((byte)(CalcAlpha * 255));

                    g.DrawText(useMS, new SKPoint((float)UsePos.X + (float)(2 * pOwner.ScaleFactor), (float)UsePos.Y + (float)(2 * pOwner.ScaleFactor)), skBackground);
                    g.DrawText(useMS, UsePos, skForeground);

                }

            }

            if (false)
            {
                g.DrawText(useMS, new SKPoint((float)XPosition + (float)(2 * pOwner.ScaleFactor), (float)YPosition + (float)(2 * pOwner.ScaleFactor)), MSPaintBG);
                g.DrawText(useMS, PosPaint, MSPaint);
            }
          

            LastPaintMSPositions.Enqueue((Millis*1000,  PosPaint));
            while ((Millis*1000)-LastPaintMSPositions.Peek().Item1 > 200)
                LastPaintMSPositions.Dequeue();
            //this arc drawing doesn't work, for some reason.
            //g.DrawArc(SecondBound, 0, (float)(360 * (1 - Millis)), false, new SKPaint() { StrokeWidth = 0.05f, Color = SKColors.Yellow, Style = SKPaintStyle.Stroke, IsStroke = true,StrokeCap = SKStrokeCap.Square,StrokeMiter = 0 });
            g.DrawText(sSecondsLeft, DrawPosition, SecondsPaint);
            Source.lastMillis = Millis;
        }
        private Queue<(double,SKPoint)> LastPaintMSPositions = new Queue<(double,SKPoint)>();
        public override void RenderStats(IStateOwner pOwner, SKCanvas pRenderTarget, UnpauseDelayGameState Source, GameStateSkiaDrawParameters Element)
        {
            RenderingProvider.Static.DrawStateStats(pOwner, pRenderTarget, Source._ReturnState, Element);
            //Draw Faded out overlay to darken things up.
            DrawFadeOverlay(pRenderTarget, Element.Bounds);


        }
        SKPaint fadeBrush = new SKPaint() { Color = new SKColor(0, 0, 0, 200) };
        private void DrawFadeOverlay(SKCanvas g, SKRect Bounds)
        {
            g.DrawRect(Bounds, fadeBrush);
        }
    }
}
