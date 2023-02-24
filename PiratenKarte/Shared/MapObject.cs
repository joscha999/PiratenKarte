namespace PiratenKarte.Shared;

public class MapObject {
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public LatitudeLongitude LatLon { get; set; }
    public List<ObjectComment> Comments { get; set; } = new();
    public StorageDefinition? Storage { get; set; }
}