using PiratenKarte.Shared.Validation;

namespace PiratenKarte.Shared;

public class User {
    public Guid Id { get; set; }
    public required string Username { get; set; }

    public void Validate(ErrorBag errorBag) {
        if (string.IsNullOrWhiteSpace(Username))
            errorBag.Fail("User.Username", "Benutzername muss gesetzt sein!");
    }
}