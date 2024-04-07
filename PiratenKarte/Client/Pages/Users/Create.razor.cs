using BlazorUtils.HttpUtils;
using Microsoft.AspNetCore.Components;
using OneOf;
using PiratenKarte.Shared;
using PiratenKarte.Shared.RequestModels;
using PiratenKarte.Shared.Unions;
using PiratenKarte.Shared.Validation;

namespace PiratenKarte.Client.Pages.Users;

public partial class Create {
    [Inject]
    public required HttpClient Http { get; init; }

    protected override string PermissionFilter => "users_create";

    private UserDTO User = new UserDTO { Username = "" };
    private string Password = "";
    private bool ShowPassword;

    private bool Submitting;

    private readonly ErrorBag ErrorBag = new ErrorBag();

    private void Reset() {
        User = new UserDTO { Username = "" };
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

        var userData = new UserData {
            User = User,
            Password = Password
        };

        await Http.CreatePostJson<CreateUserResponse>()
            .To("Users/Create")
            .WithJsonRequestValue(userData)
            .OnUnauthorized(() => NavManager.NavigateTo("/signin"))
            .OnStatusCode(code => ErrorBag.Fail(
                "ServerError", $"Der Benutzer konnte nicht erstellt werden ({code})."))
            .OnModel(response => {
                if (response == null) {
                    ErrorBag.Fail("ServerError", "Der Benutzername konnte nicht erstellt werden (Unbekannter Fehler)");
                } else if (response.Taken) {
                    ErrorBag.Fail("ServerError", "Der Benutzername ist bereits in verwendung.");
                } else if (response.Created) {
                    NavManager.NavigateTo($"/users/view/{response.Id}/");
                }
            })
            .WithBeforeExecute(() => Submitting = true)
            .WithAfterExecute(() => {
                Submitting = false;

                if (!ErrorBag.AnyError)
                    Reset();
            })
            .ExecuteAsync();
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