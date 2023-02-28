namespace PiratenKarte.DAL.Models;

public class Permission : IDbIdentifier {
    public Guid Id { get; set; }
    public required string Key { get; set; }
    public required string ReadableName { get; set; }
}