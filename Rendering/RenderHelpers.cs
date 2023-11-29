using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris.Rendering
{
    public static class RenderHelpers
    {
        
        public static Color MixColor(Color ColorA, Color ColorB, float percentage)
        {
            float[] ColorAValues = new float[] { (float)ColorA.A, (float)ColorA.R, (float)ColorA.G, (float)ColorA.B };
            float[] ColorBValues = new float[] { (float)ColorB.A, (float)ColorB.R, (float)ColorB.G, (float)ColorB.B };
            float[] ColorCValues = new float[4];


            for (int i = 0; i <= 3; i++)
            {
                ColorCValues[i] = (ColorAValues[i] * percentage) + (ColorBValues[i] * (1 - percentage));
            }


            return Color.FromArgb((int)ColorCValues[0], (int)ColorCValues[1], (int)ColorCValues[2], (int)ColorCValues[3]);
        }
        public static Color MixColor(Color ColorA, Color ColorB)
        {
            return MixColor(ColorA, ColorB, 0.5f);
        }

        public static T ClampValue<T>(T Value, T min, T max) where T : IComparable
        {
            //cast to IComparable
            IComparable cvalue = (IComparable)Value;
            IComparable cmin = (IComparable)min;
            IComparable cmax = (IComparable)max;

            //return (T)(cvalue.CompareTo(cmin)< 0 ?cmin:cvalue.CompareTo(cmax)>0?max:Value);
            if (cvalue.CompareTo(cmin) < 0)
            {
                return min;
            }
            else if (cvalue.CompareTo(cmax) > 0)
            {
                return max;
            }
            return Value;

        }
        public static SKColor MixColor(SKColor FirstColor, SKColor SecondColor, float Percentbetween)
        {
            //first we get the RGBA differences.
            float[] Firstdata = new float[] { FirstColor.Red, FirstColor.Green, FirstColor.Blue, FirstColor.Alpha };
            float[] Diffs = new float[] { SecondColor.Red-FirstColor.Red,
                                        SecondColor.Green-FirstColor.Green,
                                        SecondColor.Blue-FirstColor.Blue,
                                        SecondColor.Alpha-FirstColor.Alpha};
            //multiply each by the percentage.
            Diffs = (from c in Diffs select c * Percentbetween).ToArray();

            //now select the firstcolor component plus this new difference, clamped.
            int[] result = new int[Firstdata.Length];
            for (int i = 0; i < result.Length; i++)
            {

                result[i] = (int)ClampValue(Firstdata[i] + Diffs[i], 0, 255);

            }

            return new SKColor((byte)result[0], (byte)result[1], (byte)result[2], (byte)result[3]);
        }
        public static SKColor InvertColor(SKColor color)
        {
            return new SKColor((byte)(255 - color.Red), (byte)(255 - color.Green), (byte)(255 - color.Blue));
        }
        public static SKColor RotateHue(SKColor color, float Amount)
        {
            float currenthue, currentsat, currentlum;
            color.ToHsl(out currenthue, out currentsat, out currentlum);
            currenthue = (currenthue + Amount) % 1.0f;
            return SKColor.FromHsl(currenthue, currentsat, currentlum);

        }
        public static SKColor MatchHue(SKColor Target, SKColor Source)
        {
            float TargetHue, TargetSat, TargetLum;
            float SourceHue, SourceSat, SourceLum;
            Target.ToHsl(out TargetHue, out TargetSat, out TargetLum);
            Source.ToHsl(out SourceHue, out SourceSat, out SourceLum);
            return SKColor.FromHsl(TargetHue, SourceSat, SourceLum);


        }
        public static SKColor RandomColor()
        {
            return new SKColor((byte)TetrisGame.StatelessRandomizer.Next(256), (byte)TetrisGame.StatelessRandomizer.Next(256), (byte)TetrisGame.StatelessRandomizer.Next(256));
        }

    }
}
