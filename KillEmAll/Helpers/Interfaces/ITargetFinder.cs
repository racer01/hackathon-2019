using Hackathon.Public;

namespace KillEmAll.Helpers.Interfaces
{
    public interface ITargetFinder
    {
        // ENEMY
        Soldier[] GetVisibleEnemies(Soldier currentSoldier, float fov = 0);

        Soldier GetClosestVisibleEnemy(Soldier currentSoldier, float fov = 0f);

        Soldier GetClosestEnemyOfAll(Soldier currentSoldier);


        // TREASURE
        Treasure[] GetVisibleTreasures(Soldier currentSoldier, float fov = 0f);

        Treasure GetClosestTreasure(Soldier currentSoldier, Treasure[] treasures);

        Treasure GetClosestVisibleTreasure(Soldier currentSoldier, float fov = 0);


        // AMMO
        AmmoBonus[] GetVisibleAmmos(Soldier currentSoldier, float fov = 0f);

        AmmoBonus GetClosestAmmo(Soldier currentSoldier, AmmoBonus[] ammos);

        AmmoBonus GetClosestVisibleAmmo(Soldier currentSoldier, float fov = 0);


        // HEALTH
        HealthBonus[] GetVisibleHealths(Soldier currentSoldier, float fov = 0f);

        HealthBonus GetClosestHealth(Soldier currentSoldier, HealthBonus[] healths);

        HealthBonus GetClosestVisibleHealth(Soldier currentSoldier, float fov = 0);
    }
}
