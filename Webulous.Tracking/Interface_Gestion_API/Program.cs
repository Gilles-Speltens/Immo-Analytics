using Common;
using Interface_Gestion_API.Models;
using Microsoft.AspNetCore.Identity;
using NLog;
using NLog.Web;
using System.Runtime;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders(); // Supprime les providers par défaut
builder.Host.UseNLog();
LogManager.Setup().LoadConfigurationFromFile(builder.Configuration["NLogConfigPath"]);

builder.Services.Configure<ApiSettings>(builder.Configuration);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddHttpClient();

builder.Services.AddSingleton(sp =>
{
    var factory = sp.GetRequiredService<IHttpClientFactory>();
    var apiPath = builder.Configuration["APIPath"];

    return new RequestApiService(apiPath, factory);
});

builder.Services.AddScoped(sp =>
{
    return new WhiteListManager(new WhiteListViewModel
    {
        IPv4 = new List<IPSubnet>(),
        IPv6 = new List<IPSubnet>(),
        Domains = new List<string>()
    });
});

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.UseSession();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
