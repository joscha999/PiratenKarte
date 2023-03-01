using Microsoft.AspNetCore.Components;
using PiratenKarte.Shared;
using PiratenKarte.Shared.Validation;
using System.Net.Http.Json;

namespace PiratenKarte.Client.Pages.StorageDefinitions;

public partial class Create {
    [Inject]
    public required HttpClient Http { get; init; }
    [Inject]
    public required NavigationManager Nav { get; init; }

    private StorageDefinition Storage = new StorageDefinition { Name = "" };

    private bool Submitting;

    private ErrorBag ErrorBag = new ErrorBag();

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
        ErrorBag.Clear();
    }

    private async Task SaveObject() {
        ErrorBag.Clear();

        if (string.IsNullOrWhiteSpace(Storage.Name))
            ErrorBag.Fail("Storage.Name", "Name muss gesetzt sein!");
        if (Latitude < -90 || Latitude > 90 || Longitude < -180 || Longitude > 180) {
            ErrorBag.Fail("Storage.Position", "Längengrad muss zwischen -90 und 90 liegen. " +
                "Breitengrad muss zwischen -180 und 180 liegen");
        }

        if (ErrorBag.AnyError) {
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