using ABCRetailers.Services;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http.Features;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllersWithViews();

// --- Register the named HttpClient that FunctionsApiClient expects ---
builder.Services.AddHttpClient("Functions", client =>
{
    var baseUrl = builder.Configuration["Functions:BaseUrl"]
        ?? throw new InvalidOperationException("Functions BaseUrl missing");
    client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/api");
    client.Timeout = TimeSpan.FromSeconds(100);
});

// --- Register FunctionsApiClient manually, using IHttpClientFactory ---
builder.Services.AddHttpClient<IFunctionsApi, FunctionsApiClient>(client =>
{
    var baseUrl = builder.Configuration["Functions:BaseUrl"]
        ?? throw new InvalidOperationException("Functions BaseUrl missing");
    client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/api/");
    client.Timeout = TimeSpan.FromSeconds(100);
});


// Optional: allow larger Multipart Uploads (images, proofs, etc.)
builder.Services.Configure<FormOptions>(o =>
{
    o.MultipartBodyLengthLimit = 50 * 1024 * 1024;
});

// Logging
builder.Services.AddLogging();

// Blob Storage client
builder.Services.AddSingleton(new BlobServiceClient(
    builder.Configuration.GetConnectionString("AzureStorage")
));

var app = builder.Build();

// Set culture for decimal handling
var culture = new CultureInfo("en-US");
CultureInfo.DefaultThreadCurrentCulture = culture;
CultureInfo.DefaultThreadCurrentUICulture = culture;

// Configure HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
