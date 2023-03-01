using Microsoft.AspNetCore.Components;
using PiratenKarte.Client.Services;

namespace PiratenKarte.Client.Pages;

public partial class Settings {
    [Inject]
    public required HttpClient Http { get; init; }
    [Inject]
    public required AppStateService StateService { get; init; }
    [Inject]
    public required AuthenticationStateService AuthStateService { get; init; }
    [Inject]
    public required NavigationManager NavManager { get; init; }

    private bool UseLocalStorage {
        get => StateService.Current.StoreStateLocally;
        set {
            if (StateService.Current.StoreStateLocally == value)
                return;

            StateService.Current.StoreStateLocally = value;
            StateService.Write();
        }
    }

    private void Back() => NavManager.NavigateTo("");

    private async Task Logout() {
        await AuthStateService.Invalidate();
        NavManager.NavigateTo("signin");
    }
}