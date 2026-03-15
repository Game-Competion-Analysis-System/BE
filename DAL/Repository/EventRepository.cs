using DAL.DTO;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace DAL.Repository
{
    public class EventRepository(Swd392GameAiContext context) : IEventRepository
    {
        private readonly Swd392GameAiContext _context = context;

        public List<Event> GetAll(QueryParameters parameters, out int totalCount)
        {
            var query = _context.Events.Include(e => e.Game).AsQueryable();

            // Search
            if (!string.IsNullOrEmpty(parameters.SearchTerm))
            {
                var search = parameters.SearchTerm.ToLower();
                query = query.Where(e => 
                    (e.Eventname != null && e.Eventname.ToLower().Contains(search)) || 
                    (e.Eventtype != null && e.Eventtype.ToLower().Contains(search)));
            }

            // Filtering
            if (!string.IsNullOrEmpty(parameters.Filter))
            {
                var filter = parameters.Filter.ToLower();
                query = query.Where(e => e.Eventtype != null && e.Eventtype.ToLower() == filter);
            }

            totalCount = query.Count();

            // Sorting
            if (!string.IsNullOrEmpty(parameters.SortBy))
            {
                switch (parameters.SortBy.ToLower())
                {
                    case "eventname":
                        query = parameters.IsDescending ? query.OrderByDescending(e => e.Eventname) : query.OrderBy(e => e.Eventname);
                        break;
                    case "startdate":
                        query = parameters.IsDescending ? query.OrderByDescending(e => e.Startdate) : query.OrderBy(e => e.Startdate);
                        break;
                    default:
                        query = query.OrderBy(e => e.Eventid);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(e => e.Eventid);
            }

            // Paging
            return query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToList();
        }

        public Event? GetById(int id) => _context.Events.Include(e => e.Game).FirstOrDefault(e => e.Eventid == id);

        public List<Event> GetByGame(int gameId) => _context.Events.Where(e => e.Gameid == gameId).ToList();

        public List<Event> SearchByName(string name)
        {
            var pattern = $"%{name.Replace(" ", "%").Replace("-", "%")}%";
            return _context.Events
                .Include(e => e.Game)
                .Where(e => e.Eventname != null && EF.Functions.ILike(e.Eventname, pattern))
                .ToList();
        }

        public void Add(Event @event)
        {
            _context.Events.Add(@event);
            _context.SaveChanges();
        }

        public void Update(Event @event)
        {
            _context.Events.Update(@event);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var @event = _context.Events.Find(id);
            if (@event != null)
            {
                _context.Events.Remove(@event);
                _context.SaveChanges();
            }
        }
    }
}
