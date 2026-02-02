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
        List<Game> GetAllGames();
        List<Game> GetMMORPGGames();
        void Create(Game game);
        Game? GetById(int id);
        void Add(Game game);
        void Update(Game game);
        void Delete(int id);
    }

}
