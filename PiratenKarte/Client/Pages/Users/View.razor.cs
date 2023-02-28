using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
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

    private User? User;
    private string Password = "";
    private bool ShowPassword;

    private readonly ErrorBag ErrorBag = new ErrorBag();
    private bool Submitting;

    protected override async Task OnParametersSetAsync() {
        await Reload();
        await base.OnParametersSetAsync();
    }

    private async Task Reload() {
        Submitting = true;
        User = await Http.GetFromJsonAsync<User>($"Users/Get?id={Id}");

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

        if (ErrorBag.AnyError) {
            StateHasChanged();
            return;
        }

        Submitting = true;
        await Http.PostAsJsonAsync("StorageDefinitions/Update", new UserData {
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
        StateHasChanged();
    }

    private void ToggleShowPassword() {
        ShowPassword = !ShowPassword;
        StateHasChanged();
    }
}