using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using PiratenKarte.Client.Services;
using PiratenKarte.Shared;
using PiratenKarte.Shared.RequestModels;
using System.Net.Http.Json;

namespace PiratenKarte.Client.Pages.Users;

public partial class List {
    [CascadingParameter]
    public required IModalService Modal { get; init; }

    [Inject]
    public required HttpClient Http { get; init; }
    [Inject]
    public required AppStateService AppStateService { get; init; }

    protected override string PermissionFilter => "users_read";

    private int Page;
    private int ItemsPerPage = 10;
    private int TotalItems;

    private PagedData<UserDTO>? Users;

    private bool Submitting;

    protected override async Task OnInitializedAsync() {
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
        Users = await Http.GetFromJsonAsync<PagedData<UserDTO>>(
            $"Users/GetPaged?page={Page}&itemsPerPage={ItemsPerPage}");

        TotalItems = Users?.TotalCount ?? 1;
        Submitting = false;
        StateHasChanged();
    }
}