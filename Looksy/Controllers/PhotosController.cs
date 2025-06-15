using Looksy.Data.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Looksy.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhotosController : ControllerBase
    {
        #region Const And Readonly Fields
        private readonly LooksyDbContext _context;
        #endregion

        #region Constructors
        public PhotosController(LooksyDbContext context)
        {
            _context = context;
        }
        #endregion

        #region Public
        [HttpGet]
        [Authorize]
        [Route("{photoId}")]
        public async Task<IActionResult> GetPhoto(int photoId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized("Invalid user ID");

            var photo = await _context.Photos
                .Include(p => p.Group)
                .ThenInclude(g => g.Members)
                .FirstOrDefaultAsync(p => p.Id == photoId);

            if (photo == null)
                return NotFound("Photo not found");

            var isUserInGroup = photo.Group.Members.Any(m => m.Id == userId);
            if (!isUserInGroup)
                return Forbid("You are not a member of this group");

            return Ok(new
            {
                photo.Id,
                photo.Url,
                photo.Description,
                photo.UploadedAt,
                photo.UploadedByUserId,
                photo.GroupId
            });
        }


        [HttpPost]
        [Authorize]
        [Route("upload")]
        public async Task<IActionResult> UploadPhoto([FromBody] PhotoCreateDto photoDto)
        {
            if (photoDto == null || string.IsNullOrEmpty(photoDto.Url) || photoDto.GroupId < 1)
                return BadRequest("Invalid photo data");

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized("Invalid user ID");

            var group = await _context.Groups.Include(x => x.Members).FirstOrDefaultAsync(x => x.Id == photoDto.GroupId);

            if (group == null)
                return NotFound("Group not found");

            var isUserInGroup = group.Members.Any(m => m.Id == userId);

            if (!isUserInGroup)
                return Forbid("You are not a member of this group");

            var photo = new Photo
            {
                Url = photoDto.Url,
                Description = photoDto.Description,
                UploadedAt = DateTime.UtcNow,
                UploadedByUserId = userId,
                GroupId = photoDto.GroupId
            };

            _context.Photos.Add(photo);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPhoto), new { photoId = photo.Id }, null);
        }

        [HttpDelete]
        [Authorize]
        [Route("{photoId}")]
        public async Task<IActionResult> DeletePhoto(int photoId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized("Invalid user ID");

            var photo = await _context.Photos
                .Include(p => p.Group)
                .ThenInclude(g => g.Members)
                .FirstOrDefaultAsync(p => p.Id == photoId);

            if (photo == null)
                return NotFound("Photo not found");

            var isUserInGroup = photo.Group.Members.Any(m => m.Id == userId);

            if (!isUserInGroup)
                return Forbid("You are not a member of this group");

            _context.Photos.Remove(photo);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        #endregion
    }
}
