using System;
using System.Management.Automation;
using System.Threading;
using System.Threading.Tasks;

namespace PSSharp.PowerShell.Async
{
    /// <summary>
    /// Base class for asynchronous PowerShell cmdlets. In PowerShell, cmdlets cannot be
    /// truly asynchronous: this base class produces an APIt that, when called in PowerShell,
    /// runs the asynchronous work on the Pipeline thread and blocks until it completes.
    /// </summary>
    public abstract class AsyncCmdlet : PSCmdlet, IDisposable
    {
        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        /// <inheritdoc/>
        ~AsyncCmdlet()
        {
            Dispose(false);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <inheritdoc cref="Dispose()"/>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _cts.Dispose();
            }
        }

        /// <summary>
        /// Asynchronously prepare for pipeline handling.
        /// Base implementation does nothing.
        /// </summary>
        /// <param name="cancellationToken">A token used to cancel the request. Bound to the cmdlet lifetime
        /// such that a call to <see cref="Cmdlet.StopProcessing"/> terminates this token.</param>
        /// <returns>A task that completes when the cmdlet is ready for processing.</returns>
        protected virtual Task BeginProcessingAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Asynchronously process the pipeline input or main command body.
        /// Base implementation does nothing.
        /// </summary>
        /// <param name="cancellationToken">A token used to cancel the request. Bound to the cmdlet lifetime
        /// such that a call to <see cref="Cmdlet.StopProcessing"/> terminates this token.</param>
        /// <returns>A task that completes when the command has completed processing.</returns>
        protected virtual Task ProcessRecordAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// When overridden in a derived class, performs clean-up after the command execution.
        /// Base implementation does nothing.
        /// </summary>
        /// <param name="cancellationToken">A token used to cancel the request. Bound to the cmdlet lifetime
        /// such that a call to <see cref="Cmdlet.StopProcessing"/> terminates this token.</param>
        /// <returns>A task that completes when the command has finished processing.</returns>
        protected virtual Task EndProcessingAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <inheritdoc/>
        protected sealed override void BeginProcessing()
        {
            SingleThreadedSynchronizationContext.RunSynchronized(
                () => BeginProcessingAsync(_cts.Token)
            );
        }

        /// <inheritdoc/>
        protected sealed override void ProcessRecord()
        {
            SingleThreadedSynchronizationContext.RunSynchronized(
                () => ProcessRecordAsync(_cts.Token)
            );
        }

        /// <inheritdoc/>
        protected sealed override void EndProcessing()
        {
            SingleThreadedSynchronizationContext.RunSynchronized(
                () => EndProcessingAsync(_cts.Token)
            );
        }

        /// <inheritdoc/>
        protected sealed override void StopProcessing()
        {
            _cts.Cancel();

            base.StopProcessing();
        }
    }
}
