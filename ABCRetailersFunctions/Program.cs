using Azure.Data.Tables;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = FunctionsApplication.CreateBuilder(args);

// Application Insights (optional)
builder.Services.AddApplicationInsightsTelemetryWorkerService()
                .ConfigureFunctionsApplicationInsights();

// Register TableServiceClient once
builder.Services.AddSingleton(sp =>
{
    var config = builder.Configuration;
    var storageConn = config["AzureWebJobsStorage"];
    return new TableServiceClient(storageConn);
});

//  Register named (typed) clients per table

builder.Services.AddSingleton<TableClient>(sp =>
{
    var serviceClient = sp.GetRequiredService<TableServiceClient>();
    var tableName = builder.Configuration["TABLE_CUSTOMER"];
    var client = serviceClient.GetTableClient(tableName);
    client.CreateIfNotExists();
    return client;
});

// Register a named client for Products
builder.Services.AddSingleton<TableClient>(sp =>
{
    var serviceClient = sp.GetRequiredService<TableServiceClient>();
    var tableName = builder.Configuration["TABLE_PRODUCT"];
    var client = serviceClient.GetTableClient(tableName);
    client.CreateIfNotExists();
    return client;
});

// Register a named client for Orders
builder.Services.AddSingleton<TableClient>(sp =>
{
    var serviceClient = sp.GetRequiredService<TableServiceClient>();
    var tableName = builder.Configuration["TABLE_ORDER"];
    var client = serviceClient.GetTableClient(tableName);
    client.CreateIfNotExists();
    return client;
});

// -----------------------------
// Blob Storage setup
// -----------------------------
builder.Services.AddSingleton(sp =>
{
    var config = builder.Configuration;
    var storageConn = config["AzureWebJobsStorage"];
    return new BlobServiceClient(storageConn);
});

var app = builder.Build();
app.Run();
