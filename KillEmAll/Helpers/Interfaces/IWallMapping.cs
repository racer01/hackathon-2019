﻿using Hackathon.Public;
using KillEmAll.Enums;
using KillEmAll.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KillEmAll.Helpers.Interfaces
{
    public interface IWallMapping
    {
        void Store(int x, int y, MapCell value);

        bool IsAlreadyDiscovered(int x, int y);

        void StoreVisibleArea(MapCell[,] visibleArea);

        MapCell GetCellType(int x, int y);

        /// <summary>
        /// Returns the walls above and below the specified block (including the passed block, if that's a wall too)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        PointF[] WallsOnLine(int x, int y);

        MapCell[,] GetMap();

        List<WeightedPoint> GetNeighbourCells(int[] origin, MapCell searchType = MapCell.Empty, MapCell[,] areaToSeach = null, DiagonalCheckType diagonalCheck = DiagonalCheckType.CheckIfReachable, bool includeUnknowns = false);

        List<int[]> ReachableUnknowns { get; set; }
    }
}