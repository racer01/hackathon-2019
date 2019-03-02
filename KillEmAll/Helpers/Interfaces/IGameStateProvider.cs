using Hackathon.Public;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KillEmAll.Helpers.Interfaces
{
    public interface IGameStateProvider
    {
        GameState Get();

        void Set(GameState state);
    }
}
