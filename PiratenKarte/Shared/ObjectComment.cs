namespace PiratenKarte.Shared;

public class ObjectComment {
    public Guid Id { get; set; }
    public required string User { get; set; }
    public required string Note { get; set; }
    public DateTimeOffset InsertionTime { get; set; }
}