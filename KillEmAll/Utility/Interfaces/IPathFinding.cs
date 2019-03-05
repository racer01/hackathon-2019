using Hackathon.Public;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KillEmAll.Utility.Interfaces
{
    public interface IPathFinding
    {
        bool FindShortestPathToTarget(WeightedPoint current, int[] target, Dictionary<WeightedPoint, WeightedPoint> closedDict, Dictionary<WeightedPoint, WeightedPoint> openDict, List<WeightedPoint> neighborsOrderedByScore, ref List<PointF> path);

        List<PointF> Asd(PointF start, PointF target);

    }
}
