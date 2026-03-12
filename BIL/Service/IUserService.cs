using DAL.DTO;
using DAL.Entities;
using System.Collections.Generic;

namespace BIL.Service
{
    public interface IUserService
    {
        List<UserDto> GetAll();
        User? GetById(int id);
        void Update(User user);
        void Delete(int id);
    }
}
