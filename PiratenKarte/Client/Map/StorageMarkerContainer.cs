using FisSst.BlazorMaps;
using PiratenKarte.Shared;

namespace PiratenKarte.Client.Map;

public class StorageMarkerContainer : MarkerContainer {
    private Marker? Marker;
    private Icon? Icon;

    private readonly StorageDefinition Storage;

    public StorageMarkerContainer(StorageDefinition storage, IMarkerFactory mf, IDivIconFactory dif)
        : base(new LatLng(storage.Position.Latitude, storage.Position.Longitude), mf, dif) {
        Storage = storage;
    }

    public override async Task<Marker> GetMarkerAsync() {
        if (Icon == null) {
            Icon = await DivIconFactory.CreateAsync(new DivIconOptions() {
                Html = $"<i class=\"bi bi-building-fill\"></i>",
                ClassName = "marker-icon",
                IconAnchor = new Point(14, 14)
            });
        }

        if (Marker == null) {
            Marker = await MarkerFactory.Create(Position, new MarkerOptions {
                Title = $"{Storage.Name}",
                IconRef = Icon
            });
        }

        return Marker;
    }
}