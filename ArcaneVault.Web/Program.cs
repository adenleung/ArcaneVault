/*
 * Name: Aden Leung
 * Student Admin No.: 252744K
 * Tutorial Group: IT2814
 */
using ArcaneVault.Web.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/Collection");
    options.Conventions.AuthorizeFolder("/Categories", "StaffOnly");
    options.Conventions.AuthorizeFolder("/Staff", "StaffOnly");
});
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = ".ArcaneVault.Auth.v3";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = false;
    });
builder.Services.AddAuthorization(options => options.AddPolicy("StaffOnly", policy => policy.RequireRole("Staff")));
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient<ApiClient>(client => client.BaseAddress = new Uri(builder.Configuration["ApiBaseUrl"]!));

var app = builder.Build();
app.UseExceptionHandler("/Error");
if (!app.Environment.IsDevelopment()) app.UseHsts();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();
app.Run();
