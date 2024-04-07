
namespace PiratenKarte.DAL.Models;

public class Group : IDbIdentifier {
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
}