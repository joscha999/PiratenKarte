using System.Text.Json.Serialization;

namespace PiratenKarte.Shared;

public class MarkerStyleDTO {
    public Guid Id { get; set; }
    public List<Guid> GroupIds { get; set; } = [];
    public string StyleName { get; set; } = "Neuer Marker Stil";

    [JsonIgnore]
    public string CssClassName => "CSS" + Id.ToString();

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

    public string GenerateCssString() => @$".{CssClassName} {{
    height: calc({DefaultHeightPx}px * var(--marker-multiplier));
    width: calc({DefaultWidthPx}px * var(--marker-multiplier));
    background-color: {BackgroundColor};
    color: {TextColor};
    font-size: calc({FontSizePercent}% * var(--marker-multiplier));
    border-radius: {BorderRadiusPercent}%;
    border: 1px solid black;
    display: flex;
    justify-content: center;
    align-items: center;
    user-select: none;
    cursor: pointer;
}}";
}