using DAL.Entities;
using System.Collections.Generic;

namespace BIL.Service
{
    public interface IGuildService
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
