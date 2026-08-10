/*
 * Name: Aden Leung
 * Student Admin No.: 252744K
 * Tutorial Group: IT2814
 */
using ArcaneVault.Api.Data;
using ArcaneVault.Api.DTOs;
using ArcaneVault.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArcaneVault.Api.Controllers;

[ApiController, Route("api/accounts")]
public class AccountsController(ArcaneVaultDbContext db, ApiTokenService tokens) : ControllerBase
{
    [HttpPost("register")]
    public async Task<ActionResult<LoginResponse>> Register(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var userName = request.UserName.Trim().ToLowerInvariant();
        if (await db.ArcaneVaultUsers.AnyAsync(x => x.Email == email))
            return Conflict(new { message = "An account with this email already exists." });
        if (await db.ArcaneVaultUsers.AnyAsync(x => x.UserName == userName))
            return Conflict(new { message = "This username is already taken." });

        var user = new ArcaneVaultUser
        {
            UserName = userName, Email = email, PasswordHash = PasswordSecurity.Hash(request.Password), RoleId = 2
        };
        db.ArcaneVaultUsers.Add(user);
        try { await db.SaveChangesAsync(); }
        catch (DbUpdateException)
        {
            return Conflict(new { message = "That username or email is already registered." });
        }
        return Created("", new LoginResponse(user.UserName, user.Email, "User", tokens.Create(user.UserName, "User")));
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.ArcaneVaultUsers.Include(x => x.Role)
            .SingleOrDefaultAsync(x => x.Email == email && !x.IsDeleted);
        if (user is null || !PasswordSecurity.Verify(request.Password, user.PasswordHash))
            return Unauthorized(new { message = "Incorrect email or password." });
        var role = user.Role?.RoleName ?? "User";
        return new LoginResponse(user.UserName, user.Email, role, tokens.Create(user.UserName, role));
    }
}
