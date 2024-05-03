using FisSst.BlazorMaps;
using Microsoft.AspNetCore.Components;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;
using PiratenKarte.Shared;
using PiratenKarte.Client.Map;
using System.Globalization;
using PiratenKarte.Shared.RequestModels;
using PiratenKarte.Client.Services;
using Microsoft.JSInterop;
using Blazored.Modal.Services;
using PiratenKarte.Client.Pages.SharedSubPages;
using Blazored.Modal;

namespace PiratenKarte.Client.Pages;

public partial class PMap {
    [AllowNull]
    private FisSst.BlazorMaps.Map Map;

    [Inject]
    public required IMarkerFactory MarkerFactory { get; init; }
    [Inject]
    public required IDivIconFactory DivIconFactory { get; init; }

    [Inject]
    public required IGeolocationService GeolocationService { get; init; }

    [Inject]
    public required HttpClient Http { get; init; }
    [Inject]
    public required NavigationManager NavManager { get; init; }
    [Inject]
    public required AppStateService AppStateService { get; init; }
    [Inject]
    public required AuthenticationStateService AuthStateService { get; init; }

    [Parameter]
    public double? Latitude { get; set; }
    [Parameter]
    public double? Longitude { get; set; }
    [Parameter]
    public Guid? SetObject { get; set; }

    [CascadingParameter]
    public required IModalService ModalService { get; init; }

    private MapMode Mode;

    private bool MapRendered;
    private List<MapObjectDTO>? MapObjects;
    private List<StorageDefinitionDTO>? StorageDefinitions;
    private List<MarkerStyleDTO>? MarkerStyles;

    private readonly Dictionary<Guid, MarkerContainer> Markers = [];

    private CustomPositionMarkerContainer? GPSMarkerContainer;
    private CustomPositionMarkerContainer? SelectionContainer;

    private bool GPSActive;
    private LatLng? GPSPosition;

    private static PeriodicTimer? MarkerUpdateTimer;

    private readonly PositionOptions _positionOptions = new() {
        EnableHighAccuracy = true,
        MaximumAge = 1_000,
        Timeout = 15_000
    };

    private readonly MapOptions Options = new() {
        DivId = "map",
        Center = new LatLng(0, 0),
        Zoom = 7,
        UrlTileLayer = "https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png",
        SubOptions = new MapSubOptions() {
            Attribution = "&copy; <a lhref='http://www.openstreetmap.org/copyright'>OpenStreetMap</a>",
            TileSize = 256,
            MaxZoom = 19
        }
    };

    protected override async Task OnInitializedAsync() {
        while (!AuthStateService.TriedAuthenticating)
            await Task.Delay(10);

        if (AuthStateService.GetLoginState() != LoginState.LoggedIn)
            return;

        if (SetObject != null)
            Mode = MapMode.Chose;

        await Reload();
    }

    private async Task Reload() {
        if (AuthStateService.HasExact("objects_read"))
            MapObjects = await Http.GetFromJsonAsync<List<MapObjectDTO>>("MapObjects/GetMap");
        if (AuthStateService.HasExact("storagedefinitions_read"))
            StorageDefinitions = await Http.GetFromJsonAsync<List<StorageDefinitionDTO>>("StorageDefinitions/GetAll");

        var styleIds = new HashSet<Guid>();

        if (MapObjects != null) {
            foreach (var mo in MapObjects) {
                styleIds.Add(mo.MarkerStyleId);
            }
        }

        var response = await Http.PostAsJsonAsync("MarkerStyles/GetForDisplay", styleIds.ToList());
        MarkerStyles = await response.Content.ReadFromJsonAsync<List<MarkerStyleDTO>>() ?? [];

        if (MapRendered)
            await CreateMarkersAsync();
    }

