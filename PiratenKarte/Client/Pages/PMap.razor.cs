using FisSst.BlazorMaps;
using Microsoft.AspNetCore.Components;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
using System.Net.Http.Json;
using PiratenKarte.Shared;
using PiratenKarte.Client.Map;
using System.Globalization;
using PiratenKarte.Shared.RequestModels;
using PiratenKarte.Client.Services;

namespace PiratenKarte.Client.Pages;

public partial class PMap {
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
    public required AppStateService StateService { get; init; }
    [Inject]
    public required AuthenticationStateService AuthStateService { get; init; }

    [Parameter]
    public double? Latitude { get; set; }
    [Parameter]
    public double? Longitude { get; set; }
    [Parameter]
    public Guid? SetObject { get; set; }

    private bool MapRendered;
    private List<MapObject>? MapObjects;
    private List<StorageDefinition>? StorageDefinitions;

    private readonly List<MarkerContainer> Markers = new();

    private MapMode Mode;

    private readonly MapOptions Options = new() {
        DivId = "map",
        Center = new(52.1512, 9.9494),
        Zoom = 13,
        UrlTileLayer = "https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png",
        SubOptions = new MapSubOptions() {
            Attribution = "&copy; <a lhref='http://www.openstreetmap.org/copyright'>OpenStreetMap</a>",
            TileSize = 512,
            ZoomOffset = -1,
            MaxZoom = 19,
        }
    };

    protected override async Task OnInitializedAsync() {
        if (AuthStateService.GetLoginState() != LoginState.LoggedIn) {
            NavManager.NavigateTo("/signin");
            return;
        }

        if (SetObject != null)
            Mode = MapMode.Chose;

        if (AuthStateService.HasExact("objects_read"))
            MapObjects = await Http.GetFromJsonAsync<List<MapObject>>("MapObjects/GetMap");
        if (AuthStateService.HasExact("storagedefinitions_read"))
            StorageDefinitions = await Http.GetFromJsonAsync<List<StorageDefinition>>("StorageDefinitions/GetAll");

        if (MapRendered)
            await CreateMarkersAsync();
    }

    private async Task AfterMapRendered() {
        if (Latitude != null && Longitude != null) {
            await Map.SetView(new LatLng(Latitude.Value, Longitude.Value));
            await Map.SetZoom(13);

            await UpdateState(null);
        } else {
            await Map.SetView(new LatLng(StateService.Current.MapPosition.Latitude,
                StateService.Current.MapPosition.Longitude));
            await Map.SetZoom(StateService.Current.MapZoom);
        }

        await Map.OnZoomLevelChange(UpdateState);
        await Map.OnMouseUp(UpdateState);

        MapRendered = true;
        if (MapObjects != null && StorageDefinitions != null)
            await CreateMarkersAsync();
    }

    private async Task UpdateState(Event? e) {
        var center = await Map.GetCenter();
        StateService.Current.MapPosition = new LatitudeLongitude(center.Lat, center.Lng);
        StateService.Current.MapZoom = await Map.GetZoom();
        StateService.Write();
    }

    private async Task CreateMarkersAsync() {
        if (MapObjects != null) {
            foreach (var mo in MapObjects) {
                var container = new PosterMarkerContainer(mo, MarkerFactory, DivIconFactory);
                var marker = await container.GetMarkerAsync();
                await marker.AddTo(Map);

                var id = mo.Id;
                await marker.OnClick(e => {
                    NavManager.NavigateTo($"/mapobjects/view/{id}");
                    return Task.CompletedTask;
                });

                Markers.Add(container);
            }
        }

        if (StorageDefinitions != null) {
            foreach (var sd in StorageDefinitions) {
                var container = new StorageMarkerContainer(sd, MarkerFactory, DivIconFactory);
                var marker = await container.GetMarkerAsync();
                await marker.AddTo(Map);

                var id = sd.Id;
                await marker.OnClick(e => {
                    NavManager.NavigateTo($"/storagedefinitions/view/{id}");
                    return Task.CompletedTask;
                });

                Markers.Add(container);
            }
        }
    }

    private void BtnModeClicked() {
        Mode = Mode == MapMode.View ? MapMode.Chose : MapMode.View;
        SetObject = null;

        StateHasChanged();
    }

    private async Task BtnAcceptClicked() {
        var center = await Map.GetCenter();

        if (SetObject == null) {
            //new
            NavManager.NavigateTo($"/mapobjects/create/{center.Lat.ToString(CultureInfo.InvariantCulture)}" +
                $"/{center.Lng.ToString(CultureInfo.InvariantCulture)}/");
        } else {
            //update
            await Http.PostAsJsonAsync("MapObjects/UpdatePosition", new SetObjectPosition {
                ObjectId = SetObject.Value,
                Position = new LatitudeLongitude(center.Lat, center.Lng)
            });

            NavManager.NavigateTo($"/mapobjects/view/{SetObject.Value}");
            SetObject = null;
        }
    }

    private enum MapMode {
        View,
        Chose
    }
}