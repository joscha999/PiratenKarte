using BlazorUtils.HttpUtils;
using Microsoft.AspNetCore.Components;
using PiratenKarte.Shared;
using PiratenKarte.Shared.RequestModels;
using PiratenKarte.Shared.Validation;

namespace PiratenKarte.Client.Pages.Groups;

public partial class Create {
    protected override string PermissionFilter => "groups_create";

    [Inject]
    public required HttpClient Http { get; init; }

    private GroupDTO Group = new();
    private readonly ErrorBag ErrorBag = new ErrorBag();
    private bool Submitting;

    private void Reset() {
        Group = new();
        ErrorBag.Clear();
    }

    private async Task CreateGroup() {
        ErrorBag.Clear();

        if (string.IsNullOrWhiteSpace(Group.Name))
            ErrorBag.Fail("Group.Name", "Name muss angegeben werden.");

        if (ErrorBag.AnyError) {
            StateHasChanged();
            return;
        }

        await Http.CreatePostJson<CreateGroupResponse>()
            .To("Group/CreateEx")
            .WithJsonRequestValue(Group)
            .OnUnauthorized(() => NavManager.NavigateTo("/signin"))
            .OnStatusCode(code => ErrorBag.Fail(
                "ServerError", $"Die Gruppe konnte nicht erstellt werden ({code})."))
            .OnModel(response => {
                if (response == null) {
                    ErrorBag.Fail("ServerError", "Die Gruppe konnte nicht erstellt werden (Unbekannter Fehler)");
                } else if (response.NameTaken) {
                    ErrorBag.Fail("ServerError", "Es existiert bereits eine Gruppe mit dem Namen");
                } else if (response.Id == null) {
                    ErrorBag.Fail("ServerError", "Die Gruppe konnte nicht erstellt werden (Unbekannter Fehler)");
                } else {
                    NavManager.NavigateTo($"/groups/view/{response.Id}/");
                }
            })
            .WithBeforeExecute(() => Submitting = true)
            .WithAfterExecute(() => {
                Submitting = false;

                if (!ErrorBag.AnyError)
                    Reset();
            })
            .ExecuteAsync();
    }
}