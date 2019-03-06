using Hackathon.Public;
using System.Collections.Generic;

namespace KillEmAll.Utility.Interfaces
{
    public interface IPathFinding
    {
        List<PointF> GetPath(PointF start, PointF target);
    }
}
