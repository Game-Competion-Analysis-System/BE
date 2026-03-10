using BIL.Service;
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
        public IActionResult GetAll() => Ok(_service.GetAll());

        [HttpGet("profile")]
        public IActionResult GetProfile()
        {
            var userIdStr = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return Unauthorized();

            var user = _service.GetById(userId);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpPut("profile")]
        public IActionResult UpdateProfile([FromBody] User user)
        {
            var userIdStr = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !int.TryParse(userIdStr, out int userId))
                return Unauthorized();

            user.Userid = userId;
            _service.Update(user);
            return Ok(user);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "admin")]
        public IActionResult Delete(int id)
        {
            _service.Delete(id);
            return Ok(new { message = "User deleted successfully" });
        }
    }
}
