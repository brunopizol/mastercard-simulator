using Mastercard.Application.DTOs;

namespace Mastercard.Application.UseCases
{
    public class ForwardPurchasesResult
    {
        public int Forwarded { get; set; }
        public List<object> Sent { get; set; } = new();
        public List<ForwardResponse> Responses { get; set; } = new();
    }
}