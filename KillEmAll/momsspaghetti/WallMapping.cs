using System;
using System.Collections.Generic;
using System.Linq;
using Hackathon.Public;

namespace KillEmAll.momsspaghetti
{
    public class WallMapping
    {
        private const int WALL_SIZE = 1;

        private bool[,] _wallMap;

        public WallMapping(int mapSizeX, int mapSizeY)
        {
            _wallMap = new bool[mapSizeX, mapSizeY];
        }

        public void Store(int x, int y)
        {
            if (x >= _wallMap.GetLength(0) || x < 0 || y >= _wallMap.GetLength(1) || y < 0)
                return;
            _wallMap[x, y] = true;
        }

        public List<PointF> GetCrossedWalls(PointF point1, PointF point2)
        {
            var crossedWalls = new List<PointF>();

            var minWallX = (int)Math.Min(Math.Floor(point1.X), Math.Floor(point2.X));
            var maxWallX = (int)Math.Max(Math.Floor(point1.X), Math.Floor(point2.X));
            var minWallY = (int)Math.Min(Math.Floor(point1.Y), Math.Floor(point2.Y));
            var maxWallY = (int)Math.Max(Math.Floor(point1.Y), Math.Floor(point2.Y));

            for (var i = minWallX; i <= maxWallX; i++)
            {
                for (var j = minWallY; j <= maxWallY; j++)
                {
                    if (_wallMap[i, j])
                    {
                        var x1 = i;
                        var x2 = i + WALL_SIZE;
                        var y1 = j;
                        var y2 = j + WALL_SIZE;

                        if ((point1.X - point2.X != 0) && (point1.Y - point2.Y != 0))
                        {
                            var points = new List<PointF>();
                            var slope = GetSlope(point1, point2);
                            points.Add(GetPointOfLineForX(point1, slope, x1));
                            points.Add(GetPointOfLineForX(point1, slope, x2));
                            points.Add(GetPointOfLineForY(point1, slope, y1));
                            points.Add(GetPointOfLineForY(point1, slope, y2));

                            if (points.Any(p => p.X >= x1 && p.X <= x2 && p.Y >= y1 && p.Y <= y2))
                            {
                                crossedWalls.Add(new PointF(i, j));
                            }
                        }
                        else if (point1.X - point2.X == 0)
                        {
                            if (point1.X >= x1 && point1.X <= x2)
                                crossedWalls.Add(new PointF(i, j));
                        }
                        else if (point1.Y - point2.Y == 0)
                        {
                            if (point1.Y >= y1 && point1.Y <= y2)
                                crossedWalls.Add(new PointF(i, j));
                        }
                    }
                }
            }
            return crossedWalls;
        }

        private float GetSlope(PointF startingPoint, PointF endingPoint)
        {
            if (endingPoint.X - startingPoint.X != 0)
                return (endingPoint.Y - startingPoint.Y) / (endingPoint.X - startingPoint.X);
            else
                return float.MaxValue;
        }

        private PointF GetPointOfLineForX(PointF knownPoint, float slope, float x)
        {
            var y = (x - knownPoint.X) * slope + knownPoint.Y;
            return new PointF(x, y);
        }

        private PointF GetPointOfLineForY(PointF knownPoint, float slope, float y)
        {
            if (slope != 0)
                return new PointF((y - knownPoint.Y) / slope + knownPoint.X, y);
            else
                return new PointF(float.MaxValue, y);
        }
    }
}