using DAL.DTO;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace DAL.Repository
{
    public class UserRepository(Swd392GameAiContext context) : IUserRepository
    {
        private readonly Swd392GameAiContext _context = context;

        public List<User> GetAll(QueryParameters parameters, out int totalCount)
        {
            var query = _context.Users.AsQueryable();

            // Search
            if (!string.IsNullOrEmpty(parameters.SearchTerm))
            {
                var search = parameters.SearchTerm.ToLower();
                query = query.Where(u => 
                    (u.Username != null && u.Username.ToLower().Contains(search)) || 
                    (u.Email != null && u.Email.ToLower().Contains(search)));
            }

            // Filtering
            if (!string.IsNullOrEmpty(parameters.Filter))
            {
                var filter = parameters.Filter.ToLower();
                query = query.Where(u => u.Role != null && u.Role.ToLower() == filter);
            }

            totalCount = query.Count();

            // Sorting
            if (!string.IsNullOrEmpty(parameters.SortBy))
            {
                switch (parameters.SortBy.ToLower())
                {
                    case "username":
                        query = parameters.IsDescending ? query.OrderByDescending(u => u.Username) : query.OrderBy(u => u.Username);
                        break;
                    case "email":
                        query = parameters.IsDescending ? query.OrderByDescending(u => u.Email) : query.OrderBy(u => u.Email);
                        break;
                    default:
                        query = query.OrderBy(u => u.Userid);
                        break;
                }
            }
            else
            {
                query = query.OrderBy(u => u.Userid);
            }

            // Paging
            return query
                .Skip((parameters.PageNumber - 1) * parameters.PageSize)
                .Take(parameters.PageSize)
                .ToList();
        }

        public User? GetById(int id) => _context.Users.Find(id);

        public void Update(User user)
        {
            _context.Users.Update(user);
            _context.SaveChanges();
        }

        public void Add(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var user = _context.Users
                .Include(u => u.Imageuploads)
                    .ThenInclude(u => u.Aianalyses)
                        .ThenInclude(a => a.Aiextractedfields)
                .Include(u => u.Imageuploads)
                    .ThenInclude(u => u.Aianalyses)
                        .ThenInclude(a => a.Leaderboards)
                            .ThenInclude(l => l.Leaderboardentries)
                .FirstOrDefault(u => u.Userid == id);

            if (user != null)
            {
                foreach (var upload in user.Imageuploads)
                {
                    foreach (var analysis in upload.Aianalyses)
                    {
                        foreach (var lb in analysis.Leaderboards)
                        {
                            _context.Leaderboardentries.RemoveRange(lb.Leaderboardentries);
                        }
                        _context.Leaderboards.RemoveRange(analysis.Leaderboards);
                        _context.Aiextractedfields.RemoveRange(analysis.Aiextractedfields);
                    }
                    _context.Aianalyses.RemoveRange(upload.Aianalyses);
                }
                _context.Imageuploads.RemoveRange(user.Imageuploads);
                _context.Users.Remove(user);
                _context.SaveChanges();
            }
        }
    }
}
