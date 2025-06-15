using Looksy.Models.DTOs;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Looksy.Models
{
    public class User : BaseModel
    {
        [Required]
        [StringLength(20, MinimumLength = 2)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(30, MinimumLength = 2)]
        public string LastName { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        public List<Group> Groups { get; set; } = new List<Group>();
        public ICollection<Photo> UploadedPhotos { get; set; } = new List<Photo>();

        public User(UserCreateDto user)
        {
            FirstName = user.FirstName;
            LastName = user.LastName;
            Username = user.Username;
            Email = user.Email;
            PasswordHash = string.Empty; // Password will be set later after hashing
        }

        public User() { }

    }
}
