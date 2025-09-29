using System;

namespace AGDConverter
{
    public static class CoordinateConverter
    {
        /// <summary>
        /// Converts latitude and longitude to EPSG:4326 coordinates
        /// For EPSG:4326, we use latitude and longitude directly as X,Y coordinates
        /// </summary>
        /// <param name="latitude">Latitude in decimal degrees</param>
        /// <param name="longitude">Longitude in decimal degrees</param>
        /// <param name="referenceLat">Reference latitude (unused for EPSG:4326)</param>
        /// <param name="referenceLon">Reference longitude (unused for EPSG:4326)</param>
        /// <returns>Tuple containing (X, Y) coordinates in decimal degrees</returns>
        public static (double X, double Y) ConvertToLocalCoordinates(double latitude, double longitude, 
            double referenceLat, double referenceLon)
        {
            // For EPSG:4326 (WGS84), we use longitude as X and latitude as Y directly
            // This is the standard geographic coordinate system
            return (longitude, latitude);
        }
        
        /// <summary>
        /// Finds the reference point (center) for the coordinate system from a list of topology points
        /// </summary>
        /// <param name="points">List of topology points</param>
        /// <returns>Tuple containing (referenceLat, referenceLon)</returns>
        public static (double ReferenceLat, double ReferenceLon) FindReferencePoint(List<TopologyPoint> points)
        {
            if (points == null || points.Count == 0)
                throw new ArgumentException("Points list cannot be null or empty");
            
            double minLat = points.Min(p => p.Latitude);
            double maxLat = points.Max(p => p.Latitude);
            double minLon = points.Min(p => p.Longitude);
            double maxLon = points.Max(p => p.Longitude);
            
            double referenceLat = (minLat + maxLat) / 2.0;
            double referenceLon = (minLon + maxLon) / 2.0;
            
            return (referenceLat, referenceLon);
        }
    }
}
