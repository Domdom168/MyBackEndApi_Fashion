namespace MyBackEndApi.DTOs.CustomerLogin
{
    public class CustomerAuthResponseDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public string? Token { get; set; } // optionally return token
    }
}
