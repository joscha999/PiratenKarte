using Microsoft.AspNetCore.Components;
using PiratenKarte.Client.Services;

namespace PiratenKarte.Client.Components.Authorization;

public partial class PermissionView {
    [Inject]
    public required AuthenticationStateService StateService { get; init; }

    [Parameter]
    public RenderFragment? Authorized { get; init; }
    [Parameter]
    public RenderFragment? UnAuthorized { get; init; }
    [Parameter]
    public RenderFragment? ChildContent { get; init; }
    [Parameter]
    public required string PermissionFilter { get; init; }
    public bool CanView() {
        if (PermissionFilter == "*")
            return true;

        return StateService.HasPattern(PermissionFilter);
    }
}