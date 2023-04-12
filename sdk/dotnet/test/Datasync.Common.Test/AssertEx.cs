// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using FluentAssertions;
using Microsoft.AspNetCore.Datasync;
using Microsoft.AspNetCore.Datasync.Extensions;
using Microsoft.Datasync.Client;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace Datasync.Common.Test;

/// <summary>
/// A set of additional assertions used within the unit test projects.
/// </summary>
[ExcludeFromCodeCoverage]
public static class AssertEx
{
    /// <summary>
    /// the format for time comparisons - ms accuracy
    /// </summary>
    private const string format = "yyyy-MM-dd'T'HH:mm:ss.fffK";

    /// <summary>
    /// Asserts if the two dates are "close" to one another (enough so to be valid in terms of test speed)
    /// timings)
    /// </summary>
    /// <param name="expected"></param>
    /// <param name="actual"></param>
    /// <param name="interval"></param>
    public static void CloseTo(DateTimeOffset expected, DateTimeOffset actual, int interval = 3000)
    {
        var ms = Math.Abs((expected.Subtract(actual)).TotalMilliseconds);
        Assert.True(ms < interval, $"Date {expected} and {actual} are {ms}ms apart");
    }

    /// <summary>
    /// Asserts if the dictionary contains the provided value.
    /// </summary>
    /// <param name="key">the header name</param>
    /// <param name="expected">the header value</param>
    /// <param name="dict">The dictionary to check</param>
    public static void Contains(string key, string expected, IDictionary<string, string> dict)
    {
        Assert.True(dict.TryGetValue(key, out string value), $"Dictionary does not contain key {key}");
        Assert.Equal(expected, value);
    }

    /// <summary>
    /// Asserts if the header dictionary contains the provided value.
    /// </summary>
    /// <param name="key">the header name</param>
    /// <param name="expected">the header value</param>
    /// <param name="headers">The headers</param>
    public static void Contains(string key, string expected, IDictionary<string, IEnumerable<string>> headers)
    {
        Assert.True(headers.TryGetValue(key, out IEnumerable<string> values), $"Dictionary does not contain key {key}");
        Assert.True(values.Count() == 1, $"Dictionary contains multiple values for {key}");
        Assert.Equal(expected, values.Single());
    }

    /// <summary>
    /// Asserts if the headers contains the specific header provided
    /// </summary>
    /// <param name="headers"></param>
    /// <param name="headerName"></param>
    /// <param name="expected"></param>
    public static void HasHeader(HttpHeaders headers, string headerName, string expected)
    {
        string allHeaders = Enumerable.Empty<(String name, String value)>().Concat(
            headers.SelectMany(kvp => kvp.Value.Select(v => (name: kvp.Key, value: v)))
        ).Aggregate(seed: new StringBuilder(), func: (sb, pair) => sb.Append(pair.name).Append(": ").AppendLine(pair.value), resultSelector: sb => sb.ToString());

        Assert.True(headers.TryGetValues(headerName, out IEnumerable<string> values), $"The request/response does not contain header {headerName} (Headers = {allHeaders})");
        Assert.True(values.Count() == 1, $"There are {values.Count()} values for header {headerName} (values = {string.Join(';', values)})");
        Assert.True(values.First().Equals(expected), $"Value of header {headerName} does not match (expected: {expected}, got: {values.First()})");
    }

    /// <summary>
    /// Asserts if the headers contains the specific header provided
    /// </summary>
    /// <param name="response"></param>
    /// <param name="headerName"></param>
    /// <param name="expected"></param>
    public static void HasHeader(HttpResponseMessage response, string headerName, string expected)
    {
        if (headerName.Equals("Last-Modified", StringComparison.InvariantCultureIgnoreCase) || headerName.StartsWith("Content-", StringComparison.InvariantCultureIgnoreCase))
        {
            HasHeader(response.Content.Headers, headerName, expected);
        }
        else
        {
            HasHeader(response.Headers, headerName, expected);
        }
    }

    /// <summary>
    /// Asserts if the provided actual date is in between the provided start and end dates.
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <param name="actual"></param>
    public static void IsBetween(DateTimeOffset start, DateTimeOffset end, DateTimeOffset actual)
    {
        Assert.True(actual >= start, $"Date {actual} is earlier than expected start date {start}");
        Assert.True(actual <= end, $"Date {actual} is later than expected end date {end}");
    }

