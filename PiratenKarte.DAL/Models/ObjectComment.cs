namespace PiratenKarte.DAL.Models;

public class ObjectComment {
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public required string Content { get; set; }
    public DateTimeOffset InsertionTime { get; set; }
}