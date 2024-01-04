namespace Datasync.Common.Models;

/// <summary>
/// The model returned by the <see cref="CountryData.GetCountries"/> method.
/// </summary>
public class Country
{
    /// <summary>
    /// The 2-letter ISO code for the country.
    /// </summary>
    public string IsoCode { get; set; } = string.Empty;

    /// <summary>
    /// The official name of the country.
    /// </summary>
    public string CountryName { get; set; } = string.Empty;

    /// <summary>
    /// The latitude and longitude of the country's capital city.
    /// </summary>
    public double Latitude { get; set; }
    public double Longitude { get; set; }
}
