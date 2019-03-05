using Hackathon.Public;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KillEmAll.Helpers.Interfaces
{
    public interface ITargetFinder
    {
        /// <summary>
        /// Returns an array of Treasures visible to the current soldier.
        /// </summary>
        /// <param name="currentSoldier">The reference soldier.</param>
        /// <param name="fov">
        ///     Optional fov, meaning only treasures that are in the soldiers 'field of view' will be returned.
        ///     This should help in prioritizing treasures that won't require much turning to reach (it's very slow to completely turn around).
        /// </param>
        /// <returns>Array of treasures. Empty array if no treasures are visible.</returns>
        Treasure[] GetVisibleTreasures(Soldier currentSoldier, float fov = 0f);

        /// <summary>
        /// Returns an array of Soldiers visible to the current soldier.
        /// </summary>
        /// <param name="currentSoldier">The reference soldier.</param>
        ///  /// <param name="fov">
        ///     Optional fov, meaning only enemies that are in the soldiers 'field of view' will be returned.
        ///     This should help in prioritizing enemies that won't require much turning to reach (it's very slow to completely turn around).
        /// </param>
        /// <returns>Array of soldiers. Empty array if no soldiers are visible.</returns>
        Soldier[] GetVisibleEnemies(Soldier currentSoldier, float fov = 0);

        /// <summary>
        /// Returns closest enemy to the current soldier.
        /// </summary>
        /// <param name="currentSoldier">Reference soldier.</param>
        /// <returns>Soldier object or null if no enemies are visible.</returns>
        Soldier GetClosestVisibleEnemy(Soldier currentSoldier, float fov = 0f);

        Soldier GetClosestEnemyOfAll(Soldier currentSoldier);
    }
}
