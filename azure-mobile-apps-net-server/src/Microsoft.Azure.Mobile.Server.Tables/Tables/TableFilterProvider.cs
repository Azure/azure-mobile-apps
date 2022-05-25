// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.OData;
using System.Web.Http.OData.Query;

namespace Microsoft.Azure.Mobile.Server.Tables
{
    /// <summary>
    /// The <see cref="TableFilterProvider"/> registers specialized <see cref="IActionFilter"/> instances
    /// used by the <see cref="TableController{T}"/>. The filters are registered as part of the custom controller configuration
    /// which can be configured using the dependency injection engine using the type <see cref="ITableControllerConfigProvider"/>.
    /// </summary>
    public class TableFilterProvider : IFilterProvider
    {
        private readonly QueryableAttribute queryFilter;
        private readonly QueryFilterProvider queryFilterProvider;
        private readonly FilterInfo tableFilter;

        /// <summary>
        /// Initializes a new instance of the <see cref="TableFilterProvider"/> using the default <see cref="QueryableAttribute"/>
        /// implementation for executing the query.
        /// </summary>
        public TableFilterProvider()
            : this(new EnableQueryAttribute() { PageSize = TableUtils.PageSize })
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TableFilterProvider"/> using the provided <see cref="QueryableAttribute"/>
        /// implementation for executing the query. 
        /// </summary>
        public TableFilterProvider(IActionFilter queryFilter)
        {
            if (queryFilter == null)
            {
                throw new ArgumentNullException("queryFilter");
            }

            this.queryFilter = new QueryableAttribute() { PageSize = TableUtils.PageSize };
            this.queryFilterProvider = new QueryFilterProvider(queryFilter);
            this.tableFilter = new FilterInfo(new TableQueryFilter(), FilterScope.Global);
        }

        /// <inheritdoc />
        public IEnumerable<FilterInfo> GetFilters(HttpConfiguration configuration, HttpActionDescriptor actionDescriptor)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            if (actionDescriptor == null)
            {
                throw new ArgumentNullException("actionDescriptor");
            }

            // We always add the table filter as we don't know whether there is a custom query filter on the controller and/or action
            List<FilterInfo> filters = new List<FilterInfo>
            {
                this.tableFilter
            };

            // Ask all other filter providers whether they have added a query filter
            FilterInfo[] filterInfos = configuration.Services.GetFilterProviders()
                .Where(provider => provider.GetType() != typeof(TableFilterProvider))
                .SelectMany((IFilterProvider fp) => fp.GetFilters(configuration, actionDescriptor))
                .ToArray();

            FilterInfo[] queryFilters = this.queryFilterProvider.GetFilters(configuration, actionDescriptor).ToArray();
            if (queryFilters.Length == 0)
            {
                return filters;
            }

            // We want the query filter to be on the inside of the table filter so we add the query filter after the table filter.
            if (HasFilter(filterInfos, typeof(QueryableAttribute)))
            {
                filters.Add(new FilterInfo(this.queryFilter, FilterScope.Global));
            }
            else
            {
                filters.AddRange(queryFilters);
            }

            return filters;
        }

        internal static bool HasFilter(IEnumerable<FilterInfo> filterInfos, Type filterType)
        {
            return filterInfos != null && filterInfos.Any(fInfo => fInfo.Instance != null && fInfo.Instance.GetType() == filterType);
        }
    }
}
