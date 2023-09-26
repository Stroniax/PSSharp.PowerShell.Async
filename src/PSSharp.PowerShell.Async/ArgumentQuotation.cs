using System;
using System.Management.Automation.Language;
using System.Text.RegularExpressions;

namespace PSSharp.PowerShell.Async
{
    /// <summary>
    /// Represents <see cref="SingleQuote"/> ('), <see cref="DoubleQuote"/> ("), or <see cref="None"/>.
    /// </summary>
    public readonly partial struct ArgumentQuotation
        : IEquatable<ArgumentQuotation>,
            IComparable<ArgumentQuotation>
    {
        private enum Tag : byte
        {
            None = 0,
            SingleQuote = 1,
            DoubleQuote = 2,
        }

        private readonly Tag _tag;

        private ArgumentQuotation(Tag tag) => _tag = tag;

        /// <summary>
        /// No quotation character.
        /// </summary>
        public static ArgumentQuotation None => new ArgumentQuotation(Tag.None);

        /// <summary>
        /// The single quote character: '.
        /// </summary>
        public static ArgumentQuotation SingleQuote => new ArgumentQuotation(Tag.SingleQuote);

        /// <summary>
        /// The double quote character: ".
        /// </summary>
        public static ArgumentQuotation DoubleQuote => new ArgumentQuotation(Tag.DoubleQuote);

        /// <summary>
        /// Calls a delegated handler depending on the current variant. This provides
        /// a concise conditional handler or fluent switch-style API over the instance.
        /// </summary>
        /// <typeparam name="TArg">The type of the argument to pass to the delegate.</typeparam>
        /// <typeparam name="TResult">The return type of the delegate.</typeparam>
        /// <param name="arg">An argument to pass to the delegate.</param>
        /// <param name="matchNone">A delegate called if the current instance is <see cref="None"/>.</param>
        /// <param name="matchSingleQuote">A delegate called if the current instance is <see cref="SingleQuote"/>.</param>
        /// <param name="matchDoubleQuote">A delegate called if the current instance is <see cref="DoubleQuote"/>.</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public TResult Match<TArg, TResult>(
            TArg arg,
            Func<TArg, TResult> matchNone,
            Func<TArg, TResult> matchSingleQuote,
            Func<TArg, TResult> matchDoubleQuote
        ) =>
            _tag switch
            {
                Tag.None => matchNone(arg),
                Tag.SingleQuote => matchSingleQuote(arg),
                Tag.DoubleQuote => matchDoubleQuote(arg),
                _ => throw new InvalidOperationException($"Invalid variant: {_tag}"),
            };

        /// <summary>
        /// Returns the name of this <see cref="ArgumentQuotation"/> variant.
        /// </summary>
        /// <returns></returns>
        public override string ToString() =>
            Match(0, _ => "None", _ => "SingleQuote", _ => "DoubleQuote");

        /// <inheritdoc/>
        public override bool Equals(object? obj)
        {
            return obj is ArgumentQuotation quotation && Equals(quotation);
        }

        /// <inheritdoc/>
        public bool Equals(ArgumentQuotation other)
        {
            return _tag == other._tag;
        }

        /// <inheritdoc/>
        public override int GetHashCode() => (int)_tag;

        /// <inheritdoc/>
        public int CompareTo(ArgumentQuotation other) => _tag.CompareTo(other);

        /// <inheritdoc/>
        public static bool operator ==(ArgumentQuotation left, ArgumentQuotation right) =>
            left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(ArgumentQuotation left, ArgumentQuotation right) =>
            !(left == right);

        /// <inheritdoc/>
        public static bool operator >(ArgumentQuotation left, ArgumentQuotation right) =>
            left.CompareTo(right) > 0;

        /// <inheritdoc/>
        public static bool operator <=(ArgumentQuotation left, ArgumentQuotation right) =>
            left.CompareTo(right) <= 0;

        /// <inheritdoc/>
        public static bool operator >=(ArgumentQuotation left, ArgumentQuotation right) =>
            left.CompareTo(right) >= 0;

        /// <inheritdoc/>
        public static bool operator <(ArgumentQuotation left, ArgumentQuotation right) =>
            left.CompareTo(right) < 0;

