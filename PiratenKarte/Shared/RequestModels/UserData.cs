namespace PiratenKarte.Shared.RequestModels;

public class UserData {
    public required User User { get; set; }
    public string? Password { get; set; }
}