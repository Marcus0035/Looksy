namespace Looksy.Models.DTOs
{
    public class GroupDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt { get; set; }
        public List<UserDto> Members { get; set; } = new List<UserDto>();
    }
}
