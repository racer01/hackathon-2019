using Hackathon.Public;
using System.Collections.Generic;

namespace KillEmAll.Utility.Interfaces
{
    public interface IPathFinding
    {
        List<PointF> GetCellPath(PointF start, PointF target);
        List<PointF> CellIndexesToPoints(List<PointF> path, float shift);
    }
}
