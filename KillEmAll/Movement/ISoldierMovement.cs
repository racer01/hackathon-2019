using Hackathon.Public;
using KillEmAll.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KillEmAll
{
    interface ISoldierMovement
    {
        SoldierCommandExtension MoveToLocation(Soldier currentSoldier, Soldier targetSoldier, TargetType targetType, ref SoldierCommandExtension command);

        SoldierCommandExtension MoveToLocation(Soldier currentSoldier, PointF target, TargetType targetType, ref SoldierCommandExtension command);

        SoldierCommandExtension TurnTowards(Soldier currentSoldier, Soldier targetSoldier, ref SoldierCommandExtension command);
    }
}
