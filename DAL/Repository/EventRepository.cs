using DAL.DTO;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace DAL.Repository
{
    public class EventRepository : IEventRepository
    {
        private readonly Swd392GameAiContext _context;

        public EventRepository(Swd392GameAiContext context)
        {
            _context = context;
        }

        public List<Event> GetAll() => _context.Events.Include(e => e.Game).ToList();

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
