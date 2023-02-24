namespace PiratenKarte.Shared.RequestModels;

public class DeleteObjectComment {
    public Guid ObjectId { get; set; }
    public Guid CommentId { get; set; }
}