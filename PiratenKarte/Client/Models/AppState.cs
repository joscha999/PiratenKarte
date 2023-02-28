using Microsoft.JSInterop;
using PiratenKarte.Shared;

namespace PiratenKarte.Client.Models;

public class AppState {
    public LatitudeLongitude MapPosition { get; set; } = new(52.1512, 9.9494);
    public int MapZoom { get; set; } = 13;

    public int ItemsPerPage { get; set; } = 10;

    public bool StoreStateLocally { get; set; }

    public User? User { get; set; }
    public List<Permission> Permissions { get; set; } = new();
    public string? AuthToken { get; set; }
    public bool KeepLoggedIn { get; set; }

    public AppState SerializableClone() {
        var state = new AppState();
        state.MapPosition = MapPosition;
        state.MapZoom = MapZoom;
        state.ItemsPerPage = ItemsPerPage;
        state.StoreStateLocally = StoreStateLocally;

        if (KeepLoggedIn) {
            state.User = User;
            state.Permissions = Permissions;
            state.AuthToken = AuthToken;
            state.KeepLoggedIn = KeepLoggedIn;
        }

        return state;
    }
}