using Microsoft.AspNetCore.Components;
using PiratenKarte.Client.Services;

namespace PiratenKarte.Client.Pages;

public partial class Settings {
    [Inject]
    public required HttpClient Http { get; init; }
    [Inject]
    public required AppStateService AppStateService { get; init; }
    [Inject]
    public required AuthenticationStateService AuthStateService { get; init; }
    [Inject]
    public required NavigationManager NavManager { get; init; }

    private bool UseLocalStorage {
        get => AppStateService.Current.StoreStateLocally;
        set {
            if (AppStateService.Current.StoreStateLocally == value)
                return;

            AppStateService.Current.StoreStateLocally = value;
            AppStateService.Write();
        }
    }

    private void Back() => NavManager.NavigateTo("");

    private async Task Logout() {
        await AuthStateService.Invalidate();
        NavManager.NavigateTo("signin");
    }
}