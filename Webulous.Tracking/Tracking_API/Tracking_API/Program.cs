using Tracking_API.Model;
using Tracking_API.Middleware;
using NLog;
using NLog.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders(); // Supprime les providers par défaut
builder.Host.UseNLog();
LogManager.Setup().LoadConfigurationFromFile(builder.Configuration["NLogConfigPath"]);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.Configure<AdminSafeListOptions>(
    builder.Configuration.GetSection("AdminSafeList"));

builder.Services.AddSingleton<FileLogService>();
builder.Services.AddSingleton(
    new IPManager(new FileManager(string.Concat(builder.Configuration["PathToWhiteFilesDirectory"], "\\IpsWhiteList.txt")))
    );
builder.Services.AddSingleton(
    new DomainManager(new FileManager(string.Concat(builder.Configuration["PathToWhiteFilesDirectory"], "\\DomainsWhiteList.txt")))
    );

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowMvc", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("AllowMvc");

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseMiddleware<AdminSafeListMiddleware>();

app.MapControllers();

app.Run();
