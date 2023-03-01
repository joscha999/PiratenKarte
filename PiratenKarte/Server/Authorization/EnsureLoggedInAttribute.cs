using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PiratenKarte.Server.Authorization;

public class EnsureLoggedInAttribute : BaseAuthorizationAttribute {
    public override void OnActionExecuting(ActionExecutingContext context) {
        if (!BaseChecks(context))
            context.Result = new UnauthorizedResult();
    }
}