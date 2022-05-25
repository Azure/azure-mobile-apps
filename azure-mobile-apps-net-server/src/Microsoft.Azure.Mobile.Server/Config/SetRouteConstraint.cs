// ---------------------------------------------------------------------------- 
// Copyright (c) Microsoft Corporation. All rights reserved.
// ---------------------------------------------------------------------------- 

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Web.Http.Routing;

namespace Microsoft.Azure.Mobile.Server.Config
{
    /// <summary>
    /// A route constraint that constrains a route parameter value to be either included or excluded 
    /// from a set of values of type <typeparamref name="TSet"/>.
    /// </summary>
    /// <typeparam name="TSet">Type of set of values to include or exclude.</typeparam>
    public class SetRouteConstraint<TSet> : IHttpRouteConstraint
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SetRouteConstraint{TSet}"/> class. Using
        /// the <paramref name="matchOnExcluded"/> to indicate whether the constraint requires
        /// the parameter value to be included or excluded from the provided set of values.
        /// </summary>
        /// <param name="set">The set of values to match against.</param>
        /// <param name="matchOnExcluded">Indicates that the parameter value should be included or excluded from the set.</param>
        public SetRouteConstraint(HashSet<TSet> set, bool matchOnExcluded)
        {
            if (set == null)
            {
                throw new ArgumentNullException("set");
            }

            this.Set = set;
            this.Excluded = matchOnExcluded;
        }

        /// <summary>
        /// Indicates whether the matching parameter name should be included or excluded in the value set in order
        /// to be a match.
        /// </summary>
        public bool Excluded { get; private set; }

        /// <summary>
        /// Gets the set of excluded or included values.
        /// </summary>
        public HashSet<TSet> Set { get; private set; }

        /// <inheritdoc />
        public bool Match(HttpRequestMessage request, IHttpRoute route, string parameterName, IDictionary<string, object> values, HttpRouteDirection routeDirection)
        {
            if (parameterName == null)
            {
                throw new ArgumentNullException("parameterName");
            }

            if (values == null)
            {
                throw new ArgumentNullException("values");
            }

            TSet value;
            if (values.TryGetValue(parameterName, out value))
            {
                return this.Excluded ^ this.Set.Contains(value);
            }

            return false;
        }
    }
}
