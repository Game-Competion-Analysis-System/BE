using DAL.DTO;
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
        List<Game> GetAll(QueryParameters parameters, out int totalCount);
        List<Game> GetMMORPG();
        List<Game> SearchByName(string name);
        Game? GetById(int id);
        void Add(Game game);
        void Update(Game game);
        void Delete(int id);
    }
}
