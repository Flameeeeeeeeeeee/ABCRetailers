using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

// Register Application Insights (optional)
builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights();

// Register TableServiceClient once
builder.Services.AddSingleton(sp =>
{
    var config = builder.Configuration;
    var storageConn = config["StorageConnectionString"]; // from local.settings.json
    return new TableServiceClient(storageConn);
});

// Register specific TableClients
builder.Services.AddSingleton(sp =>
{
    var serviceClient = sp.GetRequiredService<TableServiceClient>();
    var client = serviceClient.GetTableClient("Customers");
    client.CreateIfNotExists();
    return client;
});

builder.Services.AddSingleton(sp =>
{
    var serviceClient = sp.GetRequiredService<TableServiceClient>();
    var client = serviceClient.GetTableClient("Products");
    client.CreateIfNotExists();
    return client;
});

// Add other tables later (Orders, Uploads, etc.)

var app = builder.Build();

app.Run();
