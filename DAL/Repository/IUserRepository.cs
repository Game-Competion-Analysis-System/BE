using DAL.Entities;
using System.Collections.Generic;

namespace DAL.Repository
{
    public interface IUserRepository
    {
        List<User> GetAll();
        User? GetById(int id);
        void Update(User user);
        void Delete(int id);
    }
}
