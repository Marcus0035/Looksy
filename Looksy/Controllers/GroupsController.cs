using Looksy.Models;
using Looksy.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Looksy.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroupsController : ControllerBase
    {
        #region Const And Readonly Fields
        private readonly LooksyDbContext _context;
        #endregion

        #region Properties

        #endregion

        #region Constructors
        public GroupsController(LooksyDbContext context)
        {
            _context = context;
        }
        #endregion

        #region Private

        #endregion

        #region Public
        [HttpGet]
        [Route("{userId}")]
        [Authorize]
        public async Task<IActionResult> GetGroupsByUserId(int userId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var id))
                return Unauthorized();

            if (userId != id)
                return Unauthorized();

            var user = await _context.Users.Include(x => x.Groups).FirstOrDefaultAsync(x => x.Id == userId);

            if (user == null)
            {
                return NotFound("User not found");
            }

            var groups = user.Groups.Select(g => new
            {
                g.Id,
                g.Name,
                g.CreatedAt
            });

            return Ok(groups);
        }

        [HttpGet]
        [Route("{groupId}/latest-photo")]
        [Authorize]
        public async Task<IActionResult> GetLatestPhoto(int groupId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized("Invalid user ID");

            var group = await _context.Groups
                .Include(g => g.Photos)
                .ThenInclude(x => x.UploadedBy)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group == null)
                return NotFound("Group not found");

            var latestPhoto = group.Photos.OrderByDescending(p => p.UploadedAt).FirstOrDefault();

            if (latestPhoto == null)
                return NotFound("No photos found in this group");

            return Ok(new
            {
                latestPhoto.Id,
                latestPhoto.Url,
                latestPhoto.UploadedAt,
                UploadedBy = latestPhoto.UploadedBy.Username
            });
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddGroup(GroupCreateDto groupCreateDto)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized("Invalid user ID");

            var user = await _context.Users.FindAsync(userId);

            if (user == null)
                return NotFound("User not found");

            var group = new Group
            {
                Name = groupCreateDto.Name
            };

            foreach (var memberId in groupCreateDto.MemberIds)
            {
                var member = await _context.Users.FindAsync(memberId);
                if (member != null)
                {
                    group.Members.Add(member);
                }
            }

            _context.Groups.Add(group);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetGroupsByUserId), new { userId }, null);
        }

        [HttpPut]
        [Route("{groupId}")]
        [Authorize]
        public async Task<IActionResult> ChangeMembers(int groupId, [FromBody] List<int> memberIds)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized("Invalid user ID");

            var group = await _context.Groups.Include(g => g.Members).FirstOrDefaultAsync(g => g.Id == groupId);
            if (group == null)
                return NotFound("Group not found");

            group.Members.Clear();

            foreach (var memberId in memberIds)
            {
                var member = await _context.Users.FindAsync(memberId);
                if (member != null)
                {
                    group.Members.Add(member);
                }
            }

            await _context.SaveChangesAsync();

            return Ok("Users was sucesfully changed");
        }

        [HttpDelete]
        [Route("{groupId}")]
        [Authorize]
        public async Task<IActionResult> DeleteGroup(int groupId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized("Invalid user ID");

            var group = await _context.Groups.FindAsync(groupId);

            if (group == null)
                return NotFound("Group not found");

            _context.Groups.Remove(group);
            await _context.SaveChangesAsync();

            return Ok("Group deleted successfully");
        }
        #endregion
    }
}