    private async Task AfterMapRendered() {
        await Map.OnZoomLevelChange(UpdateState);
        await Map.OnMouseUp(UpdateState);

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

        MapRendered = true;
        await CreateMarkersAsync();

        if (Latitude != null && Longitude != null) {
            await Map.SetView(new LatLng(Latitude.Value, Longitude.Value));
            await Map.SetZoom(13);

            if (SetObject != null)
                await UpdateSelectionMarker();

            await UpdateState(null);
        } else {
            await Map.SetView(new LatLng(AppStateService.Current.MapPosition.Latitude,
                AppStateService.Current.MapPosition.Longitude));
            await Map.SetZoom(AppStateService.Current.MapZoom);
        }
    }

    private async Task UpdateState(Event? e) {
        var center = await ClampMap();
        if (center == null)
            return;

        if (SelectionContainer != null)
            await SelectionContainer.SetPosition(center);

        AppStateService.Current.MapPosition = new LatitudeLongitudeDTO(center.Lat, center.Lng);
        AppStateService.Current.MapZoom = await Map.GetZoom();
        AppStateService.Write();
        StateHasChanged();
    }

    private async Task<LatLng?> ClampMap() {
        var center = await Map.GetCenter();
        if (center == null)
            return null;

        center.Lat = Math.Clamp(center.Lat, -90, 90);
        center.Lng = Math.Clamp(center.Lng, -180, 180);

        await Map.SetView(center);
        return center;
    }

    private async Task CreateMarkersAsync() {
        if (MapObjects != null) {
            foreach (var mo in MapObjects) {
                if (Markers.TryGetValue(mo.Id, out var existingMarker)) {
                    var style = MarkerStyles?.Find(s => s.Id == mo.MarkerStyleId);

                    if (existingMarker is StyledMarkerContainer styledMarker) {
                        // StyledMarker - always requires rebuild
                        await DeleteMarker(existingMarker, mo.Id);
                        await BuildMarker(mo);
                    } else if (existingMarker is PosterMarkerContainer posterMarker && style != null) {
                        // StyledMarker but old Marker was non-styled, requires rebuild

                        await DeleteMarker(existingMarker, mo.Id);
                        await BuildMarker(mo);
                    } else {
                        // All else we can just make sure the Position is correct

                        await existingMarker.SetPosition(new LatLng(mo.LatLon.Latitude, mo.LatLon.Longitude));
                    }

                    continue;
                }

                await BuildMarker(mo);
            }

            var toDelete = new List<(Guid, MarkerContainer)>();
            foreach (var (id, existingMarker) in Markers) {
                if (existingMarker is StorageMarkerContainer)
                    continue;

                if (!MapObjects.Any(m => m.Id == id))
                    toDelete.Add((id, existingMarker));
            }

            foreach (var del in toDelete)
                await DeleteMarker(del.Item2, del.Item1);
        }

        if (StorageDefinitions != null) {
            foreach (var sd in StorageDefinitions) {
                if (Markers.ContainsKey(sd.Id))
                    continue;

                var container = new StorageMarkerContainer(sd, MarkerFactory, DivIconFactory);
                var marker = await container.GetMarkerAsync();
                await marker.AddTo(Map);

                var id = sd.Id;
                await marker.OnClick(e => {
                    NavManager.NavigateTo($"/storagedefinitions/view/{id}");
                    return Task.CompletedTask;
                });

                Markers.Add(sd.Id, container);
            }
        }
    }

    private async Task BuildMarker(MapObjectDTO mo) {
        var style = MarkerStyles?.Find(s => s.Id == mo.MarkerStyleId);
        MarkerContainer container;

        if (mo.MarkerStyleId == Guid.Empty || style == null) {
            container = new PosterMarkerContainer(mo, MarkerFactory, DivIconFactory);
        } else {
            container = new StyledMarkerContainer(mo, style, MarkerFactory, DivIconFactory);
        }

        var marker = await container.GetMarkerAsync();
        await marker.AddTo(Map);

        var id = mo.Id;
        await marker.OnClick(async _ => await MarkerClicked(id));

        Markers.Add(mo.Id, container);
    }

