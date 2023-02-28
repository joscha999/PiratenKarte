using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PiratenKarte.DAL;
using PiratenKarte.DAL.Models;

namespace PiratenKarte.Server.Authorization;

public abstract class BaseAuthorizationAttribute : ActionFilterAttribute {
    protected DB? DB;
    protected User? User;

    protected bool BaseChecks(ActionExecutingContext context) {
        DB = context.HttpContext.RequestServices.GetService<DB>();
        if (DB == null) {
            context.Result = new StatusCodeResult(500);
            return false;
        }

        if (!context.HttpContext.Request.Headers.TryGetValue("authtoken", out var authTokens)) {
            context.Result = new UnauthorizedResult();
            return false;
        }

        if (!context.HttpContext.Request.Headers.TryGetValue("userid", out var userIds)) {
            context.Result = new UnauthorizedResult();
            return false;
        }

        var token = authTokens.FirstOrDefault();
        var guid = userIds.FirstOrDefault();
        if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(guid) || !Guid.TryParse(guid, out var userId)) {
            context.Result = new UnauthorizedResult();
            return false;
        }

        if (!DB.TokenRepo.CheckToken(token, userId, TokenType.Login)) {
            context.Result = new UnauthorizedResult();
            return false;
        }

        User = DB.UserRepo.Get(userId);
        if (User == null) {
            context.Result = new UnauthorizedResult();
            return false;
        }

        return true;
    }
}