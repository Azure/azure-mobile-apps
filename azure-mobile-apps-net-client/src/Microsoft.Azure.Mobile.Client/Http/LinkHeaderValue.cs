// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System;
using System.Text.RegularExpressions;

namespace Microsoft.WindowsAzure.MobileServices
{
    internal class LinkHeaderValue
    {
        static readonly Regex pattern = new Regex(@"^(?<uri>.*?);\s*rel\s*=\s*(?<rel>\w+)\s*$");
        public Uri Uri { get; private set; }
        public string Relation { get; private set; }

        public LinkHeaderValue(string uri, string rel)
        {
            Uri.TryCreate(uri, UriKind.RelativeOrAbsolute, out Uri value);
            Uri = value;
            Relation = rel;
        }

        public static LinkHeaderValue Parse(string value)
        {
            string uri = null, rel = null;

            if (!string.IsNullOrEmpty(value))
            {
                Match result = pattern.Match(value ?? string.Empty);
                uri = result.Groups["uri"].Value;
                rel = result.Groups["rel"].Value;
            }

            return new LinkHeaderValue(uri, rel);
        }
    }
}
