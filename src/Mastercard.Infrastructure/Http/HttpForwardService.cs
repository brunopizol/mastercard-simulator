using Mastercard.Application.DTOs;
using Mastercard.Application.Interfaces;
using System.Net.Http.Json;

namespace Mastercard.Infrastructure.Http
{
    public class HttpForwardService : IForwardService
    {
        private readonly HttpClient _httpClient;

        public HttpForwardService(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<ForwardServiceResponse> ForwardPurchaseAsync(object payload, string url, CancellationToken ct)
        {
            try
            {
                var response = await _httpClient.PostAsJsonAsync(url, payload, ct);
                var content = response.Content != null ? await response.Content.ReadAsStringAsync(ct) : null;
                return new ForwardServiceResponse 
                { 
                    StatusCode = (int)response.StatusCode, 
                    Body = content 
                };
            }
            catch (Exception ex)
            {
                return new ForwardServiceResponse 
                { 
                    StatusCode = 0, 
                    Body = ex.Message 
                };
            }
        }
    }
}