using Hackathon.Public;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.Composition;
using System;

namespace KillEmAll
{
    [Export(typeof(ISquadLogic))]
    [ExportMetadata("SquadName", "trainer")]
    [ExportMetadata("SquadImageResource", "KillEmAll.Resources.Face.png")]
    public class KillEmAllSquadLogicCopy : ISquadLogic
    {
        public void Initialize(string squadId, GameOptions options)
        {
        }

        public IEnumerable<SoldierCommand> Update(GameState state)
        {
            var chosenOne = state.VisibleEnemies.FirstOrDefault();

            return state.MySquad.Select(s =>
            {
                var command = new SoldierCommand() { Soldier = s };

                command.MoveForward = true;
                command.RotateRight = true;
                return command;
            });
        }
    }
}
