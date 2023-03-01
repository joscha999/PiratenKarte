using Microsoft.AspNetCore.Components;
using PiratenKarte.Client.Services;

namespace PiratenKarte.Client.Components.Authorization;

public partial class SelfView {
    [Parameter]
    public Guid Current { get; init; }

    [Parameter]
    public RenderFragment? Self { get; init; }
    [Parameter]
    public RenderFragment? Other { get; init; }
    [Parameter]
    public RenderFragment? ChildContent { get; init; }

    [Inject]
    public required AuthenticationStateService StateService { get; init; }
}