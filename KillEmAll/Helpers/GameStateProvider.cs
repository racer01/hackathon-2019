using Hackathon.Public;
using KillEmAll.Helpers.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KillEmAll.Helpers
{
    public class GameStateProvider : IGameStateProvider
    {
        private GameState _state;

        public GameState Get()
        {
            return _state;
        }

        public void Set(GameState state)
        {
            _state = state;
        }
    }
}
