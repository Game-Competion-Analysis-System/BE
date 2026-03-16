using DAL.DTO;
using DAL.Entities;
using System.Collections.Generic;

namespace BIL.Service
{
    public interface IGuildService
    {
        PagedResult<GuildDto> GetAll(QueryParameters parameters);
        GuildDto? GetById(int id);
        List<GuildDto> GetByServer(int serverId);
        List<GuildDto> SearchByName(string name);
        void Add(Guild guild);
        void Update(Guild guild);
        void Delete(int id);
    }
}
