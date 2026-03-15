using DAL.DTO;
using DAL.Entities;
using DAL.Repository;
using System.Collections.Generic;

namespace BIL.Service
{
    public class GuildService(IGuildRepository repo) : IGuildService
    {
        public PagedResult<Guild> GetAll(QueryParameters parameters)
        {
            var guilds = repo.GetAll(parameters, out int totalCount);
            return new PagedResult<Guild>
            {
                Items = guilds,
                TotalCount = totalCount,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize
            };
        }
        public Guild? GetById(int id) => repo.GetById(id);
        public List<Guild> GetByServer(int serverId) => repo.GetByServer(serverId);
        public List<Guild> SearchByName(string name) => repo.SearchByName(name);
        public void Add(Guild guild) => repo.Add(guild);
        public void Update(Guild guild) => repo.Update(guild);
        public void Delete(int id) => repo.Delete(id);
    }
}
