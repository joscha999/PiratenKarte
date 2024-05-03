namespace PiratenKarte.DAL.Models;

public struct LatitudeLongitude {
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public LatitudeLongitude(double latitude, double longitude) {
        Latitude = latitude;
        Longitude = longitude;
    }

    public override readonly string ToString() => $"{Latitude}, {Longitude}";
}