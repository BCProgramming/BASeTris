using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris
{
    [Serializable]
    public class Polygon : ICloneable, ISerializable
    {

        private List<Vector> points = new List<Vector>();
        private List<Vector> edges = new List<Vector>();

        /*
         * The following code is by Randolph Franklin, it returns 1 for interior points and 0 for exterior points.

    int pnpoly(int npol, float *xp, float *yp, float x, float y)
    {
      int i, j, c = 0;
      for (i = 0, j = npol-1; i < npol; j = i++) {
        if ((((yp[i] <= y) && (y < yp[j])) ||
             ((yp[j] <= y) && (y < yp[i]))) &&
            (x < (xp[j] - xp[i]) * (y - yp[i]) / (yp[j] - yp[i]) + xp[i]))
          c = !c;
      }
      return c;
    }

         * 
         * */





        /*You need to determine which points lie inside. 
         * After removing these points, you can insert one set of "outside" points into the other. 
         * Your insertion points (e.g. where you have the arrow in the picture on the right) 
         * are where you had to remove points from the input sets.
         * */

        public bool Contains(Vector Location)
        {
            return PointInPoly(Location);


        }
        public bool Contains(Polygon Otherpoly)
        {


            return Otherpoly.Points.All(this.Contains);

        }
        //returns an array representing the distances of each point from the center point of this polygon.
        public float[] Radii()
        {
            PointF CenterP = Center;
            return (from m in points select TrigFunctions.Distance(CenterP, m)).ToArray();


        }
        public float AverageRadius()
        {
            return Radii().Average();
        }
        public bool PointInPoly(PointF checkpoint)
        {

            int i, j;
            bool c = false;
            //iterate through all the points...
            int npol = points.Count();
            float X = checkpoint.X;
            float Y = checkpoint.Y;
            for (i = 0, j = npol - 1; i < npol; j = i++)
            {
                if ((((points[i].Y <= Y) && (Y < points[j].Y)) ||
                    ((points[j].Y <= Y) && (Y < points[i].Y))) &&
                (X < (points[j].X - points[i].X) * (Y - points[i].Y) / (points[j].Y - points[i].Y) + points[i].X))
                    c = !c;



            }
            return c;


        }
        /*
        public Polygon Union(Polygon otherunion)
        {
            //first find the intersection of all edges.

            if (!IntersectsWith(otherunion)) return null;
           
            


        }*/
        public Polygon(IEnumerable<PointF> points) : this(from p in points select (Vector)p)
        {

        }
        public Polygon(params PointF[] polypoints)
        {


            foreach (PointF looppoint in polypoints)
            {
                points.Add(new Vector(looppoint.X, looppoint.Y));


            }

            BuildEdges();

        }
        public Polygon(SerializationInfo info, StreamingContext context)
        {

            points = (List<Vector>)info.GetValue("points", typeof(List<Vector>));

        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {

            info.AddValue("points", points);

        }
        public RectangleF GetBounds()
        {
            PointF TopLeft = new PointF(Points.Min((t) => t.X), Points.Min((t) => t.Y));
            PointF BottomRight = new PointF(Points.Max((t) => t.X), Points.Max((t) => t.Y));
            return new RectangleF(TopLeft, new SizeF(BottomRight.X - TopLeft.X, BottomRight.Y - TopLeft.Y));





        }
        public bool IntersectsWith(Polygon otherpoly)
        {
            return (GeometryHelper.PolygonCollision(this, otherpoly, new Vector(0, 0)).Intersect);


        }
        public Polygon(RectangleF rf)
        {

            points.Add(new Vector(rf.Left, rf.Top));
            points.Add(new Vector(rf.Right, rf.Top));
            points.Add(new Vector(rf.Right, rf.Bottom));
            points.Add(new Vector(rf.Left, rf.Bottom));
            BuildEdges();

        }
        public Polygon(Vector CenterSpot, int NumSides, double Radius, double startingAngle)
        {
            double currentangle = startingAngle;
            double anglediff = (Math.PI * 2) / (float)NumSides;


            for (int i = 0; i < NumSides; i++)
            {
                Vector newpoint = new Vector((float)(Math.Sin(currentangle) * Radius + CenterSpot.X),
                                            (float)(Math.Cos(currentangle) * Radius + CenterSpot.Y));

                Points.Add(newpoint);
                currentangle += anglediff;
            }


            BuildEdges();

        }
        public Polygon(List<Vector> frompoints)
        {

            points = frompoints;
            BuildEdges();


        }
        public Polygon(IEnumerable<Vector> frompoints) : this(frompoints.ToList())
        {



        }
        public void BuildEdges()
        {
            Vector p1;
            Vector p2;
            edges.Clear();
            for (int i = 0; i < points.Count; i++)
            {
                p1 = points[i];
                if (i + 1 >= points.Count)
                {
                    p2 = points[0];
                }
                else
                {
                    p2 = points[i + 1];
                }
                edges.Add(p2 - p1);
            }
        }

        public List<Vector> Edges
        {
            get { return edges; }
        }

        public List<Vector> Points
        {
            get { return points; }
        }
        public List<PointF> getPoints()
        {
            return new List<PointF>(from pp in Points select new PointF(pp.X, pp.Y));
        }
        public Vector Center
        {
            get
            {
                float totalX = 0;
                float totalY = 0;
                for (int i = 0; i < points.Count; i++)
                {
                    totalX += points[i].X;
                    totalY += points[i].Y;
                }

                return new Vector(totalX / (float)points.Count, totalY / (float)points.Count);
            }
        }

        public void Offset(Vector v)
        {
            Offset(v.X, v.Y);
        }

        public void Offset(float x, float y)
        {
            for (int i = 0; i < points.Count; i++)
            {
                Vector p = points[i];
                points[i] = new Vector(p.X + x, p.Y + y);
            }
        }

        public override string ToString()
        {
            string result = "";

            for (int i = 0; i < points.Count; i++)
            {
                if (result != "") result += " ";
                result += "{" + points[i].ToString(true) + "}";
            }

            return result;
        }



        //splits this polygon into triangular sectors, each of which is returned as a new polygon in the return array.

        public Polygon[] Split()
        {
            Polygon[] returnpoly = new Polygon[Points.Count];
            for (int i = 0; i < Points.Count - 1; i++)
            {

                //First, second, Center will be the polygon order.
                Polygon pg = new Polygon(new PointF[] { Points[i], Points[i + 1], Center });

                returnpoly[i] = pg;
            }

            // add last poly. Last-First-Center
            returnpoly[returnpoly.Length - 1] = new Polygon(new PointF[] { Points.Last(), Points.First(), Center });


            return returnpoly;

        }
        public static Polygon operator +(Polygon pg, Vector Addthis)
        {
            Polygon pclone = (Polygon)pg.Clone();
            for (int i = 0; i < pclone.Points.Count; i++)
            {

                pclone.Points[i] += Addthis;
            }


            return pclone;
        }
        public static Polygon operator -(Polygon pg, Vector Addthis)
        {

            return pg + (new Vector(-Addthis.X, -Addthis.Y));

        }
        public static implicit operator List<Vector>(Polygon pg)
        {

            return pg.Points;
        }
        public static implicit operator Vector[] (Polygon pg)
        {

            return pg.Points.ToArray();

        }
        public static implicit operator PointF[] (Polygon pg)
        {

            return (from p in pg.Points select (PointF)p).ToArray();

        }
        public static implicit operator Polygon(RectangleF fromrect)
        {

            return new Polygon(fromrect);

        }
        #region ICloneable Members

        public object Clone()
        {
            return new Polygon(points.Clone());
        }

        #endregion
        public double Area()
        {
            double accum = 0;
            //returns the area of this polygon.
            //calculate each triangle...
            for (int i = 0; i < points.Count - 1; i += 2)
            {
                //item i and i+1, and the center, form the triangle.
                PointF firstpoint = points[i];
                PointF secondpoint = points[i + 1];
                PointF midspot = TrigFunctions.MidPoint(firstpoint, secondpoint);
                double H = TrigFunctions.Distance(Center, midspot);
                //width is distance between the two points.
                double W = TrigFunctions.Distance(firstpoint, secondpoint);
                accum += (W * H) / 2;

            }

            return (float)accum;

        }


        private PointF rescale(PointF center, PointF target, float scale)
        {
            return new PointF((target.X - center.X) * scale + center.X,
                (target.Y - center.Y) * scale + center.Y);



        }

        public Polygon Scale(float scaleamount)
        {
            //rescales the polygon, returning the scaled result.
            PointF center = Center;
            var result = from p in Points select rescale(center, p, scaleamount);
            return new Polygon(result);
        }
    }

    [Serializable]
    public struct Vector : ICloneable, ISerializable
    {

        public float X;
        public float Y;
        public static readonly Vector Empty = new Vector(0, 0);


        static public Vector FromPoint(Point p)
        {
            return Vector.FromPoint(p.X, p.Y);
        }

        static public Vector FromPoint(int x, int y)
        {
            return new Vector((float)x, (float)y);
        }

        public void Offset(Vector useoffset)
        {

            X += useoffset.X;
            Y += useoffset.Y;

        }

        public Vector(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }
        public Vector(SerializationInfo info, StreamingContext context)
        {
            X = info.GetSingle("X");
            Y = info.GetSingle("Y");

        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("X", X);
            info.AddValue("Y", Y);

        }
        public float Magnitude
        {
            get { return (float)Math.Sqrt(X * X + Y * Y); }
        }
        public Vector Rotate90()
        {


            return new Vector(Y, -X);



        }
        public void Normalize()
        {
            float magnitude = Magnitude;
            X = X / magnitude;
            Y = Y / magnitude;
        }

        public Vector GetNormalized()
        {
            float magnitude = Magnitude;

            return new Vector(X / magnitude, Y / magnitude);
        }

        public float DotProduct(Vector vector)
        {
            return this.X * vector.X + this.Y * vector.Y;
        }

        public float DistanceTo(Vector vector)
        {
            return (float)Math.Sqrt(Math.Pow(vector.X - this.X, 2) + Math.Pow(vector.Y - this.Y, 2));
        }

        public static implicit operator Point(Vector p)
        {
            return new Point((int)p.X, (int)p.Y);
        }

        public static implicit operator PointF(Vector p)
        {
            return new PointF(p.X, p.Y);
        }
        public static implicit operator Vector(PointF p)
        {
            return new Vector(p.X, p.Y);

        }
        public static implicit operator Vector(Point p)
        {

            return new Vector(p.X, p.Y);

        }
        public static Vector operator +(Vector a, Vector b)
        {
            return new Vector(a.X + b.X, a.Y + b.Y);
        }

        public static Vector operator -(Vector a)
        {
            return new Vector(-a.X, -a.Y);
        }

        public static Vector operator -(Vector a, Vector b)
        {
            return new Vector(a.X - b.X, a.Y - b.Y);
        }

        public static Vector operator *(Vector a, float b)
        {
            return new Vector(a.X * b, a.Y * b);
        }

        public static Vector operator *(Vector a, int b)
        {
            return new Vector(a.X * b, a.Y * b);
        }

        public static Vector operator *(Vector a, double b)
        {
            return new Vector((float)(a.X * b), (float)(a.Y * b));
        }

        public override bool Equals(object obj)
        {
            Vector v = (Vector)obj;

            return X == v.X && Y == v.Y;
        }

        public bool Equals(Vector v)
        {
            return X == v.X && Y == v.Y;
        }

        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode();
        }

        public static bool operator ==(Vector a, Vector b)
        {
            return a.X == b.X && a.Y == b.Y;
        }

        public static bool operator !=(Vector a, Vector b)
        {
            return a.X != b.X || a.Y != b.Y;
        }

        public override string ToString()
        {
            return X + ", " + Y;
        }

        public string ToString(bool rounded)
        {
            if (rounded)
            {
                return (int)Math.Round(X) + ", " + (int)Math.Round(Y);
            }
            else
            {
                return ToString();
            }
        }



        #region ICloneable Members

        public object Clone()
        {
            return new Vector(this.X, this.Y);
        }

        #endregion
    }



    class GeometryHelper
    {
        // Structure that stores the results of the PolygonCollision function



        // Calculate the projection of a polygon on an axis

        // and returns it as a [min, max] interval


        // Structure that stores the results of the PolygonCollision function
        public struct PolygonCollisionResult
        {
            public bool WillIntersect; // Are the polygons going to intersect forward in time?
            public bool Intersect; // Are the polygons currently intersecting
            public Vector MinimumTranslationVector; // The translation to apply to polygon A to push the polygons appart.
        }




        // Check if polygon A is going to collide with polygon B for the given velocity
        public static PolygonCollisionResult PolygonCollision(Polygon polygonA, Polygon polygonB, Vector velocity)
        {
            if (polygonA == null || polygonB == null)
            {
                Debug.Print("balls");

            }
            PolygonCollisionResult result = new PolygonCollisionResult();
            result.Intersect = true;
            result.WillIntersect = true;

            int edgeCountA = polygonA.Edges.Count;
            int edgeCountB = polygonB.Edges.Count;
            float minIntervalDistance = float.PositiveInfinity;
            Vector translationAxis = new Vector();
            Vector edge;

            // Loop through all the edges of both polygons
            for (int edgeIndex = 0; edgeIndex < edgeCountA + edgeCountB; edgeIndex++)
            {
                if (edgeIndex < edgeCountA)
                {
                    edge = polygonA.Edges[edgeIndex];
                }
                else
                {
                    edge = polygonB.Edges[edgeIndex - edgeCountA];
                }

                // ===== 1. Find if the polygons are currently intersecting =====

                // Find the axis perpendicular to the current edge
                Vector axis = new Vector(-edge.Y, edge.X);
                axis.Normalize();

                // Find the projection of the polygon on the current axis
                float minA = 0; float minB = 0; float maxA = 0; float maxB = 0;
                ProjectPolygon(axis, polygonA, ref minA, ref maxA);
                ProjectPolygon(axis, polygonB, ref minB, ref maxB);

                // Check if the polygon projections are currentlty intersecting
                if (IntervalDistance(minA, maxA, minB, maxB) > 0) result.Intersect = false;

                // ===== 2. Now find if the polygons *will* intersect =====

                // Project the velocity on the current axis
                float velocityProjection = axis.DotProduct(velocity);

                // Get the projection of polygon A during the movement
                if (velocityProjection < 0)
                {
                    minA += velocityProjection;
                }
                else
                {
                    maxA += velocityProjection;
                }

                // Do the same test as above for the new projection
                float intervalDistance = IntervalDistance(minA, maxA, minB, maxB);
                if (intervalDistance > 0) result.WillIntersect = false;

                // If the polygons are not intersecting and won't intersect, exit the loop
                if (!result.Intersect && !result.WillIntersect) break;

                // Check if the current interval distance is the minimum one. If so store
                // the interval distance and the current distance.
                // This will be used to calculate the minimum translation vector
                intervalDistance = Math.Abs(intervalDistance);
                if (intervalDistance < minIntervalDistance)
                {
                    minIntervalDistance = intervalDistance;
                    translationAxis = axis;

                    Vector d = polygonA.Center - polygonB.Center;
                    if (d.DotProduct(translationAxis) < 0) translationAxis = -translationAxis;
                }
            }

            // The minimum translation vector can be used to push the polygons appart.
            // First moves the polygons by their velocity
            // then move polygonA by MinimumTranslationVector.
            if (result.WillIntersect) result.MinimumTranslationVector = translationAxis * minIntervalDistance;

            return result;
        }

        // Calculate the distance between [minA, maxA] and [minB, maxB]
        // The distance will be negative if the intervals overlap
        public static float IntervalDistance(float minA, float maxA, float minB, float maxB)
        {
            if (minA < minB)
            {
                return minB - maxA;
            }
            else
            {
                return minA - maxB;
            }
        }
        public static PointF PercentLine(PointF PointA, PointF PointB, float Percentage)
        {
            //get the point that is Percentage between PointA and PointB.

            //get the difference, and multiply by percentage.
            PointF diff = new PointF(Percentage * (PointB.X - PointA.X), Percentage * (PointB.Y - PointA.Y));
            //return PointA + diff
            return new PointF(PointA.X + diff.X, PointA.Y + diff.Y);



        }
        // Calculate the projection of a polygon on an axis and returns it as a [min, max] interval
        public static void ProjectPolygon(Vector axis, Polygon polygon, ref float min, ref float max)
        {
            // To project a point on an axis use the dot product
            float d = axis.DotProduct(polygon.Points[0]);
            min = d;
            max = d;
            for (int i = 0; i < polygon.Points.Count; i++)
            {
                d = polygon.Points[i].DotProduct(axis);
                if (d < min)
                {
                    min = d;
                }
                else
                {
                    if (d > max)
                    {
                        max = d;
                    }
                }
            }
        }


    }
}
