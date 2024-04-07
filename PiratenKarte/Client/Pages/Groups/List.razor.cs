using Microsoft.AspNetCore.Components;
using PiratenKarte.Client.Services;
using PiratenKarte.Shared;
using PiratenKarte.Shared.RequestModels;
using System.Net.Http.Json;

namespace PiratenKarte.Client.Pages.Groups;

public partial class List {
    protected override string PermissionFilter => "groups_read";

    [Inject]
    public required HttpClient Http { get; init; }
    [Inject]
    public required AppStateService AppStateService { get; init; }

    private int Page;
    private int ItemsPerPage = 10;
    private int TotalItems;

    private PagedData<GroupDTO>? Groups;
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
        Groups = await Http.GetFromJsonAsync<PagedData<GroupDTO>>(
                    $"Group/GetPaged?page={Page}&itemsPerPage={ItemsPerPage}");

        TotalItems = Groups?.TotalCount ?? 1;
        Submitting = false;
        StateHasChanged();
    }
}