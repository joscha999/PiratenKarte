using Microsoft.AspNetCore.Components;

namespace PiratenKarte.Client.Components.Tables;

public partial class TableRow {
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}