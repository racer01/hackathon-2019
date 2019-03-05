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
        private IUtilityCache _utilityCache;
        private IPointUtility _pointUtility;
        private IPathFinding _pathFinding;

        public void Initialize(string squadId, GameOptions options)
        {
            _stopWatch = new Stopwatch();
            _randomGen = new Random();
            _utilityCache = new UtilityCache();
            _pointUtility = new PointUtility(_utilityCache);
            _soldierMovement = new SoldierMovement(_pointUtility);
            _stateProvider = new GameStateProvider();
            _wallMapping = new WallMapping(options.MapSize.Width, options.MapSize.Height);
            _targetFinder = new TargetFinder(_stateProvider, _wallMapping, _pointUtility);
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

                ////var target = _targetFinder.GetClosestVisibleEnemy(s);
                ////if (target != null)
                ////    return _soldierMovement.MoveToLocation(s, target, TargetType.ENEMY, ref command);


                //var pathToTake = new List<PointF>();
                //var target = _targetFinder.GetClosestEnemyOfAll(s);
                //if (target != null)
                //{
                //    pathToTake = _pathFinding.Asd(s.Position, target.Position);
                //    if (pathToTake.Count != 0)
                //    {
                //        pathToTake[0] = target.Position;
                //        var nextTarget = pathToTake.Count > 1 ? pathToTake.ElementAt(pathToTake.Count - 2) : pathToTake.First();
                //        return _soldierMovement.MoveToLocation(s, nextTarget, TargetType.OBJECT, ref command);
                //    }
                //}

                //if (_wallMapping.ReachableUnknown == null)
                //    return command;

                //var unknownTarget = new PointF(_wallMapping.ReachableUnknown[0], _wallMapping.ReachableUnknown[1]);
                //pathToTake = _pathFinding.Asd(s.Position, unknownTarget);
                //if (pathToTake.Count == 0)
                //    return command;

                //pathToTake[0] = unknownTarget;

                //var nextPoint = pathToTake.Count > 1 ? pathToTake.ElementAt(pathToTake.Count - 2) : pathToTake.First();

                //if (((int)s.Position.X) == nextPoint.X && ((int)s.Position.Y) == nextPoint.Y)
                //{
                //    command.RotateRight = true;
                //    return command;
                //}

                //return _soldierMovement.MoveToLocation(s, nextPoint, TargetType.OBJECT, ref command);
            });

            _stopWatch.Stop();
            Console.Clear();
            Console.WriteLine($"Update complated in: {_stopWatch.Elapsed}");
            _stopWatch.Reset();

            return commands;
        }

        private SoldierCommand Explore(Soldier s, SoldierCommand command)
        {
            if (_wallMapping.ReachableUnknowns.Count == 0)
                return command;

            //HACK
            _wallMapping.ReachableUnknowns.RemoveAll(ru =>
            {
                var cellType = _wallMapping.GetCellType(ru[0], ru[1]);
                if (cellType != MapCell.Unknown)
                    return true;
                return false;
            });
            //END HACK

            var u = _wallMapping.ReachableUnknowns.First();

            var unknownTarget = new PointF(u[0], u[1]);
            var pathToTake = _pathFinding.Asd(s.Position, unknownTarget);
            if (pathToTake.Count == 0)
            {
                _wallMapping.ReachableUnknowns.RemoveAll(r => r.SequenceEqual(new int[] { (int)unknownTarget.X, (int)unknownTarget.Y }));
                return command;
            }
                

            pathToTake[0] = unknownTarget;

            var nextPoint = pathToTake.Count > 1 ? pathToTake.ElementAt(pathToTake.Count - 2) : pathToTake.First();

            if (((int)s.Position.X) == (int)unknownTarget.X && ((int)s.Position.Y) == (int)unknownTarget.Y)
            {

                _wallMapping.ReachableUnknowns.RemoveAll(r => r.SequenceEqual(new int[] { (int)unknownTarget.X, (int)unknownTarget.Y }));
                return command;
            }

            return _soldierMovement.MoveToLocation(s, nextPoint, TargetType.OBJECT, ref command);
        }
    }
}
