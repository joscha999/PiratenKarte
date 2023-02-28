using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using PiratenKarte.Server.Controllers;
using PiratenKarte.DAL.Models;

namespace PiratenKarte.Server.Authorization;

public class CrudPermission : BaseAuthorizationAttribute {
    private readonly string PermissionSuffix;

    public CrudPermission(string permissionSuffix) {
        PermissionSuffix = permissionSuffix;
    }

    public override void OnActionExecuting(ActionExecutingContext context) {
        if (!BaseChecks(context))
            return;

        if (context.Controller is not CrudController controller) {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (!DB!.UserRepo.HasPermission(User!.Id, controller.PermissionBaseName + PermissionSuffix)) {
            context.Result = new UnauthorizedResult();
            return;
        }

        base.OnActionExecuting(context);
    }
}