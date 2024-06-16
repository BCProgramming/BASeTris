using OpenTK.Mathematics;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Rendering.Skia
{
    public class SkiaRenderAssistant
    {
        private SKRect _LastDrawBounds;
        public SKRect LastDrawBounds { get { return _LastDrawBounds; } set { _LastDrawBounds = value; } }
        private void PaintPartitionedState(IStateOwner pOwner, Vector2i ClientSize, GameState PaintState, SKCanvas canvas)
        {
            

            RenderHelpers.GetHorizontalSizeData(ClientSize.Y, ClientSize.X, out float FieldWidth, out float StatWidth);
            var renderer = RenderingProvider.Static.GetHandler(typeof(SKCanvas), PaintState.GetType(), typeof(GameStateSkiaDrawParameters));
            if (renderer != null)
            {
                if (renderer is IStateRenderingHandler staterender)
                {
                    SKRect FieldRect = new SKRect(0, 0, FieldWidth, ClientSize.Y);
                    SKRect StatsRect = new SKRect(FieldWidth, 0, FieldWidth + StatWidth, ClientSize.Y);
                    _LastDrawBounds = FieldRect;

                    using (SKAutoCanvasRestore r = new SKAutoCanvasRestore(canvas))
                    {
                        canvas.ClipRect(FieldRect);
                        staterender.Render(pOwner, canvas, PaintState, new GameStateSkiaDrawParameters(FieldRect));
                    }
                    using (SKAutoCanvasRestore r = new SKAutoCanvasRestore(canvas))
                    {
                        canvas.ClipRect(StatsRect);
                        staterender.RenderStats(pOwner, canvas, PaintState, new GameStateSkiaDrawParameters(StatsRect));
                    }

                }
                else
                {
                    ;
                }
            }
            else
            {
                ;
            }
        }
        public void PaintStateSkia(IStateOwner pOwner,GameState CurrentGameState,Vector2i ClientSize, SKCanvas canvas)
        {
            if (CurrentGameState.SupportedDisplayMode == GameState.DisplayMode.Full)
            {
                canvas.Clear(SKColors.Pink);
                var renderer = RenderingProvider.Static.GetHandler(typeof(SKCanvas), CurrentGameState.GetType(), typeof(GameStateSkiaDrawParameters));
                if (renderer != null)
                {
                    if (renderer is IStateRenderingHandler staterender)
                    {
                        canvas.Save();
                        var FullRect = new SKRect(0, 0, ClientSize.X, ClientSize.Y);
                        canvas.ClipRect(FullRect);
                        staterender.Render(pOwner, canvas, CurrentGameState,
                            new GameStateSkiaDrawParameters(FullRect));
                        //canvas.DrawLine(new SKPoint(0, 0), new SKPoint(ClientSize.Width, ClientSize.Height), new SKPaint() { Color = SKColors.Black });
                        canvas.Restore();
                        _LastDrawBounds = FullRect;

                    }
                }
            }
            else if (CurrentGameState.SupportedDisplayMode == GameState.DisplayMode.Partitioned)
            {
                SKRect LastDraw;
                //StandardTetrisGameStateSkiaRenderingHandler.PaintPartitionedState(this, CurrentGameState, canvas, new GameStateSkiaDrawParameters(new SKRect(0, 0, ClientSize.Width, ClientSize.Height)),out LastDraw,out _);
                //_LastDrawBounds = LastDraw;
                PaintPartitionedState(pOwner,ClientSize,CurrentGameState, canvas);
            }
        }


    }
}
