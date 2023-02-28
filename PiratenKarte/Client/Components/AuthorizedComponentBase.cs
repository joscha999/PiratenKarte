﻿using Microsoft.AspNetCore.Components;
using PiratenKarte.Client.Services;
using System.Text.RegularExpressions;

namespace PiratenKarte.Client.Components;

public abstract class AuthorizedComponentBase : ComponentBase {
    [Inject]
    public required AppStateService StateService { get; init; }
    [Inject]
    public required NavigationManager NavManager { get; init; }

    protected abstract string PermissionFilter { get; }

    protected override void OnInitialized() {
        if (!CanView()) {
            NavManager.NavigateTo("");
            return;
        }

        base.OnInitialized();
    }

    protected override Task OnInitializedAsync() {
        if (!CanView()) {
            NavManager.NavigateTo("");
            return Task.CompletedTask;
        }

        return base.OnInitializedAsync();
    }

    protected bool CanView() {
        if (StateService.Current.User == null)
            return false;
        if (StateService.Current.Permissions.Count == 0)
            return false;

        return StateService.Current.Permissions.Any(p => Regex.IsMatch(p.Key, PermissionFilter));
    }
}