/*
 * Name: Aden Leung
 * Student Admin No.: 252744K
 * Tutorial Group: IT2814
 */
using ArcaneVault.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArcaneVault.Api.Controllers;

public abstract class ApiControllerBase(ArcaneVaultDbContext db, ApiTokenService tokens) : ControllerBase
{
    private bool _identityResolved;
    private ApiIdentity? _identity;
    private ApiIdentity? Identity
    {
        get
        {
            if (_identityResolved) return _identity;
            _identityResolved = true;
            // Only a signed bearer token is accepted. Editable username/role headers are deliberately ignored.
            var header = Request.Headers.Authorization.FirstOrDefault();
            var token = header?.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase) == true ? header[7..].Trim() : null;
            var signed = tokens.Validate(token);
            if (signed is null) return null;
            // Re-read the account so deleted users or changed roles invalidate an otherwise correctly signed token.
            var account = db.ArcaneVaultUsers.AsNoTracking().Include(x => x.Role)
                .SingleOrDefault(x => x.UserName == signed.UserName && !x.IsDeleted);
            if (account?.Role is null || !string.Equals(account.Role.RoleName, signed.RoleName, StringComparison.OrdinalIgnoreCase)) return null;
            return _identity = new ApiIdentity(account.UserName, account.Role.RoleName);
        }
    }
    protected string CurrentUser => Identity?.UserName ?? string.Empty;
    protected bool IsStaff => string.Equals(Identity?.RoleName, "Staff", StringComparison.OrdinalIgnoreCase);
    protected ActionResult? RequireUser() => Identity is null ? Unauthorized(new { message = "Your session has expired. Please log in again." }) : null;
    protected ActionResult? RequireStaff() => RequireUser() ?? (IsStaff ? null : Forbid());
}
