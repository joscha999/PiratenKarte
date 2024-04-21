using Microsoft.AspNetCore.Components;
using PiratenKarte.Shared;
using System.Net.Http.Json;

namespace PiratenKarte.Client.Pages.MarkerStyles;

public partial class List {
    protected override string PermissionFilter => "markerstyles_read";

    [Inject]
    public required HttpClient Http { get; init; }

    private List<MarkerStyleDTO>? MarkerStyles;

    protected override async Task OnInitializedAsync() {
        MarkerStyles = await Http.GetFromJsonAsync<List<MarkerStyleDTO>>("MarkerStyles/GetAll");
        StateHasChanged();
    }
}