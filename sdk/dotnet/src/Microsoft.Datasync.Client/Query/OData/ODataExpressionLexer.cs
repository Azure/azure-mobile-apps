// Copyright (c) Microsoft Corporation. All Rights Reserved.
// Licensed under the MIT License.

using System.Collections.Generic;

namespace Microsoft.Datasync.Client.Query.OData
{
    /// <summary>
    /// The tokenizer for an OData expression.
    /// </summary>
    internal sealed class ODataExpressionLexer
    {
        /// <summary>
        /// A dictionary of the token classifications for identifiers.
        /// </summary>
        private readonly Dictionary<string, QueryTokenKind> identifierClassification = new()
        {
            { "add", QueryTokenKind.Add },
            { "and", QueryTokenKind.And },
            { "div", QueryTokenKind.Divide },
            { "eq", QueryTokenKind.Equal },
            { "ge", QueryTokenKind.GreaterThanEqual },
            { "gt", QueryTokenKind.GreaterThan },
            { "le", QueryTokenKind.LessThanEqual },
            { "lt", QueryTokenKind.LessThan },
            { "mod", QueryTokenKind.Modulo },
            { "mul", QueryTokenKind.Multiply },
            { "ne", QueryTokenKind.NotEqual },
            { "not", QueryTokenKind.Not },
            { "or", QueryTokenKind.Or },
            { "sub", QueryTokenKind.Subtract }
        };

        /// <summary>
        /// Creates a new <see cref="ODataExpressionLexer"/>.
        /// </summary>
        /// <param name="expression">The expression to tokenize.</param>
        internal ODataExpressionLexer(string expression)
        {
            Text = expression;
            Token = new QueryToken();
            SetTextPosition(0);
            NextToken();
        }

        /// <summary>
        /// The current character
        /// </summary>
        public char CurrentChar { get; private set; }

        /// <summary>
        /// The expression being tokenized
        /// </summary>
        internal string Text { get; }

        /// <summary>
        /// The current position within the expression.
        /// </summary>
        private int TextPosition { get; set; }

        /// <summary>
        /// The current token
        /// </summary>
        public QueryToken Token { get; private set; }

        /// <summary>
        /// Gets the next token in the expression.
        /// </summary>
        /// <returns>A <see cref="QueryToken"/>.</returns>
        /// <exception cref="ODataException">If a syntax error is encountered.</exception>
        public QueryToken NextToken()
        {
            SkipWhiteSpace();

            int tokenPosition = TextPosition;
            QueryTokenKind tokenKind;
            switch (CurrentChar)
            {
                case '(':
                    NextChar();
                    tokenKind = QueryTokenKind.OpenParen;
                    break;
                case ')':
                    NextChar();
                    tokenKind = QueryTokenKind.CloseParen;
                    break;
                case ',':
                    NextChar();
                    tokenKind = QueryTokenKind.Comma;
                    break;
                case '-':
                    NextChar();
                    tokenKind = QueryTokenKind.Minus;
                    break;
                case '/':
                    NextChar();
                    tokenKind = QueryTokenKind.Dot;
                    break;
                case '\'':
                    CompleteStringLiteral();
                    tokenKind = QueryTokenKind.StringLiteral;
                    break;
                default:
                    if (IsIdentifierStart(CurrentChar) || CurrentChar == '@' || CurrentChar == '_')
                    {
                        CompleteIdentifier();
                        tokenKind = QueryTokenKind.Identifier;
                        break;
                    }
                    if (char.IsDigit(CurrentChar))
                    {
                        tokenKind = CompleteNumberLiteral();
                        break;
                    }
                    if (TextPosition == Text.Length)
                    {
                        tokenKind = QueryTokenKind.End;
                        break;
                    }
                    throw new ODataException("The specified OData expression has syntax errors.", Text, TextPosition);
            }

            Token.Kind = tokenKind;
            Token.Text = Text.Substring(tokenPosition, TextPosition - tokenPosition);
            Token.Position = tokenPosition;

            if (Token.Kind == QueryTokenKind.Identifier && identifierClassification.ContainsKey(Token.Text))
            {
                Token.Kind = identifierClassification[Token.Text];
            }

            return Token;
        }

        /// <summary>
        /// Reads the stream until the supplied character
        /// </summary>
        /// <param name="ch">The character to read up to.</param>
        /// <returns>The <see cref="QueryToken"/> with the contents read</returns>
        public QueryToken ReadUntilCharacter(char ch)
        {
            int startPosition = TextPosition;
            AdvanceToNextOccuranceOf(ch);

            Token.Kind = QueryTokenKind.Unknown;
            Token.Text = Text.Substring(startPosition, TextPosition - startPosition);
            Token.Position = startPosition;
            return Token;
        }

