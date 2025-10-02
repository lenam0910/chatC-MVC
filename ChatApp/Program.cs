using ChatApp.Data;
using ChatApp.Hubs;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowApp", policy =>
    {
        policy.WithOrigins(
                "https://prn-chat.lxhn.vn", // nếu frontend cùng host thì CORS không cần; nhưng cứ để rõ ràng
                "https://frontend.example.com", // ví dụ: SPA ở domain khác
                "http://localhost:5173"        // ví dụ: local dev
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // cần cho cookie/Auth + SignalR
    });
});


builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services
    .AddSignalR()
    .AddStackExchangeRedis(
        builder.Configuration.GetConnectionString("Redis"),
        o => o.Configuration.ChannelPrefix = "prnchat");

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Đứng sau Ingress (TLS terminate), cần nhận scheme/proto đúng
app.UseForwardedHeaders(new ForwardedHeadersOptions {
  ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Chat}/{action=Index}/{id?}");

app.MapHub<ChatHub>("/chatHub");
app.MapGet("/healthz", () => Results.Ok("ok"));

app.Run();
