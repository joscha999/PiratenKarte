﻿namespace FisSst.BlazorMaps
{
    /// <summary>
    /// Determines Icon's properties.
    /// </summary>
    public class IconOptions
    {
        public IconOptions()
        {
            IconUrl = null;
            IconRetinaUrl = null;
            IconSize = null;
            IconAnchor = null;
            PopupAnchor = Point.Zero;
            TooltipAnchor = Point.Zero;
            ShadowUrl = null;
            ShadowRetinaUrl = null;
            ShadowSize = null;
            ShadowAnchor = null;
            ClassName = string.Empty;
        }

        public string IconUrl { get; init; }
        public string IconRetinaUrl { get; init; }
        public Point IconSize { get; init; }
        public Point IconAnchor { get; init; }
        public Point PopupAnchor { get; init; }
        public Point TooltipAnchor { get; init; }
        public string ShadowUrl { get; init; }
        public string ShadowRetinaUrl { get; init; }
        public Point ShadowSize { get; init; }
        public Point ShadowAnchor { get; init; }
        public string ClassName { get; init; }
    }
}
