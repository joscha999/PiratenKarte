using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using PiratenKarte.Client.Components.Modals;
using PiratenKarte.Client.Models;
using PiratenKarte.Shared;
using PiratenKarte.Shared.RequestModels;
using PiratenKarte.Shared.Validation;
using System.Globalization;
using System.Net.Http.Json;

namespace PiratenKarte.Client.Pages.MapObjects;

public partial class View {
    [CascadingParameter]
    public required IModalService Modal { get; init; }

    [Parameter]
    public Guid Id { get; set; }

    [Inject]
    public required HttpClient Http { get; init; }
    [Inject]
    public required AppSettings Settings { get; init; }

    protected override string PermissionFilter => "objects_read";

    private MapObjectDTO? Object;
    private List<StorageDefinitionDTO>? StorageDefinitions;
    private byte[]? QRCode;

    private GroupDTO? Group;

    private string NewCommentContent = "";

    private Guid? _selectedStorageId;
    private Guid? SelectedStorageId {
        get => _selectedStorageId;
        set {
            if (_selectedStorageId == value)
                return;

            _selectedStorageId = value;
        }
    }

    private double? ObjLatitude {
        get => Object?.LatLon.Latitude;
        set {
            if (Object == null)
                return;

            Object.LatLon = new LatitudeLongitudeDTO(value ?? 0, Object.LatLon.Longitude);
        }
    }

    private double? ObjLongitude {
        get => Object?.LatLon.Longitude;
        set {
            if (Object == null)
                return;

            Object.LatLon = new LatitudeLongitudeDTO(Object.LatLon.Latitude, value ?? 0);
        }
    }

    private readonly ErrorBag ErrorBag = new ErrorBag();
    private bool Submitting;

    private List<MapObjectLogEntryDTO>? LogEntries;

    protected override async Task OnParametersSetAsync() {
        QRCode = QrCodeGenerator.Generate(Id, Settings.Domain);

        await Reload();
        await base.OnParametersSetAsync();
    }

    private async Task Reload() {
        Submitting = true;
        Object = await Http.GetFromJsonAsync<MapObjectDTO>($"MapObjects/View?id={Id}");
        StorageDefinitions = await Http.GetFromJsonAsync<List<StorageDefinitionDTO>>("StorageDefinitions/GetForUser");
        _selectedStorageId = Object!.Storage?.Id;

        var response = await Http.PostAsJsonAsync("Group/GetSingle", Object.GroupId);
        Group = await response.Content.ReadFromJsonAsync<GroupDTO>();

        if (AuthStateService.HasExact("log_read"))
            LogEntries = await Http.GetFromJsonAsync<List<MapObjectLogEntryDTO>>($"MapObjects/GetLog?id={Id}");

        Submitting = false;
        StateHasChanged();
    }

    private async Task CreateNewComment() {
        Submitting = true;

        await Http.PostAsJsonAsync("MapObjects/AddComment", new NewObjectComment {
            ObjectId = Id,
            Comment = new ObjectCommentDTO {
                Content = NewCommentContent,
                InsertionTime = DateTimeOffset.Now
            }
        });

        NewCommentContent = "";
        await Reload();
        Submitting = false;
    }

    private async Task DeleteComment(Guid guid) {
        Submitting = true;

        await Http.PostAsJsonAsync("MapObjects/DeleteComment", new DeleteObjectComment {
            ObjectId = Id,
            CommentId = guid
        });

        await Reload();
        Submitting = false;
    }

    private async Task UpdateStorage() {
        Submitting = true;

        await Http.PostAsJsonAsync("MapObjects/SetStorage", new SetObjectStorage {
            ObjectId = Id,
            StorageId = SelectedStorageId
        });

        await Reload();
        Submitting = false;
    }

    private async Task UpdateObject() {
        ErrorBag.Clear();

        if (string.IsNullOrWhiteSpace(Object?.Name))
            ErrorBag.Fail("Object.Name", "Name muss gesetzt sein!");
        if (ObjLatitude < -90 || ObjLatitude > 90 || ObjLongitude < -180 || ObjLongitude > 180) {
            ErrorBag.Fail("Object.Position", "L�ngengrad muss zwischen -90 und 90 liegen. " +
                "Breitengrad muss zwischen -180 und 180 liegen");
        }

        if (ErrorBag.AnyError) {
            StateHasChanged();
            return;
        }

        Submitting = true;

        await Http.PostAsJsonAsync("MapObjects/UpdateEx", Object);

        await Reload();
        Submitting = false;
    }

    private void UpdateObjectPositionViaMap() {
        if (Object == null)
            return;

        NavManager.NavigateTo($"/{Object.LatLon.Latitude.ToString(CultureInfo.InvariantCulture)}"
                            + $"/{Object.LatLon.Longitude.ToString(CultureInfo.InvariantCulture)}"
                            + $"/{Object.Id}/");
    }

    private void Back() => NavManager.NavigateTo("/mapobjects/list");

    private async Task Delete() {
        if (Object == null) {
            Back();
            return;
        }

        var param = new ModalParameters()
            .Add(nameof(ConfirmModal.Title), "L�schen Best�tigen")
            .Add(nameof(ConfirmModal.Content), $"Soll \"{Object.Name}\" wirklich gel�scht werden?");
        var confirmModal = Modal.Show<ConfirmModal>("", param);
        var result = await confirmModal.Result;

        if (result.Cancelled)
            return;

        Submitting = true;
        await Http.PostAsJsonAsync("MapObjects/Delete", Object.Id);

        Submitting = false;
        Back();
    }
}