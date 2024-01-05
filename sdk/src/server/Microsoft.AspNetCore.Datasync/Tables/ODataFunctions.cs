using Microsoft.Spatial;

namespace Microsoft.AspNetCore.Datasync.Tables;

/// <summary>
/// Implements the actual OData functions.  We do it this way so that we don't
/// have to worry about extension methods and the like.
/// </summary>
internal static class ODataFunctions
{
    /// <summary>
    /// The distance between two points.
    /// </summary>
    internal static double GeoDistance(GeographyPoint p0, GeographyPoint p1)
        => DistanceBetweenPlaces(p0.Latitude, p0.Longitude, p1.Latitude, p1.Longitude);

    /// <summary>
    /// Calculates the distance between two points on a sphere.
    /// </summary>
    /// <remarks>
    /// cos(d) = sin(φА)·sin(φB) + cos(φА)·cos(φB)·cos(λА − λB),
    ///  where φА, φB are latitudes and λА, λB are longitudes
    /// Distance = d * R
    /// </remarks>
    public static double DistanceBetweenPlaces(double lon1, double lat1, double lon2, double lat2)
    {
        if (lat1 == lat2 && lon1 == lon2)
        {
            return 0.0;
        }
        const double R = 6371; // Radius of Earth in km

        double sLat1 = Math.Sin(Radians(lat1));
        double sLat2 = Math.Sin(Radians(lat2));
        double cLat1 = Math.Cos(Radians(lat1));
        double cLat2 = Math.Cos(Radians(lat2));
        double cLon = Math.Cos(Radians(lon1) - Radians(lon2));

        double d = Math.Acos((sLat1 * sLat2) + (cLat1 * cLat2 * cLon));
        double dist = R * d;
        return dist;
    }

    /// <summary>
    /// Convert degrees to Radians
    /// </summary>
    /// <param name="degrees">Degrees</param>
    /// <returns>The equivalent in radians</returns>
    public static double Radians(double degrees) => degrees * Math.PI / 180;
}
