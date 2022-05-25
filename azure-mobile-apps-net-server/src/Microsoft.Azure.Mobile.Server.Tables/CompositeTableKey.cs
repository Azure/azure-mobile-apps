// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Azure.Mobile.Server.Properties;

namespace Microsoft.Azure.Mobile.Server
{
    /// <summary>
    /// A <see cref="CompositeTableKey"/> contains one or more keys used to identify a single row in a table.
    /// The string format of a <see cref="CompositeTableKey"/> is a comma separated list (without LWS) of optionally single-quoted terms.
    /// The terms only have to be quoted if they contain a comma.
    /// </summary>
    public class CompositeTableKey
    {
        private const char Separator = ',';
        private const char SingleQuote = '\'';

        /// <summary>
        /// Initialize a new instance of the <see cref="CompositeTableKey"/> with a given number of <see cref="string"/>
        /// representing an ordered list of segments making up the composite key.
        /// </summary>
        /// <param name="segments">The ordered set of <see cref="string"/> segments making up the composite key.</param>
        public CompositeTableKey(params string[] segments)
        {
            if (segments == null)
            {
                throw new ArgumentNullException("segments");
            }

            this.Segments = new Collection<string>(segments);
        }

        private enum KeyReaderState
        {
            Separator = 0,
            QuotedKey,
            QuoteWithinQuotedKey
        }

        /// <summary>
        /// Gets the ordered <see cref="Collection{T}"/> of segments making up the composite key.
        /// </summary>
        public Collection<string> Segments { get; private set; }

        /// <summary>
        /// Parse a string as a <see cref="CompositeTableKey"/>. The value has to be a comma separated list (without LWS) of optionally single-quoted terms.
        /// If the value is not valid then an <see cref="ArgumentException"/> is thrown.
        /// </summary>
        /// <param name="tableKey">The value containing the composite key.</param>
        /// <returns>A new <see cref="CompositeTableKey"/> instance.</returns>
        public static CompositeTableKey Parse(string tableKey)
        {
            string[] subkeys = ParseTableKey(tableKey);
            return new CompositeTableKey(subkeys);
        }

        /// <summary>
        /// Attempts creating a new <see cref="CompositeTableKey"/> from a given <paramref name="tableKey"/>.
        /// The return value indicates whether the parsing succeeded.
        /// </summary>
        /// <param name="tableKey">The value containing the composite key.</param>
        /// <param name="compositeTableKey">If the method returns <c>true</c> then <paramref name="compositeTableKey"/> contains the result; otherwise <c>null</c>.</param>
        /// <returns></returns>
        public static bool TryParse(string tableKey, out CompositeTableKey compositeTableKey)
        {
            try
            {
                compositeTableKey = Parse(tableKey);
                return true;
            }
            catch
            {
                compositeTableKey = null;
                return false;
            }
        }

        private static string[] ParseTableKey(string key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            if (key.Length == 0)
            {
                return new string[0];
            }

            List<string> parsedKeys = new List<string>();
            int keyStartOffset = 0;
            KeyReaderState state = KeyReaderState.Separator;
            for (int cnt = 0; cnt < key.Length; cnt++)
            {
                char ch = key[cnt];
                switch (state)
                {
                    case KeyReaderState.Separator:
                        if (ch == SingleQuote)
                        {
                            keyStartOffset = cnt + 1;
                            state = KeyReaderState.QuotedKey;
                        }
                        else
                        {
                            throw new ArgumentException(TResources.TableKeys_InvalidKey.FormatForUser(key));
                        }

                        break;

                    case KeyReaderState.QuotedKey:
                        if (ch == SingleQuote)
                        {
                            state = KeyReaderState.QuoteWithinQuotedKey;
                        }

                        break;

                    case KeyReaderState.QuoteWithinQuotedKey:
                        if (ch == Separator)
                        {
                            parsedKeys.Add(key.Substring(keyStartOffset, cnt - keyStartOffset - 1));
                            state = KeyReaderState.Separator;
                        }

                        break;
                }
            }

            if (state == KeyReaderState.QuoteWithinQuotedKey)
            {
                parsedKeys.Add(key.Substring(keyStartOffset, key.Length - keyStartOffset - 1));
            }
            else
            {
                throw new ArgumentException(TResources.TableKeys_InvalidKey.FormatForUser(key));
            }

            return parsedKeys.ToArray();
        }

        /// <summary>
        /// Generates a <see cref="string"/> representation of the composite key.
        /// </summary>
        /// <returns>A <see cref="string"/> representation of the composite key.</returns>
        public override string ToString()
        {
            if (this.Segments.Count == 0)
            {
                return string.Empty;
            }

            IEnumerable<string> quotedKeys = this.Segments.Select(key => SingleQuote + key + SingleQuote);
            return String.Join(Separator.ToString(), quotedKeys);
        }
    }
}
