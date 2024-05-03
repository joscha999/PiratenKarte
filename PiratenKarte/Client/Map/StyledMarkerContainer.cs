using FisSst.BlazorMaps;
using PiratenKarte.Shared;

namespace PiratenKarte.Client.Map;

public class StyledMarkerContainer : MarkerContainer {
    private Icon? Icon;

    private readonly MapObjectDTO Poster;
    private readonly MarkerStyleDTO MarkerStyle;

    public StyledMarkerContainer(MapObjectDTO poster, MarkerStyleDTO style, IMarkerFactory mf, IDivIconFactory df)
        : base(new LatLng(poster.LatLon.Latitude, poster.LatLon.Longitude), mf, df) {
        Poster = poster;
        MarkerStyle = style;
    }

    public override async Task<Marker> GetMarkerAsync() {
        if (Icon == null) {
            Icon = await DivIconFactory.CreateAsync(new DivIconOptions() {
                Html = GenerateHtml(),
                IconAnchor = new Point(MarkerStyle.DefaultWidthPx / 2, MarkerStyle.DefaultHeightPx / 2)
            });
        }

        if (Marker == null) {
            Marker = await MarkerFactory.Create(Position, new MarkerOptions {
                Title = $"{Poster.Name}",
                IconRef = Icon
            });
        }

        return Marker;
    }

    private string GenerateHtml() {
        var html = $"<div class=\"{MarkerStyle.CssClassName}\">";

        if (!string.IsNullOrEmpty(MarkerStyle.Icon)) {
            html += Environment.NewLine + $"<i class=\"{MarkerStyle.Icon}\"></i>";
        }

        if (!string.IsNullOrEmpty(MarkerStyle.Text)) {
            html += Environment.NewLine + $"<span>{MarkerStyle.Text}</span>";
        }

        return html + Environment.NewLine + "</div>";
    }
}