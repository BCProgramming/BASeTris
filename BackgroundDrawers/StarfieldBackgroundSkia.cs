using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.BackgroundDrawers
{
    public class StarfieldBackgroundSkiaCapsule:BackgroundDrawData
    {
        public StarfieldStarData[] Stars = null;
        public double WarpFactor = 1;
        public SKRect? Bounds { get; set; }
        public SKPoint DirectionAdd { get; set; }
        public int StarCount = 265;
        public static StarfieldStarData[] GenerateStars(float CenterX, float CenterY, SKRect Bounds,int Count)
        {
            float[] AvailableFactors = new float[] { 1f, 0.5f, 0.25f, 1.1f, 1.25f, 0.1f };
            float[] AvailableScales = new float[] { 1f, 1.25f, 1.5f, 2f, 3f, 5f };
            float[] chooseweights = new float[] { 200, 40, 10, 1, 1, 1 };
            var Stars = new StarfieldStarData[Count];
            for (int i = 0; i < Stars.Length; i++)
            {
                float sx = (float)(CenterX + (TetrisGame.rgen.NextDouble() - 0.5) * Bounds.Width);
                float sy = (float)(CenterY + (TetrisGame.rgen.NextDouble() - 0.5) * Bounds.Height);

                Stars[i] = new StarfieldStarData(sx, sy);
                Stars[i].SpeedFactor = TetrisGame.Choose(AvailableFactors) / 3;
                Stars[i].SizeMultiplier = RandomHelpers.Static.Select(AvailableScales,chooseweights);
            }
            return Stars;
        }
     

    }
    public class StarfieldStarData
    {
        public float SizeMultiplier = 1.0f;
        static SKColor[] PossibleColors = new SKColor[] { SKColors.White, SKColors.Yellow, SKColors.LightBlue, SKColors.Red, SKColors.Brown, SKColors.Beige };
        public SKPaint StarPaint = new SKPaint() { Color = TetrisGame.Choose(PossibleColors) };
        public float X { get; set; }
        public float Y { get; set; }

        public float SpeedFactor { get; set; } = 1f;
        public StarfieldStarData(float pX, float pY)
        {
            X = pX;
            Y = pY;
        }
    }
    public class StarfieldBackgroundSkia : Background<StarfieldBackgroundSkiaCapsule>
    {
        //private StarfieldBackgroundSkiaCapsule Data = null;


        public StarfieldBackgroundSkia(StarfieldBackgroundSkiaCapsule capsule)
        {
            Data = capsule;
        }


        public override void FrameProc(IStateOwner pState)
        {
            var boundcheck = Data.Bounds;
            if (boundcheck != null)
            {
                var usebounds = boundcheck.Value;
                float MiddleX = (float)(usebounds.Width / 2 + usebounds.Left);
                float MiddleY = (float)(usebounds.Height / 2 + usebounds.Top);
                if (Data.Stars == null)
                {
                    Data.Stars = StarfieldBackgroundSkiaCapsule.GenerateStars(MiddleX, MiddleY, usebounds, Data.StarCount);
                }



                foreach (var stardraw in Data.Stars)
                {


                    stardraw.X = (float)(stardraw.X + ((stardraw.X - MiddleX) * 0.025) * (stardraw.SpeedFactor * Data.WarpFactor) + Data.DirectionAdd.X);
                    stardraw.Y = (float)(stardraw.Y + ((stardraw.Y - MiddleY) * 0.025) * (stardraw.SpeedFactor * Data.WarpFactor) + Data.DirectionAdd.Y);


                    if (stardraw.X < usebounds.Left - 50 || stardraw.X > usebounds.Right + 50 ||
                        stardraw.Y < usebounds.Top - 50 || stardraw.Y > usebounds.Bottom + 50)
                    {
                        float sx = (float)(MiddleX + (TetrisGame.rgen.NextDouble() - 0.5) * (usebounds.Width / 3));
                        float sy = (float)(MiddleY + (TetrisGame.rgen.NextDouble() - 0.5) * (usebounds.Width / 3));
                        stardraw.X = sx;
                        stardraw.Y = sy;
                    }
                }
            }

        }
        
    }
}
