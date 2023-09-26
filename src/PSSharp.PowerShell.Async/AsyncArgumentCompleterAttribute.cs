#if NET7_0_OR_GREATER
using System;
using System.Collections;
using System.Collections.Generic;
using System.Management.Automation;
using System.Management.Automation.Language;
using System.Threading;
using System.Threading.Tasks;

namespace PSSharp.PowerShell.Async;

/// <summary>
/// Base class for asynchronous argument completion. In PowerShell, completions are not
/// truly asynchronous: this base class produces an API that, when called in PowerShell,
/// blocks until completions are available or a timeout has expired.
/// </summary>
public abstract class AsyncArgumentCompleterAttribute
    : ArgumentCompleterAttribute,
        IArgumentCompleter,
        IArgumentCompleterFactory
{
    /// <summary>
    /// The time waited before <see cref="CompleteArgumentAsync"/> is aborted.
    /// </summary>
    protected virtual TimeSpan Timeout => TimeSpan.FromSeconds(1);

    /// <summary>
    /// Calls <see cref="CompleteArgumentAsync(ArgumentCompleterContext, CancellationToken)"/> to
    /// retrieve argument completion suggestions.
    /// </summary>
    /// <param name="commandName">Name of the command for which completion is requested.</param>
    /// <param name="parameterName">Name of the parameter for which completion is requested.</param>
    /// <param name="wordToComplete">The partial word token being completed.</param>
    /// <param name="commandAst">The AST where completion was requested.</param>
    /// <param name="fakeBoundParameters">Parameters known to the runtime when completion is requested.</param>
    /// <returns></returns>
    public IEnumerable<CompletionResult> CompleteArgument(
        string commandName,
        string parameterName,
        string wordToComplete,
        CommandAst commandAst,
        IDictionary fakeBoundParameters
    )
    {
        // do not block for longer than the configured timeout
        using var cts = new CancellationTokenSource(Timeout);

        // cancel on Ctrl+C. Only works with some hosts, but PowerShell does not
        // give us better access to Ctrl+C handling AFAIK
        // ideally we would also listen to an escape keypress
        void OnCancelKeyPress(object? _, ConsoleCancelEventArgs __)
        {
            cts.Cancel();
        }
        Console.CancelKeyPress += OnCancelKeyPress;

        try
        {
            var context = GetCompleterContext(
                commandName,
                parameterName,
                wordToComplete,
                commandAst,
                fakeBoundParameters
            );

            return BlockingEnumerate(cts, context);
        }
        finally
        {
            Console.CancelKeyPress -= OnCancelKeyPress;
        }
    }

    private static ArgumentCompleterContext GetCompleterContext(
        string commandName,
        string parameterName,
        string wordToComplete,
        CommandAst commandAst,
        IDictionary fakeBoundParameters
    )
    {
        var quote = ArgumentQuotation.Extract(wordToComplete, out var pattern);
        var wc = WildcardPattern.Get(pattern + "*", WildcardOptions.IgnoreCase);
        var context = new ArgumentCompleterContext(
            commandName,
            parameterName,
            wordToComplete,
            wc,
            quote,
            commandAst,
            fakeBoundParameters
        );
        return context;
    }

    private IEnumerable<CompletionResult> BlockingEnumerate(
        CancellationTokenSource cts,
        ArgumentCompleterContext context
    )
    {
        // complete the argument asynchronously
        // return as many completions as load in three seconds
        // ignore OperationCanceledException
        using var enumerator = CompleteArgumentAsync(context, cts.Token)
            .ToBlockingEnumerable()
            .GetEnumerator();
        var results = new List<CompletionResult>();
        while (MoveNextWhenNotCanceled(enumerator))
        {
            results.Add(enumerator.Current);
        }
        return results;
    }

    private static bool MoveNextWhenNotCanceled(IEnumerator<CompletionResult> enumerator)
    {
        try
        {
            return enumerator.MoveNext();
        }
        catch (ObjectDisposedException)
        {
            return false;
        }
        catch (OperationCanceledException)
        {
            return false;
        }
    }

    /// <summary>
    /// Asynchronously identify completion results. Note that this call is not truly made
    /// asynchronously as the PowerShell pipeline thread will block and wait for these
    /// completions. <paramref name="cancellationToken"/> will be aborted if the request
    /// takes too long. In derived classes, configure the timeout duration via
    /// <see cref="Timeout"/>. Completion results yielded before the timeout has elapsed
    /// will be published even if cancellation is requested.
    /// </summary>
    /// <param name="context">Context regarding the requested completion.</param>
    /// <param name="cancellationToken">A token used to cancel the request.</param>
    /// <returns>An asynchronous sequence of completion results.</returns>
    public abstract IAsyncEnumerable<CompletionResult> CompleteArgumentAsync(
        ArgumentCompleterContext context,
        CancellationToken cancellationToken
    );

    IArgumentCompleter IArgumentCompleterFactory.Create() => this;
}

#endif
