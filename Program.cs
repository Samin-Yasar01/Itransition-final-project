using FormsApp.Data;
using FormsApp.Models;
using FormsApp.Features.Authentication.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using FormsApp.Features.Templates.Services;
using FormsApp.Features.Forms.Services;
using FormsApp.Features.Admin.Services;
using FormsApp.Features.Search.Services;
using FormsApp.Features.Social.Services;
using FormsApp.Features.Templates.Repositories;
using FormsApp.Hubs;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Localization;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
    

builder.Services.AddControllersWithViews()
    .AddRazorOptions(options =>
    {
        // Clear default view locations
        options.ViewLocationFormats.Clear();

        // Add feature-based locations
        options.ViewLocationFormats.Add("/Features/{1}/Views/{0}.cshtml");
        options.ViewLocationFormats.Add("/Features/{1}/Views/{0}" + RazorViewEngine.ViewExtension);
        options.ViewLocationFormats.Add("/Features/Shared/Views/{0}.cshtml");

        // Add default fallbacks (optional)
        options.ViewLocationFormats.Add("/Views/{1}/{0}.cshtml");
        options.ViewLocationFormats.Add("/Views/Shared/{0}.cshtml");
    });

// Authentication
builder.Services.AddScoped<IAuthService, AuthService>();

// Templates
builder.Services.AddScoped<ITemplateRepository, TemplateRepository>();
builder.Services.AddScoped<ITemplateService, TemplateService>();

// Forms
builder.Services.AddScoped<IFormService, FormService>();

// Admin
builder.Services.AddScoped<IAdminService, AdminService>();

// Search
builder.Services.AddScoped<ISearchService, SearchService>();

// Social
builder.Services.AddScoped<ISocialService, SocialService>();

// SignalR for real-time comments
builder.Services.AddSignalR();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Search}/{action=Index}/{id?}");
app.MapRazorPages();

// Map SignalR hub
app.MapHub<CommentsHub>("/commentsHub");

// Seed admin role and user
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    // Create Admin role if it doesn't exist
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }

    // Create default admin user if it doesn't exist
    var adminEmail = "admin@example.com";
    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new ApplicationUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            FirstName = "Admin",
            LastName = "User",
            ProfilePictureUrl = "/images/default-avatar.png" // Add default value
        };
        var result = await userManager.CreateAsync(adminUser, "Admin@123");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Admin");
        }
        else
        {
            // Add error logging here
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"Error: {error.Description}");
            }
        }
    }
}

app.Run();