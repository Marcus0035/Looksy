namespace Looksy.Models.DTOs
{
    public class ChangePasswordDto
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmNewPassword { get; set; }
        public ChangePasswordDto() { }
        public ChangePasswordDto(string currentPassword, string newPassword, string confirmNewPassword)
        {
            CurrentPassword = currentPassword;
            NewPassword = newPassword;
            ConfirmNewPassword = confirmNewPassword;
        }
    }
}
