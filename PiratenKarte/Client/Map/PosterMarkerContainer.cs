using FisSst.BlazorMaps;
using PiratenKarte.Shared;

namespace PiratenKarte.Client.Map;

public class PosterMarkerContainer : MarkerContainer {
    private Marker? Marker;
    private Icon? Icon;

    private readonly MapObjectDTO Poster;

    public PosterMarkerContainer(MapObjectDTO poster, IMarkerFactory mf, IDivIconFactory dif)
        : base(new LatLng(poster.LatLon.Latitude, poster.LatLon.Longitude), mf, dif) {
        Poster = poster;
    }

    public override async Task<Marker> GetMarkerAsync() {
        if (Icon == null) {
            Icon = await DivIconFactory.CreateAsync(new DivIconOptions() {
                Html = $"",
                ClassName = "marker-dot",
                IconAnchor = new Point(7, 7)
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
}