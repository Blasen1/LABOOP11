using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WebApp.Data;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<ApplicationDbContext>();
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequiredLength = 3;
    options.Password.RequiredUniqueChars = 1;
    options.SignIn.RequireConfirmedAccount = false;
});
builder.Services.AddControllersWithViews();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
    var context = services.GetRequiredService<ApplicationDbContext>();

    await context.Database.MigrateAsync();

    var ivanExists = await userManager.FindByEmailAsync("ivan@gmail.com") != null;
    var mariaExists = await userManager.FindByEmailAsync("tana@gmail.com") != null;
    var testExists = await userManager.FindByEmailAsync("test@example.com") != null;

    if (!ivanExists)
    {
        var user1 = new IdentityUser { UserName = "ivan@gmail.com", Email = "ivan@gmail.com", EmailConfirmed = true };
        await userManager.CreateAsync(user1, "Password123!");
    }

    if (!mariaExists)
    {
        var user2 = new IdentityUser { UserName = "tana@gmail.com", Email = "tana@gmail.com", EmailConfirmed = true };
        await userManager.CreateAsync(user2, "Password123!");
    }

    if (!testExists)
    {
        var testUser = new IdentityUser { UserName = "test@gmail.com", Email = "test@gmail.com", EmailConfirmed = true };
        await userManager.CreateAsync(testUser, "Test123!");
    }
}


if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}


app.UseRouting();

app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

app.Run();
