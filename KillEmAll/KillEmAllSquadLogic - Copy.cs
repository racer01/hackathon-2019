using Hackathon.Public;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.Composition;
using System;
using KillEmAll.Extensions;

namespace KillEmAll
{
    [Export(typeof(ISquadLogic))]
    [ExportMetadata("SquadName", "copy")]
    [ExportMetadata("SquadImageResource", "KillEmAll.Resources.Face.png")]
    public class KillEmAllSquadLogicCopy : ISquadLogic
    {
        private Soldier _chosenOne;
        private Random _randomGen;

        //private ISoldierMovement _soldierMovement;

        public void Initialize(string squadId, GameOptions options)
        {
            _randomGen = new Random();
            //_soldierMovement = new SoldierMovement();
        }

        public IEnumerable<SoldierCommand> Update(GameState state)
        {
            _chosenOne = state.VisibleEnemies.FirstOrDefault();

            return state.MySquad.Select(s => {
                var command = new SoldierCommandExtension() { Soldier = s };

                command.MoveForward = true;
                command.RotateRight = true;
                return command;
                //return _soldierMovement.MoveToLocationLEGACY(s, _chosenOne, ref command);
            });
        }
    }
}
