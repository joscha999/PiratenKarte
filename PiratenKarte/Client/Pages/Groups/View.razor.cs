using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using PiratenKarte.Client.Components.Modals;
using PiratenKarte.Shared;
using PiratenKarte.Shared.Validation;
using System.Net.Http.Json;

namespace PiratenKarte.Client.Pages.Groups;

public partial class View {
    protected override string PermissionFilter => "groups_read";

    [CascadingParameter]
    public required IModalService Modal { get; init; }

    [Parameter]
    public Guid Id { get; set; }

    [Inject]
    public required HttpClient Http { get; init; }

    private GroupDTO? Group;

    private readonly ErrorBag ErrorBag = new ErrorBag();
    private bool Submitting;

    protected override async Task OnParametersSetAsync() {
        await Reload();
        await base.OnParametersSetAsync();
    }

    private async Task Reload() {
        Submitting = true;
        var response = await Http.PostAsJsonAsync("Group/GetSingle", Id);
        Group = await response.Content.ReadFromJsonAsync<GroupDTO>();

        Submitting = false;
        StateHasChanged();
    }

    private async Task Update() {
        ErrorBag.Clear();

        if (string.IsNullOrEmpty(Group?.Name))
            ErrorBag.Fail("Group.Name", "Name muss angegeben werden.");

        if (ErrorBag.AnyError) {
            StateHasChanged();
            return;
        }

        Submitting = true;
        await Http.PostAsJsonAsync("Group/Update", Group);
        await Reload();
        Submitting = false;
    }

    private void Back() => NavManager.NavigateTo("/groups/list");

    private async Task Delete() {
        // TODO: This would cause a bunch of issues - needs cascade delete
        throw new NotImplementedException();

        if (Group == null) {
            Back();
            return;
        }

        var param = new ModalParameters()
            .Add(nameof(ConfirmModal.Title), "Löschen Bestätigen")
            .Add(nameof(ConfirmModal.Content), $"Soll \"{Group.Name}\" wirklich gelöscht werden? " +
            "Alle Objekte im Lager werden in die freie Natur geworfen D:");
        var confirmModal = Modal.Show<ConfirmModal>("", param);
        var result = await confirmModal.Result;

        if (result.Cancelled)
            return;

        Submitting = true;
        await Http.PostAsJsonAsync("Group/Delete", Group.Id);
        Submitting = false;
        Back();
    }
}