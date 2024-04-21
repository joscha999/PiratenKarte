namespace PiratenKarte.DAL.Models;

public class MapObject : IDbIdentifier {
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public LatitudeLongitude LatLon { get; set; }
    public List<ObjectComment> Comments { get; set; } = [];
    public StorageDefinition? Storage { get; set; }
    public Guid GroupId { get; set; }
    public Guid MarkerStyleId { get; set; }
}