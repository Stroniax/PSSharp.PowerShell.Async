using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Text;

namespace PSSharp.PowerShell.Async
{
    /// <summary>
    /// Context for requested PowerShell argument completion.
    /// </summary>
    public sealed class ArgumentCompleterContext : IEquatable<ArgumentCompleterContext>
    {
        /// <summary>
        /// Name of the command for which completion is requested.
        /// </summary>
        public string CommandName { get; }

        /// <summary>
        /// Name of the parameter for which completion is requested.
        /// </summary>
        public string ParameterName { get; }

        /// <summary>
        /// The partial word token being completed. Generally it is preferable to use <see cref="Wildcard"/>
        /// directly, and use <see cref="Quotation"/> in conjunction; these members handle removing and re-adding
        /// quotes to completions.
        /// </summary>
        public string WordToComplete { get; }

        /// <summary>
        /// A wildcard expression generated from <see cref="WordToComplete"/>.
        /// </summary>
        public WildcardPattern Wildcard { get; }

        /// <summary>
        /// The quotation style of <see cref="WordToComplete"/>.
        /// </summary>
        public ArgumentQuotation Quotation { get; }

        /// <summary>
        /// The AST where completion was requested.
        /// </summary>
        public CommandAst CommandAst { get; }

        /// <summary>
        /// Parameters known to the runtime when completion is requested.
        /// </summary>
        public IDictionary FakeBoundParameters { get; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public ArgumentCompleterContext(
            string commandName,
            string parameterName,
            string wordToComplete,
            WildcardPattern wildcard,
            ArgumentQuotation quotation,
            CommandAst commandAst,
            IDictionary fakeBoundParameters
        )
        {
            CommandName = commandName;
            ParameterName = parameterName;
            WordToComplete = wordToComplete;
            Wildcard = wildcard;
            Quotation = quotation;
            CommandAst = commandAst;
            FakeBoundParameters = fakeBoundParameters;
        }

        /// <inheritdoc/>
        public override bool Equals(object? obj) =>
            obj is ArgumentCompleterContext context && Equals(context);

        /// <inheritdoc/>
        public bool Equals(ArgumentCompleterContext? other)
        {
            return other != null
                && CommandName == other.CommandName
                && ParameterName == other.ParameterName
                && WordToComplete == other.WordToComplete
                && EqualityComparer<WildcardPattern>.Default.Equals(Wildcard, other.Wildcard)
                && Quotation.Equals(other.Quotation)
                && EqualityComparer<CommandAst>.Default.Equals(CommandAst, other.CommandAst)
                && EqualityComparer<IDictionary>.Default.Equals(
                    FakeBoundParameters,
                    other.FakeBoundParameters
                );
        }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            int hashCode = 197883433;
            hashCode =
                hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(CommandName);
            hashCode =
                hashCode * -1521134295
                + EqualityComparer<string>.Default.GetHashCode(ParameterName);
            hashCode =
                hashCode * -1521134295
                + EqualityComparer<string>.Default.GetHashCode(WordToComplete);
            hashCode =
                hashCode * -1521134295
                + EqualityComparer<WildcardPattern>.Default.GetHashCode(Wildcard);
            hashCode = hashCode * -1521134295 + Quotation.GetHashCode();
            hashCode =
                hashCode * -1521134295
                + EqualityComparer<CommandAst>.Default.GetHashCode(CommandAst);
            hashCode =
                hashCode * -1521134295
                + EqualityComparer<IDictionary>.Default.GetHashCode(FakeBoundParameters);
            return hashCode;
        }

        /// <summary>
        /// Gets a string representation of the current instance.
        /// </summary>
        /// <returns>A string representation of the current instance.</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("ArgumentCompleterContext { ");
            AppendMember(nameof(CommandName), CommandName);
            sb.Append(", ");
            AppendMember(nameof(ParameterName), ParameterName);
            sb.Append(", ");
            AppendMember(nameof(WordToComplete), WordToComplete);
            sb.Append(", ");
            AppendMember(nameof(FakeBoundParameters) + ".Count", FakeBoundParameters.Count);
            sb.Append(" }");

            return sb.ToString();

            void AppendMember(string name, object value)
            {
                sb.Append(name).Append(" = \"").Append(value).Append("\"");
            }
        }

        /// <inheritdoc/>
        public static bool operator ==(
            ArgumentCompleterContext? left,
            ArgumentCompleterContext? right
        )
        {
            return left?.Equals(right) ?? right is null;
        }

        /// <inheritdoc/>
        public static bool operator !=(
            ArgumentCompleterContext? left,
            ArgumentCompleterContext? right
        )
        {
            return !(left == right);
        }
    }
}
