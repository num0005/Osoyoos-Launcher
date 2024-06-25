using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace ToolkitLauncher
{
    /// <summary>
    /// Interaction logic for CancelableProgressBarWindow.xaml
    /// </summary>
    public partial class CancelableProgressBarWindowBase : Window, ICancellableProgress<double>
    {

        private readonly Dispatcher dispatcher;
        public CancelableProgressBarWindowBase()
        {
            InitializeComponent();
            CanCancel = true;
            Show();
            dispatcher = Dispatcher;
        }

        public bool CanCancel { get; private set;}

        public bool IsCancelled { get => tokenSource.IsCancellationRequested; }
        public string Status {

            get
            {
                return getStatus();
            }
            set
            {
                setStatus(value);
            }
        }
        private double _maxValue = 0;
        public double MaxValue { 
            get => _maxValue; 
            set {
                _maxValue = value;
                update();
            }
        }

        private string getStatus()
        {
            return dispatcher.Invoke(() => { return currentStatus.Text; });
        }

        private async void setStatus(string value)
        {
            await dispatcher.BeginInvoke(() => { currentStatus.Text = value; });
        }

        private double _currentProgress = 0;
        /// <summary>
        /// The progress
        /// </summary>
        public virtual double CurrentProgress {
            get => _currentProgress;
            set
            {
                _currentProgress = value;
                update();
            }
        }

        public void DisableCancellation()
        {
            CanCancel = false;
        }

        public void Dispose()
        {
            tokenSource.Cancel();
            tokenSource.Dispose();
        }

        public CancellationToken GetCancellationToken()
        {
            return tokenSource.Token;
        }

        public void Report(double value)
        {
            CurrentProgress += value;
        }

        protected async void update()
        {
            await dispatcher.BeginInvoke(() => { updateNoInvoke(); });
        }
        protected void updateNoInvoke()
        {
            closeButton.IsEnabled = CanCancel || Complete;
            closeButton.Content = Complete ? "Close" : "Cancel";
            if (!Complete && (IsCancelled || CurrentProgress > MaxValue || MaxValue == 0))
            {
                progress.IsIndeterminate = true;
            } else if (Complete)
            {
                stopwatch.Stop();
                string timeElapsed = String.Format("({0:00}:{1:00})", stopwatch.Elapsed.Hours, stopwatch.Elapsed.Minutes);
                if (IsCancelled)
                    Status = cancelReason is null ? "Canceled! " + timeElapsed : $"Canceled ({cancelReason})! " + timeElapsed;
                else
                    Status = "Done! " + timeElapsed;
                progress.IsIndeterminate = false;
                progress.Value = progress.Maximum;
            }
            else
            {
                progress.IsIndeterminate = false;
                progress.Maximum = MaxValue;
                progress.Value = CurrentProgress;
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!Complete)
                e.Cancel = true;
            if (CanCancel)
                Cancel();
            base.OnClosing(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            Dispose();
            base.OnClosed(e);
        }

        public async void Cancel(string? message = null)
        {
            if (!CanCancel)
                throw new InvalidOperationException("No cancellable!");
            if (!tokenSource.IsCancellationRequested)
            {
                cancelReason = message;
                Status = message is not null ? $"Canceling ({message})..." : "Canceling...";
                tokenSource.Cancel();

                await dispatcher.BeginInvoke(() => { update(); });
            }
        }

        private string? cancelReason = null;

        private readonly CancellationTokenSource tokenSource = new();

        private bool _complete = false;
        public bool Complete {
            set {if (_complete != value) { _complete = value; update(); } }
            get => _complete;
        }

        private void closeButton_Click(object sender, RoutedEventArgs e)
        {
            if (Complete)
                Close();
            else
                Cancel();
        }

        private Stopwatch stopwatch = Stopwatch.StartNew();
    }

    /// <summary>
    /// Templated class to support different types of progress bar
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CancelableProgressBarWindow<T> : CancelableProgressBarWindowBase, ICancellableProgress<T> where T : struct,
          IComparable,
          IComparable<T>,
          IConvertible,
          IEquatable<T>,
          IFormattable
    {
        private T _currentProgress;
        public new T CurrentProgress
        {
            get => _currentProgress;
            set
            {
                _currentProgress = value;
                base.CurrentProgress = (dynamic)_currentProgress;
            }
        }

        public void Report(T value)
        {
            CurrentProgress = (dynamic)CurrentProgress + value;
        }

        private T _maxValue;
        public new T MaxValue
        {
            get => _maxValue;
            set
            {
                _maxValue = value;
                base.MaxValue = (dynamic)_maxValue;
            }
        }
    }
}
