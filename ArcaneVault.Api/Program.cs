/*
 * Name: Aden Leung
 * Student Admin No.: 252744K
 * Tutorial Group: IT2814
 */
using ArcaneVault.Api.Data;
using Microsoft.EntityFrameworkCore;
using ArcaneVault.Api.Services;

EnvFileLoader.LoadFromParents(".env.local");

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<ArcaneVaultDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("ArcaneVault")));
builder.Services.AddSingleton<ApiTokenService>();
builder.Services.AddHttpClient<OpenAiService>();

var app = builder.Build();
app.Use(async (context, next) =>
{
    try { await next(); }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Unhandled API error for {Method} {Path}", context.Request.Method, context.Request.Path);
        if (!context.Response.HasStarted)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(new
            {
                type = "https://httpstatuses.com/500",
                title = "The request could not be completed.",
                status = 500,
                message = "The server could not complete this request. Check the ArcaneVault.Api Output window."
            });
        }
    }
});
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.MapControllers();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ArcaneVaultDbContext>();
    await DatabaseBootstrap.PrepareAsync(db, app.Logger);
    await DbSeeder.SeedAsync(db);
}

app.Run();
