using System;
using System.Collections.Generic;
using System.Linq;
using Hackathon.Public;

namespace KillEmAll.momsspaghetti
{
    public class WallMapping
    {
        private const int WALL_SIZE = 1;

        private bool[,] _wallMap;

        public WallMapping(int mapSizeX, int mapSizeY)
        {
            _wallMap = new bool[mapSizeX, mapSizeY];
        }

        public void Store(int x, int y)
        {
            if (x >= _wallMap.GetLength(0) || x < 0 || y >= _wallMap.GetLength(1) || y < 0)
                return;
            _wallMap[x, y] = true;
        }

       
    }
}