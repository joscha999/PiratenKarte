using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using PiratenKarte.Shared.Validation;

namespace PiratenKarte.Client.Pages.Users.SubPages;

public partial class UserPasswordComponent {
    [Parameter]
    public required ErrorBag ErrorBag { get; init; }
    [Parameter]
    public EventCallback<(string pw, string pwRepeat)> UpdatePassword { get; init; }

    private string Password = "";
    private string PasswordRepeat = "";
    private bool ShowPassword;

    private async Task PasswordKeyPressed(KeyboardEventArgs args) {
        if (args.Code == "Enter" || args.Code == "NumpadEnter")
            await UpdatePassword.InvokeAsync((Password, PasswordRepeat));
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

    private async Task Update() => await UpdatePassword.InvokeAsync((Password, PasswordRepeat));
}