using Hackathon.Public;
using KillEmAll.Helpers.Interfaces;
using System;
using System.Collections.Generic;
using KillEmAll.Utility;

namespace KillEmAll.Helpers
{
    public class TargetFinder : ITargetFinder
    {
        private readonly IGameStateProvider _gameStateProvider;
        private readonly IWallMapping _wallMapping;
        private readonly MovementUtility _movementUtility;

        public TargetFinder(IGameStateProvider gameStateProvider, IWallMapping wallMapping, MovementUtility movementUtility)
        {
            _gameStateProvider = gameStateProvider;
            _wallMapping = wallMapping;
            _movementUtility = movementUtility;
        }

        public Soldier GetClosestVisibleEnemy(Soldier currentSoldier, float fov = 0)
        {
            var enemies = GetVisibleEnemies(currentSoldier);
            return GetClosestEnemy(currentSoldier, enemies);
        }

        public Soldier GetClosestEnemyOfAll(Soldier currentSoldier)
        {
            var enemies = _gameStateProvider.Get().VisibleEnemies;
            return GetClosestEnemy(currentSoldier, enemies);
        }

        public Soldier GetClosestEnemy(Soldier currentSoldier, Soldier[] enemies)
        {
            Soldier closestSoldier = null;
            var closestDistance = double.MaxValue;
            foreach (var enemy in enemies)
            {
                if (enemy == null)
                    break;

                var distance = _movementUtility.DistanceBetween(currentSoldier.Position, enemy.Position);
                if (distance < closestDistance)
                {
                    closestSoldier = enemy;
                    closestDistance = distance;
                }
            }
            return closestSoldier;
        }

        public Soldier[] GetVisibleEnemies(Soldier currentSoldier, float fov = 0)
        {
            var gameState = _gameStateProvider.Get();
            var visibleEnemies = new Soldier[gameState.MySquad.Length * (gameState.Squads.Length - 1)];

            for (var i = 0; i < gameState.VisibleEnemies.Length; i++)
            {
                var target = gameState.VisibleEnemies[i];
                var walls = _wallMapping.GetCrossedWalls(currentSoldier.Position, target.Position);

                if (walls != null && walls.Count > 0)
                    continue;

                visibleEnemies[i] = target;
            }
            return visibleEnemies;
        }

        public Treasure[] GetVisibleTreasures(Soldier currentSoldier, float fov = 0)
        {
            var gameState = _gameStateProvider.Get();
            var visibleTreasures = new Treasure[gameState.MySquad.Length * (gameState.Squads.Length - 1)];

            for (var i = 0; i < gameState.VisibleTreasures.Length; i++)
            {
                var target = gameState.VisibleTreasures[i];
                var walls = _wallMapping.GetCrossedWalls(currentSoldier.Position, target.Position);

                if (walls != null && walls.Count > 0)
                    continue;

                visibleTreasures[i] = target;
            }
            return visibleTreasures;
        }
    }
}
