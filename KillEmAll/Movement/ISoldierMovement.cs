using Hackathon.Public;

namespace KillEmAll
{
    interface ISoldierMovement
    {
        SoldierCommand MoveToLocation(Soldier currentSoldier, PointF targetPoint, float virtualRadius, ref SoldierCommand command);

        SoldierCommand MoveToObject(Soldier currentSoldier, GameObject targetObject, TargetType targetType, ref SoldierCommand command);
    }
}
