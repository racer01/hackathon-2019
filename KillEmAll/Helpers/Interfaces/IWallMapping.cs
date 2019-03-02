using Hackathon.Public;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KillEmAll.Helpers.Interfaces
{
    public interface IWallMapping
    {
        void Store(int x, int y);

        /// <summary>
        /// Returns the walls above and below the specified block (including the passed block, if that's a wall too)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        PointF[] WallsOnLine(int x, int y);
    }
}
