using Hackathon.Public;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.Composition;
using System;
using KillEmAll.Helpers.Interfaces;
using KillEmAll.Helpers;
using KillEmAll.Utility.Interfaces;
using KillEmAll.Utility;
using WallMapping = KillEmAll.Helpers.WallMapping;

namespace KillEmAll
{
    [Export(typeof(ISquadLogic))]
    [ExportMetadata("SquadName", "ThisIsFine")]
    [ExportMetadata("SquadImageResource", "KillEmAll.Resources.ThisIsFine.png")]
    public class KillEmAllSquadLogic : ISquadLogic
    {
        private Random _randomGen;

        private ISoldierMovement _soldierMovement;
        private ITargetFinder _targetFinder;
        private IGameStateProvider _stateProvider;
        private IWallMapping _wallMapping;
        private MovementUtility _movementUtility;
        private IPathFinding _pathFinding;
        private float _cellSize;
        private SoldierPathMapping _pathMapping;
        private List<DiscoveryPoint> _emptyCells;

        public void Initialize(string squadId, GameOptions options)
        {
            _cellSize = 1.0f;
            _randomGen = new Random();
            _movementUtility = new MovementUtility();
            _soldierMovement = new SoldierMovement(_movementUtility);
            _stateProvider = new GameStateProvider();
            _wallMapping = new WallMapping(options.MapSize.Width, options.MapSize.Height);
            _targetFinder = new TargetFinder(_stateProvider, _wallMapping, _movementUtility, options);
            _pathFinding = new PathFinding(_wallMapping);
            _pathMapping = new SoldierPathMapping();
            _emptyCells = new List<DiscoveryPoint>();
        }

        public IEnumerable<Hackathon.Public.SoldierCommand> Update(GameState state)
        {
            _stateProvider.Set(state);

            // update our kown map
            _wallMapping.StoreVisibleArea(state.VisibleArea);

            var commands = state.MySquad.Select(soldier =>
            {
                var command = new SoldierCommand() { Soldier = soldier };
                var target = SelectTarget(soldier, state);

                if (target == null)
                    return command;

                if (IsTargetVisibleForCurrentSoldier(soldier, target))
                {
                    GetCommandForVisibleTarget(soldier, target, ref command);
                    return command;
                }

                GetCommandForNotVisibleTarget(soldier, target, ref command);
                return command;
            });
            _targetFinder.ClearTargetMapping();
            return commands;
        }

        private bool IsTargetVisibleForCurrentSoldier(Soldier soldier, GameObject target)
        {
            return _wallMapping.GetCrossedWalls(soldier.Position, target.Position).Count == 0;
        }

        private void GetCommandForNotVisibleTarget(Soldier soldier, GameObject target, ref SoldierCommand command)
        {
            // SELECT THE NEXT STEP, IGNORE THE TARGET IF WE HAVE A PATH STORED ALREADY FOR THE CURRENT SOLDIER
            var nextCell = GetNextStepTowardsTarget(soldier, target);

            if (nextCell == null)
                return;

            if (target is DiscoveryPoint)
            {
                // it's enough to roughly be in the same cell when discovering
                // -> there's no reason to move to the exact target location
                // -> stop and remove discovery point 
                // -> next update we'll get a new discovery point
                if (IsSoldierInSameCellAsTarget(soldier.Position, target.Position))
                {
                    _wallMapping.ReachableUnkownList.RemoveAt(_wallMapping.ReachableUnkownList.Count - 1);
                    return;
                }
            }
            _soldierMovement.MoveToLocation(soldier, nextCell, 0.2f, ref command);
        }

        private void GetCommandForVisibleTarget(Soldier soldier, GameObject target, ref SoldierCommand command)
        {
            if (_pathMapping.PathExists(soldier))
                _pathMapping.RemovePath(soldier);

            if (target is Soldier)
                _soldierMovement.TargetEnemy(soldier, target, ref command);
            else
                _soldierMovement.MoveToObject(soldier, target, ref command);
        }

        private bool CanMoveToNextTarget(PointF soldierPos, PointF currentTargetPos, PointF nextTargetPos)
        {
            currentTargetPos = new PointF((int)currentTargetPos.X, (int)currentTargetPos.Y);
            nextTargetPos = new PointF((int)nextTargetPos.X, (int)nextTargetPos.Y);

            var betweenOnX = false;
            if (currentTargetPos.X <= nextTargetPos.X)
                betweenOnX = soldierPos.X >= currentTargetPos.X && soldierPos.X <= nextTargetPos.X;
            else
                betweenOnX = soldierPos.X < currentTargetPos.X && soldierPos.X > nextTargetPos.X;

            var betweenOnY = false;
            if (currentTargetPos.Y <= nextTargetPos.Y)
                betweenOnY = soldierPos.Y >= currentTargetPos.Y && soldierPos.Y <= nextTargetPos.Y;
            else
                betweenOnY = soldierPos.Y <= currentTargetPos.Y && soldierPos.Y >= nextTargetPos.Y;

            return betweenOnX && betweenOnY;
        }

        private bool IsSoldierInSameCellAsTarget(PointF soldierPos, PointF targetPos, float margin = 0.01f)
        {
            var nextCellX = (int)targetPos.X;
            var nextCellY = (int)targetPos.Y;

            var lowerXLimit = nextCellX + margin;
            var upperXLimit = nextCellX + 1 - margin;

            var lowerYLimit = nextCellY + margin;
            var upperYLimit = nextCellY + 1 - margin;

            return lowerXLimit < soldierPos.X && upperXLimit > soldierPos.X && soldierPos.Y < upperYLimit && soldierPos.Y > lowerYLimit;
        }

