namespace PiratenKarte.Shared.RequestModels;

public class NewObjectComment {
    public Guid ObjectId { get; set; }
    public required ObjectComment Comment { get; set; }
}