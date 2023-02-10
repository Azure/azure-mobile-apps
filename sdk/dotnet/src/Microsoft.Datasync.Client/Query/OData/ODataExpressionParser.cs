// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using Microsoft.Datasync.Client.Query.Linq.Nodes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Microsoft.Datasync.Client.Query.OData
{
    /// <summary>
    /// Parse an OData string into the appropriate <see cref="QueryNode"/>
    /// type.
    /// </summary>
    internal sealed class ODataExpressionParser
    {
        private readonly ODataExpressionLexer lexer;

        /// <summary>
        /// A validation table that maps a function call name to the number of expected arguments.
        /// </summary>
        /// <remarks>
        /// "substring" is special cased because it can take either 2 or 3 arguments.
        /// </remarks>
        private readonly Dictionary<string, int> functionArgCount = new()
        {
            { "day", 1 },
            { "month", 1 },
            { "year", 1 },
            { "minute", 1 },
            { "second", 1 },
            { "floor", 1 },
            { "ceiling", 1 },
            { "round", 1 },
            { "tolower", 1 },
            { "toupper", 1 },
            { "length", 1 },
            { "trim", 1 },
            { "contains", 2 },
            { "startswith", 2 },
            { "endswith", 2 },
            { "concat", 2 },
            { "indexof", 2 },
            { "replace", 3 }
        };

        /// <summary>
        /// Parses the <c>$filter</c> portion of an OData query string.
        /// </summary>
        /// <param name="filter">The unescaped <c>$filter</c> statement.</param>
        /// <returns>The <see cref="QueryNode"/> representing the statement.</returns>
        public static QueryNode ParseFilter(string filter)
            => new ODataExpressionParser(filter).ParseFilter();

        /// <summary>
        /// Parses the <c>$orderBy</c> portion of an OData query string.
        /// </summary>
        /// <param name="orderBy">The unescaped <c>$orderBy</c> statement.</param>
        /// <returns>The list of <see cref="OrderByNode"/> elements representing the statement.</returns>
        public static IList<OrderByNode> ParseOrderBy(string orderBy)
            => new ODataExpressionParser(orderBy).ParseOrderBy();

        /// <summary>
        /// Creates a new <see cref="ODataExpressionParser"/> for a specific statement.
        /// </summary>
        /// <param name="expression">The OData expression to evaluate.</param>
        private ODataExpressionParser(string expression)
        {
            lexer = new ODataExpressionLexer(expression);
        }

        /// <summary>
        /// Parses the current expression as a <c>$filter</c> OData string.
        /// </summary>
        /// <returns>The <see cref="QueryNode"/> representing the statement.</returns>
        internal QueryNode ParseFilter()
        {
            QueryNode expression = ParseExpression();
            ValidateTokenIsType(QueryTokenKind.End, "The specified OData $filter expression has syntax errors.");
            return expression;
        }

        /// <summary>
        /// Parses the current expression as a <c>$orderBy</c> OData string.
        /// </summary>
        /// <returns>The list of <see cref="OrderByNode"/> elements representing the statement.</returns>
        internal IList<OrderByNode> ParseOrderBy()
        {
            var orderings = new List<OrderByNode>();
            while(true)
            {
                QueryNode expression = ParseExpression();
                OrderByDirection direction = OrderByDirection.Ascending;
                if (TokenIsIdentifier("asc"))
                {
                    lexer.NextToken();
                }
                else if (TokenIsIdentifier("desc"))
                {
                    lexer.NextToken();
                    direction = OrderByDirection.Descending;
                }

                orderings.Add(new OrderByNode(expression, direction));
                if (lexer.Token.Kind != QueryTokenKind.Comma)
                {
                    break;
                }
                lexer.NextToken();  // Eat the comma
            }
            ValidateTokenIsType(QueryTokenKind.End, "The specified OData $orderBy expression has syntax errors.");
            return orderings;
        }

        #region Expression Parser
        /// <summary>
        /// Parse an entire expression.
        /// </summary>
        /// <returns>The <see cref="QueryNode"/> representing the expression.</returns>
        private QueryNode ParseExpression()
            => ParseLogicalOr();

        /// <summary>
        /// Parses a set of "a or b" clauses.
        /// </summary>
        /// <returns>The <see cref="QueryNode"/> representing the expression.</returns>
        private QueryNode ParseLogicalOr()
        {
            QueryNode left = ParseLogicalAnd();
            while (lexer.Token.Kind == QueryTokenKind.Or)
            {
                lexer.NextToken();
                QueryNode right = ParseLogicalAnd();
                left = new BinaryOperatorNode(BinaryOperatorKind.Or, left, right);
            }
            return left;
        }

        /// <summary>
        /// Parses a set of "a and b" clauses.
        /// </summary>
        /// <returns>The <see cref="QueryNode"/> representing the expression.</returns>
        private QueryNode ParseLogicalAnd()
        {
            QueryNode left = ParseComparison();
            while (lexer.Token.Kind == QueryTokenKind.And)
            {
                lexer.NextToken();
                QueryNode right = ParseComparison();
                left = new BinaryOperatorNode(BinaryOperatorKind.And, left, right);
            }
            return left;
        }

        /// <summary>
        /// Parses a set of comparison clauses.
        /// </summary>
        /// <returns>A <see cref="QueryNode"/> representing the expression.</returns>
        private QueryNode ParseComparison()
        {
            QueryNode left = ParseAdditive();
            while (lexer.Token.Kind.IsComparisonOperator())
            {
                QueryTokenKind operatorKind = lexer.Token.Kind;
                lexer.NextToken();
                QueryNode right = ParseAdditive();
                left = new BinaryOperatorNode(operatorKind.ToBinaryOperatorKind(), left, right);
            }
            return left;
        }

        /// <summary>
        /// Returns a set of add/subtract clauses.
        /// </summary>
        /// <returns>A <see cref="QueryNode"/> representing the expression.</returns>
        private QueryNode ParseAdditive()
        {
            QueryNode left = ParseMultiplicative();
            while (lexer.Token.Kind == QueryTokenKind.Add || lexer.Token.Kind == QueryTokenKind.Subtract)
            {
                QueryTokenKind operatorKind = lexer.Token.Kind;
                lexer.NextToken();
                QueryNode right = ParseMultiplicative();
                left = new BinaryOperatorNode(operatorKind.ToBinaryOperatorKind(), left, right);
            }
            return left;
        }

        /// <summary>
        /// Returns a set of multiply / divide / modulo clauses.
        /// </summary>
        /// <returns>A <see cref="QueryNode"/> representing the expression.</returns>
        private QueryNode ParseMultiplicative()
        {
            QueryNode left = ParseUnary();
            while (lexer.Token.Kind == QueryTokenKind.Multiply || lexer.Token.Kind == QueryTokenKind.Divide || lexer.Token.Kind == QueryTokenKind.Modulo)
            {
                QueryTokenKind operatorKind = lexer.Token.Kind;
                lexer.NextToken();
                QueryNode right = ParseUnary();
                left = new BinaryOperatorNode(operatorKind.ToBinaryOperatorKind(), left, right);
            }
            return left;
        }

        /// <summary>
        /// Parses a unary expression.
        /// </summary>
        /// <returns>A <see cref="QueryNode"/> representing the expression.</returns>
        private QueryNode ParseUnary()
        {
            if (lexer.Token.Kind == QueryTokenKind.Minus || lexer.Token.Kind == QueryTokenKind.Not)
            {
                QueryTokenKind operatorKind = lexer.Token.Kind;
                int operatorPosition = lexer.Token.Position;
                lexer.NextToken();
                if (operatorKind == QueryTokenKind.Minus && lexer.Token.Kind.IsNumberLiteral())
                {
                    lexer.Token.Text = $"-{lexer.Token.Text}";
                    lexer.Token.Position = operatorPosition;
                    return ParsePrimary();
                }

                QueryNode expression = ParseUnary();
                UnaryOperatorKind opKind = operatorKind == QueryTokenKind.Minus ? UnaryOperatorKind.Negate : UnaryOperatorKind.Not;
                return new UnaryOperatorNode(opKind, expression);
            }
            return ParsePrimary();
        }

        /// <summary>
        /// Parses constants, conversion, and member access clauses.
        /// </summary>
        /// <returns>A <see cref="QueryNode"/> representing the expression.</returns>
        private QueryNode ParsePrimary()
        {
            QueryNode expression = lexer.Token.Kind switch
            {
                QueryTokenKind.Identifier => ParseIdentifier(),
                QueryTokenKind.StringLiteral => ParseStringLiteral(),
                QueryTokenKind.IntegerLiteral => ParseIntegerLiteral(),
                QueryTokenKind.RealLiteral => ParseRealLiteral(),
                QueryTokenKind.OpenParen => ParseParenExpression(),
                _ => throw new ODataException("Expression expected", lexer.Text, lexer.Token.Position)
            };

            while (lexer.Token.Kind == QueryTokenKind.Dot)
            {
                lexer.NextToken();
                expression = ParseMemberAccess(expression);
            }
            return expression;
        }

        /// <summary>
        /// Parses an identifier that hasn't been reclassified.
        /// </summary>
        /// <returns>A <see cref="QueryNode"/> representing the identifier.</returns>
        private QueryNode ParseIdentifier()
        {
            ValidateTokenIsType(QueryTokenKind.Identifier, "Expected identifier.");
            if (lexer.Token.Text == "true")
            {
                lexer.NextToken();
                return new ConstantNode(true);
            }
            else if (lexer.Token.Text == "false")
            {
                lexer.NextToken();
                return new ConstantNode(false);
            }
            else if (lexer.Token.Text == "null")
            {
                lexer.NextToken();
                return new ConstantNode(null);
            }
            return ParseMemberAccess(null);
        }

        /// <summary>
        /// Parses a string literal.
        /// </summary>
        /// <returns>A <see cref="QueryNode"/> representing the string literal.</returns>
        private ConstantNode ParseStringLiteral()
        {
            ValidateTokenIsType(QueryTokenKind.StringLiteral, "Expected string literal");
            string value = lexer.Token.Text.Substring(1, lexer.Token.Text.Length - 2).Replace("''", "'");
            lexer.NextToken();
            return new ConstantNode(value);
        }

        /// <summary>
        /// Parses an integer literal
        /// </summary>
        /// <returns>A <see cref="QueryNode"/> representing the integer literal.</returns>
        private QueryNode ParseIntegerLiteral()
        {
            ValidateTokenIsType(QueryTokenKind.IntegerLiteral, "Expected integer literal");
            if (!Int64.TryParse(lexer.Token.Text, out long value))
            {
                throw new ODataException($"The specified odata query has an invalid integer '{lexer.Token.Text}'", lexer.Text, lexer.Token.Position);
            }
            lexer.NextToken();
            if (string.Equals(lexer.Token.Text, "L", StringComparison.OrdinalIgnoreCase))
            {
                lexer.NextToken(); // Eat the L or l on the end of a long.
            }
            return new ConstantNode(value);
        }

        /// <summary>
        /// Parses a floating point literal
        /// </summary>
        /// <returns>A <see cref="QueryNode"/> representing the floating point literal.</returns>
        private QueryNode ParseRealLiteral()
        {
            ValidateTokenIsType(QueryTokenKind.RealLiteral, "Expected real literal.");
            string text = lexer.Token.Text;

            char last = char.ToUpper(text[text.Length - 1]);
            if (last == 'F' || last == 'M' || last == 'D')
            {
                // so terminating F/f, M/m, D/d have no effect.
                text = text.Substring(0, text.Length - 1);
            }

            object value = null;
            switch (last)
            {
                case 'M':
                    decimal mVal;
                    if (Decimal.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out mVal))
                    {
                        value = mVal;
                    }
                    break;
                case 'F':
                    float fVal;
                    if (Single.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out fVal))
                    {
                        value = fVal;
                    }
                    break;
                case 'D':
                default:
                    double dVal;
                    if (Double.TryParse(text, NumberStyles.Any, CultureInfo.InvariantCulture, out dVal))
                    {
                        value = dVal;
                    }
                    break;
            }
            if (value == null)
            {
                throw new ODataException($"The OData query has an invalid real literal '{text}'", lexer.Text, lexer.Token.Position);
            }

            lexer.NextToken();
            return new ConstantNode(value);
        }

        /// <summary>
        /// Parses a parenthesised expression.
        /// </summary>
        /// <returns>A <see cref="QueryNode"/> representing the expression.</returns>
        private QueryNode ParseParenExpression()
        {
            ValidateTokenIsType(QueryTokenKind.OpenParen, "'(' expected.");
            lexer.NextToken();
            QueryNode expression = ParseExpression();
            ValidateTokenIsType(QueryTokenKind.CloseParen, "')' expected.");
            lexer.NextToken();
            return expression;
        }

        /// <summary>
        /// Parses a member access node.  If the potential node is a function,
        /// then parses the function instead.
        /// </summary>
        /// <param name="instance">The parsed expression.</param>
        /// <returns>A <see cref="QueryNode"/> representing the expression.</returns>
        private QueryNode ParseMemberAccess(QueryNode instance)
        {
            var errorPosition = lexer.Token.Position;
            string id = GetIdentifier();
            lexer.NextToken();
            if (lexer.Token.Kind == QueryTokenKind.OpenParen)
            {
                return ParseFunction(id, errorPosition);
            }
            else
            {
                return new MemberAccessNode(instance, id);
            }
        }

        /// <summary>
        /// Parses a function call expression.
        /// </summary>
        /// <param name="functionName">The name of the expression.</param>
        /// <param name="errorPos">The start of the expression for error reporting.</param>
        /// <returns>A <see cref="QueryNode"/> representing the expression.</returns>
        private QueryNode ParseFunction(string functionName, int errorPos)
        {
            if (lexer.Token.Kind == QueryTokenKind.OpenParen)
            {
                if (functionName == "cast")
                {
                    return ParseTypeCoercion(errorPos);
                }
                else
                {
                    var args = ParseArgumentList();
                    ValidateFunction(functionName, args, errorPos);
                    return new FunctionCallNode(functionName, args);
                }
            }
            else
            {
                throw new ODataException("'(' expected.", lexer.Text, errorPos);
            }
        }

        /// <summary>
        /// Parses a type coercion statement.
        /// </summary>
        /// <param name="errorPos">The start of the function call.</param>
        /// <returns>A <see cref="QueryNode"/> that represents the statement</returns>
        private QueryNode ParseTypeCoercion(int errorPos)
        {
            ValidateTokenIsType(QueryTokenKind.OpenParen, "'(' expected.");

            // Construct the call, crudely.
            QueryToken token = lexer.ReadUntilCharacter(')');
            string[] arguments = token.Text.Split(',').Select(arg => arg.Trim()).ToArray();
            if (arguments.Length != 2)
            {
                throw new ODataException("Invalid cast (args != 2)", lexer.Text, errorPos);
            }

            QueryNode node;
            try
            {
                node = EdmTypeSupport.ToQueryNode(arguments[0], arguments[1]);
            }
            catch (Exception ex)
            {
                throw new ODataException("Invalid cast", ex, lexer.Text, errorPos);
            }

            // This should be the close parens.
            lexer.NextToken();
            ValidateTokenIsType(QueryTokenKind.CloseParen, "')' expected.");
            lexer.NextToken();

            return node;
        }

        /// <summary>
        /// Parses the arguments to a function call (including the open and close parens).
        /// </summary>
        /// <returns>A list of <see cref="QueryNode"/> objects for the arguments.</returns>
        private IList<QueryNode> ParseArgumentList()
        {
            ValidateTokenIsType(QueryTokenKind.OpenParen, "'(' expected.");
            lexer.NextToken();

            IList<QueryNode> args = lexer.Token.Kind != QueryTokenKind.CloseParen ? ParseArguments() : new List<QueryNode>();

            ValidateTokenIsType(QueryTokenKind.CloseParen, "')' expected.");
            lexer.NextToken();
            return args;
        }

        /// <summary>
        /// Parses the arguments to a function call (without the open and close parens).
        /// </summary>
        /// <returns>A list of <see cref="QueryNode"/> objects for the arguments.</returns>
        private IList<QueryNode> ParseArguments()
        {
            var args = new List<QueryNode>();
            while (true)
            {
                args.Add(ParseExpression());
                if (lexer.Token.Kind != QueryTokenKind.Comma)
                {
                    break;
                }
                lexer.NextToken();
            }
            return args;
        }
        #endregion

        /// <summary>
        /// Validates that the next token is an identifier and returns it.
        /// </summary>
        /// <returns>The identifier.</returns>
        private string GetIdentifier()
        {
            ValidateTokenIsType(QueryTokenKind.Identifier, "Expected identifier");
            return lexer.Token.Text;
        }

        /// <summary>
        /// Returns <c>true</c> if the current token is the specified identifier.
        /// </summary>
        /// <param name="identifier">The identifier for comparison.</param>
        /// <returns><c>true</c> if the current token is the specified identifier.</returns>
        private bool TokenIsIdentifier(string identifier)
            => lexer.Token.Kind == QueryTokenKind.Identifier && lexer.Token.Text == identifier;

        /// <summary>
        /// Validates that the function has the right number of arguments.
        /// </summary>
        /// <param name="functionName">The name of the function.</param>
        /// <param name="functionArgs">The list of arguments.</param>
        /// <param name="errorPos">The position of the start of the function call expression.</param>
        /// <exception cref="ODataException">If the function call is not valid.</exception>
        private void ValidateFunction(string functionName, IList<QueryNode> functionArgs, int errorPos)
        {
            if (functionArgCount.ContainsKey(functionName))
            {
                if (functionArgs.Count != functionArgCount[functionName])
                {
                    throw new ODataException($"Function '{functionName}' requires {functionArgCount[functionName]} arguments.", lexer.Text, errorPos);
                }
            }
            else if (functionName == "substring" && functionArgs.Count != 2 && functionArgs.Count != 3)
            {
                throw new ODataException("Function 'substring' requires 2 or 3 arguments.", lexer.Text, errorPos);
            }
        }

        /// <summary>
        /// Validates that the token is of the specified type, throwing an exception if not.
        /// </summary>
        /// <param name="tokenKind">The expected token kind.</param>
        /// <param name="message">The error message if the token kind doesn't match.</param>
        private void ValidateTokenIsType(QueryTokenKind tokenKind, string message)
        {
            if (lexer.Token.Kind != tokenKind)
            {
                throw new ODataException(message, lexer.Text, lexer.Token.Position);
            }
        }
    }
}
