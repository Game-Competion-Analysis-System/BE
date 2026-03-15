using DAL.DTO;
using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BIL.Service
{
    public interface IGameService
    {
        PagedResult<GameDto> GetAllGames(QueryParameters parameters);
        List<Game> GetMMORPGGames();
        List<Game> SearchByName(string name);
        void Create(Game game);
        Game? GetById(int id);
        void Add(Game game);
        void Update(Game game);
        void Delete(int id);
    }

}