    private async Task DeleteMarker(MarkerContainer marker, Guid id) {
        await marker.RemoveFromMap(Map);
        Markers.Remove(id);
    }

    private async Task MarkerClicked(Guid id) {
        if (MapObjects == null)
            return;

        var obj = MapObjects.Find(mo => mo.Id == id);
        if (obj == null)
            return;

        var parameters = new ModalParameters()
            .Add(nameof(EditObjectModal.MapObjects), MapObjects)
            .Add(nameof(EditObjectModal.EditObject), obj)
            .Add(nameof(EditObjectModal.MarkerStylesMap), MarkerStyles);

        var modalRef = ModalService.Show<EditObjectModal>(parameters);
        await modalRef.Result;

        await Reload();
        return;
    }

    private async Task BtnModeClicked() {
        var currCenter = await Map.GetCenter();
        var parameters = new ModalParameters()
            .Add(nameof(SelectionMap.Latitude), currCenter.Lat)
            .Add(nameof(SelectionMap.Longitude), currCenter.Lng)
            .Add(nameof(SelectionMap.MapObjects), MapObjects)
            .Add(nameof(SelectionMap.MarkerStylesMap), MarkerStyles);

        var modalRef = ModalService.Show<SelectionMap>(parameters);
        await modalRef.Result;

        await Reload();
        return;

        Mode = Mode == MapMode.View ? MapMode.Chose : MapMode.View;
        SetObject = null;

        if (Mode == MapMode.Chose) {
            await UpdateSelectionMarker();
        } else if (Mode == MapMode.View && SelectionContainer != null) {
            await SelectionContainer.RemoveFromMap(Map);
        }

        await ClampMap();
        StateHasChanged();
    }

    private async Task UpdateSelectionMarker() {
        if (SelectionContainer == null) {
            SelectionContainer = new(await Map.GetCenter(),
                "selection-marker-dot", null, MarkerFactory, DivIconFactory);
            await SelectionContainer.GetMarkerAsync();
        }

        await SelectionContainer.SetPosition(await Map.GetCenter());
        await SelectionContainer.AddToMap(Map);
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
                Position = new LatitudeLongitudeDTO(center.Lat, center.Lng)
            });

            NavManager.NavigateTo($"/mapobjects/view/{SetObject.Value}");
            SetObject = null;
        }
    }

    private void BtnAcceptOSM() {
        AppStateService.Current.AcceptedOSM = true;
        AppStateService.Write();
    }

    private async Task BtnToggleGPS() {
        GPSActive = !GPSActive;

        if (GPSActive) {
            GeolocationService.GetCurrentPosition(OnPositionChanged, OnPositionError, _positionOptions);
        } else {
            if (GPSMarkerContainer != null)
                await GPSMarkerContainer.RemoveFromMap(Map);
        }
    }

    // Note on void instead of Task return Type: Has to be this way since WatchPosition requires Action
    private async void OnPositionChanged(GeolocationPosition position) {
        if (GPSActive) {
            GPSPosition = new LatLng(position.Coords.Latitude, position.Coords.Longitude);

            GeolocationService.GetCurrentPosition(OnPositionChanged, OnPositionError, _positionOptions);

            if (GPSMarkerContainer == null) {
                GPSMarkerContainer = new(GPSPosition,
                    "geo-marker-dot", null, MarkerFactory, DivIconFactory);
                await GPSMarkerContainer.GetMarkerAsync();
            }

            await GPSMarkerContainer.SetPosition(GPSPosition);
            await GPSMarkerContainer.AddToMap(Map);
            StateHasChanged();
        }
    }

    private void OnPositionError(GeolocationPositionError error) {
        GPSActive = false;
        GPSPosition = null;
        StateHasChanged();
    }

    private async Task JumpToGPS() => await Map.SetView(GPSPosition);

    private enum MapMode {
        View,
        Chose
    }
}