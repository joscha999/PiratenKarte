using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using PiratenKarte.Client.Components.Modals;
using PiratenKarte.Client.Models;
using PiratenKarte.Shared;
using PiratenKarte.Shared.RequestModels;
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
    public required NavigationManager NavManager { get; init; }
    [Inject]
    public required AppSettings Settings { get; init; }

    private MapObject? Object;
    private List<StorageDefinition>? StorageDefinitions;
    private byte[]? QRCode;

    private string NewCommentName = "";
    private string NewCommentContent = "";

    private Guid? _selectedStorageId;
    private Guid? SelectedStorageId {
        get => _selectedStorageId;
        set {
            if (_selectedStorageId == value)
                return;

            _selectedStorageId = value;
            Console.WriteLine(value == null ? "NULL" : value);
        }
    }

    private double? ObjLatitude {
        get => Object?.LatLon.Latitude;
        set {
            if (Object == null)
                return;

            Object.LatLon = new LatitudeLongitude(value ?? 0, Object.LatLon.Longitude);
        }
    }

    private double? ObjLongitude {
        get => Object?.LatLon.Longitude;
        set {
            if (Object == null)
                return;

            Object.LatLon = new LatitudeLongitude(Object.LatLon.Latitude, value ?? 0);
        }
    }

    private string? NameError;
    private bool Submitting;

    protected override async Task OnParametersSetAsync() {
        QRCode = QrCodeGenerator.Generate(Id, Settings.Domain);

        await Reload();
        await base.OnParametersSetAsync();
    }

    private async Task Reload() {
        Submitting = true;
        Object = await Http.GetFromJsonAsync<MapObject>($"MapObjects/Get?id={Id}");
        StorageDefinitions = await Http.GetFromJsonAsync<List<StorageDefinition>>("StorageDefinitions/GetAll");
        _selectedStorageId = Object!.Storage?.Id;

        Submitting = false;
        StateHasChanged();
    }

    private async Task CreateNewComment() {
        Submitting = true;

        await Http.PostAsJsonAsync("MapObjects/AddComment", new NewObjectComment {
            ObjectId = Id,
            Comment = new ObjectComment {
                User = NewCommentName,
                Note = NewCommentContent,
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
        NameError = null;

        if (string.IsNullOrWhiteSpace(Object?.Name)) {
            NameError = "Name muss gesetzt sein!";
            StateHasChanged();
            return;
        }

        Submitting = true;

        await Http.PostAsJsonAsync("MapObjects/Update", Object);

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
            .Add(nameof(ConfirmModal.Title), "Löschen Bestätigen")
            .Add(nameof(ConfirmModal.Content), $"Soll \"{Object.Name}\" wirklich gelöscht werden?");
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