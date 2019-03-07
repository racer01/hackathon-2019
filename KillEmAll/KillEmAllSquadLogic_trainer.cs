using Hackathon.Public;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.Composition;
using System;
using KillEmAll.Helpers.Interfaces;
using KillEmAll.Utility;
using KillEmAll.Utility.Interfaces;
using KillEmAll.Helpers;
using System.Diagnostics;

namespace KillEmAll
{
    [Export(typeof(ISquadLogic))]
    [ExportMetadata("SquadName", "trainer")]
    [ExportMetadata("SquadImageResource", "KillEmAll.Resources.Face.png")]
    public class KillEmAllSquadLogicCopy : ISquadLogic
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
        private SoldierPathMapping _pathMapping;
        private bool DISABLE_TRAINER = true;

        public void Initialize(string squadId, GameOptions options)
        {
            _cellSize = 1.0f;
            _stopWatch = new Stopwatch();
            _randomGen = new Random();
            _movementUtility = new MovementUtility();
            _soldierMovement = new SoldierMovement(_movementUtility);
            _stateProvider = new GameStateProvider();
            _wallMapping = new WallMapping(options.MapSize.Width, options.MapSize.Height);
            _targetFinder = new TargetFinder(_stateProvider, _wallMapping, _movementUtility, options);
            _pathFinding = new PathFinding(_wallMapping);
            _pathMapping = new SoldierPathMapping();
        }

        public IEnumerable<Hackathon.Public.SoldierCommand> Update(GameState state)
        {
            if (DISABLE_TRAINER)
                return state.MySquad.Select(s => new SoldierCommand() { Soldier = s });
            

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
                    if (_pathMapping.PathExists(soldier))
                        _pathMapping.RemovePath(soldier);

                    if (target is Soldier)
                        return _soldierMovement.TargetEnemy(soldier, target, ref command);
                    return _soldierMovement.MoveToObject(soldier, target, ref command);
                }

                // SELECT THE NEXT STEP, IGNORE THE TARGET IF WE HAVE A PATH STORED ALREADY FOR THE CURRENT SOLDIER
                var nextCell = GetNextStepTowardsTarget(soldier, target);


                if (IsTargetReached(soldier.Position, nextCell))
                    return command;

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
            //ENEMY
            GameObject target = _targetFinder.GetClosestVisibleEnemy(soldier);
            if (target != null)
                return target;

            target = _targetFinder.GetClosestEnemyOfAll(soldier);
            if (target != null)
                return target;
            //


            //TREASURE
            target = _targetFinder.GetClosestVisibleTreasure(soldier);
            if (target != null)
                return target;

            target = _targetFinder.GetClosestTreasure(soldier, state.VisibleTreasures);
            if (target != null)
                return target;
            //


            //AMMO
            target = _targetFinder.GetClosestVisibleAmmo(soldier);
            if (target != null)
                return target;

            target = _targetFinder.GetClosestAmmo(soldier, state.VisibleAmmoBonuses);
            if (target != null)
                return target;
            //

            //HEALTH
            target = _targetFinder.GetClosestVisibleHealth(soldier);
            if (target != null)
                return target;

            return state.VisibleHealthBonuses.FirstOrDefault();
        }

        private PointF SelectNextTargetFromPath(Soldier soldier, List<PointF> path)
        {
            PointF nextCell = null;
            if (path.Count > 1)
            {
                nextCell = path.ElementAt(path.Count - 2);
                path.RemoveAt(path.Count - 2);
            }
            else if (path.Count == 1)
            {
                nextCell = path.First();
            }
            return nextCell;
        }

        private bool IsTargetReached(PointF soldierPos, PointF targetPos)
        {
            //SOLDIER
            var roundedX1 = Math.Round(soldierPos.X, 1);
            var roundedY1 = Math.Round(soldierPos.Y, 1);

            //TARGET
            var roundedX2 = Math.Round(targetPos.X, 1);
            var roundedY2 = Math.Round(targetPos.Y, 1);

            return roundedX1 == roundedX2 && roundedY1 == roundedY2;
        }

        private PointF GetNextStepTowardsTarget(Soldier soldier, GameObject target)
        {
            List<PointF> path;
            if (_pathMapping.PathExists(soldier))
                path = _pathMapping.GetPath(soldier);
            else
                path = _pathFinding.GetCellPath(soldier.Position, target.Position);

            if (path.Count == 0)
                return null;


            path = _pathFinding.CellIndexesToPoints(path, _cellSize / 2);
            return SelectNextTargetFromPath(soldier, path);
        }
    }
}
