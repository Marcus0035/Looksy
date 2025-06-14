using Looksy.Infrastructure.Data.Models;
using System.ComponentModel.DataAnnotations;

public class Photo : BaseModel
{
    [Url]
    public string Url { get; set; }

    public string? Description { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public int UploadedByUserId { get; set; }
    public User UploadedBy { get; set; }

    public int GroupId { get; set; }
    public Group Group { get; set; }
}
