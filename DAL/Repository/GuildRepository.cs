using DAL.DTO;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace DAL.Repository
{
    public class GuildRepository(Swd392GameAiContext context) : IGuildRepository
    {
        private readonly Swd392GameAiContext _context = context;

        public List<Guild> GetAll(QueryParameters parameters, out int totalCount)
        {
            var query = _context.Guilds.Include(g => g.Server).AsQueryable();

            // Search
            if (!string.IsNullOrEmpty(parameters.SearchTerm))
            {
                var search = parameters.SearchTerm.ToLower();
                query = query.Where(g => g.Guildname != null && g.Guildname.ToLower().Contains(search));
            }

            // Filtering
            if (!string.IsNullOrEmpty(parameters.Filter) && int.TryParse(parameters.Filter, out int serverId))
            {
                query = query.Where(g => g.Serverid == serverId);
            }

            totalCount = query.Count();

            // Sorting
            if (!string.IsNullOrEmpty(parameters.SortBy))
            {
                switch (parameters.SortBy.ToLower())
                {
                    case "guildname":
                        query = parameters.IsDescending ? query.OrderByDescending(g => g.Guildname) : query.OrderBy(g => g.Guildname);
                        break;
                    default:
                        query = query.OrderBy(g => g.Guildid);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(g => g.Guildid);
            }

            // Paging
            return query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToList();
        }

        public Guild? GetById(int id) => _context.Guilds.Include(g => g.Server).FirstOrDefault(g => g.Guildid == id);

        public List<Guild> GetByServer(int serverId) => _context.Guilds.Where(g => g.Serverid == serverId).ToList();

        public List<Guild> SearchByName(string name)
        {
            var pattern = $"%{name.Replace(" ", "%").Replace("-", "%")}%";
            return _context.Guilds
                .Include(g => g.Server)
                .Where(g => g.Guildname != null && EF.Functions.ILike(g.Guildname, pattern))
                .ToList();
        }

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
