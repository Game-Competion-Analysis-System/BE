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
    public class GameService(IGameRepository repo) : IGameService
    {
        public PagedResult<GameDto> GetAllGames(QueryParameters parameters)
        {
            var games = repo.GetAll(parameters, out int totalCount);
            return new PagedResult<GameDto>
            {
                Items = games.Select(g => new GameDto
                {
                    GameId = g.Gameid,
                    GameName = g.Gamename,
                    Genre = g.Genre,
                    CompanyName = g.Company?.Companyname
                }).ToList(),
                TotalCount = totalCount,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize
            };
        }

        public List<Game> GetMMORPGGames() => repo.GetMMORPG();

        public List<Game> SearchByName(string name) => repo.SearchByName(name);

        public void Create(Game game) => repo.Add(game);
        public Game? GetById(int id) => repo.GetById(id);
        public void Add(Game game) => repo.Add(game);
        public void Update(Game game) => repo.Update(game);
        public void Delete(int id) => repo.Delete(id);
    }
}