        // TODO: smart priorization, also include discovering
        private GameObject SelectTarget(Soldier soldier, GameState state)
        {
            GameObject target = _targetFinder.GetClosestVisibleEnemy(soldier);
            if (target != null)
                return target;

            target = _targetFinder.GetClosestEnemyOfAll(soldier);
            if (target != null)
                return target;

            target = _targetFinder.GetClosestVisibleTreasure(soldier);
            if (target != null)
                return target;

            target = _targetFinder.GetClosestTreasure(soldier, state.VisibleTreasures);
            if (target != null)
                return target;

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

            target = _targetFinder.GetClosestHealth(soldier, state.VisibleHealthBonuses);
            if (target != null)
                return target;


            //DISCOVER
            var discoverTarget = _wallMapping.ReachableUnkownList.LastOrDefault();

            if (discoverTarget != null)
                return new DiscoveryPoint(Guid.NewGuid().ToString(), new PointF(discoverTarget.X, discoverTarget.Y), 0, 0, 0);

            // IF WE DISCOVERED THE WHOLE MAP AND STILL CANT SEE ANYTHING
            return PickRandomEmptySpotOnMap();
        }

        private DiscoveryPoint PickRandomEmptySpotOnMap()
        {
            if (_emptyCells.Count == 0)
            {
                var map = _wallMapping.GetMap();
                for (var i = 0; i < map.GetLength(0); i++)
                {
                    for (var j = 0; j < map.GetLength(1); j++)
                    {
                        if (map[i, j] == MapCell.Empty)
                            _emptyCells.Add(new DiscoveryPoint(Guid.NewGuid().ToString(), new PointF(i, j), 0, 0, 0));
                    }
                }
            }
            var index = _randomGen.Next(0, _emptyCells.Count);

            return _emptyCells[index];
        }

        private PointF SelectNextTargetFromPath(Soldier soldier, List<PointF> path)
        {
            PointF nextCell = null;
            if (path.Count > 1)
            {
                // TAKE THE LAST CELL FROM THE PATH, THATS THE NEXT TARGET
                nextCell = path.ElementAt(path.Count - 1);
                path.RemoveAt(path.Count - 1);
                _pathMapping.UpdatePath(soldier, path);
            }
            else
            {
                _pathMapping.RemovePath(soldier);
            }
            return nextCell;
        }

        private bool IsTargetReached(PointF soldierPos, PointF targetPos, int roundingPrecision)
        {
            //SOLDIER
            var roundedX1 = Math.Round(soldierPos.X, roundingPrecision);
            var roundedY1 = Math.Round(soldierPos.Y, roundingPrecision);

            //TARGET
            var roundedX2 = Math.Round(targetPos.X, roundingPrecision);
            var roundedY2 = Math.Round(targetPos.Y, roundingPrecision);

            return roundedX1 == roundedX2 && roundedY1 == roundedY2;
        }

        private PointF GetNextStepTowardsTarget(Soldier soldier, GameObject target)
        {
            Path path = null;
            if (_pathMapping.PathExists(soldier) && _pathMapping.GetPath(soldier)?.Route.Count > 0)
            {
                path = _pathMapping.GetPath(soldier);

                // IF OUR LAST TARGET WANST A SOLDER
                // AND OUR CURRENT TARGET IS A SOLDER
                // WE NEED TO STOP FOLLOWIGN THE CURRENT TARGET
                // AND GO TO THE SOLDIER
                if (target is Soldier && !(path.TargetType == typeof(Soldier)))
                {
                    path = _pathMapping.GetPath(soldier);
                }
            }
            else
            {
                var rotue = _pathFinding.GetCellPath(soldier.Position, target.Position);
                // LAST INDEX IS ALWAYS OUR CURRENT POSITION, DONT NEED IT
                rotue?.RemoveAt(rotue.Count - 1);

                // ONLY SHIFT CELL INDEXES FOR NEW PATHS
                // IF THE PATH WAS RETRIEVED FROM PATHMAPPING, IT'S VALUES WILL ALREADY BE SHIFTED
                rotue = _pathFinding.CellIndexesToPoints(rotue, _cellSize / 2);

                path = new Path() { Route = rotue, TargetType = target.GetType() };
                // IF WE STORED DISCOVERY POINT PATH, THEN THE SOLDIER WOULDN'T DO ANYTHING ELSE UNTIL HE REACHES THAT POINT
                // DONT SOCRE THE PATH -> NEXT UPDATE IF THE SOLDIER SEES SOMETHING, HE WILL MOVE TOWARDS THAT POINT INSTEAD
                if (!(target is DiscoveryPoint))
                {
                    _pathMapping.StoreNewPath(soldier, path);
                }
            }

            if (path == null || path.Route?.Count == 0)
                return null;

            //var furthestVisibleIndex = GetFurthestVisiblePointOnPath(soldier, path);

            //if (!(furthestVisibleIndex < 0 || furthestVisibleIndex >= path.Route.Count))
            //{
            //    path.Route.RemoveRange(furthestVisibleIndex, path.Route.Count - 1 - furthestVisibleIndex);
            //    _pathMapping.UpdatePath(soldier, path.Route);
            //}

            if (!IsSoldierInSameCellAsTarget(soldier.Position, path.Route.LastOrDefault()))
                return path.Route.LastOrDefault();

            return SelectNextTargetFromPath(soldier, path.Route);
        }

        private int GetFurthestVisiblePointOnPath(Soldier soldier, Path path)
        {
            PointF current;
            for (var i = path.Route.Count - 1; i >= 0; i--)
            {
                current = path.Route[i];
                if (_wallMapping.GetCrossedWalls(soldier.Position, current)?.Count > 0)
                    return i + 1;
            }
            return 1;

        }
    }
}
