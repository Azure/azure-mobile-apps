// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Query;
using Microsoft.Datasync.Client.Query.Linq.Nodes;
using Microsoft.Datasync.Client.Serialization;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Datasync.Client.SQLiteStore.Utils
{
    /// <summary>
    /// A converter for turning a <see cref="QueryDescription"/> into a SQL statement.
    /// </summary>
    internal sealed class SqlQueryFormatter : QueryNodeVisitor<QueryNode>
    {
        /// <summary>
        /// The query being processed.
        /// </summary>
        private readonly QueryDescription query;

        /// <summary>
        /// The SQL statement being built.
        /// </summary>
        private StringBuilder sql;

        /// <summary>
        /// Creates a new <see cref="SqlQueryFormatter"/> for the specified query.
        /// </summary>
        /// <param name="query">The query to be processed.</param>
        private SqlQueryFormatter(QueryDescription query)
        {
            this.query = query;
        }

        /// <summary>
        /// The parameters for the SQL statement.
        /// </summary>
        public Dictionary<string, object> Parameters = new();

        /// <summary>
        /// Formats a <c>DELETE</c> query.
        /// </summary>
        /// <param name="query">The query to format.</param>
        /// <param name="parameters">When complete, the parameters for the SQL statement.</param>
        /// <returns>The SQL statement.</returns>
        public static string FormatCountStatement(QueryDescription query, out Dictionary<string, object> parameters)
        {
            var formatter = new SqlQueryFormatter(query);
            var sql = formatter.FormatSelectCount();
            parameters = formatter.Parameters;
            return sql;
        }

        /// <summary>
        /// Formats a <c>DELETE</c> query.
        /// </summary>
        /// <param name="query">The query to format.</param>
        /// <param name="parameters">When complete, the parameters for the SQL statement.</param>
        /// <returns>The SQL statement.</returns>
        public static string FormatDeleteStatement(QueryDescription query, out Dictionary<string, object> parameters)
        {
            var formatter = new SqlQueryFormatter(query);
            var sql = formatter.FormatDelete();
            parameters = formatter.Parameters;
            return sql;
        }

        /// <summary>
        /// Formats a <c>SELECT</c> query.
        /// </summary>
        /// <param name="query">The query to format.</param>
        /// <param name="parameters">When complete, the parameters for the SQL statement.</param>
        /// <returns>The SQL statement.</returns>
        public static string FormatSelectStatement(QueryDescription query, out Dictionary<string, object> parameters)
        {
            var formatter = new SqlQueryFormatter(query);
            var sql = formatter.FormatSelect();
            parameters = formatter.Parameters;
            return sql;
        }

        /// <summary>
        /// The internal version of the <see cref="FormatDeleteStatement"/> entry point.
        /// </summary>
        /// <returns>The SQL statement.</returns>
        private string FormatDelete()
        {
            var delQuery = query.Clone();
            delQuery.Selection.Clear();
            delQuery.Selection.Add(SystemProperties.JsonIdProperty);
            delQuery.IncludeTotalCount = false;

            var formatter = new SqlQueryFormatter(delQuery);
            string selectIdQuery = formatter.FormatSelect();
            Parameters = formatter.Parameters;
            return $"DELETE FROM [{delQuery.TableName}] WHERE [{SystemProperties.JsonIdProperty}] IN ({selectIdQuery})";
        }

        /// <summary>
        /// The internal version of the <see cref="FormatSelectStatement"/> entry point, used
        /// to get a SELECT statement.
        /// </summary>
        /// <returns>The SQL statement.</returns>
        private string FormatSelect()
        {
            string fieldList = query.Selection.Count > 0 ? string.Join(", ", query.Selection.Select(c => $"[{c}]")) : "*";
            return FormatQuery("SELECT " + fieldList);
        }

        /// <summary>
        /// The internal version of the <see cref="FormatCountStatement"/> entry point, used
        /// to get a SELECT COUNT statement.
        /// </summary>
        /// <returns>The SQL statement.</returns>
        private string FormatSelectCount()
        {
            sql = new StringBuilder();
            Parameters.Clear();

            sql.Append("SELECT COUNT(1) AS [count] FROM [").Append(query.TableName).Append(']');
            if (query.Filter != null)
            {
                FormatWhereClause(query.Filter);
            }
            return sql.ToString().TrimEnd();
        }

        /// <summary>
        /// Formats a query statement.
        /// </summary>
        /// <param name="command">The initial command (prior to the <c>FROM {table}</c>)</param>
        /// <returns>The SQL statement.</returns>
        private string FormatQuery(string command)
        {
            sql = new StringBuilder(command);
            Parameters.Clear();

            sql.Append(" FROM [").Append(query.TableName).Append(']');
            if (query.Filter != null)
            {
                FormatWhereClause(query.Filter);
            }
            if (query.Ordering.Count > 0)
            {
                FormatOrderByClause(query.Ordering);
            }
            if (query.Top.HasValue || query.Skip.HasValue)
            {
                sql.Append(" LIMIT ").Append(query.Top ?? int.MaxValue);
                if (query.Skip.HasValue)
                {
                    sql.Append(" OFFSET ").Append(query.Skip.Value);
                }
            }

            return sql.ToString().TrimEnd();
        }

        /// <summary>
        /// Adds an <c>ORDER BY</c> clause to the SQL.
        /// </summary>
        /// <param name="orderings">The required ordering.</param>
        private void FormatOrderByClause(IList<OrderByNode> orderings)
        {
            sql.Append(" ORDER BY ");
            string separator = String.Empty;

            foreach (OrderByNode node in orderings)
            {
                sql.Append(separator);
                node.Expression.Accept(this);
                if (node.Direction == OrderByDirection.Descending)
                {
                    sql.Append(" DESC");
                }
                separator = ", ";
            }
        }

        /// <summary>
        /// Adds a <c>WHERE</c> clause to the SQL.
        /// </summary>
        /// <param name="expression">The filter expression.</param>
        private void FormatWhereClause(QueryNode expression)
        {
            sql.Append(" WHERE ");
            expression.Accept(this);
        }

        /// <summary>
        /// Creates a parameter for the provided value.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>The name of the parameter.</returns>
        private string CreateParameter(object value)
        {
            int paramNo = Parameters.Count + 1;
            string paramName = $"@p{paramNo}";
            Parameters.Add(paramName, SqlColumnType.SerializeValue(new JValue(value), allowNull: true));
            return paramName;
        }

        #region QueryNodeVisitor<QueryNode>
        /// <summary>
        /// Visitor for a binary operator node.
        /// </summary>
        /// <param name="node">The incoming node.</param>
        /// <returns>The processed node.</returns>
        public override QueryNode Visit(BinaryOperatorNode node)
        {
            sql.Append('(');

            QueryNode left = node.LeftOperand;
            QueryNode right = node.RightOperand;

            if (left != null)
            {
                if (node.OperatorKind == BinaryOperatorKind.Modulo)
                {
                    left = new ConvertNode(left, typeof(int));
                }
                left = left.Accept(this);
            }

            if (right is ConstantNode rightConstant && rightConstant.Value == null)
            {
                if (node.OperatorKind == BinaryOperatorKind.Equal)
                {
                    sql.Append(" IS ");
                }
                else if (node.OperatorKind == BinaryOperatorKind.NotEqual)
                {
                    sql.Append(" IS NOT ");
                }
            }
            else
            {
                sql.Append(' ').Append(node.OperatorKind.ToSqlOperator()).Append(' ');
            }

            if (right != null)
            {
                right = right.Accept(this);
            }
            sql.Append(')');

            return (left != node.LeftOperand || right != node.RightOperand) ? new BinaryOperatorNode(node.OperatorKind, left, right) : node;
        }

        /// <summary>
        /// Visitor for a constant.
        /// </summary>
        /// <param name="node">The incoming node.</param>
        /// <returns>The processed node.</returns>
        public override QueryNode Visit(ConstantNode node)
        {
            if (node.Value == null)
            {
                sql.Append("NULL");
            }
            else
            {
                sql.Append(CreateParameter(node.Value));
            }
            return node;
        }

        /// <summary>
        /// Visitor for a conversion node.
        /// </summary>
        /// <param name="node">The incoming node.</param>
        /// <returns>The processed node.</returns>
        public override QueryNode Visit(ConvertNode node)
        {
            sql.Append("CAST(");
            QueryNode source = node.Source.Accept(this);
            sql.Append(" AS ").Append(SqlColumnType.GetStoreCastType(node.TargetType)).Append(')');
            return (source != node.Source) ? new ConvertNode(source, node.TargetType) : node;
        }

        /// <summary>
        /// Visitor for a function call node.
        /// </summary>
        /// <param name="node">The incoming node</param>
        /// <returns>the processed node.</returns>
        /// <exception cref="InvalidOperationException">if the function is not recognized or supported.</exception>
        public override QueryNode Visit(FunctionCallNode node) => node.Name switch
        {
            "day" => FormatDateFunction(node, "%d"),
            "month" => FormatDateFunction(node, "%m"),
            "year" => FormatDateFunction(node, "%Y"),
            "hour" => FormatDateFunction(node, "%H"),
            "minute" => FormatDateFunction(node, "%M"),
            "second" => FormatDateFunction(node, "%S"),
            "floor" => FormatFloorFunction(node),
            "ceiling" => FormatCeilingFunction(node),
            "round" => FormatMathFunction(node, "ROUND", ",0"),
            "tolower" => FormatStringFunction(node, "LOWER"),
            "toupper" => FormatStringFunction(node, "UPPER"),
            "length" => FormatStringFunction(node, "LENGTH"),
            "trim" => FormatStringFunction(node, "TRIM"),
            "contains" => FormatLikeFunction(node, true, 1, 0, true),
            "startswith" => FormatLikeFunction(node, false, 1, 0, true),
            "endswith" => FormatLikeFunction(node, true, 1, 0, false),
            "concat" => FormatConcatFunction(node),
            "indexof" => FormatIndexOfFunction(node),
            "replace" => FormatStringFunction(node, "REPLACE"),
            "substring" => FormatSubstringFunction(node),
            _ => throw new InvalidOperationException($"Function call '{node.Name}' not valid with SQLite driver")
        };

        /// <summary>
        /// Visitor for a member access operator.
        /// </summary>
        /// <param name="node">The incoming node.</param>
        /// <returns>The processed node.</returns>
        public override QueryNode Visit(MemberAccessNode node)
        {
            sql.Append('[').Append(node.MemberName).Append(']');
            return node;
        }

        /// <summary>
        /// Visitor for a unary operator.
        /// </summary>
        /// <param name="node">The incoming node.</param>
        /// <returns>The processed node.</returns>
        public override QueryNode Visit(UnaryOperatorNode node)
        {
            if (node.OperatorKind == UnaryOperatorKind.Negate)
            {
                sql.Append("-(");
            }
            else if (node.OperatorKind == UnaryOperatorKind.Not)
            {
                sql.Append("NOT(");
            }
            QueryNode operand = node.Operand.Accept(this);
            sql.Append(')');
            return (operand != node.Operand) ? new UnaryOperatorNode(node.OperatorKind, operand) : node;
        }
        #endregion

        /// <summary>
        /// The CEILING function is not available in the SQLitePCL.raw package we use for SQLite
        /// access (because it is needs to be compiled in), so we need to use a workaround.
        /// </summary>
        /// <remarks>
        /// The conversion for ceil is <c>(CAST(x as INT) + (x > CAST(x as INT)))</c>
        /// </remarks>
        /// <param name="node">The node being processed.</param>
        /// <returns>The processed node.</returns>
        private QueryNode FormatCeilingFunction(FunctionCallNode node)
        {
            var castToInt = new ConvertNode(node.Arguments[0], typeof(int));
            var comparison = new BinaryOperatorNode(BinaryOperatorKind.GreaterThan, node.Arguments[0], new ConvertNode(node.Arguments[0], typeof(int)));
            var ceiling = new BinaryOperatorNode(BinaryOperatorKind.Add, castToInt, comparison);
            ceiling.Accept(this);
            return node;
        }

        /// <summary>
        /// The FLOOR function is not available in the SQLitePCL.raw package we use for SQLite
        /// access (because it is needs to be compiled in), so we need to use a workaround.
        /// </summary>
        /// <remarks>
        /// The converstion for floor is <c>(CAST(x as INT) - (x < CAST(x as INT)))</c>
        /// </remarks>
        /// <param name="node">The node being processed.</param>
        /// <returns>The processed node.</returns>
        private QueryNode FormatFloorFunction(FunctionCallNode node)
        {
            var castToInt = new ConvertNode(node.Arguments[0], typeof(int));
            var comparison = new BinaryOperatorNode(BinaryOperatorKind.LessThan, node.Arguments[0], new ConvertNode(node.Arguments[0], typeof(int)));
            var ceiling = new BinaryOperatorNode(BinaryOperatorKind.Subtract, castToInt, comparison);
            ceiling.Accept(this);
            return node;
        }

        /// <summary>
        /// Generate a CONCAT SQL statement.
        /// </summary>
        /// <param name="node">The node being processed.</param>
        /// <returns>The processed node.</returns>
        private QueryNode FormatConcatFunction(FunctionCallNode node)
        {
            string separator = string.Empty;
            foreach (QueryNode arg in node.Arguments)
            {
                sql.Append(separator);
                arg.Accept(this);
                separator = " || ";
            }
            return node;
        }

        /// <summary>
        /// Generate as <c>CAST(STRFTIME('%d', ...)</c> method.
        /// </summary>
        /// <param name="node">The node being processed.</param>
        /// <param name="formatString"></param>
        /// <returns>The processed node.</returns>
        private QueryNode FormatDateFunction(FunctionCallNode node, string formatString)
        {
            sql.AppendFormat("CAST(strftime('{0}', datetime(", formatString);
            node.Arguments[0].Accept(this);
            // datetimes are stored with ms accuracy.
            sql.Append(" / 1000.0, 'unixepoch')) AS INTEGER)");
            return node;
        }

        /// <summary>
        /// Generate a INSTR SQL statement.
        /// </summary>
        /// <param name="node">The node being processed.</param>
        /// <returns>The processed node.</returns>
        private QueryNode FormatIndexOfFunction(FunctionCallNode node)
        {
            QueryNode result = FormatStringFunction(node, "INSTR");
            sql.Append(" - 1");
            return result;
        }

        /// <summary>
        /// Generate a <c>LIKE('%pattern%', {value})</c> value.
        /// </summary>
        /// <param name="node">The node being processed.</param>
        /// <param name="startAny">If <c>true</c>, start the pattern with a '%'</param>
        /// <param name="patternIndex">The pattern index.</param>
        /// <param name="valueIndex">The value index.</param>
        /// <param name="endAny">If <c>true</c>, end the pattern with a '%'</param>
        /// <returns>The processed node.</returns>
        private QueryNode FormatLikeFunction(FunctionCallNode node, bool startAny, int patternIndex, int valueIndex, bool endAny)
        {
            sql.Append("LIKE(");
            if (startAny)
            {
                sql.Append("'%' || ");
            }
            node.Arguments[patternIndex].Accept(this);
            if (endAny)
            {
                sql.Append(" || '%'");
            }
            sql.Append(", ");
            node.Arguments[valueIndex].Accept(this);
            sql.Append(')');
            return node;
        }

        /// <summary>
        /// Generate a Math function.
        /// </summary>
        /// <param name="node">The node being processed.</param>
        /// <returns>The processed node.</returns>
        private QueryNode FormatMathFunction(FunctionCallNode node, string fn, string suffix = "")
        {
            sql.Append(fn).Append('(');
            node.Arguments[0].Accept(this);
            if (!string.IsNullOrEmpty(suffix))
            {
                sql.Append(suffix);
            }
            sql.Append(')');
            return node;
        }

        /// <summary>
        /// Generate a string function SQL expression.
        /// </summary>
        /// <param name="node">The node being processed.</param>
        /// <param name="fn">The string function.</param>
        /// <returns>The processed node.</returns>
        private QueryNode FormatStringFunction(FunctionCallNode node, string fn)
        {
            sql.AppendFormat("{0}(", fn);
            string separator = string.Empty;
            foreach (QueryNode arg in node.Arguments)
            {
                sql.Append(separator);
                arg.Accept(this);
                separator = ", ";
            }
            sql.Append(')');
            return node;
        }

        /// <summary>
        /// Generate a SUBSTR SQL expression.
        /// </summary>
        /// <param name="node">The node being processed.</param>
        /// <returns>The processed node.</returns>
        private QueryNode FormatSubstringFunction(FunctionCallNode node)
        {
            sql.Append("SUBSTR(");
            node.Arguments[0].Accept(this);
            if (node.Arguments.Count > 1)
            {
                sql.Append(", ");
                node.Arguments[1].Accept(this);
                sql.Append(" + 1");  // need to add 1 since SQL is 1 based, but OData is zero based
                if (node.Arguments.Count > 2)
                {
                    sql.Append(", ");
                    node.Arguments[2].Accept(this);
                }
            }
            sql.Append(')');
            return node;
        }
    }
}
