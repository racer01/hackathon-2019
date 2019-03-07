using Hackathon.Public;
using KillEmAll.Enums;

namespace KillEmAll.Helpers.Interfaces
{
    public interface IMovementUtility
    {
        Directions GetDirection(double currentAngle, double targetAngle);
        
        double GetTargetAngle(GameObject me, PointF targetPos);

        double GetAngleTolerance(GameObject me, GameObject notMe);

        double DistanceBetween(PointF point1, PointF point2);
    }
}
