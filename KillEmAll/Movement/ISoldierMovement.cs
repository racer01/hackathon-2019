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
        SoldierCommandExtension MoveToLocationLEGACY(Soldier currentSoldier, Soldier targetSoldier, ref SoldierCommandExtension command);

        SoldierCommandExtension MoveToTarget(Soldier currentSoldier, Soldier targetSoldier, ref SoldierCommandExtension command);

        SoldierCommandExtension TurnTowards(Soldier currentSoldier, Soldier targetSoldier, ref SoldierCommandExtension command);
    }
}
