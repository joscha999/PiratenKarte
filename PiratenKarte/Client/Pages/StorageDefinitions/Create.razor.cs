using Microsoft.AspNetCore.Components;
using PiratenKarte.Client.Extensions;
using PiratenKarte.Client.Services;
using PiratenKarte.Shared;
using PiratenKarte.Shared.Validation;
using System.Net.Http.Json;

namespace PiratenKarte.Client.Pages.StorageDefinitions;

public partial class Create {
    [Inject]
    public required HttpClient Http { get; init; }
    [Inject]
    public required NavigationManager Nav { get; init; }

    private StorageDefinitionDTO Storage = new StorageDefinitionDTO { Name = "" };

    private bool Submitting;

    private ErrorBag ErrorBag = new ErrorBag();

    private List<GroupDTO>? Groups;
    private Guid SelectedGroupId;

    private double Latitude {
        get => Storage.Position.Latitude;
        set => Storage.Position = new LatitudeLongitudeDTO(value, Storage.Position.Longitude);
    }

    private double Longitude {
        get => Storage.Position.Longitude;
        set => Storage.Position = new LatitudeLongitudeDTO(Storage.Position.Latitude, value);
    }

    protected override string PermissionFilter => "storagedefinitions_create";

    protected override async Task OnParametersSetAsync() {
        if (!AuthStateService.IsAuthenticated)
            return;

        var response = await Http.PostAsJsonAsync("Group/GetForUser", AuthStateService.User.Id);
        Groups = await response.Content.ReadFromJsonAsync<List<GroupDTO>>();

        if (Groups?.Count > 0)
            SelectedGroupId = Groups[0].Id;
    }

    private void Reset() {
        Storage = new StorageDefinitionDTO { Name = "" };
        ErrorBag.Clear();
    }

    private async Task SaveObject() {
        ErrorBag.Clear();

        if (string.IsNullOrWhiteSpace(Storage.Name))
            ErrorBag.Fail("Storage.Name", "Name muss angegeben werden.");
        if (Latitude < -90 || Latitude > 90 || Longitude < -180 || Longitude > 180) {
            ErrorBag.Fail("Storage.Position", "Längengrad muss zwischen -90 und 90 liegen. " +
                "Breitengrad muss zwischen -180 und 180 liegen");
        }
        if (SelectedGroupId == Guid.Empty)
            ErrorBag.Fail("Storage.Group", "Gruppe muss angegeben werden.");

        if (ErrorBag.AnyError) {
            StateHasChanged();
            return;
        }

        Submitting = true;
        Storage.GroupId = SelectedGroupId;

        var result = await Http.PostAsJsonAsync("StorageDefinitions/Create", Storage);
        var str = await result.Content.ReadAsStringAsync();
        var id = Guid.Parse(str.Trim('\"'));

        Nav.NavigateTo($"/storagedefinitions/view/{id}/");
        Reset();
        Submitting = false;
    }
}