    /// <summary>
    /// Ensures that the response has the required headers for conditional HTTP usage.
    /// </summary>
    /// <param name="expected">The <see cref="ITableData"/> representing the entity returned.</param>
    /// <param name="response">The response to check.</param>
    public static void ResponseHasConditionalHeaders(ITableData expected, HttpResponseMessage response)
    {
        var lastModified = expected.UpdatedAt.ToString(DateTimeFormatInfo.InvariantInfo.RFC1123Pattern, CultureInfo.InvariantCulture);
        HasHeader(response, HeaderNames.ETag, expected.GetETag());
        HasHeader(response, HeaderNames.LastModified, lastModified);
    }

    /// <summary>
    /// Assert that the two JObjects are identical enough.
    /// </summary>
    /// <param name="expected"></param>
    /// <param name="actual"></param>
    public static void JsonEqual(JObject expected, JObject actual)
    {
        static bool DoCheck(JProperty prop) => prop.Value.Type switch
        {
            JTokenType.Null => false,
            JTokenType.Boolean => prop.Value.Value<bool>(),
            _ => true
        };

        // Each property in the expectedItem should be present in the actualItem unless the value is null.
        foreach (JProperty expectedProp in expected.Properties().Where(prop => DoCheck(prop)))
        {
            JProperty actualProp = actual.Properties().SingleOrDefault(ap => ap.Name == expectedProp.Name);
            // The deleted and updatedAt fields can be null, but will have a default value.
            if (expectedProp.Name == "deleted" && expectedProp.Value.Value<bool>() == false)
            {
                Assert.Null(actualProp);
            }
            else if (expectedProp.Name == "updatedAt" && DateTimeOffset.Parse(expectedProp.Value.Value<string>()) == DateTimeOffset.MinValue)
            {
                // actualProp can either be null or min value
                if (actualProp != null)
                {
                    Assert.Equal(DateTimeOffset.MinValue, DateTimeOffset.Parse(expectedProp.Value.Value<string>()));
                }
                // It's ok for actualProp to be null too!
            }
            else
            {
                Assert.Equal(expectedProp.Value, actualProp.Value);
            }
        }
    }

    /// <summary>
    /// Ensures that the two JObject lists are identical.
    /// </summary>
    /// <param name="expected"></param>
    /// <param name="actual"></param>
    public static void SequenceEqual(List<JObject> expected, List<JObject> actual)
    {
        Assert.Equal(expected.Count, actual.Count);
        for (int i = 0; i < expected.Count; i++)
        {
            JsonEqual(expected[i], actual[i]);
        }
    }

    /// <summary>
    /// Asserts if the system properties are set.
    /// </summary>
    /// <param name="entity">The entity that is being checked</param>
    public static void SystemPropertiesSet(ITableData entity, DateTimeOffset? startTime = null, int interval = 3000)
    {
        if (startTime.HasValue)
        {
            IsBetween(startTime.Value, DateTimeOffset.Now, entity.UpdatedAt);
        }
        else
        {
            CloseTo(DateTimeOffset.Now, entity.UpdatedAt, interval);
        }
        Assert.NotEmpty(entity.Version);
    }

    public static void SystemPropertiesChanged(ITableData original, ITableData replacement)
    {
        Assert.NotEqual(original.UpdatedAt, replacement.UpdatedAt);
        Assert.NotEqual(Convert.ToBase64String(original.Version), Convert.ToBase64String(replacement.Version));
    }

    /// <summary>
    /// Compares the server-side data to the client-side data.
    /// </summary>
    /// <param name="expected"></param>
    /// <param name="actual"></param>
    public static void SystemPropertiesMatch(ITableData expected, DatasyncClientData actual)
    {
        Assert.Equal(expected.Id, actual.Id);
        Assert.Equal(expected.UpdatedAt.ToUniversalTime().ToString(format), actual.UpdatedAt.ToUniversalTime().ToString(format));
        Assert.Equal(expected.Deleted, actual.Deleted);
        Assert.Equal(Convert.ToBase64String(expected.Version), actual.Version);
    }

    /// <summary>
    /// Compares the server-side data to the client-side data.
    /// </summary>
    /// <param name="expected"></param>
    /// <param name="actual"></param>
    public static void SystemPropertiesMatch(ITableData expected, JsonDocument actual)
    {
        Assert.Equal(expected.Id, actual.RootElement.GetProperty("id").GetString());
        Assert.Equal(expected.UpdatedAt.ToUniversalTime().ToString(format), actual.RootElement.GetProperty("updatedAt").GetString());
        Assert.Equal(expected.Deleted, actual.RootElement.GetProperty("deleted").GetBoolean());
        Assert.Equal(Convert.ToBase64String(expected.Version), actual.RootElement.GetProperty("version").GetString());
    }
}
