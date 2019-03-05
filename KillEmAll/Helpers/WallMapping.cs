using Hackathon.Public;
using KillEmAll.Enums;
using KillEmAll.Helpers.Interfaces;
using KillEmAll.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KillEmAll.Helpers
{
    public class WallMapping : IWallMapping
    {
        private const int WALL_SIZE = 1;
        private const int WALL_CHECK_RANGE = 3;
        private const int X = 0;
        private const int Y = 1;

        private MapCell[,] _cells;
        private static PointF[] _blockingWalls = new PointF[WALL_CHECK_RANGE];
        private static int[,] _skipMapping;
        public List<int[]> ReachableUnknowns { get; set; }

        private readonly Dictionary<RelativePosition, int[]> Directions = new Dictionary<RelativePosition, int[]>
        {
            {RelativePosition.ABOVE, new int[] { 0, 1 } },
            {RelativePosition.BELOW, new int[] { 0, -1 } },
            {RelativePosition.RIGHT, new int[] { 1, 0 } },
            {RelativePosition.LEFT, new int[] { -1, 0 } },
            {RelativePosition.ABOVE | RelativePosition.RIGHT, new int[] { 1, 1 } },
            {RelativePosition.ABOVE | RelativePosition.LEFT, new int[] { -1, 1 } },
            {RelativePosition.BELOW | RelativePosition.RIGHT, new int[] { 1, -1 } },
            {RelativePosition.BELOW | RelativePosition.LEFT, new int[] { -1, -1 } },
        };

        public WallMapping(int mapSizeX, int mapSizeY)
        {
            _cells = new MapCell[mapSizeX, mapSizeY];
            _skipMapping = new int[mapSizeX, mapSizeY];
            ReachableUnknowns = new List<int[]>();
        }

        public bool IsAlreadyDiscovered(int x, int y)
        {
            var value = _cells[x, y];
            return value == MapCell.Wall || value == MapCell.Empty;
        }

        public MapCell[,] GetMap()
        {
            return _cells;
        }

        public void Store(int x, int y, MapCell value)
        {
            if (x >= _cells.GetLength(0) || x < 0 || y >= _cells.GetLength(1) || y < 0)
                return;

            _cells[x, y] = value;
        }

        private void ClearBlockedWalls()
        {
            for (var i = 0; i < _blockingWalls.Length; i++)
                _blockingWalls[i] = null;
        }

        public PointF[] WallsOnLine(int x, int y)
        {
            if (x >= _cells.GetLength(0) || x < 0 || y >= _cells.GetLength(1) || y < 0)
                return null;

            ClearBlockedWalls();

            var above = y + WALL_SIZE;
            var below = y - WALL_SIZE;

            var foundWalls = 0;
            if (_cells[x, y] == MapCell.Wall)
            {
                _blockingWalls[0] = new PointF(x, y);
                foundWalls++;
            }
            if (above < _cells.GetLength(1) && _cells[x, above] == MapCell.Wall)
            {
                _blockingWalls[1] = new PointF(x, above);
                foundWalls++;
            }
            if (below >= 0 && _cells[x, below] == MapCell.Wall)
            {
                _blockingWalls[2] = new PointF(x, below);
                foundWalls++;
            }

            if (foundWalls == 0)
                return null;

            return _blockingWalls;
        }

        /// <summary>
        /// Stores the newly discovered areas in an array. If there are multiple walls (already discovered) next to each other, it skips them.
        /// </summary>
        /// <param name="visibleArea"></param>
        public void StoreVisibleArea(MapCell[,] visibleArea)
        {
            //ReachableUnknowns = new List<int[]>();

            for (var i = 0; i < visibleArea.GetLength(0); i++)
            {
                var jumpCount = -1;
                var skipX = i;
                var skipY = 0;
                var skipAmount = 1;
                for (var j = 0; j < visibleArea.GetLength(1); j += 1 + skipAmount)
                {
                    skipAmount = _skipMapping[i, j];
                    if (skipAmount >= 1)
                        continue;

                    if (jumpCount == -1)
                        skipY = j;

                    if (IsAlreadyDiscovered(i, j))
                    {
                        jumpCount++;
                        continue;
                    }

                    var current = visibleArea[i, j];
                    if (current == MapCell.Wall || current == MapCell.Empty)
                    {
                        Store(i, j, current);

                        //if (current == MapCell.Empty)
                        //{
                        //    var unknownNeighbors = GetNeighbourCells(new int[] { i, j }, searchType: MapCell.Unknown, areaToSeach: visibleArea, includeUnknows: true);

                        //    if (unknownNeighbors != null && unknownNeighbors.Count != 0)
                        //    {
                        //        for (var u = 0; u < unknownNeighbors.Count; u++)
                        //        {
                        //            var point = new int[] { unknownNeighbors[u].X, unknownNeighbors[u].Y };
                        //            ReachableUnknowns.Add(point);
                        //        }
                        //    }
                                
                        //}
                        jumpCount++;
                    }
                    else
                    {
                        if (jumpCount <= 0)
                            continue;

                        if (skipX < _skipMapping.GetLength(0) && skipY < _skipMapping.GetLength(1) && skipX >= 0 && skipY >= 0)
                            _skipMapping[skipX, skipY] = jumpCount;

                        jumpCount = -1;
                    }
                }
            }
        }

        public MapCell GetCellType(int x, int y)
        {
            return _cells[x, y];
        }

        // TODO: REFACTOR, TOO COMPLICATED
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
            var direction = Directions[position];
            var nextX = origin[X] + direction[X];
            var nextY = origin[Y] + direction[Y];

            return new WeightedPoint() { X = nextX, Y = nextY, RelativePosition = position };
        }
    }
}
