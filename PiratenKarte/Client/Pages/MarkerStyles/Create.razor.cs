using BlazorUtils.HttpUtils;
using Microsoft.AspNetCore.Components;
using PiratenKarte.Shared;
using PiratenKarte.Shared.Validation;

namespace PiratenKarte.Client.Pages.MarkerStyles;

public partial class Create {
    protected override string PermissionFilter => "markerstyles_create";

    [Inject]
    public required HttpClient Http { get; init; }

    private readonly MarkerStyleDTO MarkerStyle = new();
    private readonly ErrorBag ErrorBag = new();
    private bool Submitting;

    private async Task CreateMarkerStyle() {
        ErrorBag.Clear();

        if (string.IsNullOrEmpty(MarkerStyle.StyleName))
            ErrorBag.Fail("MarkerStyle.StyleName", "Stilname muss angegeben werden");

        if (ErrorBag.AnyError) {
            StateHasChanged();
            return;
        }

        MarkerStyle.GroupIds = AuthStateService.User!.GroupIds;

        await Http.CreatePostJson<Guid>()
            .To("MarkerStyles/Create")
            .WithJsonRequestValue(MarkerStyle)
            .OnModel(id => {
                if (id == Guid.Empty) {
                    ErrorBag.Fail("ServerError", "Der Stil konnte nicht erstellt werden.");
                } else {
                    NavManager.NavigateTo("markerstyles/editor/" + id);
                }
            })
            .WithBeforeExecute(() => Submitting = true)
            .WithAfterExecute(() => Submitting = false)
            .ExecuteAsync();
    }
}