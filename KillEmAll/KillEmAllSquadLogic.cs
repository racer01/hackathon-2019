using Hackathon.Public;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.Composition;
using System;
using KillEmAll.Helpers.Interfaces;
using KillEmAll.Helpers;
using KillEmAll.Utility.Interfaces;
using KillEmAll.Utility;
using System.Diagnostics;
using KillEmAll.momsspaghetti;
using WallMapping = KillEmAll.Helpers.WallMapping;

namespace KillEmAll
{
    [Export(typeof(ISquadLogic))]
    [ExportMetadata("SquadName", "MLSlayer")]
    [ExportMetadata("SquadImageResource", "KillEmAll.Resources.Face.png")]
    public class KillEmAllSquadLogic : ISquadLogic
    {
        private Random _randomGen;
        private Stopwatch _stopWatch;

        private ISoldierMovement _soldierMovement;
        private ITargetFinder _targetFinder;
        private IGameStateProvider _stateProvider;
        private IWallMapping _wallMapping;
        private MovementUtility _movementUtility;
        private IPathFinding _pathFinding;

        public void Initialize(string squadId, GameOptions options)
        {
            _stopWatch = new Stopwatch();
            _randomGen = new Random();
            _movementUtility = new MovementUtility();
            _soldierMovement = new SoldierMovement(_movementUtility);
            _stateProvider = new GameStateProvider();
            _wallMapping = new WallMapping(options.MapSize.Width, options.MapSize.Height);
            _targetFinder = new TargetFinder(_stateProvider, _wallMapping, _movementUtility);
            _pathFinding = new PathFinding(_wallMapping);
        }

        public IEnumerable<Hackathon.Public.SoldierCommand> Update(GameState state)
        {
            _stopWatch.Start();
            _stateProvider.Set(state);

            // update our kown map
            _wallMapping.StoreVisibleArea(state.VisibleArea);

            var commands = state.MySquad.Select(s => {
                var command = new SoldierCommand() { Soldier = s };

                GameObject target = state.VisibleTreasures.FirstOrDefault();

                if (target == null)
                    target = state.VisibleAmmoBonuses.FirstOrDefault();

                if (target == null)
                    target = state.VisibleHealthBonuses.FirstOrDefault();

                if (target == null)
                    return command;

                var pathToTreasure = _pathFinding.Asd(s.Position, target.Position);

                if (pathToTreasure.Count == 0)
                    return command;

                pathToTreasure[0] = target.Position;

                var nextPoint = pathToTreasure.Count > 1 ? pathToTreasure.ElementAt(pathToTreasure.Count - 2) : pathToTreasure.First();

                return _soldierMovement.MoveToLocation(s, nextPoint, TargetType.OBJECT, ref command);
            });

            _stopWatch.Stop();
            Console.Clear();
            Console.WriteLine($"Update complated in: {_stopWatch.Elapsed}");
            _stopWatch.Reset();

            return commands;
        }
    }
}
