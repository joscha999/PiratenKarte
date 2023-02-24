using Microsoft.AspNetCore.Components;
using System.Text.RegularExpressions;

namespace PiratenKarte.Client.Components.Navigation;

public partial class NavButton {
    [Parameter]
    public string? Href { get; init; }
    [Parameter]
    public required string Title { get; init; }

    [Parameter]
    public string? URLMatch { get; init; }
    [Parameter]
    public bool MatchEmpty { get; init; }

    [Parameter]
    public string? Icon { get; init; }
    [Parameter]
    public RenderFragment? ChildContent { get; init; }

    [Inject]
    public required NavigationManager NavManager { get; init; }

    private bool IsActive;
    private bool Collapsed = true;

    protected override void OnParametersSet() {
        if (MatchEmpty || URLMatch != null || Href != null)
            NavManager.LocationChanged += (s, e) => UpdateIsActive();

        UpdateIsActive();
        base.OnParametersSet();
    }

    private void UpdateIsActive() {
        IsActive = false;

        var url = NavManager.ToBaseRelativePath(NavManager.Uri);
        if (MatchEmpty && string.IsNullOrEmpty(url)) {
            IsActive = true;
            StateHasChanged();
            return;
        }

        if (URLMatch != null) {
            IsActive = Regex.IsMatch(url, URLMatch);
        } else if (Href != null) {
            IsActive = Href == url;
        }
        StateHasChanged();
    }

    private void OnClick() {
        if (Href == null) {
            Collapsed = !Collapsed;
        } else {
            NavManager.NavigateTo(Href);
        }
    }
}