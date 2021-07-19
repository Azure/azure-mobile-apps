﻿// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Table.Query.Nodes;
using Microsoft.Datasync.Client.Table.Query.Tables;
using Microsoft.Datasync.Client.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.Json.Serialization;

namespace Microsoft.Datasync.Client.Table.Query
{
    /// <summary>
    /// Walks the LINQ expression tree to compile the expression into a simplified tree of
    /// nodes that can be used for OData or SQL production.
    /// </summary>
    internal sealed class FilterBuildingExpressionVisitor
    {
        private static readonly MethodInfo concatMethod = typeof(string).GetRuntimeMethod("Concat", new Type[] { typeof(string), typeof(string) });
        private static readonly MethodInfo toStringMethod = typeof(object).GetTypeInfo().GetDeclaredMethod("ToString");

        /// <summary>
        /// <para>** Do not remove this code. **</para>
        /// </summary>
        /// <remarks>
        /// Some compilers will remove method infos that are never called by an application.
        /// This will break reflection scenarios when the methodInfos searched for via reflection
        /// were not used in the application code and so were removed by the compiler. We search
        /// for the methodInfos for Object.ToString() and String.Concat(string, string) via
        /// reflection, so we need this code here to ensure that don't get removed by the compiler.
        /// </remarks>
        [SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "See code comments - this is required!!!")]
        static FilterBuildingExpressionVisitor()
        {
            string aString = new Object().ToString();
            aString = String.Concat(aString, "a string");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FilterBuildingExpressionVisitor"/>
        /// </summary>
        /// <param name="clientOptions">The client options for the table being queried</param>
        internal FilterBuildingExpressionVisitor(DatasyncClientOptions clientOptions) : base()
        {
            Validate.IsNotNull(clientOptions, nameof(clientOptions));
            ClientOptions = clientOptions;
        }

        /// <summary>
        /// The client options for the table being queried
        /// </summary>
        internal DatasyncClientOptions ClientOptions { get; }

        /// <summary>
        /// The accumulator for the <see cref="QueryNode"/> being generated by the compiler
        /// </summary>
        internal Stack<QueryNode> FilterExpression { get; } = new Stack<QueryNode>();

        /// <summary>
        /// Translate an expression tree into a compiled OData query.
        /// </summary>
        /// <param name="expression">The expression tree to translate</param>
        /// <param name="clientOptions">The client options for the table being queried</param>
        /// <returns>An OData <see cref="QueryNode"/></returns>
        internal static QueryNode Compile(Expression expression, DatasyncClientOptions clientOptions)
        {
            var visitor = new FilterBuildingExpressionVisitor(clientOptions);
            visitor.Visit(expression);
            return visitor.FilterExpression.FirstOrDefault();
        }

        /// <summary>
        /// Gets the table member name referenced by an expression, or return null.
        /// </summary>
        /// <param name="expression">The expression to check</param>
        /// <param name="clientOptions">the client options for the table.</param>
        /// <returns>the table member name</returns>
        /// <remarks>
        /// Excluded from code coverage
        /// TODO: we cannot test the <c>member.Expression.NodeType == ExpressionType.Parameter</c> without an example.
        /// </remarks>
        internal static string GetTableMemberName(Expression expression, DatasyncClientOptions clientOptions)
        {
            Validate.IsNotNull(expression, nameof(expression));
            Validate.IsNotNull(clientOptions, nameof(clientOptions));

            if (expression is MemberExpression member && member.Expression.NodeType == ExpressionType.Parameter)
            {
                return ResolvePropertyName(member.Member, clientOptions);
            }
            return null;
        }

        /// <summary>
        /// Visits a node of the expression and all sub-nodes, effectively walking the expression tree.
        /// </summary>
        /// <param name="node">The node to visit</param>
        /// <returns>The visited node.</returns>
        internal Expression Visit(Expression node)
        {
            if (node != null)
            {
                // Don't convert to switch expression - this is easier to read.
#pragma warning disable IDE0066 // Convert switch statement to expression
                switch (node.NodeType)
                {
                    case ExpressionType.Add:
                    case ExpressionType.And:
                    case ExpressionType.AndAlso:
                    case ExpressionType.ArrayIndex:
                    case ExpressionType.Coalesce:
                    case ExpressionType.Divide:
                    case ExpressionType.Equal:
                    case ExpressionType.ExclusiveOr:
                    case ExpressionType.GreaterThan:
                    case ExpressionType.GreaterThanOrEqual:
                    case ExpressionType.LeftShift:
                    case ExpressionType.LessThan:
                    case ExpressionType.LessThanOrEqual:
                    case ExpressionType.Modulo:
                    case ExpressionType.Multiply:
                    case ExpressionType.NotEqual:
                    case ExpressionType.Or:
                    case ExpressionType.OrElse:
                    case ExpressionType.Power:
                    case ExpressionType.RightShift:
                    case ExpressionType.Subtract:
                        return VisitBinaryExpression((BinaryExpression)node);

                    case ExpressionType.Constant:
                        return VisitConstantExpression((ConstantExpression)node);

                    case ExpressionType.MemberAccess:
                        return VisitMemberExpression((MemberExpression)node);

                    case ExpressionType.Call:
                        return VisitMethodCallExpression((MethodCallExpression)node);

                    case ExpressionType.Convert:
                    case ExpressionType.Not:
                    case ExpressionType.Quote:
                        return VisitUnaryExpression((UnaryExpression)node);

                    default:
                        throw new NotSupportedException($"'{node}' is not supported in a 'Where' query expression");
                }
#pragma warning restore IDE0066 // Convert switch statement to expression
            }
            return node;
        }

        /// <summary>
        /// Process a binary expression
        /// </summary>
        /// <param name="node">The expression to visit</param>
        /// <returns>The visited expression</returns>
        /// <remarks>
        /// Excluded from code coverage
        /// TODO: Need a trigger statement for the IsEnumExpression path
        /// </remarks>
        internal Expression VisitBinaryExpression(BinaryExpression node)
        {
            if (IsEnumExpression(node, out UnaryExpression enumExpression, out ConstantExpression constant) && enumExpression != null && constant != null)
            {
                Type enumType = enumExpression.Operand.Type;
                string enumString = Enum.ToObject(enumType, constant.Value).ToString();
                Expression call = Expression.Call(enumExpression, toStringMethod);
                Visit(Expression.MakeBinary(node.NodeType, call, Expression.Constant(enumString)));
            }
            else if (node.NodeType == ExpressionType.Add && node.Left.Type == typeof(string) && node.Right.Type == typeof(string))
            {
                Visit(Expression.Call(concatMethod, new[] { node.Left, node.Right }));
            }
            else
            {
                var op = new BinaryOperatorNode(node.NodeType.ToBinaryOperatorKind());
                FilterExpression.Push(op);
                Visit(node.Left);
                Visit(node.Right);
                SetChildren(op);
            }

            return node;
        }

        /// <summary>
        /// Process a constant expression
        /// </summary>
        /// <param name="node">The expression to visit</param>
        /// <returns>The visited expression</returns>
        private Expression VisitConstantExpression(ConstantExpression node)
        {
            FilterExpression.Push(new ConstantNode(node.Value));
            return node;
        }

        /// <summary>
        /// Process a member expression
        /// </summary>
        /// <param name="node">The expression to visit</param>
        /// <returns>The visited expression</returns>
        private Expression VisitMemberExpression(MemberExpression node)
        {
            // Is the member the name of a member?
            var memberName = GetTableMemberName(node, ClientOptions);
            if (memberName != null)
            {
                FilterExpression.Push(new MemberAccessNode(null, memberName));
                return node;
            }

            // Is this member actually a function that looks like a property (e.g. string.Length)
            var key = new MemberInfoKey(node.Member);
            var methodName = InstanceProperties.GetMethodName(key);
            if (methodName != null)
            {
                var fnCallNode = new FunctionCallNode(methodName);
                FilterExpression.Push(fnCallNode);
                Visit(node.Expression);
                SetChildren(fnCallNode);
                return node;
            }

            // Otherwise we don't support it
            throw new NotSupportedException($"The member '{node.Member.Name} is not supported in the 'Where' clause");
        }

        /// <summary>
        /// Process method calls for OData
        /// </summary>
        /// <param name="node">The node to visit</param>
        /// <returns>The visited node</returns>
        internal Expression VisitMethodCallExpression(MethodCallExpression node)
        {
            var key = new MemberInfoKey(node.Method);
            if (MethodNames.TryGetValue(key, out string methodName, out bool isStatic))
            {
                var fnCallNode = new FunctionCallNode(methodName);
                FilterExpression.Push(fnCallNode);
                foreach (var argument in (isStatic ? Array.Empty<Expression>() : new Expression[] { node.Object }).Concat(node.Arguments))
                {
                    Visit(argument);
                }
                SetChildren(fnCallNode);
            }
            else if (node.Method.GetRuntimeBaseDefinition().Equals(toStringMethod))
            {
                Visit(node.Object);
            }
            else
            {
                throw new NotSupportedException($"'{node}' is not supported in a 'Where' clause");
            }
            return node;
        }

        /// <summary>
        /// Process unary expressions
        /// </summary>
        /// <param name="node">The node to visit</param>
        /// <returns>The visited node</returns>
        /// <remarks>
        /// Excluded from code coverage
        /// TODO: Find test cases for Quote, non-implciit convert and negate that will trigger here.
        /// </remarks>
        [ExcludeFromCodeCoverage]
        internal Expression VisitUnaryExpression(UnaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Not:
                    Visit(node.Operand);
                    FilterExpression.Push(new UnaryOperatorNode(UnaryOperatorKind.Not, FilterExpression.Pop()));
                    break;
                case ExpressionType.Quote:
                    Visit(node.Operand);
                    break;
                case ExpressionType.Convert:
                    if (!IsConversionImplicit(node, node.Operand.Type, node.Type))
                    {
                        throw new NotSupportedException($"Implicit conversion from '{node.Operand.Type}' to '{node.Type}' is not supported by a 'Where' {node.NodeType}' clause.");
                    }
                    Visit(node.Operand);
                    break;
                default:
                    throw new NotSupportedException($"The operator '{node.NodeType}' is not supported in a 'Where' clause");
            }
            return node;
        }

