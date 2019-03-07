using Hackathon.Public;

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
