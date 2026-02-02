using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository
{
    public interface IGameRepository
    {
        List<Game> GetAll();
        List<Game> GetMMORPG();
        Game? GetById(int id);
        void Add(Game game);
        void Update(Game game);
        void Delete(int id);
    }
}
