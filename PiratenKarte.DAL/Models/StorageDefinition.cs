namespace PiratenKarte.DAL.Models;

public class StorageDefinition : IDbIdentifier {
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public LatitudeLongitude Position { get; set; }
    public Guid GroupId { get; set; }
}