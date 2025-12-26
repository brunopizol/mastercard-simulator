using Mastercard.Application.Interfaces;
using Mastercard.Core.Entities;

namespace Mastercard.Api.Endpoints
{
    public static class SimulationEndpoints
    {
        public static void MapSimulationEndpoints(this RouteGroupBuilder group)
        {
            group.MapGet("/simulate", GenerateSinglePurchase)
                .WithName("GetSinglePurchase")
                .WithDescription("Generates a single purchase payload");

            group.MapGet("/simulate/{count:int}", GenerateMultiplePurchases)
                .WithName("GetMultiplePurchases")
                .WithDescription("Generates multiple purchase payloads (1-100)");
        }

        private static IResult GenerateSinglePurchase(IPurchaseGenerator generator)
        {
            var payload = generator.Generate();
            return Results.Ok(payload);
        }

        private static IResult GenerateMultiplePurchases(int count, IPurchaseGenerator generator)
        {
            var clampedCount = Math.Clamp(count, 1, 100);
            var list = new List<PurchasePayload>();
            for (int i = 0; i < clampedCount; i++)
                list.Add(generator.Generate());
            return Results.Ok(list);
        }
    }
}