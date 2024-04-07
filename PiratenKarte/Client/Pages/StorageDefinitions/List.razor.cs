using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using PiratenKarte.Client.Services;
using PiratenKarte.Shared;
using PiratenKarte.Shared.RequestModels;
using System.Net.Http.Json;

namespace PiratenKarte.Client.Pages.StorageDefinitions;

public partial class List {
    [CascadingParameter]
    public required IModalService Modal { get; init; }

    [Inject]
    public required HttpClient Http { get; init; }
    [Inject]
    public required AppStateService AppStateService { get; init; }

    protected override string PermissionFilter => "storagedefinitions_read";

    private int Page;
    private int ItemsPerPage = 10;
    private int TotalItems;

    private PagedData<StorageDefinitionDTO>? Objects;

    private bool Submitting;

    protected override async Task OnInitializedAsync() {
        ItemsPerPage = AppStateService.Current.ItemsPerPage;

        await Reload();
        await base.OnInitializedAsync();
    }

    private async Task ChangePage(int page) {
        AppStateService.Current.ItemsPerPage = ItemsPerPage;
        AppStateService.Write();

        Page = page;
        await Reload();
    }

    private async Task Reload() {
        Submitting = true;
        Objects = await Http.GetFromJsonAsync<PagedData<StorageDefinitionDTO>>(
            $"StorageDefinitions/GetPaged?page={Page}&itemsPerPage={ItemsPerPage}");

        TotalItems = Objects?.TotalCount ?? 1;

        Submitting = false;
        StateHasChanged();
    }
}