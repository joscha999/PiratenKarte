namespace PiratenKarte.DAL.Models;

public class MarkerStyle : IDbIdentifier {
    public Guid Id { get; set; }
    public List<Guid> GroupIds { get; set; } = [];
    public string StyleName { get; set; } = "Neuer Marker Stil";

    //CSS
    public int DefaultHeightPx { get; set; } = 14;
    public int DefaultWidthPx { get; set; } = 14;
    public string BackgroundColor { get; set; } = "#f80";
    public string TextColor { get; set; } = "#fff";
    public int BorderRadiusPercent { get; set; } = 10;
    public int FontSizePercent { get; set; } = 100;

    //HTML
    public string? Icon { get; set; }
    public string? Text { get; set; }
}