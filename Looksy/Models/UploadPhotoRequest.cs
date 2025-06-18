using Microsoft.AspNetCore.Mvc;

namespace Looksy.Models
{
    public class UploadPhotoRequest
    {
        [FromForm]
        public int GroupId { get; set; }

        [FromForm]
        public string? Description { get; set; }

        [FromForm]
        public IFormFile File { get; set; } = default!;
    }

}