        /// <summary>
        /// Given the start of an identifier, progress until the end of the identifier.
        /// </summary>
        private void CompleteIdentifier()
        {
            do
            {
                NextChar();
            }
            while (IsIdentifierPart(CurrentChar) || CurrentChar == '_');
        }

        /// <summary>
        /// Given the start of a number, progress until the end of the number.
        /// </summary>
        /// <returns>The type of number literal found.</returns>
        private QueryTokenKind CompleteNumberLiteral()
        {
            QueryTokenKind tokenKind = QueryTokenKind.IntegerLiteral;
            // Look for the integer part.
            SkipDigits();

            // Look for a decimal point.
            if (CurrentChar == '.')
            {
                tokenKind = QueryTokenKind.RealLiteral;
                NextChar();
                if (!char.IsDigit(CurrentChar))
                {
                    throw new ODataException("Failed to parse number in OData expression - digit expected.", Text, TextPosition);
                }
                SkipDigits();
            }

            // Look for exponent.
            if (CurrentChar == 'E' || CurrentChar == 'e')
            {
                tokenKind = QueryTokenKind.RealLiteral;
                NextChar();
                if (CurrentChar == '+' || CurrentChar == '-')
                {
                    NextChar();
                }
                if (!char.IsDigit(CurrentChar))
                {
                    throw new ODataException("Failed to parse number in OData expression - digit expected.", Text, TextPosition);
                }
                SkipDigits();
            }

            // Check for a number type specifier.
            if (CurrentChar == 'F' || CurrentChar == 'f' || CurrentChar == 'M' || CurrentChar == 'm' || CurrentChar == 'D' || CurrentChar == 'd')
            {
                tokenKind = QueryTokenKind.RealLiteral;
                NextChar();
            }

            return tokenKind;
        }

        /// <summary>
        /// Called when the current character is a quote to move
        /// beyond the string literal.
        /// </summary>
        /// <exception cref="ODataException">If the terminating quote character cannot be found.</exception>
        private void CompleteStringLiteral()
        {
            char quote = CurrentChar;
            do
            {
                AdvanceToNextOccuranceOf(quote);
                if (TextPosition == Text.Length)
                {
                    throw new ODataException("The specified OData query has an unterminated string literal.", Text, TextPosition);
                }
                NextChar();
            }
            while (CurrentChar == quote);
        }

        /// <summary>
        /// Moves to a terminating character, while sitting on the starting character.
        /// </summary>
        /// <param name="ending">The terminating character.</param>
        private void AdvanceToNextOccuranceOf(char ending)
        {
            NextChar();
            while (TextPosition < Text.Length && CurrentChar != ending)
            {
                NextChar();
            }
        }

        /// <summary>
        /// Moves to the next character in the expression, setting both the
        /// <see cref="TextPosition"/> and <see cref="CurrentChar"/>.
        /// </summary>
        private void NextChar()
        {
            if (TextPosition < Text.Length)
            {
                TextPosition++;
            }
            CurrentChar = (TextPosition < Text.Length) ? Text[TextPosition] : '\0';
        }

        /// <summary>
        /// Sets the text position to a specific position, setting the
        /// <see cref="CurrentChar"/> at the same time.
        /// </summary>
        /// <param name="position"></param>
        private void SetTextPosition(int position)
        {
            TextPosition = position;
            CurrentChar = (TextPosition < Text.Length) ? Text[TextPosition] : '\0';
        }

        /// <summary>
        /// Skips the digits in a number.
        /// </summary>
        private void SkipDigits()
        {
            do
            {
                NextChar();
            }
            while (char.IsDigit(CurrentChar));
        }

        /// <summary>
        /// Skips white space in the expression.
        /// </summary>
        private void SkipWhiteSpace()
        {
            while (char.IsWhiteSpace(CurrentChar))
            {
                NextChar();
            }
        }

        /// <summary>
        /// Token classification for the start of an identifier.
        /// </summary>
        /// <param name="ch"></param>
        /// <returns><c>true</c> if the provided character can be used as the first character of an identifier.</returns>
        private static bool IsIdentifierStart(char ch) => char.IsLetter(ch);

        /// <summary>
        /// Token classification for a non-start of an identifier.
        /// </summary>
        /// <param name="ch"></param>
        /// <returns><c>true</c> if the provided character can be used as a subsequent character of an identifier.</returns>
        private static bool IsIdentifierPart(char ch) => char.IsLetterOrDigit(ch) || ch == '_' || ch == '-';
    }
}
