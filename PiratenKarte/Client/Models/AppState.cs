using PiratenKarte.Shared;

namespace PiratenKarte.Client.Models;

public class AppState {
    public bool AcceptedOSM { get; set; }

    public LatitudeLongitudeDTO MapPosition { get; set; } = new(52.1512, 9.9494);
    public int MapZoom { get; set; } = 7;

    public int ItemsPerPage { get; set; } = 10;

    public bool StoreStateLocally { get; set; }

    public ScalePercent MarkerScale { get; set; } = ScalePercent.Scale100;
}

public enum ScalePercent {
    Scale25,
    Scale50,
    Scale75,
    Scale100,
    Scale125,
    Scale150,
    Scale175,
    Scale200,
    Scale225,
    Scale250,
    Scale275,
    Scale300
}

public static class ScalePercentExtension {
    public static string ToIntString(this ScalePercent scale)
        => scale.ToString().Replace("Scale", "");

    public static float ToFloat(this ScalePercent scale)
        => int.Parse(scale.ToString().Replace("Scale", "")) / 100f;
}