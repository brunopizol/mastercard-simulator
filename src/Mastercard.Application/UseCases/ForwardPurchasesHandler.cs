using Mastercard.Application.DTOs;
using Mastercard.Application.Interfaces;
using Mastercard.Core.Exceptions;

namespace Mastercard.Application.UseCases
{
    public class ForwardPurchasesHandler
    {
        private readonly IPurchaseGenerator _purchaseGenerator;
        private readonly IForwardService _forwardService;

        public ForwardPurchasesHandler(IPurchaseGenerator purchaseGenerator, IForwardService forwardService)
        {
            _purchaseGenerator = purchaseGenerator ?? throw new ArgumentNullException(nameof(purchaseGenerator));
            _forwardService = forwardService ?? throw new ArgumentNullException(nameof(forwardService));
        }

        public async Task<ForwardPurchasesResult> HandleAsync(ForwardRequest request, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request.Url))
                throw new InvalidForwardRequestException("Url is required in the request body");

            var count = Math.Clamp(request.Count, 1, 100);
            var sent = new List<object>();
            var responses = new List<ForwardResponse>();

            for (int i = 0; i < count; i++)
            {
                var payload = _purchaseGenerator.Generate();
                sent.Add(payload);

                try
                {
                    var response = await _forwardService.ForwardPurchaseAsync(payload, request.Url, ct);
                    responses.Add(new ForwardResponse 
                    { 
                        StatusCode = response.StatusCode, 
                        Body = response.Body 
                    });
                }
                catch (Exception ex)
                {
                    responses.Add(new ForwardResponse 
                    { 
                        StatusCode = 0, 
                        Body = ex.Message 
                    });
                }
            }

            return new ForwardPurchasesResult 
            { 
                Forwarded = sent.Count, 
                Sent = sent, 
                Responses = responses 
            };
        }
    }
}