using Microsoft.AspNetCore.Components;

namespace PiratenKarte.Client.Components.Forms;

public partial class FormError {
    [Parameter]
    public string? Error { get; set; }
}