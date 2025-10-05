using System.Globalization;
using ABCRetailers.Services;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http.Features;

namespace ABCRetailers
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            builder.Services.AddHttpClient<IFunctionsApi, FunctionsApiClient>((sp, client) =>
            {
                var cfg = sp.GetRequiredService<IConfiguration>();
                var baseUrl = cfg["Functions:BaseUrl"]
                    ?? throw new InvalidOperationException("Functions BaseUrl missing");
                client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/api");
                client.Timeout = TimeSpan.FromSeconds(100);
            });


            //optional: allow larger Multipart Uploads (images, proofs, etc.)
            builder.Services.Configure<FormOptions>(o =>
            {
                o.MultipartBodyLengthLimit = 50 * 1024 * 1024;
            });

            Console.WriteLine("Functions BaseUrl: " + builder.Configuration["Functions:BaseUrl"]);


            //Add logging
            builder.Services.AddLogging();
            // inside builder.Services section:
            builder.Services.AddSingleton(new BlobServiceClient(builder.Configuration.GetConnectionString("AzureStorage")));
            var app = builder.Build();

            //Set the culture for decimal handling (Fixes price issue)
            var culture = new CultureInfo("en-US");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
        }
    }
}