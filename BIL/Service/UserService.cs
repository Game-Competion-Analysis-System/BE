using DAL.DTO;
using DAL.Entities;
using DAL.Repository;
using System.Collections.Generic;

namespace BIL.Service
{
    public class UserService(IUserRepository repo) : IUserService
    {
        public PagedResult<UserDto> GetAll(QueryParameters parameters)
        {
            var users = repo.GetAll(parameters, out int totalCount);
            return new PagedResult<UserDto>
            {
                Items = users.Select(u => new UserDto
                {
                    UserId = u.Userid,
                    Username = u.Username,
                    Email = u.Email,
                    Role = u.Role
                }).ToList(),
                TotalCount = totalCount,
                PageNumber = parameters.PageNumber,
                PageSize = parameters.PageSize
            };
        }
        public UserDto? GetById(int id)
        {
            var u = repo.GetById(id);
            return u == null ? null : new UserDto
            {
                UserId = u.Userid,
                Username = u.Username,
                Email = u.Email,
                Role = u.Role
            };
        }
        public void Update(User user) => repo.Update(user);

        public void UpdateProfile(int userId, UpdateProfileDto dto)
        {
            var user = repo.GetById(userId);
            if (user != null)
            {
                user.Username = dto.Username ?? user.Username;
                user.Email = dto.Email ?? user.Email;
                repo.Update(user);
            }
        }

        public void Delete(int id) => repo.Delete(id);

        public UserDto Create(CreateUserDto dto)
        {
            var user = new User
            {
                Username = dto.Username,
                Email = dto.Email,
                Passwordhash = dto.Password, // In a real app, hash this!
                Role = dto.Role ?? "user"
            };

            repo.Add(user);
            
            return new UserDto
            {
                UserId = user.Userid,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role
            };
        }
    }
}
