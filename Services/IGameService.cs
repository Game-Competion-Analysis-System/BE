using GameCompetionAnalysisSystem.Models;

namespace GameCompetionAnalysisSystem.Services
{
    public interface IGameService
    {
        IEnumerable<Game> GetAllGames();
        Game? GetById(int id);
        void Add(Game game);
        void Update(Game game);
        void Delete(int id);
        IEnumerable<Game> GetMMORPGGames();
    }
}
