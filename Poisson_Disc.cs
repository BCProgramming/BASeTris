using OpenTK.Mathematics;
using SkiaSharp;
using SkiaSharp.Views.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BASeTris
{
    public static class FastPoissonDiskSampling
    {
        public const float InvertRootTwo = 0.70710678118f; // Becaust two dimension grid.
        public const int DefaultIterationPerPoint = 30;

        #region "Structures"
        private class Settings
        {
            public SKPoint BottomLeft;
            public SKPoint TopRight;
            public SKPoint Center;
            public SKRect Dimension;

            public float MinimumDistance;
            public int IterationPerPoint;

            public float CellSize;
            public int GridWidth;
            public int GridHeight;
        }

        private class Bags
        {
            public SKPoint?[,] Grid;
            public List<SKPoint> SamplePoints;
            public List<SKPoint> ActivePoints;
        }
        #endregion


        public static List<SKPoint> Sampling(SKPoint bottomLeft, SKPoint topRight, float minimumDistance)
        {
            return Sampling(bottomLeft, topRight, minimumDistance, DefaultIterationPerPoint);
        }

        public static List<SKPoint> Sampling(SKPoint bottomLeft, SKPoint topRight, float minimumDistance, int iterationPerPoint)
        {
            var settings = GetSettings(
                bottomLeft,
                topRight,
                minimumDistance,
                iterationPerPoint <= 0 ? DefaultIterationPerPoint : iterationPerPoint
            );

            var bags = new Bags()
            {
                Grid = new SKPoint?[settings.GridWidth + 1, settings.GridHeight + 1],
                SamplePoints = new List<SKPoint>(),
                ActivePoints = new List<SKPoint>()
            };

            GetFirstPoint(settings, bags);

            do
            {
                var index = Random.Shared.Next(0, bags.ActivePoints.Count);

                var point = bags.ActivePoints[index];

                var found = false;
                for (var k = 0; k < settings.IterationPerPoint; k++)
                {
                    found = found | GetNextPoint(point, settings, bags);
                }

                if (found == false)
                {
                    bags.ActivePoints.RemoveAt(index);
                }
            }
            while (bags.ActivePoints.Count > 0);

            return bags.SamplePoints;
        }

        #region "Algorithm Calculations"
        private static bool GetNextPoint(SKPoint point, Settings set, Bags bags)
        {
            var found = false;
            var p = GetRandPosInCircle(set.MinimumDistance, 2f * set.MinimumDistance) + point;

            if (set.Dimension.Contains(p) == false)
            {
                return false;
            }

            var minimum = set.MinimumDistance * set.MinimumDistance;
            var index = GetGridIndex(p, set);
            var drop = false;

            // Although it is Mathf.CeilToInt(set.MinimumDistance / set.CellSize) in the formula, It will be 2 after all.
            var around = 2;
            
            var fieldMin = new SKPointI((int)(MathF.Max(0, index.X - around)),(int)(MathF.Max(0, index.Y - around)));
            var fieldMax = new SKPointI((int)(MathF.Min(set.GridWidth, index.X + around)), (int)(MathF.Min(set.GridHeight, index.Y + around)));

            for (var i = fieldMin.X; i <= fieldMax.X && drop == false; i++)
            {
                for (var j = fieldMin.Y; j <= fieldMax.Y && drop == false; j++)
                {
                    var q = bags.Grid[i, j];
                    if (q.HasValue == true && SqrMagnitude(q.Value - p) <= minimum)
                    {
                        drop = true;
                    }
                }
            }

            if (drop == false)
            {
                found = true;

                bags.SamplePoints.Add(p);
                bags.ActivePoints.Add(p);
                bags.Grid[index.X, index.Y] = p;
            }

            return found;
        }
        private static double SqrMagnitude(SKPoint src)
        {

            return src.X * src.X + src.Y * src.Y;

        }
        private static void GetFirstPoint(Settings set, Bags bags)
        {
            var first = new SKPoint(
                (float)Random.Shared.NextDouble(set.BottomLeft.X, set.TopRight.X),
                (float)Random.Shared.NextDouble(set.BottomLeft.Y, set.TopRight.Y)
            );

            var index = GetGridIndex(first, set);

            bags.Grid[index.X, index.Y] = first;
            bags.SamplePoints.Add(first);
            bags.ActivePoints.Add(first);
        }
        #endregion

        #region "Utils"
        private static SKPointI GetGridIndex(SKPoint point, Settings set)
        {
            return new SKPointI(
                (int)MathF.Floor((point.X - set.BottomLeft.X) / set.CellSize),
                (int)MathF.Floor((point.Y - set.BottomLeft.Y) / set.CellSize)
            );
        }

        private static Settings GetSettings(SKPoint bl, SKPoint tr, float min, int iteration)
        {
            var dimension = (tr - bl);
            var cell = min * InvertRootTwo;
            var centercalc = (bl + tr);
            var Corner1 = new SKPoint(bl.X, bl.Y);
            var Corner2 = new SKPoint(dimension.X, dimension.Y);
            return new Settings()
            {
                BottomLeft = bl,
                TopRight = tr,

                Center = new SKPoint(centercalc.X * 0.5f, centercalc.Y * 0.5f),
            
                Dimension = new SKRect(Corner1.X,Corner1.Y,Corner2.X,Corner2.Y),

                MinimumDistance = min,
                IterationPerPoint = iteration,

                CellSize = cell,
                GridWidth = (int)MathF.Ceiling(dimension.X / cell),
                GridHeight = (int)MathF.Ceiling(dimension.Y / cell)
            };
        }

        private static SKPoint GetRandPosInCircle(float fieldMin, float fieldMax)
        {
            var theta = Random.Shared.NextDouble(0f, MathF.PI * 2f);
            var radius = Math.Sqrt(Random.Shared.NextDouble(fieldMin * fieldMin, fieldMax * fieldMax));

            return new SKPoint((float)(radius * Math.Cos(theta)), (float)(radius * Math.Sin(theta)));
        }
        #endregion
    }
}
