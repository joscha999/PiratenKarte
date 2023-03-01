using Microsoft.AspNetCore.Mvc;

namespace PiratenKarte.Server.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public abstract class PKController : ControllerBase {
    public string? GetAuthToken() {
        if (!Request.Headers.TryGetValue("authtoken", out var authTokens))
            return null;
        var token = authTokens.FirstOrDefault();
        if (string.IsNullOrEmpty(token))
            return null;

        return token;
    }

    public Guid? GetUserId() {
        if (!Request.Headers.TryGetValue("userid", out var userIds))
            return null;
        var userId = userIds.FirstOrDefault();
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var guid))
            return null;

        return guid;
    }
}