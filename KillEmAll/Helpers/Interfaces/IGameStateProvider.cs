using Hackathon.Public;

namespace KillEmAll.Helpers.Interfaces
{
    public interface IGameStateProvider
    {
        GameState Get();

        void Set(GameState state);
    }
}
