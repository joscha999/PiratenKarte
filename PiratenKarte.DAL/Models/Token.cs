namespace PiratenKarte.DAL.Models;

public class Token : IDbIdentifier {
    public Guid Id { get; set; }
    public User? User { get; set; }
    public TokenType Type { get; set; }
    public required string Content { get; set; }
    public DateTime ValidTill { get; set; }
}

public enum TokenType {
    Login
}