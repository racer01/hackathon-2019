using Hackathon.Public;
using KillEmAll.Enums;
using KillEmAll.Utility;
using System.Collections.Generic;

namespace KillEmAll.Helpers.Interfaces
{
    public interface IWallMapping
    {
        MapCell[,] GetMap();

        MapCell GetCellType(int x, int y);
        
        void Store(int x, int y, MapCell value);

        bool IsAlreadyDiscovered(int x, int y);

        void StoreVisibleArea(MapCell[,] visibleArea);

        List<WeightedPoint> GetNeighbourCells(int[] origin, MapCell searchType = MapCell.Empty, MapCell[,] areaToSeach = null, DiagonalCheckType diagonalCheck = DiagonalCheckType.CheckIfReachable, bool includeUnknowns = false);

        List<PointF> GetCrossedWalls(PointF point1, PointF point2);

        List<WeightedPoint> ReachableUnkownList { get; set; }
    }
}
