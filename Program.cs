using Hangfire;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TTTN3.Models;
using Hangfire.SqlServer;
using TTTN3.Areas.Admin.Controllers;
using TTTN3.Interfaces;
using TTTN3.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Facebook;
using System.Configuration;
using PayPal.Api;
using Microsoft.Extensions.DependencyInjection.Extensions;

//using TTTN3.Responsitory;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddTransient<IMailSender,EmailSender>();
builder.Services.AddControllersWithViews();
builder.Services.AddHttpContextAccessor();
builder.Services.AddMvc().AddSessionStateTempDataProvider();
builder.Services.AddSession();
builder.Services.AddRazorPages();
//builder.Services.AddDbContext<DataContext>(options =>
//{
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DbConnect"));
//});
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DbConnect"))
           .UseLoggerFactory(LoggerFactory.Create(builder => builder.AddDebug()))
           .EnableSensitiveDataLogging()
);
builder.Services.AddDistributedMemoryCache();
//builder.Services.AddScoped<IBlogResponsitory, BlogResponsitory>();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.IsEssential = true;
}
);

builder.Services.AddIdentity<AppUserModel, IdentityRole>()
    .AddEntityFrameworkStores<DataContext>().AddDefaultTokenProviders();

builder.Services.Configure<IdentityOptions>(options =>
{
    // Password settings.
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 1;

    options.User.RequireUniqueEmail = true;

    options.User.AllowedUserNameCharacters = null;
});
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Admin", policy =>
    {
        // Đặt các yêu cầu cho chính sách 'Admin' ở đây
        policy.RequireRole("Admin"); // Chỉ yêu cầu người dùng có vai trò 'Admin'
    });
});

builder.Services.AddAuthentication().AddFacebook(options =>
{
    options.AppId = "935397418243371";
    options.AppSecret = "6667697c3935d7d3baba088923b269a1";
});

// Configure Hangfire
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("HangfireDbConnect"), new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.Zero,
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true
    }));
//builder.Services.AddTransient<PaypalConfiguration>();

var app = builder.Build();
app.UseSession();
app.UseStaticFiles();
// khai báo 
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    
}

app.UseStatusCodePagesWithReExecute("/Account/AccessDenied");
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.UseHangfireDashboard("/hangfire");

RecurringJob.AddOrUpdate<invoicesController>(x => x.UpdateInvoiceStatus(), Cron.Daily);

app.UseHangfireServer(); // Start the Hangfire server


app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
        name: "ShopIndex",
        pattern: "Shop/Filter_Product",
        defaults: new { controller = "Shop", action = "Filter_Product" }
    );

app.Run();
//app.Run(async (context) =>
//{
//    // Redirect to the desired default URL
//    context.Response.Redirect("/Home/Index");
//    await Task.CompletedTask;
//});
