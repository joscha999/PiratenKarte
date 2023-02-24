namespace PiratenKarte.Shared.RequestModels;

public class SetObjectPosition {
    public Guid ObjectId { get; set; }
    public LatitudeLongitude Position { get; set; }
}