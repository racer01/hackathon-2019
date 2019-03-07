using Hackathon.Public;
using KillEmAll.Enums;
using KillEmAll.Helpers.Interfaces;
using KillEmAll.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KillEmAll.Helpers
{
    public class WallMapping : IWallMapping
    {
        private const int WALL_SIZE = 1;
        private const int WALL_CHECK_RANGE = 3;
        private const int X = 0;
        private const int Y = 1;

        private readonly MapCell[,] _cells;
        private static readonly PointF[] _blockingWalls = new PointF[WALL_CHECK_RANGE];


        // USED FOR FAST LOOKUP (we know a coordinate, and want to find out if that is a reachable unknown)
        public bool[,] ReachableUnknownPoints;

        // used select a reachable unknown (we cant use the bool[,] for this, since we don't know wich coordinates are the unkowns)
        public List<WeightedPoint> ReachableUnkownList { get; set; }


        private static int[,] _skipMapping;

        private readonly Dictionary<RelativePosition, int[]> _directions = new Dictionary<RelativePosition, int[]>
        {
            {RelativePosition.ABOVE, new [] { 0, 1 } },
            {RelativePosition.BELOW, new [] { 0, -1 } },
            {RelativePosition.RIGHT, new [] { 1, 0 } },
            {RelativePosition.LEFT, new [] { -1, 0 } },
            {RelativePosition.ABOVE | RelativePosition.RIGHT, new [] { 1, 1 } },
            {RelativePosition.ABOVE | RelativePosition.LEFT, new [] { -1, 1 } },
            {RelativePosition.BELOW | RelativePosition.RIGHT, new [] { 1, -1 } },
            {RelativePosition.BELOW | RelativePosition.LEFT, new [] { -1, -1 } },
        };

        public WallMapping(int mapSizeX, int mapSizeY)
        {
            _cells = new MapCell[mapSizeX, mapSizeY];
            _skipMapping = new int[mapSizeX, mapSizeY];
            ReachableUnknownPoints = new bool[mapSizeX, mapSizeY];
            ReachableUnkownList = new List<WeightedPoint>();
        }

        public MapCell[,] GetMap()
        {
            return _cells;
        }
        public MapCell GetCellType(int x, int y)
        {
            return _cells[x, y];
        }

        public void Store(int x, int y, MapCell value)
        {
            if (x >= _cells.GetLength(0) || x < 0 || y >= _cells.GetLength(1) || y < 0)
                return;
            _cells[x, y] = value;
        }

        public bool IsAlreadyDiscovered(int x, int y)
        {
            var value = _cells[x, y];
            return value == MapCell.Wall || value == MapCell.Empty;
        }

        // Stores the newly discovered areas in an array. 
        // If there are multiple walls (already discovered) next to each other, it skips them.
        public void StoreVisibleArea(MapCell[,] visibleArea)
        {
            for (var i = 0; i < visibleArea.GetLength(0); i++)
            {
                for (var j = 0; j < visibleArea.GetLength(1); j++)
                {
                    if (IsAlreadyDiscovered(i, j))
                        continue;

                    var current = visibleArea[i, j];
                    if (current == MapCell.Wall || current == MapCell.Empty)
                    {
                        // IF WE ARE HERE -> this cell must be a wall or an empty walkable spot, meaning it cant be an unkown anymore
                        ReachableUnknownPoints[i, j] = false;

                        ReachableUnkownList = ReachableUnkownList.Where(ru => ru.X != i || ru.Y != j).ToList();

                        // if current cell is empty, it means the neighbors might be reachable unkowns
                        // this will add (top, right) neighbors as reachable unkowns, which is obviously not correct
                        // but the above 2 lines will correct that -> when we pass over those points we'll 
                        // see if they're a wall or empty space and remove them from the ReachableUnkown list if necessary
                        if (current == MapCell.Empty)
                            StoreNeighborsAsReachableUnknown(i, j);

                        Store(i, j, current);
                    }
                }
            }
        }

        private void StoreNeighborsAsReachableUnknown(int x, int y)
        {
            // VODOO MAGIC!!, ONLY STORE TOP AND RIGHT!!
            var above = GetCellInDirection(new int[] { x, y }, RelativePosition.ABOVE);
            var right = GetCellInDirection(new int[] { x, y }, RelativePosition.RIGHT);

            if (!IsCellOutOfBounds(above))
            {
                if (!ReachableUnknownPoints[above.X, above.Y] && _cells[above.X, above.Y] == MapCell.Unknown)
                {
                    ReachableUnkownList.Add(new WeightedPoint() { X = above.X, Y = above.Y });
                    ReachableUnknownPoints[above.X, above.Y] = true;
                }
            }

            if (!IsCellOutOfBounds(right))
            {
                if (!ReachableUnknownPoints[right.X, right.Y] && _cells[right.X, right.Y] == MapCell.Unknown)
                {
                    ReachableUnkownList.Add(new WeightedPoint() { X = right.X, Y = right.Y });
                    ReachableUnknownPoints[right.X, right.Y] = true;
                }
            }
        }

        private bool IsCellOutOfBounds(WeightedPoint pos)
        {
            return pos.X >= _cells.GetLength(0) || pos.X < 0 || pos.Y >= _cells.GetLength(1) || pos.Y < 0;
        }

        public List<WeightedPoint> GetNeighbourCells(int[] origin, MapCell searchType = MapCell.Empty, MapCell[,] areaToSeach = null, DiagonalCheckType diagonalCheck = DiagonalCheckType.CheckIfReachable, bool includeUnknows = false)
        {
            var result = new List<WeightedPoint>(8);
            if (areaToSeach == null)
                areaToSeach = GetMap();

            var topCellType = StoreNeighborCell(origin, RelativePosition.ABOVE, searchType, areaToSeach, ref result, includeUnknows);
            var rightCellType = StoreNeighborCell(origin, RelativePosition.RIGHT, searchType, areaToSeach, ref result, includeUnknows);
            var leftCellType = StoreNeighborCell(origin, RelativePosition.LEFT, searchType, areaToSeach, ref result, includeUnknows);
            var botCellType = StoreNeighborCell(origin, RelativePosition.BELOW, searchType, areaToSeach, ref result, includeUnknows);

            // IF THERE IS A WALL ABOVE US, DONT CHECK DIAGONALLY UP
            if (diagonalCheck == DiagonalCheckType.Always || (!(topCellType == MapCell.Wall) && diagonalCheck != DiagonalCheckType.Never))
            {
                // IF THERE ARE NO WALLS ABOVE, WE CAN CHECK THE TOP-RIGHT CORNER
                if (diagonalCheck != DiagonalCheckType.Never)
                    StoreNeighborCell(origin, RelativePosition.ABOVE | RelativePosition.RIGHT, searchType, areaToSeach, ref result);

                // IF THERE ARE NO WALLS ABOVE, WE CAN CHECK THE TOP-LEFT CORNER
                if (diagonalCheck != DiagonalCheckType.Never)
                    StoreNeighborCell(origin, RelativePosition.ABOVE | RelativePosition.LEFT, searchType, areaToSeach, ref result);
            }
            else if (diagonalCheck != DiagonalCheckType.Never)
            {
                if (leftCellType != MapCell.Wall || diagonalCheck == DiagonalCheckType.Always)
                    StoreNeighborCell(origin, RelativePosition.ABOVE | RelativePosition.LEFT, searchType, areaToSeach, ref result);

                if (rightCellType != MapCell.Wall || diagonalCheck == DiagonalCheckType.Always)
                    StoreNeighborCell(origin, RelativePosition.ABOVE | RelativePosition.RIGHT, searchType, areaToSeach, ref result);
            }


            // IF THERE IS A WALL BELOW US, DONT CHECK DIAGONALLY DOWN
            if (diagonalCheck == DiagonalCheckType.Always || (!(botCellType == MapCell.Wall) && diagonalCheck != DiagonalCheckType.Never))
            {
                // IF THERE ARE NO WALLS BELOW, WE CAN CHECK THE BOT-RIGHT CORNER
                if (diagonalCheck != DiagonalCheckType.Never)
                    StoreNeighborCell(origin, RelativePosition.BELOW | RelativePosition.RIGHT, searchType, areaToSeach, ref result);

                // IF THERE ARE NO WALLS BELOW, WE CAN CHECK THE BOT-LEFT CORNER
                if (diagonalCheck != DiagonalCheckType.Never)
                    StoreNeighborCell(origin, RelativePosition.BELOW | RelativePosition.LEFT, searchType, areaToSeach, ref result);
            }
            else if (diagonalCheck != DiagonalCheckType.Never)
            {
                if (leftCellType != MapCell.Wall || diagonalCheck == DiagonalCheckType.Always)
                    StoreNeighborCell(origin, RelativePosition.BELOW | RelativePosition.LEFT, searchType, areaToSeach, ref result);

                if (rightCellType != MapCell.Wall || diagonalCheck == DiagonalCheckType.Always)
                    StoreNeighborCell(origin, RelativePosition.BELOW | RelativePosition.RIGHT, searchType, areaToSeach, ref result);
            }

            return result;
        }

        private MapCell StoreNeighborCell(int[] origin, RelativePosition position, MapCell searchType, MapCell[,] map, ref List<WeightedPoint> neighbors, bool includeUnknows = false)
        {
            var nextCell = GetCellInDirection(origin, position);
            if (nextCell.X >= map.GetLength(0) || nextCell.X < 0 || nextCell.Y >= map.GetLength(1) || nextCell.Y < 0)
                return MapCell.Unknown;

            var type = map[nextCell.X, nextCell.Y];

            if (type == searchType || (includeUnknows && type == MapCell.Unknown))
                neighbors.Add(nextCell);

            return type;
        }

        private WeightedPoint GetCellInDirection(int[] origin, RelativePosition position)
        {
            var direction = _directions[position];
            var nextX = origin[X] + direction[X];
            var nextY = origin[Y] + direction[Y];

            return new WeightedPoint() { X = nextX, Y = nextY, RelativePosition = position };
        }


        ///////////////////////////////////////////////////////////////////////////////
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
                    if (_cells[i, j] == MapCell.Wall)
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

        private static float GetSlope(PointF startingPoint, PointF endingPoint)
        {
            if (endingPoint.X - startingPoint.X != 0)
                return (endingPoint.Y - startingPoint.Y) / (endingPoint.X - startingPoint.X);
            return float.MaxValue;
        }

        private static PointF GetPointOfLineForX(PointF knownPoint, float slope, float x)
        {
            var y = (x - knownPoint.X) * slope + knownPoint.Y;
            return new PointF(x, y);
        }

        private static PointF GetPointOfLineForY(PointF knownPoint, float slope, float y)
        {
            if (slope != 0)
                return new PointF((y - knownPoint.Y) / slope + knownPoint.X, y);
            return new PointF(float.MaxValue, y);
        }
    }
}
