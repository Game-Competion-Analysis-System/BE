using DAL.Entities;
using System.Collections.Generic;

namespace DAL.Repository
{
    public interface IGuildRepository
    {
        List<Guild> GetAll();
        Guild? GetById(int id);
        List<Guild> GetByServer(int serverId);
        List<Guild> SearchByName(string name);
        void Add(Guild guild);
        void Update(Guild guild);
        void Delete(int id);
    }
}
