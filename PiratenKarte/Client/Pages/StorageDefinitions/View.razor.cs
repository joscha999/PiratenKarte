using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using PiratenKarte.Client.Components.Modals;
using PiratenKarte.Shared;
using System.Net.Http.Json;

namespace PiratenKarte.Client.Pages.StorageDefinitions;

public partial class View {
    [CascadingParameter]
    public required IModalService Modal { get; init; }

    [Parameter]
    public Guid Id { get; set; }

    [Inject]
    public required HttpClient Http { get; init; }
    [Inject]
    public required NavigationManager NavManager { get; init; }

    private StorageDefinition? Storage;

    private double? ObjLatitude {
        get => Storage?.Position.Latitude;
        set {
            if (Storage == null)
                return;

            Storage.Position = new LatitudeLongitude(value ?? 0, Storage.Position.Longitude);
        }
    }

    private double? ObjLongitude {
        get => Storage?.Position.Longitude;
        set {
            if (Storage == null)
                return;

            Storage.Position = new LatitudeLongitude(Storage.Position.Latitude, value ?? 0);
        }
    }

    private string? NameError;
    private bool Submitting;

    protected override async Task OnParametersSetAsync() {
        await Reload();
        await base.OnParametersSetAsync();
    }

    private async Task Reload() {
        Submitting = true;
        Storage = await Http.GetFromJsonAsync<StorageDefinition>($"StorageDefinitions/Get?id={Id}");

        Submitting = false;
        StateHasChanged();
    }

    private async Task Update() {
        NameError = null;

        if (string.IsNullOrEmpty(Storage?.Name)) {
            NameError = "Name muss gesetzt sein!";
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
            .Add(nameof(ConfirmModal.Title), "L�schen Best�tigen")
            .Add(nameof(ConfirmModal.Content), $"Soll \"{Storage.Name}\" wirklich gel�scht werden? " +
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