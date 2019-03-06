using Hackathon.Public;
using KillEmAll.Helpers.Interfaces;
using System;
using System.Collections.Generic;
using KillEmAll.Utility;

namespace KillEmAll.Helpers
{
    public class TargetFinder : ITargetFinder
    {
        private IGameStateProvider _gameStateProvider;
        private IWallMapping _wallMapping;
        private MovementUtility _movementUtility;

        public TargetFinder(IGameStateProvider gameStateProvider, IWallMapping wallMapping, MovementUtility movementUtility)
        {
            _gameStateProvider = gameStateProvider;
            _wallMapping = wallMapping;
            _movementUtility = movementUtility;
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
            double closestDistance = 999999999;
            for (var i = 0; i < enemies.Length; i++)
            {
                if (enemies[i] == null)
                    break;

                var distance = _movementUtility.DistanceBetween(currentSoldier.Position, enemies[i].Position);
                if (distance < closestDistance)
                {
                    closestSoldier = enemies[i];
                    closestDistance = distance;
                }
            }
            return closestSoldier;
        }

        public Soldier[] GetVisibleEnemies(Soldier currentSoldier, float fov = 0)
        {
            var gameState = _gameStateProvider.Get();

            var allVisibleEnemies = gameState.VisibleEnemies;

            var currentX = (int)currentSoldier.Position.X;
            var currentY = (int)currentSoldier.Position.Y;

            var visibleEnemies = new Soldier[gameState.MySquad.Length * (gameState.Squads.Length - 1)];
            List<PointF> walls = null;

            for (var i = 0; i < gameState.VisibleEnemies.Length; i++)
            {
                var target = gameState.VisibleEnemies[i];
                walls = _wallMapping.GetCrossedWalls(currentSoldier.Position, target.Position);

                if (walls != null && walls.Count > 0)
                    continue;

                visibleEnemies[i] = target;
            }
            return visibleEnemies;
        }

        public Treasure[] GetVisibleTreasures(Soldier currentSoldier, float fov = 0)
        {
            throw new NotImplementedException();
        }
    }
}
