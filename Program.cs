using System.Diagnostics;
using Server.Service;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddScoped<ICameraService, CameraService>();
builder.Services.AddScoped<ITrackService, TrackService>();
builder.Services.AddSingleton<CameraManager>();
builder.Services.AddSingleton<FrameBuffer>();
builder.Services.AddSingleton<IPlateEventService, PlateEventService>();
builder.Services.AddSingleton<PlateAnalyse>();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Настройка логирования
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Information);

// Регистрация фоновой службы
builder.Services.AddHostedService<BackService>();
builder.Services.AddHostedService<PlateEventWorker>();

Debug.WriteLine("// Регистрация фоновой службы");

var app = builder.Build();

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
