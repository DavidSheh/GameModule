using System;
using System.Collections.Generic;

/// <summary>
/// Time Related Utility Methods.
/// </summary>
public static class TimeUtility
{
    static readonly Dictionary<string, long> s_Timers = new Dictionary<string, long>();

    /// <summary>
    /// Converts a UNIX time stamp into <see cref="DateTime"/> object.
    /// </summary>
    /// <param name="timestamp">UNIX timestamp.</param>
    public static DateTime FromUnixTime(long timestamp)
    {
        var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        return origin.AddSeconds(timestamp);
    }

    /// <summary>
    /// Gets a UNIX timestamp from a <see cref="DateTime"/> object.
    /// </summary>
    /// <param name="date">Source date for conversion.</param>
    public static long ToUnixTime(DateTime date)
    {
        var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        var diff = date.ToUniversalTime() - origin;
        return (long)diff.TotalSeconds;
    }
}