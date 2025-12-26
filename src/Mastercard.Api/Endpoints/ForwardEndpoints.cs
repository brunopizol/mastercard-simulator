using Mastercard.Application.DTOs;
using Mastercard.Application.UseCases;
using Mastercard.Core.Exceptions;

namespace Mastercard.Api.Endpoints
{
    public static class ForwardEndpoints
    {
        public static void MapForwardEndpoints(this RouteGroupBuilder group)
        {
            group.MapPost("/forward", ForwardPurchases)
                .WithName("ForwardPurchases")
                .WithDescription("Forwards generated purchases to a specified URL");
        }

        private static async Task<IResult> ForwardPurchases(
            ForwardRequest request,
            ForwardPurchasesHandler handler,
            CancellationToken ct)
        {
            try
            {
                var result = await handler.HandleAsync(request, ct);
                return Results.Ok(new { result.Forwarded, result.Responses });
            }
            catch (InvalidForwardRequestException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return Results.StatusCode(500);
            }
        }
    }
}