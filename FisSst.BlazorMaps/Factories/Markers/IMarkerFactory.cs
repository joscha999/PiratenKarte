﻿using System.Threading.Tasks;

namespace FisSst.BlazorMaps
{
    /// <summary>
    /// It is responsible for creating Markers and optionally adding them to the Map.
    /// </summary>
    public interface IMarkerFactory
    {
        Task<Marker> Create(LatLng latLng);
        Task<Marker> Create(LatLng latLng, MarkerOptions options);
        Task<Marker> CreateAndAddToMap(LatLng latLng, Map map);
        Task<Marker> CreateAndAddToMap(LatLng latLng, Map map, MarkerOptions options);
    }
}
