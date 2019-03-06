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
        private float _cellSize;

        public void Initialize(string squadId, GameOptions options)
        {
            _cellSize = 1.0f;
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

            var commands = state.MySquad.Select(soldier => {
                var command = new SoldierCommand() { Soldier = soldier };
                var target = SelectTarget(soldier, state);

                if (target == null)
                    return command;

                if (_wallMapping.GetCrossedWalls(soldier.Position, target.Position).Count == 0)
                {
                    return _soldierMovement.MoveToObject(soldier, target, ref command);
                }

                var path = _pathFinding.GetCellPath(soldier.Position, target.Position);
                path = _pathFinding.CellIndexesToPoints(path, _cellSize / 2);

                if (path.Count == 0)
                    return command;

                path[0] = target.Position;
                var nextCell = path.Count > 1 ? path.ElementAt(path.Count - 2) : path.First();
                return _soldierMovement.MoveToLocation(soldier, nextCell, 0.1f, ref command);
            });

            _stopWatch.Stop();
            Console.Clear();
            Console.WriteLine($"Update complated in: {_stopWatch.Elapsed}");
            _stopWatch.Reset();

            return commands;
        }

        // TODO: smart priorization, also include discovering
        private GameObject SelectTarget(Soldier soldier, GameState state)
        {
            GameObject target = _targetFinder.GetClosestVisibleEnemy(soldier);
            if (target != null)
                return target;

            target = state.VisibleTreasures.FirstOrDefault();
            if (target != null)
                return target;

            target = state.VisibleAmmoBonuses.FirstOrDefault();
            if (target != null)
                return target;

            return state.VisibleHealthBonuses.FirstOrDefault();
        }
    }
}
