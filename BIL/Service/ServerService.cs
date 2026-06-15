using DAL.DTO;
using DAL.Entities;
using DAL.Repository;
using System.Collections.Generic;

namespace BIL.Service
{
    public class ServerService(IServerRepository repo) : IServerService
    {
        public PagedResult<ServerDto> GetAll(QueryParameters parameters)
        {
            var servers = repo.GetAll(parameters, out int totalCount);
            return new PagedResult<ServerDto>
            {
                Items = servers.Select(s => new ServerDto
                {
                    ServerId = s.Serverid,
                    ServerName = s.Servername,
                    Region = s.Region,
                    Status = s.Status,
                    GameName = s.Game?.Gamename
                }).ToList(),
                TotalCount = totalCount,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize
            };
        }
        public ServerDto? GetById(int id)
        {
            var s = repo.GetById(id);
            return s == null ? null : new ServerDto
            {
                ServerId = s.Serverid,
                ServerName = s.Servername,
                Region = s.Region,
                Status = s.Status,
                GameName = s.Game?.Gamename
            };
        }
        public List<ServerDto> GetByGame(int gameId) => repo.GetByGame(gameId).Select(s => new ServerDto
        {
            ServerId = s.Serverid,
            ServerName = s.Servername,
            Region = s.Region,
            Status = s.Status,
            GameName = s.Game?.Gamename
        }).ToList();
        public List<ServerDto> SearchByName(string name) => repo.SearchByName(name).Select(s => new ServerDto
        {
            ServerId = s.Serverid,
            ServerName = s.Servername,
            Region = s.Region,
            Status = s.Status,
            GameName = s.Game?.Gamename
        }).ToList();
        public void Add(Server server) => repo.Add(server);
        public void Update(Server server) => repo.Update(server);
        public void Delete(int id) => repo.Delete(id);
    }
}
