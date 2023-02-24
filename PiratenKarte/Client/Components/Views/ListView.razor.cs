using Microsoft.AspNetCore.Components;

namespace PiratenKarte.Client.Components.Views;

public partial class ListView<T> {
    [Parameter]
    public IEnumerable<T>? Items { get; set; }

    [Parameter]
    public required RenderFragment HeaderFragment { get; init; }
    [Parameter]
    public required RenderFragment<ListViewItem<T>> RowFragment { get; init; }
    [Parameter]
    public required RenderFragment FooterFragment { get; init; }
}

public readonly struct ListViewItem<T> {
    public T Item { get; }
    public int Index { get; }

    public ListViewItem(T item, int index) {
        Item = item;
        Index = index;
    }
}