namespace PiratenKarte.DAL.Models;

public class User : IDbIdentifier {
    public Guid Id { get; set; }
    public required string Username { get; set; }
    public string? PasswordHash { get; set; }
    public List<Permission> Permissions { get; set; } = [];
    public List<Guid> GroupIds { get; set; } = [];
}