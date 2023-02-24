using Microsoft.JSInterop;
using PiratenKarte.Shared;

namespace PiratenKarte.Client.Models;

public class AppState {
    public LatitudeLongitude MapPosition { get; set; } = new(52.1512, 9.9494);
    public int MapZoom { get; set; } = 13;

    public int ItemsPerPage { get; set; } = 10;

    public bool StoreStateLocally { get; set; }
}