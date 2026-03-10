using DAL.Entities;
using DAL.Repository;
using System.Collections.Generic;

namespace BIL.Service
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;

        public UserService(IUserRepository repo)
        {
            _repo = repo;
        }

        public List<User> GetAll() => _repo.GetAll();
        public User? GetById(int id) => _repo.GetById(id);
        public void Update(User user) => _repo.Update(user);
        public void Delete(int id) => _repo.Delete(id);
    }
}
