using Microsoft.AspNetCore.Components;
using PiratenKarte.Client.Services;
using System.Text.RegularExpressions;

namespace PiratenKarte.Client.Components.Authorization;

public partial class PermissionView {
    [Inject]
    public required AppStateService StateService { get; init; }

    [Parameter]
    public RenderFragment? Authorized { get; init; }
    [Parameter]
    public RenderFragment? UnAuthorized { get; init; }
    [Parameter]
    public RenderFragment? ChildContent { get; init; }
    [Parameter]
    public required string PermissionFilter { get; init; }

    public bool CanView() {
        if (StateService.Current.User == null)
            return false;
        if (StateService.Current.Permissions.Count == 0)
            return false;

        if (PermissionFilter == "*")
            return true;

        return StateService.Current.Permissions.Any(p => Regex.IsMatch(p.Key, PermissionFilter));
    }
}