using Microsoft.AspNetCore.Components;
using PiratenKarte.Client.Services;

namespace PiratenKarte.Client.Pages;

public partial class Settings {
    [Inject]
    public required AppStateService StateService { get; init; }

    private bool UseLocalStorage {
        get => StateService.Current.StoreStateLocally;
        set {
            if (StateService.Current.StoreStateLocally == value)
                return;

            StateService.Current.StoreStateLocally = value;
            StateService.Write();
        }
    }
}