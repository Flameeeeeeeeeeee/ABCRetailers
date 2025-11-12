using ABCRetailers.Services;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http.Features;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using ABCRetailers.Data; // <-- Make sure this using is present
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// --- Get the SQL Database connection string ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// --- Register the SQL Database DbContext ---
// (This is still needed for your new Users and Cart tables)
builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(connectionString));

// --- ADDED: Services for new MANUAL Cookie-based Login ---
// This replaces the Identity services
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Login/Index"; // Path to your new login page
        options.AccessDeniedPath = "/Home/AccessDenied"; // An "Access Denied" page
        options.ExpireTimeSpan = TimeSpan.FromDays(7);
        options.SlidingExpiration = true;
    });

builder.Services.AddAuthorization(options =>
{
    // Here we can define our "Admin" and "Customer" policies
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("Customer", policy => policy.RequireRole("Customer"));
});
// --- END ADDED BLOCK ---


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

// --- ADDED BACK: Services for Session ---
// (Your professor's LoginController needs this)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
// --- END ADDED BLOCK ---


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

// --- REMOVED: DbInitializer block ---
// (Your database is now seeded with your SQL script)


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

// --- ADDED BACK: Enable Session ---
// (Must be called before UseAuthentication/UseAuthorization)
app.UseSession();
// --- END ADDED BLOCK ---

// --- ADDED: UseAuthentication and UseAuthorization ---
// (These are for the new manual cookie system)
app.UseAuthentication();
app.UseAuthorization();
// --- END ADDED BLOCK ---

// --- REMOVED: app.MapRazorPages() ---
// (We are deleting the Areas/Identity folder)

app.MapControllerRoute(
  name: "default",
  pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();