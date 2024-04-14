using Blazored.Modal;
using Blazored.Modal.Services;
using FisSst.BlazorMaps;
using Microsoft.AspNetCore.Components;
using PiratenKarte.Client.Map;
using PiratenKarte.Client.Services;
using PiratenKarte.Shared;
using PiratenKarte.Shared.RequestModels;
using PiratenKarte.Shared.Validation;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;

namespace PiratenKarte.Client.Pages.SharedSubPages;

public partial class SelectionMap {
    [AllowNull]
    private FisSst.BlazorMaps.Map Map;

    [Inject]
    public required IMarkerFactory MarkerFactory { get; init; }
    [Inject]
    public required IDivIconFactory DivIconFactory { get; init; }

    [Inject]
    public required HttpClient Http { get; init; }
    [Inject]
    public required NavigationManager NavManager { get; init; }
    [Inject]
    public required AppStateService AppStateService { get; init; }
    [Inject]
    public required AuthenticationStateService AuthStateService { get; init; }

    [CascadingParameter]
    public required BlazoredModalInstance Modal { get; init; }

    [Parameter]
    public double Latitude { get; set; }
    [Parameter]
    public double Longitude { get; set; }
    [Parameter]
    public required List<MapObjectDTO> MapObjects { get; set; }

    private readonly List<MarkerContainer> Markers = [];
    private CustomPositionMarkerContainer? SelectionContainer;

    private MapObjectDTO NewObject = new MapObjectDTO {
        Name = "Plakat"
    };

    private static PeriodicTimer? MarkerUpdateTimer;

    private bool Submitting;
    private readonly ErrorBag ErrorBag = new();

    private List<GroupDTO>? Groups;
    private Guid SelectedGroupId;

    private static Guid? LastSelectedGroupId;

    private readonly MapOptions Options = new() {
        DivId = "selectionMap",
        Center = new LatLng(52.1543665, 9.9447473),
        Zoom = 19,
        UrlTileLayer = "https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png",
        SubOptions = new MapSubOptions() {
            Attribution = "&copy; <a lhref='http://www.openstreetmap.org/copyright'>OpenStreetMap</a>",
            TileSize = 256,
            MaxZoom = 19
        }
    };

    protected override async Task OnParametersSetAsync() {
        if (!AuthStateService.IsAuthenticated)
            return;

        var response = await Http.PostAsJsonAsync("Group/GetForUser", AuthStateService.User.Id);
        Groups = await response.Content.ReadFromJsonAsync<List<GroupDTO>>();

        if (Groups?.Count > 0) {
            Console.WriteLine(LastSelectedGroupId);
            if (LastSelectedGroupId != null && Groups.Find(g => g.Id == LastSelectedGroupId) != null) {
                SelectedGroupId = LastSelectedGroupId.Value;
            } else {
                SelectedGroupId = Groups.OrderBy(g => g.Name).First().Id;
            }
        }
    }

    private async Task AfterMapRendered() {
        await Map.SetView(new LatLng(Latitude, Longitude));

        // This should be fire and forget
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
        Task.Factory.StartNew(async () => {
            MarkerUpdateTimer?.Dispose();
            MarkerUpdateTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(50));

            while (await MarkerUpdateTimer.WaitForNextTickAsync()) {
                if (SelectionContainer != null)
                    await SelectionContainer.SetPosition(await Map.GetCenter());
            }
        });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

        await CreateMarkersAsync();
        await UpdateSelectionMarker();
    }

    private async Task UpdateSelectionMarker() {
        if (SelectionContainer == null) {
            SelectionContainer = new(await Map.GetCenter(),
                "selection-marker-dot", MarkerFactory, DivIconFactory);
            await SelectionContainer.GetMarkerAsync();
        }

        await SelectionContainer.SetPosition(await Map.GetCenter());
        await SelectionContainer.AddToMap(Map);
    }

    private async Task CreateMarkersAsync() {
        if (MapObjects != null) {
            foreach (var mo in MapObjects) {
                var container = new PosterMarkerContainer(mo, MarkerFactory, DivIconFactory);
                var marker = await container.GetMarkerAsync();
                await marker.AddTo(Map);
                Markers.Add(container);
            }
        }
    }

    private async Task Close() => await Modal.CloseAsync();

    private async Task Create() {
        await SaveObject();
        await Close();
    }

    private async Task CreateAndView() {
        var id = await SaveObject();
        NavManager.NavigateTo($"/mapobjects/view/{id}");
    }

    private async Task<Guid?> SaveObject() {
        ErrorBag.Clear();

        if (string.IsNullOrEmpty(NewObject.Name))
            ErrorBag.Fail("Object.Name", "Name muss angegeben werden.");
        if (SelectedGroupId == Guid.Empty)
            ErrorBag.Fail("Object.Group", "Gruppe muss angegeben werden.");

        if (ErrorBag.AnyError) {
            StateHasChanged();
            return null;
        }

        Submitting = true;
        LastSelectedGroupId = SelectedGroupId;

        var mapCenter = await Map.GetCenter();
        NewObject.LatLon = new LatitudeLongitudeDTO(mapCenter.Lat, mapCenter.Lng);
        NewObject.GroupId = SelectedGroupId;

        var result = await Http.PostAsJsonAsync("MapObjects/CreateSingle", new CreateNewObject {
            Object = NewObject
        });

        var str = await result.Content.ReadAsStringAsync();
        Submitting = false;
        return Guid.Parse(str.Trim('\"'));
    }
}