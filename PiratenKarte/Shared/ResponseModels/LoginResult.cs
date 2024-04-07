namespace PiratenKarte.Shared.ResponseModels;

public class LoginResult {
    public required string Token { get; set; }
    public DateTime ValidTill { get; set; }
    public required UserDTO User { get; set; }

    public required List<PermissionDTO> Permissions { get; set; }
}