        /// <summary>
        /// Check whether a conversion from one type to another will be made implicitly by the datasync server.
        /// </summary>
        /// <param name="node">The conversion expression</param>
        /// <param name="from">The type to convert from</param>
        /// <param name="to">The type to convert to</param>
        /// <returns>True if there is an implicit conversion</returns>
        /// <remarks>
        /// Excluded from code coverage
        /// TODO: Need an example of a where clause which would trigger this.
        /// </remarks>
        [ExcludeFromCodeCoverage]
        internal bool IsConversionImplicit(UnaryExpression node, Type from, Type to)
            => GetTableMemberName(node.Operand, ClientOptions) != null && ImplicitConversions.IsImplicitConversion(from, to);

        /// <summary>
        /// Checks if the provided binary expression is an enum.
        /// </summary>
        /// <param name="node">The binary expression to check</param>
        /// <param name="unaryExpression">The expression which is the enum.</param>
        /// <param name="constantExpression">The constant expression containing the enum value</param>
        /// <returns>Trye if an enum expression is found.</returns>
        /// <remarks>
        /// Excluded from code coverage
        /// TODO: Need an example of a where clause which would trigger this on left and right.
        /// </remarks>
        [ExcludeFromCodeCoverage]
        internal static bool IsEnumExpression(BinaryExpression node, out UnaryExpression unaryExpression, out ConstantExpression constantExpression)
        {
            // Case 1: enum on the left side
            if (node.Left is UnaryExpression left && IsEnumExpression(left) && node.Right is ConstantExpression leftExpr)
            {
                unaryExpression = left;
                constantExpression = leftExpr;
                return true;
            }

            // Case 2: enum on the right side
            if (node.Right is UnaryExpression right && IsEnumExpression(right) && node.Left is ConstantExpression rightExpr)
            {
                unaryExpression = right;
                constantExpression = rightExpr;
                return true;
            }

            unaryExpression = null;
            constantExpression = null;
            return false;
        }

