using Microsoft.AspNetCore.Components;
using PiratenKarte.Client.Services;
using PiratenKarte.Shared;
using PiratenKarte.Shared.RequestModels;
using System.Net.Http.Json;

namespace PiratenKarte.Client.Pages.MapObjects;

public partial class MoveManyToStorage {
    [Inject]
    public required ParameterPassService Params { get; init; }
    [Inject]
    public required HttpClient Http { get; init; }

    protected override string PermissionFilter => "objects_update";

    private List<MapObject>? Objects;

    private Guid? SelectedStorageId;
    private List<StorageDefinition>? StorageDefinitions;

    private bool Submitting;

    protected override async Task OnInitializedAsync() {
        if (!Params.TryTake("MoveManyToStorage.Objects", out Objects))
            NavManager.NavigateTo("/mapobjects/list");

        StorageDefinitions = await Http.GetFromJsonAsync<List<StorageDefinition>>("StorageDefinitions/GetAll");
        await base.OnInitializedAsync();
    }

    private async Task Submit() {
        Submitting = true;
        await Http.PostAsJsonAsync("/MapObjects/SetStorageMany/", new SetObjectStorageMany {
            StorageId = SelectedStorageId,
            ObjectIds = Objects!.ConvertAll(o => o.Id)
        });

        NavManager.NavigateTo("/mapobjects/list");
        Submitting = false;
    }

    private void Cancel() => NavManager.NavigateTo("/mapobjects/list");
}