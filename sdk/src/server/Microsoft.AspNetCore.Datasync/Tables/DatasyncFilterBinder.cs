// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.OData.UriParser;
using Microsoft.Spatial;
using System.Linq.Expressions;
using System.Reflection;

namespace Microsoft.AspNetCore.Datasync.Tables;

/// <summary>
/// The <see cref="IFilterBinder"/> for implementing geo-spatial functions in the OData query.
/// </summary>
internal class DatasyncFilterBinder : FilterBinder
{
    /// <summary>
    /// Convenience constant to get all the static methods of a class.
    /// </summary>
    internal const BindingFlags AllMethods = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

    /// <summary>
    /// The name of the geo.distance function.
    /// </summary>
    internal const string GeoDistanceFunctionName = "geo.distance";

    /// <summary>
    /// The list of functions that we support, together with the <see cref="MethodInfo"/> for the implementation.
    /// </summary>
    internal static readonly Dictionary<string, MethodInfo> FunctionMethods = new()
    {
        { GeoDistanceFunctionName, typeof(ODataFunctions).GetMethod("GeoDistance", AllMethods, new[] { typeof(GeographyPoint), typeof(GeographyPoint) })! }
    };

    /// <inheritdoc />
    public override Expression BindSingleValueFunctionCallNode(SingleValueFunctionCallNode node, QueryBinderContext context)
        => node.Name switch
        {
            GeoDistanceFunctionName => BindGeoDistanceFunctionCallNode(node, context),
            _ => base.BindSingleValueFunctionCallNode(node, context)
        };

    #region Function binding methods
    /// <summary>
    /// The filter binding for the geo.distance function.
    /// </summary>
    /// <param name="node">The node that holds the parameters for the call.</param>
    /// <param name="context">The current query binder context.</param>
    /// <returns>An expression for the function result.</returns>
    public Expression BindGeoDistanceFunctionCallNode(SingleValueFunctionCallNode node, QueryBinderContext context)
    {
        Expression[] arguments = BindArguments(node.Parameters, context);
        return Expression.Call(FunctionMethods[GeoDistanceFunctionName], arguments[0], arguments[1]);
    }
    #endregion
}
