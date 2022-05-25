// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;
using System.Collections;
using Microsoft.Azure.Mobile.Server.Properties;

namespace Microsoft.Azure.Mobile.Server.Tables
{
    /// <summary>
    /// Represents the results of a query request along with the total count of entities 
    /// identified by the request URI after all $filter system query options have been applied. 
    /// </summary>
    public class QueryResult
    {
        private const int MinCount = 0;

        private IEnumerable results;
        private long? count;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryResult"/> with a given subset of <paramref name="results"/>
        /// and optionally a <paramref name="totalCount"/>.
        /// </summary>
        /// <param name="results">The subset representing the query result.</param>
        /// <param name="totalCount">Optionally the total count or elements available.</param>
        public QueryResult(IEnumerable results, long? totalCount)
        {
            if (results == null)
            {
                throw new ArgumentNullException("results");
            }

            this.results = results;
            this.count = totalCount;
        }

        /// <summary>
        /// The results of the query.
        /// </summary>
        public IEnumerable Results
        {
            get
            {
                return this.results;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }

                this.results = value;
            }
        }

        /// <summary>
        /// The total count of entities identified by the request URI after all $filter 
        /// system query options have been applied. 
        /// </summary>
        public long? Count
        {
            get
            {
                return this.count;
            }

            set
            {
                if (value.HasValue && value < 0)
                {
                    throw new ArgumentOutOfRangeException("value", value, CommonResources.ArgMustBeGreaterThanOrEqualTo.FormatForUser(MinCount));
                }

                this.count = value;
            }
        }
    }
}
