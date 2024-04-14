using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using PiratenKarte.Client.Components.Modals;
using PiratenKarte.Shared;
using PiratenKarte.Shared.RequestModels;
using PiratenKarte.Shared.Validation;
using System.Net.Http.Json;

namespace PiratenKarte.Client.Pages.Users;

public partial class View {
    [CascadingParameter]
    public required IModalService Modal { get; init; }

    [Parameter]
    public Guid Id { get; set; }

    [Inject]
    public required HttpClient Http { get; init; }

    protected override string PermissionFilter => "users_read";

    private UserDTO? User;

    private List<PermissionDTO>? Permissions;
    private List<GroupDTO>? Groups;

    private readonly ErrorBag ErrorBag = new ErrorBag();
    private bool Submitting;

    private string Password = "";
    private string PasswordRepeat = "";

    protected override bool CanView() {
        if (Id == AuthStateService.Current.User?.Id)
            return true;

        return base.CanView();
    }

    protected override async Task OnParametersSetAsync() {
        await Reload();
        await base.OnParametersSetAsync();
    }

    private async Task Reload() {
        Submitting = true;

        if (Id == AuthStateService.Current.User?.Id) {
            User = await Http.GetFromJsonAsync<UserDTO>("Users/GetSelf");
        } else {
            User = await Http.GetFromJsonAsync<UserDTO>($"Users/Get?id={Id}");
        }

        if (Id == AuthStateService.Current.User?.Id) {
            var response = await Http.PostAsJsonAsync("Group/GetForUser", Id);
            Groups = await response.Content.ReadFromJsonAsync<List<GroupDTO>>();
        } else if (AuthStateService.HasExact("groups_add_user")) {
            var response = await Http.PostAsJsonAsync("Group/GetForEdit", Id);
            Groups = await response.Content.ReadFromJsonAsync<List<GroupDTO>>();
        } else {
            Groups = [];
        }

        if (AuthStateService.HasExact("permissions_update")) {
            Permissions = await Http.GetFromJsonAsync<List<PermissionDTO>>($"Permissions/GetVisibleFor?id={Id}");
            Permissions?.Sort((a, b) => a.ReadableName.CompareTo(b.ReadableName));
        } else {
            Permissions = AuthStateService.Current.Permissions;
        }

        Submitting = false;
        StateHasChanged();
    }

    private async Task Update() => await Update(false);
    private async Task UpdatePassword((string pw, string pwRepeat) data) {
        Password = data.pw;
        PasswordRepeat = data.pwRepeat;
        await Update(true);
    }

    private async Task Update(bool submitPassword) {
        if (User == null) {
            Back();
            return;
        }

        ErrorBag.Clear();

        User.Validate(ErrorBag);
        if (submitPassword) {
            if (string.IsNullOrWhiteSpace(Password))
                ErrorBag.Fail("User.Password", "Passwort muss gesetzt sein!");
            if (Password.Length < 12)
                ErrorBag.Fail("User.Password", "Passwort muss mindestens 12 Zeichen lang sein!");
            if (Password != PasswordRepeat)
                ErrorBag.Fail("User.Password", "Passwort und Wiederholung müssen gleich sein!");
        }

        if (ErrorBag.AnyError) {
            StateHasChanged();
            return;
        }

        Submitting = true;
        var result = await Http.PostAsJsonAsync("Users/Update", new UserData {
            User = User,
            Password = submitPassword ? Password : null
        });

        var model = await result.Content.ReadFromJsonAsync<UpdateUserResponse>();
        Submitting = false;

        if (model != null && model.Updated) {
            // If we updated the password our Token got yeeted, make sure to go back to login
            if (submitPassword) {
                AuthStateService.InvalidateLocal();
                NavManager.NavigateTo("/signin");
            } else {
                await Reload();
            }
        } else if (model == null) {
            ErrorBag.Fail("ServerError", "Benutzerdaten konnten nicht aktualisiert werden.");
            StateHasChanged();
        } else if (model.Taken) {
            ErrorBag.Fail("ServerError", "Der Benutzername ist bereits in verwendung.");
            StateHasChanged();
        }
    }

    private void Back() {
        if (AuthStateService.HasExact("users_list")) {
            NavManager.NavigateTo("/users/list");
        } else {
            NavManager.NavigateTo("/");
        }
    }

    private async Task Delete() {
        if (User == null) {
            Back();
            return;
        }

        var param = new ModalParameters()
            .Add(nameof(ConfirmModal.Title), "Löschen Bestätigen")
            .Add(nameof(ConfirmModal.Content), $"Soll \"{User.Username}\" wirklich gelöscht werden?");
        var confirmModal = Modal.Show<ConfirmModal>("", param);
        var result = await confirmModal.Result;

        if (result.Cancelled)
            return;

        Submitting = true;
        await Http.PostAsJsonAsync("Users/Delete", User.Id);
        Submitting = false;
        Back();
    }

    private async Task TogglePermission(string key) {
        if (Permissions == null)
            return;

        var permission = Permissions.Find(p => p.Key == key);
        if (permission == null)
            return;

        permission.Applied = !permission.Applied;
        await Http.PostAsJsonAsync("Permissions/SetPermission", new SetPermission {
            UserId = Id,
            PermissionId = permission.Id,
            State = permission.Applied
        });
    }

    private async Task ToggleGroup(Guid groupId) {
        if (Groups == null)
            return;

        var group = Groups.Find(g => g.Id == groupId);
        if (group == null)
            return;

        group.Applied = !group.Applied;
        await Http.PostAsJsonAsync("Group/SetUserGroup",
            new SetUserGroupRequest(Id, group.Id, group.Applied));
    }
}