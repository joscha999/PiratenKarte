using Microsoft.AspNetCore.Components;
using PiratenKarte.Client.Extensions;
using PiratenKarte.Shared;
using PiratenKarte.Shared.RequestModels;
using PiratenKarte.Shared.ResponseModels;
using PiratenKarte.Shared.Validation;
using System.Net.Http.Json;

namespace PiratenKarte.Client.Pages.Users;

public partial class Create {
    [Inject]
    public required HttpClient Http { get; init; }

    protected override string PermissionFilter => "users_create";

    private User User = new User { Username = "" };
    private string Password = "";
    private bool ShowPassword;

    private bool Submitting;

    private readonly ErrorBag ErrorBag = new ErrorBag();

    private void Reset() {
        User = new User { Username = "" };
        ErrorBag.Clear();
    }

    private async Task SaveObject() {
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
        var result = await Http.PostAsJsonAsync("Users/Create", new UserData {
            User = User,
            Password = Password
        });

        var resultModel = await result.ReadResultAsync<CreateUserResult>();

        if (resultModel == null) {
            ErrorBag.Fail("ServerError", "Der Benutzer konnte nicht erstellt werden.");
        } else {
            if (resultModel.ValidationFailure) {
                ErrorBag.Fail("ServerError", "Der Benutzer konnte nicht erstellt werden.");
            } else if (resultModel.UsernameAlreadyUsed) {
                ErrorBag.Fail("ServerError", "Der angegebene Benutzername ist bereits in benutzung.");
            }
        }

        Submitting = false;
        if (ErrorBag.AnyError) {
            StateHasChanged();
            return;
        }

        NavManager.NavigateTo($"/users/view/{resultModel!.Id}/");
        Reset();
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