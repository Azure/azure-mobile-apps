// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Diagnostics.CodeAnalysis;

namespace Microsoft.AzureMobile.Common.Test
{
    [ExcludeFromCodeCoverage(Justification = "Test suite")]
    public static class Utils
    {
        /// <summary>
        /// Converts an index into an ID for the Movies controller.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static string GetMovieId(int index) => string.Format("id-{0:000}", index);
    }
}
