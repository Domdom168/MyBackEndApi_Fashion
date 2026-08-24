namespace MyBackEndApi.DTOs.CustomerLogin
{
    public class UpdateProfileDto
    {
        public string? Name { get; set; }
        public string? Phone { get; set; }
    }
    public class ChangePasswordDtos
    {
        public string? OldPassword { get; set; }
        public string? NewPassword { get; set; }
    }

    public class ForgotPasswordDtos
    {
        public string? Email { get; set; }
    }

    public class ResetPasswordDtos
    {
        public string? Email { get; set; }
        public string? Token { get; set; }
        public string? NewPassword { get; set; }
    }
}
