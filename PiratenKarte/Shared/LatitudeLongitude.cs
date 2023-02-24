using System.Globalization;

namespace PiratenKarte.Shared;

public struct LatitudeLongitude {
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public LatitudeLongitude(double latitude, double longitude) {
        Latitude = latitude;
        Longitude = longitude;
    }

    public override string ToString()
        => Math.Round(Latitude, 4).ToString(CultureInfo.InvariantCulture)
        + ", " + Math.Round(Longitude, 4).ToString(CultureInfo.InvariantCulture);

    public string ToURL() => $"/{Latitude.ToString(CultureInfo.InvariantCulture)}"
                           + $"/{Longitude.ToString(CultureInfo.InvariantCulture)}/";
}