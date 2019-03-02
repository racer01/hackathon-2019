using Hackathon.Public;
using KillEmAll.Helpers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KillEmAll.Helpers
{
    public class DistanceCalculator : IDistanceCalculator
    {
        public float DistanceBetween(PointF point1, PointF point2)
        {
            return (point1.X - point2.X) * (point1.X - point2.X) + (point1.Y - point2.Y) * (point1.Y - point2.Y);
        }
    }
}
