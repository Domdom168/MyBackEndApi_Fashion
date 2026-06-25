namespace MyBackEndApi.Services
{
    public interface IBakongService
    {
        Task<string> GenerateDynamicQRAsync(decimal amount, string billNumber);
    }
}
