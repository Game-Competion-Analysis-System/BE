using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace DAL.Repository
{
    public class GuildRepository : IGuildRepository
    {
        private readonly Swd392GameAiContext _context;

        public GuildRepository(Swd392GameAiContext context)
        {
            _context = context;
        }

        public List<Guild> GetAll() => _context.Guilds.Include(g => g.Server).ToList();

        public Guild? GetById(int id) => _context.Guilds.Include(g => g.Server).FirstOrDefault(g => g.Guildid == id);

        public List<Guild> GetByServer(int serverId) => _context.Guilds.Where(g => g.Serverid == serverId).ToList();

        public void Add(Guild guild)
        {
            _context.Guilds.Add(guild);
            _context.SaveChanges();
        }

        public void Update(Guild guild)
        {
            _context.Guilds.Update(guild);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var guild = _context.Guilds.Find(id);
            if (guild != null)
            {
                _context.Guilds.Remove(guild);
                _context.SaveChanges();
            }
        }
    }
}
