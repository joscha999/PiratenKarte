using Microsoft.AspNetCore.Components;
using PiratenKarte.Shared.Validation;

namespace PiratenKarte.Client.Components.Forms;

public partial class FormError {
    [Parameter]
    public required ErrorBag ErrorBag { get; init; }
    [Parameter]
    public required string Key { get; init; }

    private IEnumerable<ErrorContainer> Errors => ErrorBag.Get(Key);
}