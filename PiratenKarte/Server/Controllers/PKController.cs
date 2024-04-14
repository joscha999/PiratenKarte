using Microsoft.AspNetCore.Mvc;
using PiratenKarte.DAL;
using PiratenKarte.DAL.Models;
using System.Diagnostics.CodeAnalysis;

namespace PiratenKarte.Server.Controllers;

[ApiController]
[Route("[controller]/[action]")]
public abstract class PKController : ControllerBase {
    protected readonly DB DB;

    protected PKController(DB db) {
        DB = db;
    }

    [NonAction]
    public string? GetAuthToken() {
        if (!Request.Headers.TryGetValue("authtoken", out var authTokens))
            return null;
        var token = authTokens.FirstOrDefault();
        if (string.IsNullOrEmpty(token))
            return null;

        return token;
    }

    [NonAction]
    public Guid? GetUserId() {
        if (!Request.Headers.TryGetValue("userid", out var userIds))
            return null;
        var userId = userIds.FirstOrDefault();
        if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var guid))
            return null;

        return guid;
    }

    [NonAction]
    public bool TryGetUser([NotNullWhen(true)] out User? user) {
        var userId = GetUserId();
        if (userId == null) {
            user = null;
            return false;
        }

        var u = DB.UserRepo.Get(userId.Value);
        if (u == null) {
            user = null;
            return false;
        }

        user = u;
        return true;
    }

    [NonAction]
    public bool IsUserInGroup(User user, Guid groupId) => user.GroupIds.Contains(groupId);

    [NonAction]
    public bool HasPermission(User user, string permission) => DB.UserRepo.HasPermission(user.Id, permission);

    [NonAction]
    public bool HasPermissionOrIsSelf(User user, string permission, User subject)
        => user.Id == subject.Id || HasPermission(user, permission);
}