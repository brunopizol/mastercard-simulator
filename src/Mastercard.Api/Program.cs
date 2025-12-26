using Mastercard.Api.Endpoints;
using Mastercard.Infrastructure.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.AddInfrastructureServices();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGroup("/api")
    .WithOpenApi()
    .WithTags("Simulation")
    .MapSimulationEndpoints();

app.MapGroup("/api")
    .WithOpenApi()
    .WithTags("Forward")
    .MapForwardEndpoints();

app.Run();