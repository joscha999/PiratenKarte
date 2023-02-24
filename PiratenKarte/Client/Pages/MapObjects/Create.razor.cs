using Microsoft.AspNetCore.Components;
using PiratenKarte.Shared;
using PiratenKarte.Shared.RequestModels;
using System.Globalization;
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

    private MapObject Object = new MapObject { Name = "" };
    private List<StorageDefinition>? StorageDefinitions;

    private Guid? SelectedStorageId;
    private int CreateManyCount = 1;

    private bool Submitting;

    private string? NameError;

    protected override async Task OnInitializedAsync() {
        StorageDefinitions = await Http.GetFromJsonAsync<List<StorageDefinition>>("StorageDefinitions/GetAll");

        await base.OnInitializedAsync();
    }

    private async Task BtnSubmitClicked() => await SaveObject(false);

    private async Task BtnSubmitToMapClicked() => await SaveObject(true);

    private void Reset() {
        Latitude = 0;
        Longitude = 0;
        Object = new MapObject { Name = "" };
        NameError = null;
    }

    private async Task SaveObject(bool toMap) {
        NameError = null;

        if (string.IsNullOrWhiteSpace(Object.Name)) {
            NameError = "Name muss gesetzt sein!";
            StateHasChanged();
            return;
        }

        if (CreateManyCount <= 0)
            throw new InvalidOperationException();

        Submitting = true;
        Object.LatLon = new LatitudeLongitude(Latitude, Longitude);

        if (CreateManyCount == 1) {
            var result = await Http.PostAsJsonAsync("MapObjects/Create", new CreateNewObject {
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
                Count = CreateManyCount
            });

            Nav.NavigateTo(toMap ? "" : "/mapobjects/list");
        }

        Reset();
        Submitting = false;
    }
}