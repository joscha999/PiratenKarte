﻿namespace FisSst.BlazorMaps
{
    /// <summary>
    /// It is a subset of MapOptions.
    /// </summary>
    public class MapSubOptions
    {
        public string Id { get; set; }
        public string Attribution { get; set; }
        public int MaxZoom { get; set; }
        public int TileSize { get; set; }
        public int ZoomOffset { get; set; }
        public string AccessToken { get; set; }
    }
}
