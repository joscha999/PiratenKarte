using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using PiratenKarte.Client.Components.Modals;
using PiratenKarte.Shared;
using PiratenKarte.Shared.Validation;
using System.Net.Http.Json;

namespace PiratenKarte.Client.Pages.StorageDefinitions;

public partial class View {
    [CascadingParameter]
    public required IModalService Modal { get; init; }

    [Parameter]
    public Guid Id { get; set; }

    [Inject]
    public required HttpClient Http { get; init; }

    private StorageDefinitionDTO? Storage;

    private double? ObjLatitude {
        get => Storage?.Position.Latitude;
        set {
            if (Storage == null)
                return;

            Storage.Position = new LatitudeLongitudeDTO(value ?? 0, Storage.Position.Longitude);
        }
    }

    private double? ObjLongitude {
        get => Storage?.Position.Longitude;
        set {
            if (Storage == null)
                return;

            Storage.Position = new LatitudeLongitudeDTO(Storage.Position.Latitude, value ?? 0);
        }
    }

    protected override string PermissionFilter => "storagedefinitions_read";

    private readonly ErrorBag ErrorBag = new ErrorBag();
    private bool Submitting;

    protected override async Task OnParametersSetAsync() {
        await Reload();
        await base.OnParametersSetAsync();
    }

    private async Task Reload() {
        Submitting = true;
        Storage = await Http.GetFromJsonAsync<StorageDefinitionDTO>($"StorageDefinitions/Get?id={Id}");

        Submitting = false;
        StateHasChanged();
    }

    private async Task Update() {
        ErrorBag.Clear();

        if (string.IsNullOrEmpty(Storage?.Name))
            ErrorBag.Fail("Storage.Name", "Name muss gesetzt sein!");
        if (ObjLatitude < -90 || ObjLatitude > 90 || ObjLongitude < -180 || ObjLongitude > 180) {
            ErrorBag.Fail("Storage.Position", "Längengrad muss zwischen -90 und 90 liegen. " +
                "Breitengrad muss zwischen -180 und 180 liegen");
        }

        if (ErrorBag.AnyError) {
            StateHasChanged();
            return;
        }

        Submitting = true;
        await Http.PostAsJsonAsync("StorageDefinitions/Update", Storage);
        await Reload();
        Submitting = false;
    }

    private void Back() => NavManager.NavigateTo("/storagedefinitions/list");

    private async Task Delete() {
        if (Storage == null) {
            Back();
            return;
        }

        var param = new ModalParameters()
            .Add(nameof(ConfirmModal.Title), "Löschen Bestätigen")
            .Add(nameof(ConfirmModal.Content), $"Soll \"{Storage.Name}\" wirklich gelöscht werden? " +
            "Alle Objekte im Lager werden in die freie Natur geworfen D:");
        var confirmModal = Modal.Show<ConfirmModal>("", param);
        var result = await confirmModal.Result;

        if (result.Cancelled)
            return;

        Submitting = true;
        await Http.PostAsJsonAsync("StorageDefinitions/Delete", Storage.Id);
        Submitting = false;
        Back();
    }
}