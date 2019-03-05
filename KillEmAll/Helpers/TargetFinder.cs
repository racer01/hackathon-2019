using Hackathon.Public;
using KillEmAll.Enums;
using KillEmAll.Helpers.Interfaces;
using KillEmAll.Utility;
using KillEmAll.Utility.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KillEmAll.Helpers
{
    public class TargetFinder : ITargetFinder
    {
        private IGameStateProvider _gameStateProvider;
        private IWallMapping _wallMapping;
        private IPointUtility _pointUtility;

        public TargetFinder(IGameStateProvider gameStateProvider, IWallMapping wallMapping, IPointUtility pointUtility)
        {
            _gameStateProvider = gameStateProvider;
            _wallMapping = wallMapping;
            _pointUtility = pointUtility;
        }

        public Soldier GetClosestVisibleEnemy(Soldier currentSoldier, float fov = 0)
        {
            var enemies = GetVisibleEnemies(currentSoldier);

            return GetClosest(currentSoldier, enemies);
        }

        public Soldier GetClosestEnemyOfAll(Soldier currentSoldier)
        {
            var enemies = _gameStateProvider.Get().VisibleEnemies;

            return GetClosest(currentSoldier, enemies);
        }

        private Soldier GetClosest(Soldier currentSoldier, Soldier[] enemies)
        {
            Soldier closestSoldier = null;
            float closestDistance = 999999999f;
            for (var i = 0; i < enemies.Length; i++)
            {
                if (enemies[i] == null)
                    break;

                var distance = _pointUtility.DistanceBetween(currentSoldier.Position, enemies[i].Position);
                if (distance < closestDistance)
                {
                    closestSoldier = enemies[i];
                    closestDistance = distance;
                }
            }
            return closestSoldier;
        }

        // TODO: implement FOV
        public Soldier[] GetVisibleEnemies(Soldier currentSoldier, float fov = 0)
        {
            var gameState = _gameStateProvider.Get();

            var allVisibleEnemies = gameState.VisibleEnemies;

            var currentX = (int)currentSoldier.Position.X;
            var currentY = (int)currentSoldier.Position.Y;

            var visibleEnemies = new Soldier[gameState.MySquad.Length * (gameState.Squads.Length - 1)];
            PointF[] walls = null;

            // iterate through each visible enemy, checking if a wall breaks line of sight to our soldier
            // this might be too slow
            for (var i = 0; i < gameState.VisibleEnemies.Length; i++)
            {
                var target = gameState.VisibleEnemies[i];
                var isOutOfSight = false;
                walls = GetWallsBetweenPoints(currentSoldier.Position, target.Position);
                if (walls == null)
                {
                    visibleEnemies[i] = target;
                    continue;
                }

                for (var j = 0; j < walls.Length; j++)
                {
                    if (walls[j] == null)
                        break;

                    if (_pointUtility.IsInBetween(currentSoldier.Position, target.Position, walls[j]))
                    {
                        // a wall is in between our solder and it's target -> target is not visible by current soldier
                        //Console.WriteLine($"||||||||| WALL IS BLOCKING TARGET (WALL({i}, {j}))|||||||||||");
                        isOutOfSight = true;
                        break;
                    }
                }
                if (!isOutOfSight)
                {
                    // if no walls are blocking the target it should be visible by the current soldier
                    visibleEnemies[i] = target;
                }
            }
            return visibleEnemies;
        }

        private PointF[] GetWallsBetweenPoints(PointF soldier, PointF target)
        {
            // REFACTOR: unnecessarily rounds down the same number multiple times
            var otherPoint = new PointF((int)target.X, (int)target.Y);
            var referencePoint = new PointF((int)soldier.X, (int)soldier.Y);

            var relativePosition = _pointUtility.GetRelativePosition(referencePoint, 0, otherPoint, 0);
            var steps = RoundedDistanceAlongAxis(soldier, target, Axis.X);

            var directionAlongX = 0;
            var directionAlongY = 0;

            if (relativePosition.HasFlag(RelativePosition.ABOVE))
                directionAlongY = 1;
            else if (relativePosition.HasFlag(RelativePosition.BELOW))
                directionAlongY = -1;

            if (relativePosition.HasFlag(RelativePosition.RIGHT))
                directionAlongX = 1;
            else if (relativePosition.HasFlag(RelativePosition.LEFT))
                directionAlongX = -1;
            
            if (relativePosition.HasFlag(RelativePosition.SAME))
                return null;

            PointF[] blockingWalls = new PointF[3 * (steps + 1)];
            PointF[] currentWalls = new PointF[3];

            var y = (int)soldier.Y;
            var x = (int)soldier.X;

            int wallCount = 0;
            for (var step = 0; step <= steps; step++)
            {
                currentWalls = _wallMapping.WallsOnLine(x, y);
                if (currentWalls != null)
                {
                    for (var i = 0; i < currentWalls.Length; i++)
                    {
                        if (currentWalls[i] != null)
                        {
                            blockingWalls[wallCount++] = currentWalls[i];
                        }
                    }
                }
                y += directionAlongY;
                x += directionAlongX;
            }

            if (wallCount == 0)
                return null;

            return blockingWalls;
        }

        private int RoundedDistanceAlongAxis(PointF point1, PointF point2, Axis axis)
        {
            if (axis == Axis.X)
                return Math.Abs((int)point1.X - (int)point2.X);
            return Math.Abs((int)point1.Y - (int)point2.Y);
        }

        public Treasure[] GetVisibleTreasures(Soldier currentSoldier, float fov = 0)
        {
            throw new NotImplementedException();
        }
    }
}
