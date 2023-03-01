using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using PiratenKarte.Client.Components.Modals;
using PiratenKarte.Client.Services;
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

    private User? User;

    private string Password = "";
    private string PasswordRepeat = "";
    private bool ShowPassword;

    private List<Permission>? Permissions;

    private readonly ErrorBag ErrorBag = new ErrorBag();
    private bool Submitting;

    protected override async Task OnParametersSetAsync() {
        await Reload();
        await base.OnParametersSetAsync();
    }

    private async Task Reload() {
        Submitting = true;
        User = await Http.GetFromJsonAsync<User>($"Users/Get?id={Id}");

        if (AuthStateService.HasExact("permissions_update")) {
            Permissions = await Http.GetFromJsonAsync<List<Permission>>($"Permissions/GetVisibleFor?id={Id}");
            Permissions?.Sort((a, b) => a.ReadableName.CompareTo(b.ReadableName));
        } else {
            Permissions = AuthStateService.Current.Permissions;
        }

        Submitting = false;
        StateHasChanged();
    }

    private async Task Update() => await Update(false);
    private async Task UpdatePassword() => await Update(true);

    private async Task Update(bool submitPassword) {
        if (User == null) {
            Back();
            return;
        }

        ErrorBag.Clear();

        User.Validate(ErrorBag);
        if (string.IsNullOrWhiteSpace(Password))
            ErrorBag.Fail("User.Password", "Passwort muss gesetzt sein!");
        if (Password.Length < 12)
            ErrorBag.Fail("User.Password", "Passwort muss mindestens 12 Zeichen lang sein!");
        if (Password != PasswordRepeat)
            ErrorBag.Fail("User.Password", "Passwort und Wiederholung müssen gleich sein!");

        if (ErrorBag.AnyError) {
            StateHasChanged();
            return;
        }

        Submitting = true;
        await Http.PostAsJsonAsync("Users/Update", new UserData {
            User = User,
            Password = submitPassword ? Password : null
        });
        await Reload();
        Submitting = false;
    }

    private void Back() => NavManager.NavigateTo("/users/list");

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

    private void RandomizePassword() {
        Password = new string(Enumerable.Range(0, 32)
            .Select(_ => (char)Random.Shared.Next(32, 127)).ToArray());
        PasswordRepeat = Password;
        StateHasChanged();
    }

    private void ToggleShowPassword() {
        ShowPassword = !ShowPassword;
        StateHasChanged();
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
}