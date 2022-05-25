// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// ----------------------------------------------------------------------------

using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using System.Web.Http.OData;
using System.Web.Http.Services;
using System.Web.Http.Tracing;
using Microsoft.Azure.Mobile;
using Microsoft.Azure.Mobile.Server;
using Microsoft.Azure.Mobile.Server.Properties;
using Microsoft.Azure.Mobile.Server.Tables;

namespace System.Net.Http
{
    /// <summary>
    /// Extension methods for the <see cref="HttpRequestMessage"/> class providing functionality
    /// related to table <see cref="TableController{T}"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class TableHttpRequestMessageExtensions
    {
        private const string IsQueryableActionKey = "MS_IsQueryableAction";
        private const string ODataExpandOption = "$expand=";
        private const string ODataFilterOption = "$filter=";
        private const string ODataOrderByOption = "$orderby=";
        private const string ODataSelectOption = "$select=";
        private const string IncludeDeletedParameter = "__includedeleted";

        internal static readonly Dictionary<string, string> SystemProperties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "createdAt", "CreatedAt" },
            { "updatedAt", "UpdatedAt" },
            { "deleted", "Deleted" },
            { "version", "Version" },
        };

        private static readonly string[] EmptyQuery = new string[0];
        private static readonly ConcurrentDictionary<Type, string[]> MappedModelProperties = new ConcurrentDictionary<Type, string[]>();

        /// <summary>
        /// Determines whether deleted rows were requested by the client.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/>request</param> to check.
        /// <returns>True if the deleted rows are requested, otherwise False.</returns>
        public static bool AreDeletedRowsRequested(this HttpRequestMessage request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            string includeDeleted = request.GetQueryNameValuePairs()
                                           .Where(p => p.Key.Equals(IncludeDeletedParameter, StringComparison.OrdinalIgnoreCase))
                                           .Select(p => p.Value)
                                           .FirstOrDefault();
            return "true".Equals(includeDeleted, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Gets the version of a table item from the HTTP <c>IfMatch</c> header. If the <c>IfMatch</c> contains an invalid value then
        /// an <see cref="HttpResponseException"/> is thrown containing an <see cref="HttpResponseMessage"/> with status code <see cref="HttpStatusCode.BadRequest"/>.
        /// </summary>
        /// <param name="request"></param>
        /// <returns>A valid version byte array representing the version, or <c>null</c> if none were present.</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope", Justification = "Response is disposed by caller.")]
        public static byte[] GetVersionFromIfMatch(this HttpRequestMessage request)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            if (request.Headers.IfMatch != null)
            {
                if (request.Headers.IfMatch.Count == 1)
                {
                    EntityTagHeaderValue ifMatch = request.Headers.IfMatch.First();
                    if (ifMatch == EntityTagHeaderValue.Any)
                    {
                        return null;
                    }

                    if (ifMatch.IsWeak)
                    {
                        string error = TResources.TableController_InvalidIfMatch.FormatForUser(ifMatch.ToString());
                        HttpResponseMessage response = request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
                        throw new HttpResponseException(response);
                    }

                    try
                    {
                        string unquotedEtag = HttpHeaderUtils.GetUnquotedString(ifMatch.Tag);
                        return Convert.FromBase64String(unquotedEtag);
                    }
                    catch
                    {
                        string error = TResources.TableController_InvalidIfMatch.FormatForUser(ifMatch.ToString());
                        HttpResponseMessage response = request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
                        throw new HttpResponseException(response);
                    }
                }
                else if (request.Headers.IfMatch.Count > 1)
                {
                    string error = TResources.TableController_InvalidIfMatch.FormatForUser(request.Headers.IfMatch.ToString());
                    HttpResponseMessage response = request.CreateErrorResponse(HttpStatusCode.BadRequest, error);
                    throw new HttpResponseException(response);
                }
            }

            return null;
        }

        /// <summary>
        /// Determines the set of properties to include in the OData $select query option for types implementing the
        /// <see cref="ITableData"/> interface. This function determines the set of selected properties and updates the
        /// request URI accordingly, if necessary.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/>request</param> to manipulate.
        /// <param name="data">The type of the data model.</param>
        /// <param name="isModified">Indicates whether the original Request URI $select clause has been manipulated or not.</param>
        /// <remarks>The query is not actually validated in this step -- this happens as part of the <see cref="QueryableAttribute"/> validation.</remarks>
        /// <returns>In the case of a type implementing, the set of properties selected; otherwise null.</returns>
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", Justification = "It's ok to have an out parameter here.")]
        public static IList<string> SetSelectedProperties(this HttpRequestMessage request, Type data, out bool isModified)
        {
            return SetSelectedProperties(request, data, null, out isModified);
        }

        /// <summary>
        /// Determines the set of properties to include in the OData $select query option for types implementing the
        /// <see cref="ITableData"/> interface. This function determines the combined set of selected properties and updates the
        /// request URI accordingly, if necessary.
        /// </summary>
        /// <param name="request">The <see cref="HttpRequestMessage"/>request</param> to manipulate.
        /// <param name="data">The type of the data model.</param>
        /// <param name="systemPropertyMap">Optional map specifying replacement names for system properties.</param>
        /// <param name="isModified">Indicates whether the original Request URI $select clause has been manipulated or not.</param>
        /// <remarks>The query is not actually validated in this step -- this happens as part of the <see cref="QueryableAttribute"/> validation.</remarks>
        /// <returns>In the case of a type implementing, the set of properties selected; otherwise null.</returns>
        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters", Justification = "It's ok to have an out parameter here.")]
        public static IList<string> SetSelectedProperties(this HttpRequestMessage request, Type data, IDictionary<string, string> systemPropertyMap, out bool isModified)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            isModified = false;

            // Check that this type implements ITableData -- if not then just return null
            if (!typeof(ITableData).IsAssignableFrom(data))
            {
                return null;
            }

            // Parse the request URI looking for $select as well as properties that should be PascalCased.
            string[] parts = EmptyQuery;
            int expandIndex = -1, filterIndex = -1, orderByIndex = -1, selectIndex = -1;
            string expand = null, filter = null, orderBy = null, select = null;

            // Get the mapped properties from the model
            string[] modelMappedProperties = GetMappedModelProperties(request, data);

            // Look for various query options
            if (!String.IsNullOrEmpty(request.RequestUri.Query))
            {
                string query = request.RequestUri.Query.Substring(1);
                parts = query.Split(new char[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
                expand = GetQueryOption(parts, ODataExpandOption, modelMappedProperties, systemPropertyMap, out expandIndex);
                filter = GetQueryOption(parts, ODataFilterOption, modelMappedProperties, systemPropertyMap, out filterIndex);
                orderBy = GetQueryOption(parts, ODataOrderByOption, modelMappedProperties, systemPropertyMap, out orderByIndex);
                select = GetQueryOption(parts, ODataSelectOption, modelMappedProperties, systemPropertyMap, out selectIndex);
            }

            // Check whether we updated any of the query options by converting them to PascalCase.
            isModified = expand != null || filter != null || orderBy != null || select != null;

            List<string> properties = new List<string>();
            if (select == null)
            {
                // if there isn't a select, we need to get the default
                properties.AddRange(modelMappedProperties);

                // Also add any manually mapped system properties
                if (systemPropertyMap != null)
                {
                    foreach (var prop in systemPropertyMap)
                    {
                        // The actual property for the system property name may be present
                        if (!properties.Contains(prop.Value))
                        {
                            properties.Add(prop.Value);
                        }
                    }
                }

                isModified = true;
            }
            else
            {
                properties.AddRange(select.Split(',').Select(s => Uri.UnescapeDataString(s)));
            }

            // If we haven't modified anything then we are done
            if (!isModified)
            {
                return properties;
            }

            if (expandIndex > -1)
            {
                parts[expandIndex] = ODataExpandOption + expand;
            }

            if (filterIndex > -1)
            {
                parts[filterIndex] = ODataFilterOption + filter;
            }

            if (orderByIndex > -1)
            {
                parts[orderByIndex] = ODataOrderByOption + orderBy;
            }

            // Update existing $select or add new $select
            if (selectIndex > -1)
            {
                parts[selectIndex] = GetSelectOption(properties);
            }
            else
            {
                parts = new List<string>(parts) { GetSelectOption(properties) }.ToArray();
            }

            if (isModified)
            {
                // Update the request URI with the new query. Character encoding will happens as part of updating the URI.
                UriBuilder modifiedRequestUri = new UriBuilder(request.RequestUri);
                modifiedRequestUri.Query = GetQuery(parts);
                request.RequestUri = modifiedRequestUri.Uri;

                // if we've rewritten the uri, we need to remove any pre-parsed odata query
                // options from the request properties, so our modifications will take effect
                request.Properties.Remove("MS_QueryNameValuePairs");
            }

            return properties;
        }

        internal static bool IsQueryableAction(this HttpRequestMessage request, HttpActionDescriptor actionDescriptor)
        {
            if (request == null)
            {
                throw new ArgumentNullException("request");
            }

            bool result;
            if (!request.Properties.TryGetValue(IsQueryableActionKey, out result))
            {
                result = actionDescriptor != null && actionDescriptor.GetFilterPipeline().Any(f =>
                {
                    // When Web Api diagnostic logging is on, all filters are wrapped in a FilterTracer.
                    // FilterTracer implements IDecorator<IFilter>, which lets us get at the inner (and actual) filter.
                    IFilter filter = f.Instance;
                    IDecorator<IFilter> decorator = filter as IDecorator<IFilter>;
                    if (decorator != null)
                    {
                        filter = decorator.Inner;
                    }

                    Type instance = filter.GetType();
                    return instance == typeof(QueryableAttribute) || instance == typeof(EnableQueryAttribute);
                });

                request.Properties[IsQueryableActionKey] = result;
            }

            return result;
        }

        internal static string[] GetMappedModelProperties(HttpRequestMessage request, Type modelType)
        {
            return MappedModelProperties.GetOrAdd(modelType, type =>
            {
                try
                {
                    return type.GetProperties()
                        .Where(p => p.GetCustomAttribute<NotMappedAttribute>() == null)
                        .Select(p => p.Name).ToArray();
                }
                catch (Exception ex)
                {
                    HttpConfiguration config = request.GetConfiguration();
                    if (config != null)
                    {
                        ITraceWriter writer = config.Services.GetTraceWriter();
                        if (writer != null)
                        {
                            writer.Error(TResources.TableController_NoProperties.FormatForUser(type.Name, ex.Message), ex, request, ServiceLogCategories.TableControllers);
                        }
                    }

                    return new string[0];
                }
            });
        }

        /// <summary>
        /// Detects whether the <paramref name="option"/> is present in an OData query
        /// and if so returns the index and the value of the option.
        /// </summary>
        /// <param name="query">The query to check.</param>
        /// <param name="option">The option to check for.</param>
        /// <param name="index">The index into the query if the option is present; -1 otherwise.</param>
        /// <param name="modelMappedProperties">The mapped properties for this model (CLR type).</param>
        /// <param name="systemPropertyMap">Optional map specifying replacement names for system properties.</param>
        /// <param name="comparer">The <see cref="StringComparison"/> to use when looking for the query parameter (default is <see cref="StringComparison.Ordinal"/>).</param>
        /// <returns>The query option value if present; null otherwise.</returns>
        private static string GetQueryOption(string[] query, string option, string[] modelMappedProperties, IDictionary<string, string> systemPropertyMap, out int index, StringComparison comparer = StringComparison.Ordinal)
        {
            for (int cnt = 0; cnt < query.Length; cnt++)
            {
                string segment = query[cnt];
                if (segment.StartsWith(option, comparer))
                {
                    index = cnt;
                    string value = segment.Substring(option.Length);

                    value = TransformSystemProperties(value, systemPropertyMap);
                    value = PascalCasedQueryOption(value, modelMappedProperties);
                    return value;
                }
            }

            index = -1;
            return null;
        }

        internal static string PascalCasedQueryOption(string queryOption, IEnumerable<string> modelMappedProperties)
        {
            return TransformProperties(queryOption, modelMappedProperties, (option, property, index) =>
            {
                option[index] = Char.ToUpperInvariant(option[index]);
                return index + property.Length; // no character added or removed
            });
        }

        internal static string TransformSystemProperties(string queryOption, IDictionary<string, string> systemPropertyMap)
        {
            if (systemPropertyMap == null || systemPropertyMap.Keys.Count == 0)
            {
                return queryOption;
            }

            return TransformProperties(queryOption, systemPropertyMap.Keys, (option, property, index) =>
            {
                int newIndex = index;
                string replacement;
                if (systemPropertyMap != null && systemPropertyMap.TryGetValue(property, out replacement))
                {
                    option.Remove(index, property.Length);
                    option.Insert(index, replacement);
                    newIndex = index + replacement.Length;
                }
                else
                {
                    option[index] = Char.ToUpperInvariant(option[index]);
                    newIndex = index + property.Length;
                }

                return newIndex;
            });
        }

        private static string TransformProperties(string queryOption, IEnumerable<string> mappedProperties, Func<StringBuilder, string, int, int> action)
        {
            StringBuilder option = new StringBuilder(queryOption);
            foreach (string mappedProperty in mappedProperties)
            {
                string property = Uri.EscapeDataString(StringUtils.ToCamelCase(mappedProperty));
                int index = 0;
                while ((index = option.ToString().IndexOf(property, index, StringComparison.Ordinal)) > -1)
                {
                    bool start = true;
                    if (index > 0)
                    {
                        char ch = option[index - 1];
                        start = ch == '(' || ch == '=' || ch == ',' || ch == '&' || ch == '0' || ch == '+';
                    }

                    bool end = true;
                    if (start && index + property.Length < option.Length)
                    {
                        char ch = option[index + property.Length];
                        end = ch == ')' || ch == '=' || ch == ',' || ch == '&' || ch == '%' || ch == '+';
                    }

                    // If both start and end are true then we have a token
                    if (start && end)
                    {
                        index = action(option, mappedProperty, index);
                    }
                    else
                    {
                        index++;
                    }
                }
            }

            return option.ToString();
        }

        private static string GetSelectOption(IEnumerable<string> properties)
        {
            return ODataSelectOption + String.Join(",", properties);
        }

        private static string GetQuery(IEnumerable<string> parts)
        {
            return String.Join("&", parts.Where(p => p.Length > 0));
        }
    }
}