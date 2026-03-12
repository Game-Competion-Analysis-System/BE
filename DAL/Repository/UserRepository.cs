using DAL.DTO;
using DAL.Entities;
using System.Collections.Generic;
using System.Linq;

namespace DAL.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly Swd392GameAiContext _context;

        public UserRepository(Swd392GameAiContext context)
        {
            _context = context;
        }

        public List<User> GetAll() => _context.Users.ToList();

        public User? GetById(int id) => _context.Users.Find(id);

        public void Update(User user)
        {
            _context.Users.Update(user);
            _context.SaveChanges();
        }

        public void Delete(int id)
        {
            var user = _context.Users.Find(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }
        }
    }
}
