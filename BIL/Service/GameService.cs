using DAL.DTO;
using DAL.Entities;
using DAL.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIL.Service
{
    public class GameService : IGameService
    {
        private readonly IGameRepository _repo;

        public GameService(IGameRepository repo)
        {
            _repo = repo;
        }

        public List<GameDto> GetAllGames() => _repo.GetAll().Select(g => new GameDto
        {
            GameId = g.Gameid,
            GameName = g.Gamename,
            Genre = g.Genre,
            CompanyName = g.Company?.Companyname
        }).ToList();

        public List<Game> GetMMORPGGames() => _repo.GetMMORPG();

        public List<Game> SearchByName(string name) => _repo.SearchByName(name);

        public void Create(Game game) => _repo.Add(game);
        public Game? GetById(int id) => _repo.GetById(id);
        public void Add(Game game) => _repo.Add(game);
        public void Update(Game game) => _repo.Update(game);
        public void Delete(int id) => _repo.Delete(id);
    }

}
