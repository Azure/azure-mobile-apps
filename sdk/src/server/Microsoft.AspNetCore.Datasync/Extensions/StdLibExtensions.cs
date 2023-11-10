using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Datasync;

internal static class StdLibExtensions
{
    /// <summary>
    /// Adds an <paramref name="item"/> to the <paramref name="collection"/> only if the <paramref name="condition"/> is <c>true</c>.
    /// </summary>
    /// <typeparam name="T">The type of object stored in the <paramref name="collection"/>.</typeparam>
    /// <param name="collection">The collection to modify.</param>
    /// <param name="condition">The condition under which the <paramref name="item"/> is added to the <paramref name="collection"/>.</param>
    /// <param name="item">The item to be added to the <paramref name="collection"/>.</param>
    internal static void AddIf<T>(this ICollection<T> collection, bool condition, T item)
    {
        if (condition)
        {
            collection.Add(item);
        }
    }

    /// <summary>
    /// Determines if the two strings are equal, ignoring case.
    /// </summary>
    internal static bool EqualsIgnoreCase(this string source, string comparison)
        => string.Equals(source, comparison, StringComparison.OrdinalIgnoreCase);

    /// <summary>
    /// Returns <c>true</c> if the provided source date is after the other date.
    /// </summary>
    /// <remarks>
    /// Returns false if other is null or if source is before other.
    /// </remarks>
    /// <param name="source">The source date.</param>
    /// <param name="other">The other date.</param>
    /// <returns><c>true</c> if the provided source date is after the other date.</returns>
    [ExcludeFromCodeCoverage]
    internal static bool IsAfter(this DateTimeOffset source, DateTimeOffset? other)
        => other.HasValue && source > other.Value;

    /// <summary>
    /// Returns <c>true</c> if the provided source date is before the other date.
    /// </summary>
    /// <remarks>
    /// Returns false if other is null or if source is after other.
    /// </remarks>
    /// <param name="source">The source date.</param>
    /// <param name="other">The other date.</param>
    /// <returns><c>true</c> if the provided source date is before the other date.</returns>
    [ExcludeFromCodeCoverage]
    internal static bool IsBefore(this DateTimeOffset source, DateTimeOffset? other)
        => other.HasValue && source <= other.Value;
}
