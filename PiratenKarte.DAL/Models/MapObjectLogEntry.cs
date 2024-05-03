namespace PiratenKarte.DAL.Models;

public class MapObjectLogEntry : IDbIdentifier {
    public Guid Id { get; set; }
    public Guid MapObjectId { get; set; }
    public string Entry { get; set; } = "";
    public DateTime TimeStamp { get; set; } = DateTime.Now;
}