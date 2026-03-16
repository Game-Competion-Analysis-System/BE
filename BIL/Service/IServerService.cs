using DAL.DTO;
using DAL.Entities;
using System.Collections.Generic;

namespace BIL.Service
{
    public interface IServerService
    {
        PagedResult<ServerDto> GetAll(QueryParameters parameters);
        ServerDto? GetById(int id);
        List<ServerDto> GetByGame(int gameId);
        List<ServerDto> SearchByName(string name);
        void Add(Server server);
        void Update(Server server);
        void Delete(int id);
    }
}
