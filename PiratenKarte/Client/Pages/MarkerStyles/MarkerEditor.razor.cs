using Microsoft.AspNetCore.Components;
using PiratenKarte.Shared;
using PiratenKarte.Shared.Validation;
using System.Net.Http.Json;

namespace PiratenKarte.Client.Pages.MarkerStyles;

public partial class MarkerEditor {
    protected override string PermissionFilter => "markerstyles_update";

    //TODO: List for Groups

    [Parameter]
    public Guid Id { get; set; }

    [Inject]
    public required HttpClient Http { get; init; }

    private MarkerStyleDTO? MarkerStyle;
    private List<GroupDTO> AppliedGroups = [];

    private List<GroupDTO>? AvailableGroups;

    private readonly ErrorBag ErrorBag = new();
    private bool Submitting;

    protected override async Task OnInitializedAsync() {
        var response = await Http.PostAsJsonAsync("MarkerStyles/GetSingle", Id);
        MarkerStyle = await response.Content.ReadFromJsonAsync<MarkerStyleDTO>();

        if (MarkerStyle == null)
            return;

        response = await Http.PostAsJsonAsync("Group/GetForUser", AuthStateService.User!.Id);
        AvailableGroups = await response.Content.ReadFromJsonAsync<List<GroupDTO>>();

        if (AvailableGroups == null)
            return;

        foreach (var group in AvailableGroups) {
            AppliedGroups.Add(new GroupDTO {
                Id = group.Id,
                Name = group.Name
            });
        }

        foreach (var groupId in MarkerStyle.GroupIds) {
            var available = AvailableGroups.Find(g => g.Id == groupId);
            //not available to user, ignore
            if (available == null)
                continue;

            var applied = AppliedGroups.Find(g => g.Id == groupId);
            //already applied, set Applied
            if (applied != null) {
                applied.Applied = true;
            } else {
                AppliedGroups.Add(new GroupDTO {
                    Id = groupId,
                    Name = available.Name,
                    Applied = true
                });
            }
        }
    }

    private async Task Update() {
        if (MarkerStyle == null)
            return;

        ErrorBag.Clear();

        if (string.IsNullOrEmpty(MarkerStyle.StyleName))
            ErrorBag.Fail("MarkerStyle.StyleName", "Stilname muss angegeben werden.");

        if (!AppliedGroups.Any(g => g.Applied))
            ErrorBag.Fail("MarkerStyle.Groups", "Ein Marker muss mindestens einer Gruppe zugewiesen sein.");

        if (ErrorBag.AnyError) {
            StateHasChanged();
            return;
        }

        MarkerStyle.GroupIds.Clear();
        MarkerStyle.GroupIds.AddRange(AppliedGroups.Where(g => g.Applied).Select(g => g.Id));

        Submitting = true;
        await Http.PostAsJsonAsync("MarkerStyles/Update", MarkerStyle);
        Back();
        Submitting = false;
    }

    private void Back() => NavManager.NavigateTo("/markerstyles/list");
}