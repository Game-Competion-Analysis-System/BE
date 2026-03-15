using DAL.DTO;
using DAL.Entities;
using System.Collections.Generic;

namespace BIL.Service
{
    public interface IUserService
    {
        PagedResult<UserDto> GetAll(QueryParameters parameters);
        User? GetById(int id);
        void Update(User user);
        void UpdateProfile(int userId, UpdateProfileDto dto);
        void Delete(int id);
    }
}
