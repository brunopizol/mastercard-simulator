using Mastercard.Application.Interfaces;
using Mastercard.Application.UseCases;
using Mastercard.Infrastructure.Http;
using Mastercard.Infrastructure.Simulation;
using Microsoft.Extensions.DependencyInjection;

namespace Mastercard.Infrastructure.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
        {
            services.AddSingleton<IPurchaseGenerator, PurchaseGenerator>();
            services.AddHttpClient<IForwardService, HttpForwardService>("forwarder");
            services.AddScoped<ForwardPurchasesHandler>();
            return services;
        }
    }
}