using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;

namespace PiratenKarte.Client.Components.Modals;

public partial class ConfirmModal {
    [CascadingParameter]
    public required BlazoredModalInstance Modal { get; init; }

    [Parameter]
    public required string Title { get; set; }
    [Parameter]
    public required string Content { get; set; }

    private async Task Confirm() => await Modal.CloseAsync(ModalResult.Ok());
    private async Task Cancel() => await Modal.CloseAsync(ModalResult.Cancel());
}