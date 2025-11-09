using ABCRetailers.Services;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http.Features;
using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using ABCRetailers.Data;

var builder = WebApplication.CreateBuilder(args);

// --- Get the SQL Database connection string ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// --- Register the SQL Database DbContext ---
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// --- Register ASP.NET Core Identity ---
builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>() // Add role support
    .AddEntityFrameworkStores<ApplicationDbContext>(); // Tell Identity to use your new DbContext


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

// --- Services for the Shopping Cart (Session) ---
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
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


// --- ADDED: Initialize the roles and admin user ---
// This ensures roles and the admin user exist on startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        // We are calling the seeder here
        await DbInitializer.Initialize(services);
    }
    catch (Exception ex)
    {
        // Log errors if seeding fails
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}
// --- END ADDED BLOCK ---


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

// --- Enable Session for Shopping Cart ---
app.UseSession();

// --- Enable Authentication ---
app.UseAuthentication();
app.UseAuthorization();

// --- Map Razor Pages (for Identity's default login UI) ---
app.MapRazorPages();

app.MapControllerRoute(
  name: "default",
  pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();