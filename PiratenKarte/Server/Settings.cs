namespace PiratenKarte.Server;

public class Settings {
    public static Settings Default { get; } = new Settings {
        Domain = "domain",
        DbFileName = "data.db"
    };

    public required string Domain { get; init; }
    public required string DbFileName { get; init; }
    public string? AdminPassword { get; set; }
}