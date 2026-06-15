using BIL.Service;
using DAL.DTO;
using DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;

namespace GameCompetionAnalysisSystem.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController(IUserService service) : ControllerBase
    {
        private readonly IUserService _service = service;

        [HttpGet]
        [Authorize(Roles = "admin")]
        public IActionResult GetList([FromQuery] QueryParameters parameters) => Ok(_service.GetAll(parameters));

        [HttpPost]
        [Authorize(Roles = "admin")]
        public IActionResult Create([FromBody] CreateUserDto userDto)
        {
            return Ok(_service.Create(userDto));
        }

        [HttpGet("profile")]
        public IActionResult GetProfile()
        {
            var userIdStr = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return Unauthorized();

            var userDto = _service.GetById(userId);
            if (userDto == null) return NotFound();
            return Ok(userDto);
        }

        [HttpPut("profile")]
        public IActionResult UpdateProfile([FromBody] UpdateProfileDto userDto)
        {
            var userIdStr = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return Unauthorized();

            _service.UpdateProfile(userId, userDto);
            return Ok(new { message = "Profile updated successfully" });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public IActionResult Delete(int id)
        {
            _service.Delete(id);
            return Ok(new { message = "Delete successful" });
        }
    }
}
