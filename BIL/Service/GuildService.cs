using DAL.DTO;
using DAL.Entities;
using DAL.Repository;
using System.Collections.Generic;

namespace BIL.Service
{
    public class GuildService(IGuildRepository repo) : IGuildService
    {
        public PagedResult<GuildDto> GetAll(QueryParameters parameters)
        {
            var guilds = repo.GetAll(parameters, out int totalCount);
            return new PagedResult<GuildDto>
            {
                Items = guilds.Select(g => new GuildDto
                {
                    GuildId = g.Guildid,
                    GuildName = g.Guildname,
                    ServerName = g.Server?.Servername
                }).ToList(),
                TotalCount = totalCount,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize
            };
        }
        public GuildDto? GetById(int id)
        {
            var g = repo.GetById(id);
            return g == null ? null : new GuildDto
            {
                GuildId = g.Guildid,
                GuildName = g.Guildname,
                ServerName = g.Server?.Servername
            };
        }
        public List<GuildDto> GetByServer(int serverId) => repo.GetByServer(serverId).Select(g => new GuildDto
        {
            GuildId = g.Guildid,
            GuildName = g.Guildname,
            ServerName = g.Server?.Servername
        }).ToList();
        public List<GuildDto> SearchByName(string name) => repo.SearchByName(name).Select(g => new GuildDto
        {
            GuildId = g.Guildid,
            GuildName = g.Guildname,
            ServerName = g.Server?.Servername
        }).ToList();
        public void Add(Guild guild) => repo.Add(guild);
        public void Update(Guild guild) => repo.Update(guild);
        public void Delete(int id) => repo.Delete(id);
    }
}
