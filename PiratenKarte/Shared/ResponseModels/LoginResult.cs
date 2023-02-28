namespace PiratenKarte.Shared.ResponseModels;

public class LoginResult {
    public required string Token { get; set; }
    public DateTime ValidTill { get; set; }
    public required User User { get; set; }

    public required List<Permission> Permissions { get; set; }
}