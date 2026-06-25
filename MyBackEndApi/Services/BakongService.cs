using Microsoft.Extensions.Options;
using MyBackEndApi.DTOs;
using MyBackEndApi.Models;
using System.Reflection.Emit;
using System.Text;
using System.Text.Json; 
namespace MyBackEndApi.Services
{
    public class BakongService : IBakongService
    {
        private readonly HttpClient _httpClient;
        private readonly BakongSettings _settings;

        public BakongService(HttpClient httpClient, IOptions<BakongSettings> options)
        {
            _httpClient = httpClient;
            _settings = options.Value;
            _httpClient.BaseAddress = new Uri(_settings.ApiBaseUrl);
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_settings.Token}");
        }

        public async Task<string> GenerateDynamicQRAsync(decimal amount, string billNumber)
        {
            var payload = new
            {
                accountId = _settings.AccountId,
                merchantName = _settings.MerchantName,
                merchantCity = _settings.MerchantCity,
                amount = amount,
                currency = _settings.Currency,
                billNumber = billNumber,
                staticQr = false,
                expiration = 1 // days
            };

            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("/khqr/generate", content);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Bakong API error: {response.StatusCode} - {error}");
            }

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<BakongApiResponse>(json);
            if (result.Status != 0)
                throw new Exception($"QR generation failed: {result.Message}");

            return result.QrString;
        }
    }
}
