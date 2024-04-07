namespace PiratenKarte.Shared;

public class StorageDefinitionDTO {
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public LatitudeLongitudeDTO Position { get; set; }
    public Guid GroupId { get; set; }
}