// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System;

namespace Microsoft.AzureMobile.Server
{
    /// <summary>
    /// A list of options for configuring the <see cref="TableController{TEntity}"/>.
    /// </summary>
    public class TableControllerOptions
    {
        private int _pageSize = 100;

        /// <summary>
        /// The page size of the results returned by a query operation.  This is also the maximum
        /// value that can be provided with the <c>$top</c> query option on list requests.
        /// </summary>
        public int PageSize
        {
            get { return _pageSize; }
            set
            {
                if (value <= 0 || value > 128000)
                {
                    throw new ArgumentException("PageSize value range is 1 - 128,000", nameof(PageSize));
                }
                _pageSize = value;
            }
        }
    }
}
