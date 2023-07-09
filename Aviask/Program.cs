using Aviask.Data;
using Aviask.Models;
using Aviask.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.WebEncoders;
using System.Text.Encodings.Web;
using System.Text.Unicode;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<AviaskContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AviaskContext") ?? throw new InvalidOperationException("Connection string 'AviaskContext' not found.")));

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

builder.Services.AddScoped<IAviaskRepository<Question, int>, QuestionRepository>();
builder.Services.AddScoped<IAviaskRepository<AnswerRecords, int>, AnswerRecordsRepository>();
builder.Services.AddScoped<IAviaskRepository<IdentityUser, string>, UserRepository>();
builder.Services.AddScoped<AviaskContext>();

/*  Identity    */
builder.Services.Configure<IdentityOptions>(options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 4;
    options.Password.RequiredUniqueChars = 1;
});


builder.Services.AddIdentity<IdentityUser, IdentityRole>()
    .AddEntityFrameworkStores<AviaskContext>()
    .AddDefaultUI()
    .AddDefaultTokenProviders();

var app = builder.Build();

app.UseAuthentication();
app.UseStatusCodePagesWithReExecute("/Error/{0}");  

//  Roles
using (var serviceScope = app.Services.CreateScope())
{
    var serviceProvider = app.Services.GetRequiredService<IServiceProvider>();
    var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    var roles = new[] { "user", "manager", "admin" };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            var newRole = new IdentityRole(role);
            await roleManager.CreateAsync(newRole);
        }
    }
}

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


app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
    endpoints.MapRazorPages();
});

app.Run();
