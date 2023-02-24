using FisSst.BlazorMaps;

namespace PiratenKarte.Client.Map;

public abstract class MarkerContainer {
    public LatLng Position { get; }

    protected readonly IMarkerFactory MarkerFactory;
    protected readonly IDivIconFactory DivIconFactory;

    public MarkerContainer(LatLng position, IMarkerFactory markerFactory, IDivIconFactory divIconFactory) {
        Position = position;
        MarkerFactory = markerFactory;
        DivIconFactory = divIconFactory;
    }

    public abstract Task<Marker> GetMarkerAsync();
}