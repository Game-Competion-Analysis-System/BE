using DAL.DTO;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository
{
    public class GameRepository : IGameRepository
    {
        private readonly Swd392GameAiContext _context;

        public GameRepository(Swd392GameAiContext context)
        {
            _context = context;
        }

        public List<Game> GetAll()
        {
            return _context.Games
                .Include(g => g.Company)
                .ToList();
        }


        public List<Game> GetMMORPG()
            => _context.Games
                .Where(g => g.Genre != null && g.Genre.Contains("MMORPG"))
                .ToList();

        public List<Game> SearchByName(string name)
        {
            var pattern = $"%{name.Replace(" ", "%").Replace("-", "%")}%";
            return _context.Games
                .Where(g => g.Gamename != null && EF.Functions.ILike(g.Gamename, pattern))
                .ToList();
        }

        public Game? GetById(int id)
        {
            return _context.Games
                .Include(g => g.Company)
                .Include(g => g.Events)
                .Include(g => g.Players)
                .FirstOrDefault(g => g.Gameid == id);
        }
        public void Add(Game game)
        {
            _context.Games.Add(game);
            _context.SaveChanges();
        }

        public void Update(Game game)
        {
            _context.Games.Update(game);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var game = _context.Games.Find(id);
            if (game != null)
            {
                _context.Games.Remove(game);
                _context.SaveChanges();
            }
        }

    }
}