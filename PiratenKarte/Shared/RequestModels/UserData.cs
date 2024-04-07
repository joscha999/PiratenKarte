namespace PiratenKarte.Shared.RequestModels;

public class UserData {
    public required UserDTO User { get; set; }
    public string? Password { get; set; }
}