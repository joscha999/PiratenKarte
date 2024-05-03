using Blazored.Modal;
using Blazored.Modal.Services;
using FisSst.BlazorMaps;
using Microsoft.AspNetCore.Components;
using PiratenKarte.Client.Components.Modals;
using PiratenKarte.Client.Map;
using PiratenKarte.Client.Services;
using PiratenKarte.Shared;
using PiratenKarte.Shared.Validation;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;

namespace PiratenKarte.Client.Pages.SharedSubPages;

public partial class EditObjectModal {
    [AllowNull]
    public FisSst.BlazorMaps.Map Map;

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

    [CascadingParameter]
    public required IModalService ModalService { get; init; }

    [Parameter]
    public required List<MapObjectDTO> MapObjects { get; set; }
    [Parameter]
    public required MapObjectDTO EditObject { get; set; }
    [Parameter]
    public required List<MarkerStyleDTO> MarkerStylesMap { get; set; }

    private List<MarkerStyleDTO>? MarkerStyles;
    private MarkerStyleDTO? SelectedStyle;

    private readonly List<MarkerContainer> Markers = [];
    private CustomPositionMarkerContainer? SelectionContainer;

    private static PeriodicTimer? MarkerUpdateTimer;

    private bool Submitting;
    private readonly ErrorBag ErrorBag = new();

    private List<GroupDTO>? Groups;
    private Guid SelectedGroupId;

    private bool MapRendered;

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

        if (Groups?.Count > 0)
            SelectedGroupId = Groups.OrderBy(g => g.Name).First().Id;

        MarkerStyles = await Http.GetFromJsonAsync<List<MarkerStyleDTO>>("MarkerStyles/GetAll") ?? [];

        // Marker style isn't directly visible by current user but we do allow for it to be used if the object uses it already
        if (!MarkerStyles.Any(m => m.Id == EditObject.MarkerStyleId)) {
            response = await Http.PostAsJsonAsync("MarkerStyles/GetSingle", EditObject.MarkerStyleId);
            var style = await response.Content.ReadFromJsonAsync<MarkerStyleDTO>();

            if (style != null)
                MarkerStyles.Add(style);
        }

        SelectedStyle = MarkerStyles.Find(m => m.Id == EditObject.MarkerStyleId);

        if (MapRendered)
            await UpdateSelectionMarker();
    }

    private async Task AfterMapRendered() {
        await Map.SetView(new LatLng(EditObject.LatLon.Latitude, EditObject.LatLon.Longitude));

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
        await UpdateSelectionMarker();
    }

    private async Task UpdateSelectionMarker() {
        if (SelectionContainer != null)
            await SelectionContainer.RemoveFromMap(Map);

        SelectionContainer = new(await Map.GetCenter(),
            SelectedStyle == null ? "selection-marker-dot" : "", SelectedStyle, MarkerFactory, DivIconFactory);
        await SelectionContainer.GetMarkerAsync();

        await SelectionContainer.SetPosition(await Map.GetCenter());
        await SelectionContainer.AddToMap(Map);
    }

    private async Task CreateMarkersAsync() {
        if (MapObjects != null) {
            foreach (var mo in MapObjects) {
                if (mo == EditObject)
                    continue;

                MarkerContainer container;
                var style = MarkerStylesMap.Find(m => m.Id == mo.MarkerStyleId);

                if (mo.MarkerStyleId == Guid.Empty || style == null) {
                    container = new PosterMarkerContainer(mo, MarkerFactory, DivIconFactory);
                } else {
                    container = new StyledMarkerContainer(mo, style, MarkerFactory, DivIconFactory);
                }
                var marker = await container.GetMarkerAsync();
                await marker.AddTo(Map);
                Markers.Add(container);
            }
        }
    }

    private async Task SetStyle(MarkerStyleDTO markerStyleDTO) {
        SelectedStyle = markerStyleDTO;
        await UpdateSelectionMarker();
    }

    private async Task Close() => await Modal.CloseAsync();

    private void ViewDetails() => NavManager.NavigateTo($"/mapobjects/view/{EditObject.Id}");

    private async Task SaveObject() {
        ErrorBag.Clear();

        if (string.IsNullOrEmpty(EditObject.Name))
            ErrorBag.Fail("Object.Name", "Name muss angegeben werden.");
        if (SelectedGroupId == Guid.Empty)
            ErrorBag.Fail("Object.Group", "Gruppe muss angegeben werden.");

        if (ErrorBag.AnyError) {
            StateHasChanged();
            return;
        }

        Submitting = true;

        var mapCenter = await Map.GetCenter();
        EditObject.LatLon = new LatitudeLongitudeDTO(mapCenter.Lat, mapCenter.Lng);
        EditObject.GroupId = SelectedGroupId;

        EditObject.MarkerStyleId = SelectedStyle?.Id ?? Guid.Empty;

        await Http.PostAsJsonAsync("MapObjects/Update", EditObject);
        Submitting = false;
        await Close();
    }

    private async Task DeleteObject() {
        var param = new ModalParameters()
            .Add(nameof(ConfirmModal.Title), "Löschen Bestätigen")
            .Add(nameof(ConfirmModal.Content), "Soll das Plakat wirklich gelöscht werden? ");
        var confirmModal = ModalService.Show<ConfirmModal>("", param);
        var result = await confirmModal.Result;

        if (result.Cancelled)
            return;

        Submitting = true;
        await Http.PostAsJsonAsync("MapObjects/Delete", EditObject.Id);
        Submitting = false;
        await Close();
    }
}