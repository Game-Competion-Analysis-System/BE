using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace DAL.Repository
{
    public class ServerRepository : IServerRepository
    {
        private readonly Swd392GameAiContext _context;

        public ServerRepository(Swd392GameAiContext context)
        {
            _context = context;
        }

        public List<Server> GetAll() => _context.Servers.Include(s => s.Game).ToList();

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
