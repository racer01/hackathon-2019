using Hackathon.Public;

namespace KillEmAll
{
    interface ISoldierMovement
    {
        SoldierCommand MoveToLocation(Soldier currentSoldier, Soldier targetSoldier, TargetType targetType, ref SoldierCommand command);

        SoldierCommand MoveToLocation(Soldier currentSoldier, PointF target, TargetType targetType, ref SoldierCommand command);

        SoldierCommand TurnTowards(Soldier currentSoldier, Soldier targetSoldier, ref SoldierCommand command);
    }
}
