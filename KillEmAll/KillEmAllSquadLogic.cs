using System;
using Hackathon.Public;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.Composition;

namespace KillEmAll
{
    [Export(typeof(ISquadLogic))]
    [ExportMetadata("SquadName", "KillEmAll")]
    [ExportMetadata("SquadImageResource", "KillEmAll.Resources.Face.png")]
    public class KillEmAllSquadLogic : ISquadLogic
    {
        private bool _shoot = true;
        public void Initialize(string squadId, GameOptions options)
        {
        }

        public IEnumerable<SoldierCommand> Update(GameState state)
        {
            var mySoldiers = state.MySquad;
            if (_shoot)
            {
                _shoot = false;
                return mySoldiers.Select(s => new SoldierCommand() {Soldier = s, Shoot = true, MoveForward = true});
            }

            Console.ReadKey();
            return mySoldiers.Select(s => new SoldierCommand() {Soldier = s});
        }
    }
}
