using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PiratenKarte.Server.Authorization;

public class PermissionAttribute : BaseAuthorizationAttribute {
    private readonly string PermissionName;

    public PermissionAttribute(string permissionName) {
        PermissionName = permissionName;
    }

    public override void OnActionExecuting(ActionExecutingContext context) {
        if (!BaseChecks(context))
            return;

        if (!DB!.UserRepo.HasPermission(User!.Id, PermissionName)) {
            context.Result = new UnauthorizedResult();
            return;
        }

        base.OnActionExecuting(context);
    }
}