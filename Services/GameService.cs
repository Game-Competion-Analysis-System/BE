using GameCompetionAnalysisSystem.Models;

namespace GameCompetionAnalysisSystem.Services
{
    public class GameService : IGameService
    {
        private readonly List<Game> _store = new();

        public IEnumerable<Game> GetAllGames() => _store;

        public Game? GetById(int id) => _store.FirstOrDefault(g => g.Gameid == id);

        public void Add(Game game)
        {
            game.Gameid = _store.Count > 0 ? _store.Max(g => g.Gameid) + 1 : 1;
            _store.Add(game);
        }

        public void Update(Game game)
        {
            var existing = GetById(game.Gameid);
            if (existing is null) return;
            existing.Name = game.Name;
            existing.Genre = game.Genre;
            existing.Publisher = game.Publisher;
        }

        public void Delete(int id) => _store.RemoveAll(g => g.Gameid == id);

        public IEnumerable<Game> GetMMORPGGames() =>
            _store.Where(g => g.Genre?.Equals("MMORPG", StringComparison.OrdinalIgnoreCase) == true);
    }
}
