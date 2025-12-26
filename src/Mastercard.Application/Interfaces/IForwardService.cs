using Mastercard.Application.DTOs;

namespace Mastercard.Application.Interfaces
{
    public interface IForwardService
    {
        Task<ForwardServiceResponse> ForwardPurchaseAsync(object payload, string url, CancellationToken ct);
    }
}