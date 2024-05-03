using FisSst.BlazorMaps;
using PiratenKarte.Shared;

namespace PiratenKarte.Client.Map;

public class CustomPositionMarkerContainer : MarkerContainer {
    private Icon? Icon;

    private string? CustomClassName;
    private MarkerStyleDTO? MarkerStyle;

    public CustomPositionMarkerContainer(LatLng initialPosition, string? customClassName,
        MarkerStyleDTO? markerStyle, IMarkerFactory mf, IDivIconFactory dif)
        : base(new LatLng(initialPosition.Lat, initialPosition.Lng), mf, dif) {
        CustomClassName = customClassName;
        MarkerStyle = markerStyle;
    }

    public override async Task<Marker> GetMarkerAsync() {
        if (Icon == null) {
            Icon = await DivIconFactory.CreateAsync(new DivIconOptions() {
                Html = GenerateHtml(),
                ClassName = CustomClassName,
                IconAnchor = GenerateAnchor()
            });
        }

        if (Marker == null) {
            Marker = await MarkerFactory.Create(Position, new MarkerOptions {
                Title = "UserPosition",
                IconRef = Icon
            });
        }

        return Marker;
    }

    private Point GenerateAnchor() {
        if (MarkerStyle == null)
            return new Point(7, 7);

        return new Point(MarkerStyle.DefaultWidthPx / 2, MarkerStyle.DefaultHeightPx / 2);
    }

    private string GenerateHtml() {
        if (MarkerStyle == null)
            return "";

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