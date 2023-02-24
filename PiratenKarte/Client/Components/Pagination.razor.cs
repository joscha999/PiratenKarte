using Microsoft.AspNetCore.Components;

namespace PiratenKarte.Client.Components;

public partial class Pagination {
    [Parameter]
    public int TotalItems { get; set; }

    [Parameter]
    public int ItemsPerPage { get; set; }
    [Parameter]
    public EventCallback<int> ItemsPerPageChanged { get; set; }

    [Parameter]
    public int CurrentPage { get; set; }

    [Parameter]
    public bool RightAlign { get; set; }

    [Parameter]
    public EventCallback<int> ChangePage { get; set; }

    private int _currentItemsPerPage;
    private int CurrentItemsPerPage {
        get => _currentItemsPerPage;
        set {
            if (_currentItemsPerPage == value)
                return;

            _currentItemsPerPage = value;
            ItemsPerPage = value;

            ItemsPerPageChanged.InvokeAsync(ItemsPerPage);
            ChangePage.InvokeAsync(CurrentPage);
        }
    }

    public bool OnlyOnePage => TotalItems <= ItemsPerPage;
    public int PageCount => (TotalItems / ItemsPerPage) + 1;

    public bool CanGoPrevious => CurrentPage != 0;
    public bool CanGoNext => CurrentPage != PageCount - 1;

    private void PreviousClicked() => ChangePage.InvokeAsync(CurrentPage - 1);
    private void NextClicked() => ChangePage.InvokeAsync(CurrentPage + 1);
    private void PageBtnClicked(int page) => ChangePage.InvokeAsync(page);

    protected override void OnParametersSet() {
        CurrentItemsPerPage = ItemsPerPage;
        base.OnParametersSet();
    }
}