// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Microsoft.Azure.Mobile;

namespace System.Net.Http.Headers
{
    /// <summary>
    /// A minimal Web Link (RFC 5988) implementation providing a mechanism for serializing links.
    /// </summary>
    internal class LinkHeaderValue : ICloneable
    {
        private const string RelParameter = "rel";
        private const string RevParameter = "rev";
        private const string TitleParameter = "title";
        private const string TitleStarParameter = "title*";
        private const string TypeParameter = "type";

        // Use list instead of dictionary since we may have multiple parameters with the same name.
        private readonly Collection<NameValueHeaderValue> parameters = new Collection<NameValueHeaderValue>();
        private Uri address;

        public LinkHeaderValue(string address)
            : this(new Uri(address))
        {
        }

        public LinkHeaderValue(Uri address)
        {
            if (address == null)
            {
                throw new ArgumentNullException("address");
            }

            this.address = address;
        }

        protected LinkHeaderValue(LinkHeaderValue source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            this.address = source.address;

            if (source.parameters != null)
            {
                foreach (var parameter in source.parameters)
                {
                    this.Parameters.Add((NameValueHeaderValue)((ICloneable)parameter).Clone());
                }
            }
        }

        public Uri Address
        {
            get
            {
                return this.address;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                this.address = value;
            }
        }

        public ICollection<NameValueHeaderValue> Parameters
        {
            get
            {
                return this.parameters;
            }
        }

        public string Rel
        {
            get { return this.GetParameter(RelParameter); }
            set { this.SetParameter(RelParameter, value); }
        }

        public string Rev
        {
            get { return this.GetParameter(RevParameter); }
            set { this.SetParameter(RevParameter, value); }
        }

        public string Title
        {
            get { return this.GetParameter(TitleParameter); }
            set { this.SetParameter(TitleParameter, value); }
        }

        public string TitleStar
        {
            get { return this.GetParameter(TitleStarParameter); }
            set { this.SetParameter(TitleStarParameter, value); }
        }

        public override string ToString()
        {
            StringBuilder header = new StringBuilder(this.address.AbsoluteUri);
            if (this.parameters.Count > 0)
            {
                header.Append("; ");
                header.Append(String.Join("; ", this.parameters));
            }

            return header.ToString();
        }

        object ICloneable.Clone()
        {
            return new LinkHeaderValue(this);
        }

        private NameValueHeaderValue FindParameter(string parameter)
        {
            foreach (NameValueHeaderValue value in this.parameters)
            {
                if (String.Equals(value.Name, parameter, StringComparison.OrdinalIgnoreCase))
                {
                    return value;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets a parameter of the given name and attempts to decode it if necessary.
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <returns>Null if the parameter is not present or the raw value if the encoding is incorrect.</returns>
        private string GetParameter(string name)
        {
            NameValueHeaderValue parameter = this.FindParameter(name);
            if (parameter != null)
            {
                // Look for parameter names ending in "*" as that signifies RFC 5987 encoding
                if (name.EndsWith("*", StringComparison.Ordinal))
                {
                    string result;
                    if (TryDecode5987(parameter.Value, out result))
                    {
                        return result;
                    }

                    return null; // Unrecognized encoding
                }

                // May not have been encoded
                return parameter.Value;
            }

            return null;
        }

        /// <summary>
        /// Add/update the given parameter in the list, encoding it if necessary. Remove if value is null/Empty 
        /// </summary>
        /// <param name="name">The name of the parameter.</param>
        /// <param name="value">The value to assign the parameter.</param>
        private void SetParameter(string name, string value)
        {
            NameValueHeaderValue parameter = this.FindParameter(name);
            if (string.IsNullOrEmpty(value))
            {
                // Remove parameter
                if (parameter != null)
                {
                    this.parameters.Remove(parameter);
                }
            }
            else
            {
                string processedValue = string.Empty;
                if (name.EndsWith("*", StringComparison.Ordinal))
                {
                    processedValue = Encode5987(value);
                }
                else
                {
                    processedValue = value;
                }

                if (parameter != null)
                {
                    parameter.Value = processedValue;
                }
                else
                {
                    this.Parameters.Add(new NameValueHeaderValue(name, processedValue));
                }
            }
        }

        /// <summary>
        /// Encode a string using RFC 5987 encoding using the format: <c>encoding'lang'PercentEncodedSpecials</c>
        /// </summary>
        /// <param name="input">The value to encode.</param>
        /// <returns>The encoded value</returns>
        private static string Encode5987(string input)
        {
            StringBuilder builder = new StringBuilder("utf-8\'\'");
            foreach (char c in input)
            {
                // attr-char = ALPHA / DIGIT / "!" / "#" / "$" / "&" / "+" / "-" / "." / "^" / "_" / "`" / "|" / "~"
                //      ; token except ( "*" / "'" / "%" )
                // Encodes as multiple utf-8 bytes
                if (c > 0x7F)
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(c.ToString());
                    foreach (byte b in bytes)
                    {
                        builder.Append(Uri.HexEscape((char)b));
                    }
                }
                else if (!HttpHeaderUtils.IsTokenChar(c) || c == '*' || c == '\'' || c == '%')
                {
                    // ASCII - Only one encoded byte
                    builder.Append(Uri.HexEscape(c));
                }
                else
                {
                    builder.Append(c);
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// Attempt to decode using RFC 5987 encoding of the format: <c>encoding'language'my%20string</c>
        /// </summary>
        /// <param name="input">The value to decode</param>
        /// <param name="output">The decoded output</param>
        /// <returns>true if value could be decoded; false otherwise.</returns>
        private static bool TryDecode5987(string input, out string output)
        {
            output = null;
            string[] parts = input.Split('\'');
            if (parts.Length != 3)
            {
                return false;
            }

            StringBuilder decoded = new StringBuilder();
            try
            {
                Encoding encoding = Encoding.GetEncoding(parts[0]);

                string dataString = parts[2];
                byte[] unescapedBytes = new byte[dataString.Length];
                int unescapedBytesCount = 0;
                for (int index = 0; index < dataString.Length; index++)
                {
                    // %FF
                    if (Uri.IsHexEncoding(dataString, index))
                    {
                        // Unescape and cache bytes, multi-byte characters must be decoded all at once
                        unescapedBytes[unescapedBytesCount++] = (byte)Uri.HexUnescape(dataString, ref index);
                        index--; // HexUnescape did +=3; Offset the for loop's ++
                    }
                    else
                    {
                        if (unescapedBytesCount > 0)
                        {
                            // Decode any previously cached bytes
                            decoded.Append(encoding.GetString(unescapedBytes, 0, unescapedBytesCount));
                            unescapedBytesCount = 0;
                        }

                        decoded.Append(dataString[index]); // Normal safe character
                    }
                }

                if (unescapedBytesCount > 0)
                {
                    // Decode any previously cached bytes
                    decoded.Append(encoding.GetString(unescapedBytes, 0, unescapedBytesCount));
                }
            }
            catch (ArgumentException)
            {
                return false; // Unknown encoding or bad characters
            }

            output = decoded.ToString();
            return true;
        }
    }
}
