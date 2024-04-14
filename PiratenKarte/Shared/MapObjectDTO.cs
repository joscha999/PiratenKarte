namespace PiratenKarte.Shared;

public class MapObjectDTO {
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }
    public LatitudeLongitudeDTO LatLon { get; set; }
    public List<ObjectCommentDTO> Comments { get; set; } = [];
    public StorageDefinitionDTO? Storage { get; set; }
    public Guid GroupId { get; set; }
}