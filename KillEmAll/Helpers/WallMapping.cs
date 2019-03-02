using Hackathon.Public;
using KillEmAll.Helpers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KillEmAll.Helpers
{
    public class WallMapping : IWallMapping
    {
        private const int WALL_SIZE = 1;
        private const int WALL_CHECK_RANGE = 3;

        private bool[,] _walls;
        private static PointF[] _blockingWalls = new PointF[WALL_CHECK_RANGE];

        public WallMapping(int mapSizeX, int mapSizeY)
        {
            _walls = new bool[mapSizeX + 1, mapSizeY + 1];
        }

        public void Store(int x, int y)
        {
            if (x >= _walls.GetLength(0) || x < 0 || y >= _walls.GetLength(1) || y < 0)
                return;

            _walls[x, y] = true;
        }

        private void ClearBlockedWalls()
        {
            for (var i = 0; i < _blockingWalls.Length; i++)
                _blockingWalls[i] = null;
        }

        public PointF[] WallsOnLine(int x, int y)
        {
            if (x >= _walls.GetLength(0) || x < 0 || y >= _walls.GetLength(1) || y < 0)
                return null;

            ClearBlockedWalls();

            var above = y + WALL_SIZE;
            var below = y - WALL_SIZE;

            var foundWalls = 0;
            if (_walls[x, y])
            {
                _blockingWalls[0] = new PointF(x, y);
                foundWalls++;
            }
            if (above < _walls.GetLength(1) && _walls[x, above])
            {
                _blockingWalls[1] = new PointF(x, above);
                foundWalls++;
            }
            if (below >= 0 && _walls[x, below])
            {
                _blockingWalls[2] = new PointF(x, below);
                foundWalls++;
            }

            if (foundWalls == 0)
                return null;

            return _blockingWalls;
        }
    }
}