        /// <summary>
        /// Parses an instance of <see cref="ArgumentQuotation"/> from its string representation.
        /// </summary>
        /// <param name="text">A string representation of <see cref="ArgumentQuotation"/>.</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"><paramref name="text"/> is <see langword="null"/>.</exception>
        /// <exception cref="FormatException">Value does not match any <see cref="ArgumentQuotation.ToString"/> or supported variant.</exception>
        public static ArgumentQuotation Parse(string text) =>
            text is null
                ? throw new ArgumentNullException(nameof(text))
                : TryParse(text, out var quote)
                    ? quote
                    : throw new FormatException(
                        "Invalid argument quotation: expected 'None', 'SingleQuote', or 'DoubleQuote'."
                    );

        /// <summary>
        /// Tries to parse an instance of <see cref="ArgumentQuotation"/> from
        /// <paramref name="text"/>.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="quotation"></param>
        /// <returns>
        /// <see langword="true"/> if <paramref name="text"/> is a string representation of
        /// <see cref="ArgumentQuotation"/>, in which case <paramref name="quotation"/> represents
        /// the identified quotation.
        /// </returns>
        public static bool TryParse(string? text, out ArgumentQuotation quotation)
        {
            if (
                string.Equals("None", text, StringComparison.OrdinalIgnoreCase)
                || text == string.Empty
            )
            {
                quotation = None;
                return true;
            }
            if (
                string.Equals("SingleQuote", text, StringComparison.OrdinalIgnoreCase)
                || text == "'"
            )
            {
                quotation = SingleQuote;
                return true;
            }
            if (
                string.Equals("DoubleQuote", text, StringComparison.OrdinalIgnoreCase)
                || text == "\""
            )
            {
                quotation = DoubleQuote;
                return true;
            }
            quotation = None;
            return false;
        }

        /// <summary>
        /// Identifies the quotation type used for argument <paramref name="text"/>
        /// and produces a variation of the expression without quotations.
        /// </summary>
        /// <param name="text">The string that may be wrapped in or begin with quotation characters.</param>
        /// <param name="unwrapped"><paramref name="text"/>, or <see cref="string.Empty"/>, without the identified
        /// quotation characters surrounding the text.</param>
        /// <returns></returns>
        public static ArgumentQuotation Extract(string? text, out string unwrapped)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                unwrapped = string.Empty;
                return None;
            }

            var quoteChar = text![0];
            var endChar = text[text.Length - 1];
            var endIndex = quoteChar == endChar ? text.Length - 1 : text.Length;
            if (quoteChar == '"')
            {
                unwrapped = text.Substring(1, endIndex - 1);
                return DoubleQuote;
            }
            else if (quoteChar == '\'')
            {
                unwrapped = text.Substring(1, endIndex - 1);
                return SingleQuote;
            }
            else
            {
                unwrapped = text;
                return None;
            }
        }

        /// <summary>
        /// Wraps <paramref name="text"/> in the quote character represented by this instance. If
        /// <paramref name="text"/> is not only word characters and this instance is <see cref="None"/>,
        /// the text will be wrapped in single quotes.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public string Quote(string text)
        {
            return Match(
                text,
                text =>
                    IsOnlyWordCharacters(text)
                        ? text
                        : $"'{CodeGeneration.EscapeSingleQuotedStringContent(text)}'",
                text => $"'{CodeGeneration.EscapeSingleQuotedStringContent(text)}'",
                text =>
                {
                    var escapedSets = new (string, string)[]
                    {
                        ("$", "`$"),
                        ("`", "``"),
                        ("\r", "`r"),
                        ("\n", "`n"),
                        ("\t", "`t"),
                        ("\"", "`\""),
                        ("\a", "`a"),
                        ("\b", "`b"),
                        ($"{(char)27}", "`e"),
                        ("\f", "`f"),
                        ("\v", "`v"),
                    };
                    foreach (var pattern in escapedSets)
                    {
                        text = text.Replace(pattern.Item1, pattern.Item2);
                    }
                    return text;
                }
            );
        }

        private static bool IsOnlyWordCharacters(string text)
        {
#if NET7_0_OR_GREATER
            return OnlyWordCharacters().IsMatch(text);
#else
            return Regex.IsMatch(text, "^\\w+%");
#endif
        }

#if NET7_0_OR_GREATER
        [GeneratedRegex("^\\w+$")]
        private static partial Regex OnlyWordCharacters();
#endif
    }
}
