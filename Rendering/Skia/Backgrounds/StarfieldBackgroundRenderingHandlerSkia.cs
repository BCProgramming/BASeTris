using BASeCamp.Rendering;
using BASeTris.BackgroundDrawers;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Rendering.Skia.Backgrounds
{
    [RenderingHandler(typeof(StarfieldBackgroundSkia), typeof(SKCanvas), typeof(BackgroundDrawData))]
    public class StarfieldBackgroundSkiaRenderingHandler : BackgroundDrawRenderHandler<SKCanvas, StarfieldBackgroundSkia, BackgroundDrawData>
    {
        public override void Render(IStateOwner pOwner, SKCanvas pRenderTarget, StarfieldBackgroundSkia Source, BackgroundDrawData Element)
        {
            if (Source.Data is null) return;
            var useElement = Element as SkiaBackgroundDrawData;
            Source.Data.Bounds = useElement.Bounds;

            if (Source.Data.Stars == null) return;
            var Stars = Source.Data.Stars;
            var g = pRenderTarget;


            double MiddleX = useElement.Bounds.Width / 2 + useElement.Bounds.Left;
            double MiddleY = useElement.Bounds.Height / 2 + useElement.Bounds.Top;
            Source.Data.Bounds = useElement.Bounds;

            foreach (var stardraw in Stars)
            {

                var x = stardraw.X;
                var y = stardraw.Y;
                var r = stardraw.SizeMultiplier * ((pOwner.ScaleFactor) * (0.001 * (Math.Sqrt(Math.Pow(x - MiddleX, 2) + Math.Pow(y - MiddleY, 2)))));

                g.DrawCircle(new SKPoint(x, y), (float)r, stardraw.StarPaint);

                //update star position now.
                /*  stardraw.X = (float)(stardraw.X + ((stardraw.X - MiddleX) * 0.025) * (stardraw.SpeedFactor * Source.Data.WarpFactor) + Source.Data.DirectionAdd.X);
                   stardraw.Y = (float)(stardraw.Y + ((stardraw.Y - MiddleY) * 0.025) * (stardraw.SpeedFactor * Source.Data.WarpFactor) + Source.Data.DirectionAdd.Y);


                   if (stardraw.X < useElement.Bounds.Left - 50 || stardraw.X > useElement.Bounds.Right + 50 ||
                       stardraw.Y < useElement.Bounds.Top - 50 || stardraw.Y > useElement.Bounds.Bottom + 50)
                   {
                       float sx = (float)(MiddleX + (TetrisGame.rgen.NextDouble() - 0.5) * useElement.Bounds.Width);
                       float sy = (float)(MiddleY + (TetrisGame.rgen.NextDouble() - 0.5) * useElement.Bounds.Height);
                       stardraw.X = sx;
                       stardraw.Y = sy;
                   }
                */

            }
        }
    }
}
