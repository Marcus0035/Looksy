using Looksy.Models;
using Looksy.Models.DTOs;
using Looksy.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Looksy.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly LooksyDbContext _context;
        private const string cNotFoundMessage = "User not found";
        private readonly UsersService _usersService;
        public UsersController(LooksyDbContext context, UsersService usersService)
        {
            _context = context;
            _usersService = usersService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var userProfile = await _usersService.GetUserByIdAsync(userId);
            if (userProfile == null)
                return NotFound("User not found");

            return Ok(userProfile);
        }

        [HttpGet("groups")]
        [Authorize]
        public async Task<IActionResult> GetMyGroups()
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized(); 

            var groups = await _context.Groups
                .Where(g => g.Members.Any(m => m.Id == userId))
                .Select(g => new
                {
                    g.Name,
                    MemberCount = g.Members.Count,
                    PhotoCount = g.Photos.Count
                })
                .ToListAsync();

            return Ok(groups);
        }

        [HttpPost]
        public async Task<IActionResult> PostProfile([FromBody] UserCreateDto user)
        {
            if (user == null)
                return BadRequest("User data is required");
            if (string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Email))
                return BadRequest("Username and Email are required");
            if (_context.Users.Any(u => u.Username == user.Username))
                return Conflict("Username already exists");
            if (_context.Users.Any(u => u.Email == user.Email))
                return Conflict("Email already exists");

            var newUser = new User(user);
            newUser.PasswordHash = new PasswordHasher<User>().HashPassword(newUser, user.Password);

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProfile), new { id = newUser.Id }, user);
        }

        [HttpPut]
        [Route("{userId}")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(int userId, [FromBody] UserCreateDto userDto)
        {
            if (userDto == null)
                return BadRequest("User data is required");
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound(cNotFoundMessage);

            user.FirstName = userDto.FirstName;
            user.LastName = userDto.LastName;
            user.Username = userDto.Username;
            user.Email = userDto.Email;

            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return NoContent();
        }


        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized("Invalid token");

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound(cNotFoundMessage);

            var hasher = new PasswordHasher<User>();
            var result = hasher.VerifyHashedPassword(user, user.PasswordHash, dto.CurrentPassword);
            if (result != PasswordVerificationResult.Success)
                return BadRequest("Wrong password");

            user.PasswordHash = hasher.HashPassword(user, dto.NewPassword);
            await _context.SaveChangesAsync();

            return Ok("Password was changed");
        }


        [HttpDelete]
        [Route("{userId}")]
        [Authorize]
        public async Task<IActionResult> DeleteProfile(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound(cNotFoundMessage);

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
