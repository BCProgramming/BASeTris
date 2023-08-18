using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace BASeTris
{
    public static class TrigFunctions
    {
        public static PointF VectorNormal(PointF ofVector)
        {
            double useangle = GetAngle(new PointF(0, 0), ofVector);
            return new PointF((float) Math.Cos(useangle), (float) Math.Sin(useangle));
        }

        public static PointF CenterPoint(RectangleF ofrect)
        {
            return new PointF(ofrect.Left + (ofrect.Width / 2), ofrect.Top + (ofrect.Height / 2));
        }

        public static Rectangle CenterRect(RectangleF largerect, Size middlesize)
        {
            return new Rectangle((int) ((largerect.Width / 2) - ((float) middlesize.Width / 2)), (int) ((largerect.Height / 2) - ((float) middlesize.Height / 2)), middlesize.Width, middlesize.Height);
        }

        public static Rectangle CenterRect(RectangleF largerect, SizeF middlesize)
        {
            return new Rectangle((int) ((largerect.Width / 2) - ((float) middlesize.Width / 2)), (int) ((largerect.Height / 2) - ((float) middlesize.Height / 2)), (int) middlesize.Width, (int) middlesize.Height);
        }

        public static double GetAngle(PointF PointA, PointF PointB)
        {
            return Math.Atan2(PointB.Y - PointA.Y, PointB.X - PointA.X);
            // var result = SignedAngleTo(new Vector3(PointA.X, PointA.Y, 0), new Vector3(PointB.X, PointB.Y, 0), new Vector3(0, 0, 1));
            // return result;
        }

        public static void IncrementLocation(IStateOwner pOwner, ref PointF Location, PointF Velocity)
        {
            PointF discard = PointF.Empty;
            IncrementLocation(pOwner, ref Location, Velocity, ref discard);
        }

        public static void IncrementLocation(IStateOwner pOwner, ref PointF Location, PointF Velocity, ref PointF Offset)
        {
            Location = new PointF(Location.X + Velocity.X, Location.Y + Velocity.Y);
        }

        public static T ClampValue<T>(T Value, T min, T max) where T : IComparable
        {
            //cast to IComparable
            IComparable cvalue = (IComparable) Value;
            IComparable cmin = (IComparable) min;
            IComparable cmax = (IComparable) max;

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

        public static PointF CenterPoint(PointF[] ofset)
        {
            //retrieve the center point of the given set.

            //retrieve the maximum and minimum X and Y coordinates.
            PointF max, min;
            getExtents(ofset, out min, out max);
            return new PointF(min.X + ((max.X - min.X) / 2), min.Y + ((max.Y - min.Y) / 2));
        }

        public static void getExtents(IEnumerable<PointF> ofset, out PointF Minimum, out PointF Maximum)
        {
            float minX = float.MaxValue, minY = float.MaxValue;
            float maxX = float.MinValue, maxY = float.MinValue;
            //iterate through every point.
            foreach (PointF iteratepoint in ofset)
            {
                if (iteratepoint.X < minX) minX = iteratepoint.X;
                if (iteratepoint.X > maxX) maxX = iteratepoint.X;

                if (iteratepoint.Y < minY) minY = iteratepoint.Y;
                if (iteratepoint.Y > maxY) maxY = iteratepoint.Y;
            }

            Minimum = new PointF(minX, minY);
            Maximum = new PointF(maxX, maxY);
        }

        public static PointF GetRandomVelocity(float usespeed)
        {
            return GetRandomVelocity(usespeed, usespeed);
        }

        public static PointF GetRandomVelocity(float minspeed, float maxspeed)
        {
            return GetRandomVelocity(minspeed, maxspeed, (Math.PI * TetrisGame.rgen.NextDouble() * 2));
        }

        public static float Distance(float X, float Y, float X2, float Y2)
        {
            return (float) Math.Sqrt(Math.Pow(X2 - X, 2) + Math.Pow(Y2 - Y, 2));
        }

        public static float Distance(PointF PointA, PointF PointB)
        {
            return (float) Math.Sqrt(Math.Pow(PointB.X - PointA.X, 2) + Math.Pow(PointB.Y - PointA.Y, 2));
        }

        public static PointF MidPoint(PointF PointA, PointF PointB)
        {
            return new PointF(PointA.X + ((PointB.X - PointA.X) / 2), PointA.Y + ((PointB.Y - PointA.Y) / 2));
        }

        public static PointF GetRandomVelocity(float minspeed, float maxspeed, double angle)
        {
            double usespeed;
            if (minspeed == maxspeed)
                usespeed = maxspeed;
            else
            {
                //choose a random speed.
                usespeed = (TetrisGame.rgen.NextDouble() * (maxspeed - minspeed)) + minspeed;
            }

            double useangle = angle;


            return new PointF((float) (Math.Cos(useangle) * usespeed), (float) (Math.Sin(useangle) * usespeed));
        }

        public static PointF GetVelocity(double speed, double angle)
        {
            return new PointF((float) (Math.Cos(angle) * speed), (float) (Math.Sin(angle) * speed));
        }

        public static Image ScaleImage(Image source, float Factor)
        {
            Size newsize = new Size((int) ((float) source.Size.Width * Factor), (int) ((float) source.Size.Height * Factor));
            Bitmap scaledimage = new Bitmap(newsize.Width, newsize.Height);
            using (Graphics guse = Graphics.FromImage(scaledimage))
            {
                guse.DrawImage(source, 0, 0, newsize.Width, newsize.Height);
            }

            return scaledimage;
        }
    }

}