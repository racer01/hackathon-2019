using Hackathon.Public;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KillEmAll.Extensions
{
    public class SoldierExtension : Soldier
    {
        // Maybe each enemy soldier should receive a priority level. This could be used to focus certain important targets.
        public int PriorityLevel { get; set; }

        public SoldierExtension(string id, string squadId, PointF position, float radius, float lookAtDirection, float speed, double timeTillCanShoot, int health, int ammo) 
            : base(id, squadId, position, radius, lookAtDirection, speed, timeTillCanShoot, health, ammo)
        {
        }
    }
}