        /// <summary>
        /// Checks if the provided unary expression is an enum.
        /// </summary>
        /// <param name="expression">The expression to check</param>
        /// <returns>True if an enum.</returns>
        internal static bool IsEnumExpression(UnaryExpression expression)
            => expression.NodeType == ExpressionType.Convert && expression.Operand.Type.GetTypeInfo().IsEnum;

        /// <summary>
        /// Resolves a given member name into the "wire" version.  This is defined as
        /// 1. The name provided in JsonPropertyName attribute
        /// 2. The name according to the namingpolicy of the serializer options for the table
        /// 3. The name
        /// </summary>
        /// <param name="memberInfo">The member name</param>
        /// <param name="clientOptions">The client options for the table.</param>
        /// <returns></returns>
        internal static string ResolvePropertyName(MemberInfo memberInfo, DatasyncClientOptions clientOptions)
        {
            var attr = memberInfo.GetCustomAttributes(typeof(JsonPropertyNameAttribute), inherit: true).FirstOrDefault();
            if (attr is JsonPropertyNameAttribute nameAttr)
            {
                return nameAttr.Name;
            }

            var namingPolicy = clientOptions.SerializerOptions.PropertyNamingPolicy;
            return namingPolicy?.ConvertName(memberInfo.Name) ?? memberInfo.Name;
        }

        /// <summary>
        /// Each <see cref="QueryNode"/> has a <see cref="QueryNode.SetChildren(IList{QueryNode})"/> method.  This
        /// copies the children from the filter expression stack into the query node.
        /// </summary>
        /// <param name="parent">The parent query node.</param>
        internal void SetChildren(QueryNode parent)
        {
            var args = new Stack<QueryNode>();
            while (FilterExpression.Peek() != parent)
            {
                args.Push(FilterExpression.Pop());
            }
            parent.SetChildren(args.ToList());
        }
    }
}
