using Microsoft.AspNetCore.Components;
using PiratenKarte.Client.Services;

namespace PiratenKarte.Client;

public partial class App {
    [Inject]
    public required AuthenticationStateService AuthStateService { get; set; }
    [Inject]
    public required NavigationManager NavigationManager { get; set; }

    protected override async Task OnInitializedAsync() {
        await AuthStateService.TryLoginFromStorage();

        if (!AuthStateService.IsAuthenticated && NavigationManager.ToBaseRelativePath(NavigationManager.Uri) != "signin") {
            NavigationManager.NavigateTo("/signin");
        }
    }
}