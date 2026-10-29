namespace Server.Helper;

/// <summary>
/// Rough solar position for a UTC instant. Times are UTC on purpose: the sun does not care about the
/// clock's time zone or daylight savings, so the arc must not jump twice a year.
/// </summary>
/// <param name="AzimuthDeg">Compass bearing of the sun: 0 = north, 90 = east, 180 = south, 270 = west.</param>
/// <param name="ElevationDeg">Angle of the sun above the horizon.</param>
/// <param name="MaxElevationDeg">Highest elevation the sun reaches on that day at that latitude.</param>
/// <param name="DeclinationDeg">Solar declination of that day.</param>
public readonly record struct SolarPosition(
    double AzimuthDeg,
    double ElevationDeg,
    double MaxElevationDeg,
    double DeclinationDeg);

public static class SolarPositionCalculator
{
    const double DegToRad = Math.PI / 180;
    const double RadToDeg = 180 / Math.PI;

    /// <summary>
    /// NOAA/Spencer approximation, accurate to a few hundredths of a degree - far more than a rough
    /// window needs. The date is fixed per call (the theme uses one spring day), so only the hour
    /// angle moves with <paramref name="utcTimeOfDay"/>.
    /// </summary>
    public static SolarPosition Calculate(double latitudeDeg, double longitudeDeg, int dayOfYear, TimeSpan utcTimeOfDay)
    {
        var gamma = 2 * Math.PI / 365 * (Math.Clamp(dayOfYear, 1, 365) - 1);

        var declination = 0.006918
                          - 0.399912 * Math.Cos(gamma) + 0.070257 * Math.Sin(gamma)
                          - 0.006758 * Math.Cos(2 * gamma) + 0.000907 * Math.Sin(2 * gamma)
                          - 0.002697 * Math.Cos(3 * gamma) + 0.00148 * Math.Sin(3 * gamma); // radians

        var equationOfTimeMinutes = 229.18 * (0.000075
                                              + 0.001868 * Math.Cos(gamma) - 0.032077 * Math.Sin(gamma)
                                              - 0.014615 * Math.Cos(2 * gamma) - 0.040849 * Math.Sin(2 * gamma));

        // Solar time in hours: UTC plus the longitude offset and the equation of time.
        var solarHours = utcTimeOfDay.TotalHours + longitudeDeg / 15 + equationOfTimeMinutes / 60;
        var hourAngle = (solarHours - 12) * 15 * DegToRad;

        var latitude = Math.Clamp(latitudeDeg, -90, 90) * DegToRad;

        var sinElevation = Math.Sin(latitude) * Math.Sin(declination)
                           + Math.Cos(latitude) * Math.Cos(declination) * Math.Cos(hourAngle);
        var elevation = Math.Asin(Math.Clamp(sinElevation, -1, 1)) * RadToDeg;

        // Horizontal unit vector of the sun (x = east, y = north). This form stays well defined all
        // day, while the acos form degenerates when the sun passes close to the zenith.
        var east = -Math.Cos(declination) * Math.Sin(hourAngle);
        var north = Math.Sin(declination) * Math.Cos(latitude)
                    - Math.Cos(declination) * Math.Cos(hourAngle) * Math.Sin(latitude);
        var azimuth = Math.Atan2(east, north) * RadToDeg;
        if (azimuth < 0)
            azimuth += 360;

        var declinationDeg = declination * RadToDeg;
        var maxElevation = 90 - Math.Abs(Math.Clamp(latitudeDeg, -90, 90) - declinationDeg);

        return new(azimuth, elevation, maxElevation, declinationDeg);
    }
}
