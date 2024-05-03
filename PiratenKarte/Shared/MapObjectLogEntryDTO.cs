namespace PiratenKarte.Shared;

public class MapObjectLogEntryDTO {
    public Guid Id { get; set; }
    public Guid MapObjectId { get; set; }
    public string Entry { get; set; } = "";
    public DateTime TimeStamp { get; set; } = DateTime.Now;
}