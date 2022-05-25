// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using Newtonsoft.Json.Linq;

namespace Microsoft.WindowsAzure.MobileServices
{
    internal static class JTokenExtensions
    {
        /// <summary>
        /// Determines if a JToken is a valid item
        /// </summary>
        /// <param name="item">The jtoken to check.</param>
        /// <returns>True if it is a valid item, False otherwise.</returns>
        public static bool IsValidItem(this JToken item)
            => item is JObject obj && HasId(obj);

        /// <summary>
        /// If specified JToken is a valid item then returns it otherwise returns null
        /// </summary>
        /// <param name="item">The jtoken to check.</param>
        /// <returns><paramref name="item"/> as JObject if it is valid, othewise null.</returns>
        public static JObject ValidItemOrNull(this JToken item)
            => IsValidItem(item) ? (JObject)item : null;

        private static bool HasId(JObject obj)
        {
            Arguments.IsNotNull(obj, nameof(obj));
            return obj.Value<string>(MobileServiceSystemColumns.Id) != null;
        }
    }
}
