using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BE.Data;
using BE.Areas.Identity.Data;
using BE.Services; // Make sure this is here
using Microsoft.AspNetCore.Identity.UI.Services;
using BE.Extensions;

var builder = WebApplication.CreateBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("BEContextConnection") ?? throw new InvalidOperationException("Connection string 'BEContextConnection' not found.");

builder.Services.AddDbContext<BEContext>(options => options.UseSqlServer(connectionString));

builder.Services.AddDefaultIdentity<BEUser>(options => options.SignIn.RequireConfirmedAccount = true).AddEntityFrameworkStores<BEContext>();

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddTransient<IEmailSender, EmailSender>();

builder.Services.AddAuthentication()
    .AddGoogle(options =>
    {
        IConfigurationSection googleAuthNSection = builder.Configuration.GetSection("Authentication:Google");
        options.ClientId = googleAuthNSection["ClientId"];
        options.ClientSecret = googleAuthNSection["ClientSecret"];
    });

// Session configuration
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// NEW: Register ProductService as a Singleton
builder.Services.AddSingleton<ProductService>(); // This ensures one instance is shared and loaded once

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Session middleware MUST be here, AFTER UseRouting() and BEFORE UseAuthentication()/UseAuthorization()
app.UseSession();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();