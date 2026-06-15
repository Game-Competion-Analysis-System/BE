using DAL.DTO;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace DAL.Repository
{
    public class ServerRepository(Swd392GameAiContext context) : IServerRepository
    {
        private readonly Swd392GameAiContext _context = context;

        public List<Server> GetAll(QueryParameters parameters, out int totalCount)
        {
            var query = _context.Servers.Include(s => s.Game).AsQueryable();

            // Search
            if (!string.IsNullOrEmpty(parameters.SearchTerm))
            {
                var search = parameters.SearchTerm.ToLower();
                query = query.Where(s => 
                    (s.Servername != null && s.Servername.ToLower().Contains(search)) || 
                    (s.Region != null && s.Region.ToLower().Contains(search)));
            }

            // Filtering
            if (!string.IsNullOrEmpty(parameters.Filter))
            {
                var filter = parameters.Filter.ToLower();
                query = query.Where(s => s.Status != null && s.Status.ToLower() == filter);
            }

            totalCount = query.Count();

            // Sorting
            if (!string.IsNullOrEmpty(parameters.SortBy))
            {
                switch (parameters.SortBy.ToLower())
                {
                    case "servername":
                        query = parameters.IsDescending ? query.OrderByDescending(s => s.Servername) : query.OrderBy(s => s.Servername);
                        break;
                    default:
                        query = query.OrderBy(s => s.Serverid);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(s => s.Serverid);
            }

            // Paging
            return query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToList();
        }

        public Server? GetById(int id) => _context.Servers.Include(s => s.Game).FirstOrDefault(s => s.Serverid == id);

        public List<Server> GetByGame(int gameId) => _context.Servers.Where(s => s.Gameid == gameId).ToList();

        public List<Server> SearchByName(string name)
        {
            var pattern = $"%{name.Replace(" ", "%").Replace("-", "%")}%";
            return _context.Servers
                .Include(s => s.Game)
                .Where(s => s.Servername != null && EF.Functions.ILike(s.Servername, pattern))
                .ToList();
        }

        public void Add(Server server)
        {
            _context.Servers.Add(server);
            _context.SaveChanges();
        }

        public void Update(Server server)
        {
            _context.Servers.Update(server);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var server = _context.Servers.Find(id);
            if (server != null)
            {
                _context.Servers.Remove(server);
                _context.SaveChanges();
            }
        }
    }
}
