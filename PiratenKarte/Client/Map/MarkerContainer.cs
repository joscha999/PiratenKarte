using FisSst.BlazorMaps;

namespace PiratenKarte.Client.Map;

public abstract class MarkerContainer {
    public LatLng Position { get; }

    protected Marker? Marker;

    protected readonly IMarkerFactory MarkerFactory;
    protected readonly IDivIconFactory DivIconFactory;

    public MarkerContainer(LatLng position, IMarkerFactory markerFactory, IDivIconFactory divIconFactory) {
        Position = position;
        MarkerFactory = markerFactory;
        DivIconFactory = divIconFactory;
    }

    public abstract Task<Marker> GetMarkerAsync();

    public async Task SetPosition(LatLng position) {
        if (Marker == null)
            return;

        await Marker.SetLatLng(position);
    }

    public async Task AddToMap(FisSst.BlazorMaps.Map map) {
        if (Marker == null)
            return;

        await Marker.AddTo(map);
    }

    public async Task RemoveFromMap(FisSst.BlazorMaps.Map map) {
        if (Marker == null)
            return;

        await Marker.RemoveFrom(map);
    }
}