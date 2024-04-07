namespace PiratenKarte.Shared.RequestModels;

public class SetObjectPosition {
    public Guid ObjectId { get; set; }
    public LatitudeLongitudeDTO Position { get; set; }
}