using Microsoft.AspNetCore.OData.Query.Expressions;
using Microsoft.OData.Edm;
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
    /// The list of functions that are supported by this binder.
    /// </summary>
    internal const string GeoDistanceFunctionName = "geo.distance";

    /// <summary>
    /// Convenience constant to get all the static methods of a class.
    /// </summary>
    internal const BindingFlags AllMethods = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

    /// <inheritdoc />
    public override Expression BindSingleValueFunctionCallNode(SingleValueFunctionCallNode node, QueryBinderContext context)
    {
        switch (node.Name)
        {
            case GeoDistanceFunctionName:
                return BindGeoDistanceFunctionCallNode(node, context);
        }
        return base.BindSingleValueFunctionCallNode(node, context);
    }

    #region Function binding methods
    /// <summary>
    /// The filter binding for the geo.distance function.
    /// </summary>
    /// <param name="node">The node that holds the parameters for the call.</param>
    /// <param name="context">The current query binder context.</param>
    /// <returns>An expression for the function result.</returns>
    public Expression BindGeoDistanceFunctionCallNode(SingleValueFunctionCallNode node, QueryBinderContext context)
    {
        MethodInfo methodInfo = typeof(ODataFunctions).GetMethod("GeoDistance", AllMethods, new[] { typeof(GeographyPoint), typeof(GeographyPoint) })!;
        Expression[] arguments = BindArguments(node.Parameters, context);
        return Expression.Call(methodInfo, arguments[0], arguments[1]);
    }
    #endregion
}
