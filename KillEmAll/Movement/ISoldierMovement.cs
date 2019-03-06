using Hackathon.Public;

namespace KillEmAll
{
    interface ISoldierMovement
    {
        SoldierCommand MoveToLocation(Soldier currentSoldier, PointF targetPoint, float virtualRadius, ref SoldierCommand command);

        SoldierCommand MoveToObject(Soldier currentSoldier, GameObject targetObject, ref SoldierCommand command);

        SoldierCommand TargetEnemy(Soldier currentSoldier, GameObject enemy, ref SoldierCommand command);
    }
}
