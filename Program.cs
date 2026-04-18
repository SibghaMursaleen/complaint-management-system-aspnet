using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SIBA.ComplaintSystem.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    // Suppress the pending model changes warning to allow the app to run
    options.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
});

// 1. Add Authentication Services
builder.Services.AddAuthentication("SibaAuth")
    .AddCookie("SibaAuth", options =>
    {
        options.LoginPath = "/Account/Auth";
        options.AccessDeniedPath = "/Account/Auth";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.UseStaticFiles();
app.UseRouting();

// 2. Enable Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

// 3. Ensure Admin Email is Updated
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var adminUser = context.Users.FirstOrDefault(u => u.Id == 1 || u.Email == "admin@siba.edu.pk");
    if (adminUser != null)
    {
        adminUser.Email = "senior@admin.siba.edu.pk";
        context.SaveChanges();
    }
}



app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Auth}/{id?}")
    .WithStaticAssets();

app.Run();
