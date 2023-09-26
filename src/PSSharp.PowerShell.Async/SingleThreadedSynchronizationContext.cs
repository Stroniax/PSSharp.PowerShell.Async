using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

#nullable disable

namespace PSSharp.PowerShell.Async
{
    /// <summary>
    /// <see cref="SynchronizationContext"/> that runs all delegated work on a single thread.
    /// </summary>
    internal sealed class SingleThreadedSynchronizationContext : SynchronizationContext, IDisposable
    {
        BlockingCollection<(SendOrPostCallback, object)> _workItemQueue =
            new BlockingCollection<(SendOrPostCallback, object)>();

        private SingleThreadedSynchronizationContext() { }

        void IDisposable.Dispose()
        {
            if (_workItemQueue != null)
            {
                _workItemQueue.Dispose();
                _workItemQueue = null;
            }
        }

        private void AssertNotDisposed()
        {
            if (_workItemQueue == null)
            {
                throw new ObjectDisposedException(GetType().Name);
            }
        }

        private void RunSynchronously()
        {
            AssertNotDisposed();

            while (_workItemQueue.TryTake(out var workItem, Timeout.InfiniteTimeSpan))
            {
                workItem.Item1(workItem.Item2);

                // Has the synchronisation context been disposed?
                if (_workItemQueue == null)
                {
                    break;
                }
            }
        }

        private void CompleteAdding()
        {
            AssertNotDisposed();

            _workItemQueue.CompleteAdding();
        }

        public override void Post(SendOrPostCallback callback, object callbackState)
        {
            AssertNotDisposed();

            try
            {
                _workItemQueue.Add((callback, callbackState));
            }
            catch (InvalidOperationException running)
            {
                throw new InvalidOperationException(
                    "Synchronization context is not accepting further work.",
                    running
                );
            }
        }

        public static void RunSynchronized(Func<Task> asyncOperation)
        {
            var initial = Current;
            try
            {
                using var context = new SingleThreadedSynchronizationContext();
                SetSynchronizationContext(context);

                var task = asyncOperation();

                task.ContinueWith(
                    operationTask => context.CompleteAdding(),
                    scheduler: TaskScheduler.Default
                );

                context.RunSynchronously();

                task.GetAwaiter().GetResult();
            }
            finally
            {
                SetSynchronizationContext(initial);
            }
        }
    }
}
