using Microsoft.AspNetCore.Components;
using PiratenKarte.Shared;
using PiratenKarte.Shared.RequestModels;
using PiratenKarte.Shared.Validation;
using System.Net.Http.Json;

namespace PiratenKarte.Client.Pages.MapObjects;

public partial class Create {
    [Parameter]
    public double Latitude { get; set; }
    [Parameter]
    public double Longitude { get; set; }

    [Inject]
    public required HttpClient Http { get; init; }
    [Inject]
    public required NavigationManager Nav { get; init; }

    protected override string PermissionFilter => "objects_create";

    private MapObjectDTO Object = new() { Name = "Plakat" };
    private List<StorageDefinitionDTO>? StorageDefinitions;

    private List<GroupDTO>? Groups;
    private Guid SelectedGroupId;

    private Guid? SelectedStorageId;
    private int CreateManyCount = 1;

    private bool Submitting;

    private readonly ErrorBag ErrorBag = new();

    protected override async Task OnInitializedAsync() {
        StorageDefinitions = await Http.GetFromJsonAsync<List<StorageDefinitionDTO>>("StorageDefinitions/GetForUser");

        await base.OnInitializedAsync();
    }

    protected override async Task OnParametersSetAsync() {
        if (!AuthStateService.IsAuthenticated)
            return;

        var response = await Http.PostAsJsonAsync("Group/GetForUser", AuthStateService.User.Id);
        Groups = await response.Content.ReadFromJsonAsync<List<GroupDTO>>();

        if (Groups?.Count > 0)
            SelectedGroupId = Groups.OrderBy(g => g.Name).First().Id;
    }

    private async Task BtnSubmitClicked() => await SaveObject(false);

    private async Task BtnSubmitToMapClicked() => await SaveObject(true);

    private void Reset() {
        Latitude = 0;
        Longitude = 0;
        Object = new MapObjectDTO { Name = "Plakat" };
        ErrorBag.Clear();
    }

    private async Task SaveObject(bool toMap) {
        ErrorBag.Clear();

        if (string.IsNullOrWhiteSpace(Object.Name))
            ErrorBag.Fail("Object.Name", "Name muss angegeben werden.");
        if (Latitude < -90 || Latitude > 90 || Longitude < -180 || Longitude > 180) {
            ErrorBag.Fail("Object.Position", "Längengrad muss zwischen -90 und 90 liegen. " +
                "Breitengrad muss zwischen -180 und 180 liegen");
        }
        if (SelectedGroupId == Guid.Empty)
            ErrorBag.Fail("Object.Group", "Gruppe muss angegeben werden.");

        if (ErrorBag.AnyError) {
            StateHasChanged();
            return;
        }

        if (CreateManyCount <= 0)
            throw new InvalidOperationException();

        Submitting = true;
        Object.LatLon = new LatitudeLongitudeDTO(Latitude, Longitude);
        Object.GroupId = SelectedGroupId;

        if (CreateManyCount == 1) {
            var result = await Http.PostAsJsonAsync("MapObjects/CreateSingle", new CreateNewObject {
                Object = Object,
                StorageId = SelectedStorageId
            });

            var str = await result.Content.ReadAsStringAsync();
            var id = Guid.Parse(str.Trim('\"'));

            Nav.NavigateTo(toMap ? "" : $"/mapobjects/view/{id}");
        } else {
            await Http.PostAsJsonAsync("MapObjects/CreateMany", new CreateNewObjectBulk {
                Template = Object,
                StorageId = SelectedStorageId,
                Count = CreateManyCount,
                GroupId = SelectedGroupId
            });

            Nav.NavigateTo(toMap ? "" : "/mapobjects/list");
        }

        Reset();
        Submitting = false;
    }
}