using Microsoft.AspNetCore.Components;
using PiratenKarte.Shared;
using System.Net.Http.Json;

namespace PiratenKarte.Client.Pages.StorageDefinitions;

public partial class Create {
    [Inject]
    public required HttpClient Http { get; init; }
    [Inject]
    public required NavigationManager Nav { get; init; }

    private StorageDefinition Storage = new StorageDefinition { Name = "" };

    private bool Submitting;

    private string? NameError;

    private double Latitude {
        get => Storage.Position.Latitude;
        set => Storage.Position = new LatitudeLongitude(value, Storage.Position.Longitude);
    }

    private double Longitude {
        get => Storage.Position.Longitude;
        set => Storage.Position = new LatitudeLongitude(Storage.Position.Latitude, value);
    }

    protected override string PermissionFilter => "storagedefinitions_create";

    private void Reset() {
        Storage = new StorageDefinition { Name = "" };
        NameError = null;
    }

    private async Task SaveObject() {
        NameError = null;

        if (string.IsNullOrEmpty(Storage.Name)) {
            NameError = "Name muss gesetzt sein!";
            StateHasChanged();
            return;
        }

        Submitting = true;
        var result = await Http.PostAsJsonAsync("StorageDefinitions/Create", Storage);
        var str = await result.Content.ReadAsStringAsync();
        var id = Guid.Parse(str.Trim('\"'));

        Nav.NavigateTo($"/storagedefinitions/view/{id}/");
        Reset();
        Submitting = false;
    }
}