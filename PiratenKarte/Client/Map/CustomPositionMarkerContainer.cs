using FisSst.BlazorMaps;

namespace PiratenKarte.Client.Map;

public class CustomPositionMarkerContainer : MarkerContainer {
    private Marker? Marker;
    private Icon? Icon;

    private string CustomClassName;

    public CustomPositionMarkerContainer(LatLng initialPosition, string customClassName,
        IMarkerFactory mf, IDivIconFactory dif)
        : base(new LatLng(initialPosition.Lat, initialPosition.Lng), mf, dif) {
        CustomClassName = customClassName;
    }

    public override async Task<Marker> GetMarkerAsync() {
        if (Icon == null) {
            Icon = await DivIconFactory.CreateAsync(new DivIconOptions() {
                Html = $"",
                ClassName = CustomClassName,
                IconAnchor = new Point(7, 7)
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