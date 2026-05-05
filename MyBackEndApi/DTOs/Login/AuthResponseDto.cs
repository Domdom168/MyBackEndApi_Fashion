namespace MyBackEndApi.DTOs.Login
{
    public class AuthResponseDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        // Tokens are in cookies, not in response body
    }
}
