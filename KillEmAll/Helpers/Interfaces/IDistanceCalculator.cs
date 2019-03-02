using Hackathon.Public;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KillEmAll.Helpers.Interfaces
{
    public interface IDistanceCalculator
    {
        float DistanceBetween(PointF point1, PointF point2);
    }
}
