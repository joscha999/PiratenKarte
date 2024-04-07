using PiratenKarte.Shared.Validation;

namespace PiratenKarte.Shared;

public class UserDTO {
    public Guid Id { get; set; }
    public required string Username { get; set; }
    public List<Guid> GroupIds { get; set; } = [];

    public void Validate(ErrorBag errorBag) {
        if (string.IsNullOrWhiteSpace(Username))
            errorBag.Fail("User.Username", "Benutzername muss gesetzt sein!");
    }
}