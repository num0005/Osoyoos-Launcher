using System;
using System.Threading;

namespace ToolkitLauncher
{
    public interface ICancellableProgress<T> : IDisposable, IProgress<T>
    {
        /// <summary>
        /// Is the operation cancellable?
        /// </summary>
        public bool CanCancel { get; }
        /// <summary>
        /// Has the operation been cancelled?
        /// </summary>
        public bool IsCancelled { get; }

        /// <summary>
        /// The a string describing the current status
        /// </summary>
        public string Status { get; set; }

        /// <summary>
        /// A value of progress indicating completion
        /// </summary>
        public T MaxValue { get; set; }

        /// <summary>
        /// The current progress value
        /// </summary>
        public T CurrentProgress { get; set; }

        /// <summary>
        /// Has the operation been cancelled? Will throw an exception if you attempt to cancel an uncancellable operation
        /// </summary>
        /// <exception cref="InvalidOperationException">Throw if you attempt to cancel an operation that isn't cancellable</exception>
        public void Cancel(string? message = null);

        /// <summary>
        /// Get a cancellation token for the operation
        /// </summary>
        /// <returns>CancellationToken or CancellationToken.None</returns>
        public CancellationToken GetCancellationToken();

        public void DisableCancellation();
    }
}
