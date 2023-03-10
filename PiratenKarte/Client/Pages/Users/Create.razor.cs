using Microsoft.AspNetCore.Components;
using OneOf;
using PiratenKarte.Client.Extensions;
using PiratenKarte.Shared;
using PiratenKarte.Shared.RequestModels;
using PiratenKarte.Shared.ResponseModels;
using PiratenKarte.Shared.Unions;
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
        var httpResponse = await Http.PostAsJsonAsync("Users/Create", new UserData {
            User = User,
            Password = Password
        });

        var result
            = await httpResponse.ReadResultAsync<OneOf<IncompleteRequest, UserNameTaken, UserCreated>>();

        result.Switch(_ => ErrorBag.Fail("ServerError", "Der Benutzer konnte nicht erstellt werden (Leere Antwort)."),
            status => ErrorBag.Fail("ServerError", $"Der Benutzer konnte nicht erstellt werden ({status.Code})."),
            _ => NavManager.NavigateTo("/signin"),
            model => model.Switch(
                _ => ErrorBag.Fail("ServerError", "Der Benutzer konnte nicht erstellt werden (Fehlende Informationen)."),
                _ => ErrorBag.Fail("ServerError", "Der Benutzername ist bereits in verwendung."),
                created => NavManager.NavigateTo($"/users/view/{created.Guid}/"))
            );

        Submitting = false;

        if (!ErrorBag.AnyError)
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