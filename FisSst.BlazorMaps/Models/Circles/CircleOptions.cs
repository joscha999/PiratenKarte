﻿namespace FisSst.BlazorMaps
{
    /// <summary>
    /// Determines Circle's properties.
    /// </summary>
    public class CircleOptions : PathOptions
    {
        public CircleOptions()
        {
            Fill = true;
            Radius = DefaultRadius;
        }

        private const double DefaultRadius = 10;
        public double Radius { get; init; }
    }
}
