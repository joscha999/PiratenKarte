using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using PiratenKarte.Client.Extensions;
using PiratenKarte.Client.Services;
using PiratenKarte.Shared.RequestModels;
using PiratenKarte.Shared.ResponseModels;
using PiratenKarte.Shared.Validation;
using System.Net.Http.Json;

namespace PiratenKarte.Client.Pages.Users;

public partial class SignIn {
    [Inject]
    public required HttpClient Http { get; init; }
    [Inject]
    public required NavigationManager NavManager { get; init; }
    [Inject]
    public required AuthenticationStateService StateService { get; init; }

    private string Username = "";
    private string Password = "";
    private bool KeepLoggedIn;

    private bool ShowPassword;
    private bool Submitting;

    private readonly ErrorBag ErrorBag = new ErrorBag();

    private void ToggleShowPassword() {
        ShowPassword = !ShowPassword;
        StateHasChanged();
    }

    private async Task Login() {
        ErrorBag.Clear();

        if (string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(Username)) {
            ErrorBag.Fail("UsernamePassword", "Benutzername und Password müssen eingetragen sein!");
            StateHasChanged();
            return;
        }

        Submitting = true;

        var result = await Http.PostAsJsonAsync("Users/CheckLogin", new LoginData {
            Username = Username,
            Password = Password,
        });
        var loginResult = await result.ReadResultAsync<LoginResult>();

        if (loginResult == null || string.IsNullOrEmpty(loginResult.Token)) {
            ErrorBag.Fail("ServerError", "Benutzername oder Passwort falsch!");
        } else {
            await StateService.Invalidate();
            StateService.Current.Token = loginResult.Token;
            StateService.Current.User = loginResult.User;
            StateService.Current.Permissions = loginResult.Permissions;
            StateService.Current.TokenValidTill = loginResult.ValidTill;
            StateService.Current.KeepLoggedIn = KeepLoggedIn;
            StateService.Write();

            NavManager.NavigateTo("");
        }

        Submitting = false;
        StateHasChanged();
    }

    private async Task OnKeyPressed(KeyboardEventArgs args) {
        if (args.Code == "Enter" || args.Code == "NumpadEnter")
            await Login();
    }
}