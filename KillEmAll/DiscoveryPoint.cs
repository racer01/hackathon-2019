using Hackathon.Public;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KillEmAll
{
    public class DiscoveryPoint : GameObject
    {
        public DiscoveryPoint(string id, PointF position, float lookAtDirection, float speed, float radius) 
            : base(id, position, lookAtDirection, speed, radius)
        {
        }
    }
}
