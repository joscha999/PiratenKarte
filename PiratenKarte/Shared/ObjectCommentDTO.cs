namespace PiratenKarte.Shared;

public class ObjectCommentDTO {
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? Username { get; set; }
    public required string Content { get; set; }
    public DateTimeOffset InsertionTime { get; set; }
}