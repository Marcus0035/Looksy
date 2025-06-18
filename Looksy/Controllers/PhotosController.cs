using Looksy.Models;
using Looksy.Models.DTOs;
using Looksy.Services;
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
        private readonly BlobService _blobService;
        #endregion

        #region Constructors
        public PhotosController(LooksyDbContext context, BlobService blobService)
        {
            _context = context;
            _blobService = blobService;
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

        [HttpGet]
        [Authorize]
        [Route("{groupId}/latest")]
        public async Task<IActionResult> GetLatestPhotoFromGroup(int groupId)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized("Invalid user ID");

            var group = await _context.Groups
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group == null)
                return NotFound("Group not found");

            if (!group.Members.Any(x => x.Id == userId))
                return Forbid("You are not a member of this group");

            var service = _blobService.GenerateSasUrlForLatestPhoto(groupId);

            return Ok(new
            {
                Url = service,
                GroupId = groupId
            });
        }

        [Authorize]
        [HttpPost]
        [Route("upload")]
        [Consumes("multipart/form-data")] // ← toto je klíč
        public async Task<IActionResult> UploadPhoto([FromForm] UploadPhotoRequest uploadPhotoRequest)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdStr, out var userId))
                return Unauthorized("Invalid user ID");

            if (uploadPhotoRequest.File == null || uploadPhotoRequest.File.Length == 0)
                return BadRequest("No file uploaded.");

            var groups = await _context.Groups
                .Include(g => g.Members)
                .Where(g => g.Members.Any(m => m.Id == userId))
                .ToListAsync();

            if (groups.Any(g => g.Id == uploadPhotoRequest.GroupId) == false)
                return Forbid("You are not a member of the specified group");

            var extension = Path.GetExtension(uploadPhotoRequest.File.FileName);

            var photo = new Photo
            {
                Url = string.Empty, // URL will be set after upload
                Description = uploadPhotoRequest.Description,
                UploadedByUserId = userId,
                GroupId = uploadPhotoRequest.GroupId
            };

            _context.Photos.Add(photo);
            await _context.SaveChangesAsync(); // Save to get the photo.Id

            // Now we can use the photo.Id to create the file name
            var fileName = $"{uploadPhotoRequest.GroupId}/{photo.Id}{extension}";

            using var stream = uploadPhotoRequest.File.OpenReadStream();

            var url = await _blobService.UploadAsync(stream, fileName);

            photo.Url = url;

            await _context.SaveChangesAsync();

            return Ok(new { url });
        }

        [HttpDelete]
        [Authorize]
        [Route("delete/{photoId}")]
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
