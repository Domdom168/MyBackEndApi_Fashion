namespace MyBackEndApi.DTOs.Login
{
    public class TokenResultDto
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public AuthResponseDto User { get; set; }
    }
}
