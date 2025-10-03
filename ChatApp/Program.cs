using ChatApp.Data;
using ChatApp.Hubs;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);




builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//builder.Services
//    .AddSignalR()
//    .AddStackExchangeRedis(
//        builder.Configuration.GetConnectionString("Redis"),
//        o => o.Configuration.ChannelPrefix = "prnchat");

builder.Services.AddSignalR().AddStackExchangeRedis("172.20.10.11:31340,password=Group1se1888");

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
