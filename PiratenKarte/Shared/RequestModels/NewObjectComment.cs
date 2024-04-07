namespace PiratenKarte.Shared.RequestModels;

public class NewObjectComment {
    public Guid ObjectId { get; set; }
    public required ObjectCommentDTO Comment { get; set; }
}