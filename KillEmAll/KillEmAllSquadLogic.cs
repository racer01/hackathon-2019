using Hackathon.Public;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.Composition;
using System;
using KillEmAll.Extensions;
using KillEmAll.Helpers.Interfaces;
using KillEmAll.Helpers;
using KillEmAll.Utility.Interfaces;
using KillEmAll.Utility;

namespace KillEmAll
{
    [Export(typeof(ISquadLogic))]
    [ExportMetadata("SquadName", "KillEmwut2L")]
    [ExportMetadata("SquadImageResource", "KillEmAll.Resources.Face.png")]
    public class KillEmAllSquadLogic : ISquadLogic
    {
        private Soldier _chosenOne;
        private Random _randomGen;

        private ISoldierMovement _soldierMovement;
        private ITargetFinder _targetFinder;
        private IGameStateProvider _stateProvider;
        private IWallMapping _wallMapping;
        private IUtilityCache _utilityCache;
        private IPointUtility _pointUtility;

        public void Initialize(string squadId, GameOptions options)
        {
            _randomGen = new Random();
            _utilityCache = new UtilityCache();
            _pointUtility = new PointUtility(_utilityCache);
            _soldierMovement = new SoldierMovement(_pointUtility);
            _stateProvider = new GameStateProvider();
            _wallMapping = new WallMapping(options.MapSize.Width, options.MapSize.Height);
            _targetFinder = new TargetFinder(_stateProvider, _wallMapping, _pointUtility);
        }

        //TODO: 
        // 1. set the NEW gamestate on each update in the provider
        // 2. empty and store all visible walls
        // 3. empty cache on each update
        // 
        public IEnumerable<SoldierCommand> Update(GameState state)
        {
            _utilityCache.Clear();
            // update gamestate
            _stateProvider.Set(state);

            // update walls
            //rip performance :(
            for (var i = 0; i < state.VisibleArea.GetLength(0); i++)
            {
                for (var j = 0; j < state.VisibleArea.GetLength(1); j++)
                {
                    var current = state.VisibleArea[i, j];
                    if (current == MapCell.Wall)
                        _wallMapping.Store(i, j);
                }
            }

            return state.MySquad.Select(s => {
                var command = new SoldierCommandExtension() { Soldier = s };

                var target = _targetFinder.GetClosestEnemy(s);

                if (target == null)
                {
                    //Console.WriteLine($"||||||||||||||| CANT SEE ENEMIES (Game Time: {state.TotalGameTime}) ||||||||||||||");
                    command.MoveForward = true;
                    return command;
                }

                return _soldierMovement.MoveToLocationLEGACY(s, target, ref command);
            });
        }
    }
}
