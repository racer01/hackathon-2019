using Hackathon.Public;
using KillEmAll.Helpers.Interfaces;
using System;
using System.Collections.Generic;
using KillEmAll.Utility;
using System.Linq;

namespace KillEmAll.Helpers
{
    public class TargetFinder : ITargetFinder
    {
        private readonly IGameStateProvider _gameStateProvider;
        private readonly IWallMapping _wallMapping;
        private readonly MovementUtility _movementUtility;
        private readonly GameOptions _gameOptions;

        public TargetFinder(IGameStateProvider gameStateProvider, IWallMapping wallMapping, MovementUtility movementUtility, GameOptions gameOptions)
        {
            _gameStateProvider = gameStateProvider;
            _wallMapping = wallMapping;
            _movementUtility = movementUtility;
            _gameOptions = gameOptions;
        }

        /////////
        // ENEMY
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
            var visibleEnemies = new Soldier[_gameOptions.SquadSize * (_gameOptions.SquadCount  - 1)];
            var storeIndex = 0;

            for (var i = 0; i < gameState.VisibleEnemies.Length; i++)
            {
                var target = gameState.VisibleEnemies[i];
                var walls = _wallMapping.GetCrossedWalls(currentSoldier.Position, target.Position);

                if (walls != null && walls.Count > 0)
                    continue;

                visibleEnemies[storeIndex++] = target;
            }
            return visibleEnemies;
        }


        ////////////
        // TREASURE
        public Treasure GetClosestVisibleTreasure(Soldier currentSoldier, List<string> chosenTreasures, float fov = 0)
        {
            var treasures = GetVisibleTreasures(currentSoldier);
            return GetClosestTreasure(currentSoldier, chosenTreasures, treasures);
        }

        public Treasure GetClosestTreasureOfAll(Soldier currentSoldier, List<string> chosenTreasures, float fov = 0)
        {
            var treasures = _gameStateProvider.Get().VisibleTreasures;
            return GetClosestTreasure(currentSoldier, chosenTreasures, treasures);
        }

        public Treasure GetClosestTreasure(Soldier currentSoldier, List<string> chosenTreasures, Treasure[] treasures)
        {
            Treasure closestTreasure = null;
            Treasure[] filtered = treasures;

            var closestDistance = double.MaxValue;

            foreach (var treasure in filtered)
            {
                if (treasure == null)
                    break;

                var distance = _movementUtility.DistanceBetween(currentSoldier.Position, treasure.Position);
                if (distance < closestDistance)
                {
                    closestTreasure = treasure;
                    closestDistance = distance;
                }
            }
            return closestTreasure;
        }

        public Treasure[] GetVisibleTreasures(Soldier currentSoldier, float fov = 0)
        {
            var gameState = _gameStateProvider.Get();
            var visibleTreasures = new Treasure[gameState.VisibleTreasures.Length];
            var storeIndex = 0;

            for (var i = 0; i < gameState.VisibleTreasures.Length; i++)
            {
                var target = gameState.VisibleTreasures[i];
                var walls = _wallMapping.GetCrossedWalls(currentSoldier.Position, target.Position);

                if (walls != null && walls.Count > 0)
                    continue;

                visibleTreasures[storeIndex++] = target;
            }
            return visibleTreasures;
        }


        ///////
        // AMMO
        public AmmoBonus GetClosestVisibleAmmo(Soldier currentSoldier, float fov = 0)
        {
            var ammos = GetVisibleAmmos(currentSoldier);
            return GetClosestAmmo(currentSoldier, ammos);
        }

        public AmmoBonus GetClosestAmmoOfAll(Soldier currentSoldier, float fov = 0)
        {
            var ammos = _gameStateProvider.Get().VisibleAmmoBonuses;
            return GetClosestAmmo(currentSoldier, ammos);
        }

        public AmmoBonus GetClosestAmmo(Soldier currentSoldier, AmmoBonus[] ammos)
        {
            AmmoBonus closestAmmo = null;
            var closestDistance = double.MaxValue;
            foreach (var ammo in ammos)
            {
                if (ammo == null)
                    break;

                var distance = _movementUtility.DistanceBetween(currentSoldier.Position, ammo.Position);
                if (distance < closestDistance)
                {
                    closestAmmo = ammo;
                    closestDistance = distance;
                }
            }
            return closestAmmo;
        }

        public AmmoBonus[] GetVisibleAmmos(Soldier currentSoldier, float fov = 0)
        {
            var gameState = _gameStateProvider.Get();
            var visibleAmmos = new AmmoBonus[gameState.VisibleAmmoBonuses.Length];
            var storeIndex = 0;

            for (var i = 0; i < gameState.VisibleAmmoBonuses.Length; i++)
            {
                var target = gameState.VisibleAmmoBonuses[i];
                var walls = _wallMapping.GetCrossedWalls(currentSoldier.Position, target.Position);

                if (walls != null && walls.Count > 0)
                    continue;

                visibleAmmos[storeIndex++] = target;
            }
            return visibleAmmos;
        }


        //////////
        // HEALTH
        public HealthBonus GetClosestVisibleHealth(Soldier currentSoldier, float fov = 0)
        {
            var healths = GetVisibleHealths(currentSoldier);
            return GetClosestHealth(currentSoldier, healths);
        }

        public HealthBonus GetClosestHealthOfAll(Soldier currentSoldier, float fov = 0)
        {
            var healths = _gameStateProvider.Get().VisibleHealthBonuses;
            return GetClosestHealth(currentSoldier, healths);
        }

        public HealthBonus GetClosestHealth(Soldier currentSoldier, HealthBonus[] healths)
        {
            HealthBonus closestHealth = null;
            var closestDistance = double.MaxValue;
            foreach (var health in healths)
            {
                if (health == null)
                    break;

                var distance = _movementUtility.DistanceBetween(currentSoldier.Position, health.Position);
                if (distance < closestDistance)
                {
                    closestHealth = health;
                    closestDistance = distance;
                }
            }
            return closestHealth;
        }

        public HealthBonus[] GetVisibleHealths(Soldier currentSoldier, float fov = 0)
        {
            var gameState = _gameStateProvider.Get();
            var visibleHealths = new HealthBonus[gameState.VisibleHealthBonuses.Length];
            var storeIndex = 0;

            for (var i = 0; i < gameState.VisibleHealthBonuses.Length; i++)
            {
                var target = gameState.VisibleHealthBonuses[i];
                var walls = _wallMapping.GetCrossedWalls(currentSoldier.Position, target.Position);

                if (walls != null && walls.Count > 0)
                    continue;

                visibleHealths[storeIndex++] = target;
            }
            return visibleHealths;
        }
    }
}
