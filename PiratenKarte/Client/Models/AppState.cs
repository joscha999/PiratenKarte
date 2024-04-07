using PiratenKarte.Shared;

namespace PiratenKarte.Client.Models;

public class AppState {
    public bool AcceptedOSM { get; set; }

    public LatitudeLongitudeDTO MapPosition { get; set; } = new(52.1512, 9.9494);
    public int MapZoom { get; set; } = 7;

    public int ItemsPerPage { get; set; } = 10;

    public bool StoreStateLocally { get; set; }
}