namespace PiratenKarte.Shared;

public class Permission {
    public Guid Id { get; set; }
    public required string Key { get; set; }
    public required string ReadableName { get; set; }

    public bool Applied { get; set; }
}