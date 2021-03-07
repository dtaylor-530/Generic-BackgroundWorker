using System.Threading;
//https://www.codeproject.com/Articles/42103/Generic-Background-Worker
//Generic Background Worker
//DaveyM69, 9 Sep 2009
//No more unboxing/casting! Use generic type parameters with this background worker


namespace System.ComponentModel.Custom.Generic
{
    /// <summary>
    /// Executes an operation on a separate thread.
    /// </summary>
    /// <typeparam name="TArgument">The type of argument passed to the worker.</typeparam>
    /// <typeparam name="TProgress">The type of ProgressChangedEventArgs.UserState.</typeparam>
    /// <typeparam name="TResult">The type of result retrieved from the worker.</typeparam>
    public class BackgroundWorker<TArgument, TProgress, TResult>
    {
        #region Constants

        public const int MinProgress = 0;
        public const int MaxProgress = 100;

        #endregion

        #region Events

        /// <summary>
        /// Occurs when System.ComponentModel.Custom.Generic.BackgroundWorker.RunWorkerAsync() is called.
        /// </summary>
        public event EventHandler<DoWorkEventArgs<TArgument, TResult>> DoWork;
        /// <summary>
        /// Occurs when System.ComponentModel.Custom.Generic.BackgroundWorker.ReportProgress(System.Int32) is called.
        /// </summary>
        public event EventHandler<ProgressChangedEventArgs<TProgress>> ProgressChanged;
        /// <summary>
        /// Occurs when the background operation has completed, has been canceled, or has raised an exception.
        /// </summary>
        public event EventHandler<RunWorkerCompletedEventArgs<TResult>> RunWorkerCompleted;

        #endregion

        #region Fields

        private AsyncOperation asyncOperation = null;
        private readonly Action workerthreadStart;
        private readonly SendOrPostCallback operationCompleted;
        private readonly SendOrPostCallback progressReporter;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the System.ComponentModel.Custom.Generic.BackgroundWorker class.
        /// </summary>
        public BackgroundWorker()
        {
            workerthreadStart = () => WorkerThreadStart();
            operationCompleted = new SendOrPostCallback(AsyncOperationCompleted);
            progressReporter = new SendOrPostCallback(ProgressReporter);
            WorkerReportsProgress = true;
            WorkerSupportsCancellation = true;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether the application has requested cancellation of a background operation.
        /// </summary>
        public bool CancellationPending
        {
            get;
            private set;
        }
        /// <summary>
        /// Gets a value indicating whether the System.ComponentModel.Custom.Generic.BackgroundWorker
        /// is running an asynchronous operation.
        /// </summary>
        public bool IsBusy
        {
            get;
            private set;
        }
        /// <summary>
        /// Gets or sets a value indicating whether the System.ComponentModel.Custom.Generic.BackgroundWorker
        /// can report progress updates. The default is true.
        /// </summary>
        public bool WorkerReportsProgress
        {
            get;
            set;
        }
        /// <summary>
        /// Gets or sets a value indicating whether the System.ComponentModel.Custom.Generic.BackgroundWorker
        /// supports asynchronous cancellation. The default is true.
        /// </summary>
        public bool WorkerSupportsCancellation
        {
            get;
            set;
        }

        #endregion

        #region Methods

        private void AsyncOperationCompleted(object state)
        {
            IsBusy = false;
            CancellationPending = false;
            OnRunWorkerCompleted((RunWorkerCompletedEventArgs<TResult>)state);
        }
        /// <summary>
        /// Requests cancellation of a pending background operation.
        /// </summary>
        /// <returns>true, if the System.ComponentModel.Custom.Generic.BackgroundWorker supports cancellation;
        /// otherwise, false.</returns>
        public bool CancelAsync()
        {
            if (!WorkerSupportsCancellation)
                return false;
            CancellationPending = true;
            return true;
        }
        /// <summary>
        /// Raises the System.ComponentModel.Custom.Generic.BackgroundWorker.DoWork event.
        /// </summary>
        /// <param name="e">A System.ComponentModel.Custom.Generic.DoWorkEventArgs
        /// that contains the event data.</param>
        protected virtual void OnDoWork(DoWorkEventArgs<TArgument, TResult> e)
        {
            DoWork?.Invoke(this, e);
        }
        /// <summary>
        /// Raises the System.ComponentModel.Custom.Generic.BackgroundWorker.ProgressChanged event.
        /// </summary>
        /// <param name="e">A System.ComponentModel.Custom.Generic.ProgressChangedEventArgs
        /// that contains the event data.</param>
        protected virtual void OnProgressChanged(ProgressChangedEventArgs<TProgress> e)
        {
            ProgressChanged?.Invoke(this, e);
        }
        /// <summary>
        /// Raises the System.ComponentModel.Custom.Generic.BackgroundWorker.RunWorkerCompleted event.
        /// </summary>
        /// <param name="e">A System.ComponentModel.Custom.Generic.RunWorkerCompletedEventArgs
        /// that contains the event data.</param>
        protected virtual void OnRunWorkerCompleted(RunWorkerCompletedEventArgs<TResult> e)
        {
            RunWorkerCompleted?.Invoke(this, e);
        }
        private void ProgressReporter(object state)
        {
            OnProgressChanged((ProgressChangedEventArgs<TProgress>)state);
        }
        /// <summary>
        /// Raises the System.ComponentModel.Custom.Generic.BackgroundWorker.ProgressChanged event.
        /// </summary>
        /// <param name="percentProgress">The percentage, from 0 to 100, of the background operation that is complete.</param>
        /// <returns>true, if the System.ComponentModel.Custom.Generic.BackgroundWorker reports progress;
        /// otherwise, false.</returns>
        public bool ReportProgress(int percentProgress)
        {
            return ReportProgress(percentProgress, default(TProgress));
        }
        /// <summary>
        /// Raises the System.ComponentModel.Custom.Generic.BackgroundWorker.ProgressChanged event.
        /// </summary>
        /// <param name="percentProgress">The percentage, from MinProgress to MaxProgress,
        /// of the background operation that is complete.</param>
        /// <param name="userState">An object to be passed to the
        /// System.ComponentModel.Custom.Generic.BackgroundWorker.ProgressChanged event.</param>
        /// <returns>true, if the System.ComponentModel.Custom.Generic.BackgroundWorker reports progress;
        /// otherwise, false.</returns>
        public bool ReportProgress(int percentProgress, TProgress userState)
        {
            if (!WorkerReportsProgress)
                return false;
            if (percentProgress < MinProgress)
                percentProgress = MinProgress;
            else if (percentProgress > MaxProgress)
                percentProgress = MaxProgress;
            ProgressChangedEventArgs<TProgress> args = new ProgressChangedEventArgs<TProgress>(percentProgress, userState);
            if (asyncOperation != null)
                asyncOperation.Post(progressReporter, args);
            else
                progressReporter(args);
            return true;
        }
        /// <summary>
        /// Starts execution of a background operation.
        /// </summary>
        /// <returns>true, if the System.ComponentModel.Custom.Generic.BackgroundWorker isn't busy;
        /// otherwise, false.</returns>
        public bool RunWorkerAsync()
        {
            return RunWorkerAsync(default(TArgument));
        }
        /// <summary>
        /// Starts execution of a background operation.
        /// </summary>
        /// <param name="argument">A parameter for use by the background operation to be executed in the
        /// System.ComponentModel.Custom.Generic.BackgroundWorker.DoWork event handler.</param>
        /// <returns>true, if the System.ComponentModel.Custom.Generic.BackgroundWorker isn't busy;
        /// otherwise, false.</returns>
        public bool RunWorkerAsync(TArgument argument)
        {
            if (IsBusy)
                return false;
            IsBusy = true;
            CancellationPending = false;
            asyncOperation = AsyncOperationManager.CreateOperation(argument);
            workerthreadStart.Invoke();
            return true;
        }
        private void WorkerThreadStart()
        {
            TResult workerResult = default(TResult);
            Exception error = null;
            bool cancelled = false;
            try
            {
                DoWorkEventArgs<TArgument, TResult> doWorkArgs = new DoWorkEventArgs<TArgument, TResult>((TArgument)asyncOperation.UserSuppliedState);
                OnDoWork(doWorkArgs);
                if (doWorkArgs.Cancel)
                    cancelled = true;
                else
                    workerResult = doWorkArgs.Result;
            }
            catch (Exception exception)
            {
                error = exception;
            }
            RunWorkerCompletedEventArgs<TResult> e = new RunWorkerCompletedEventArgs<TResult>(workerResult, error, cancelled);
            asyncOperation.PostOperationCompleted(operationCompleted, e);
        }

        #endregion
    }

    /// <summary>
    /// Executes an operation on a separate thread.
    /// </summary>
    /// <typeparam name="T">The type of argument passed to in and out of the worker.</typeparam>
    public class BackgroundWorker<T>
    {
        #region Constants

        public const int MinProgress = 0;
        public const int MaxProgress = 100;

        #endregion

        #region Events

        /// <summary>
        /// Occurs when System.ComponentModel.Custom.Generic.BackgroundWorker.RunWorkerAsync() is called.
        /// </summary>
        public event EventHandler<DoWorkEventArgs<T>> DoWork;
        /// <summary>
        /// Occurs when System.ComponentModel.Custom.Generic.BackgroundWorker.ReportProgress(System.Int32) is called.
        /// </summary>
        public event EventHandler<ProgressChangedEventArgs<T>> ProgressChanged;
        /// <summary>
        /// Occurs when the background operation has completed, has been canceled, or has raised an exception.
        /// </summary>
        public event EventHandler<RunWorkerCompletedEventArgs<T>> RunWorkerCompleted;

        #endregion

        #region Fields

        private AsyncOperation asyncOperation = null;
        private readonly BasicDelegate threadStart;
        private readonly SendOrPostCallback operationCompleted;
        private readonly SendOrPostCallback progressReporter;

        #endregion

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the System.ComponentModel.Custom.Generic.BackgroundWorker class.
        /// </summary>
        public BackgroundWorker()
        {
            threadStart = new BasicDelegate(WorkerThreadStart);
            operationCompleted = new SendOrPostCallback(AsyncOperationCompleted);
            progressReporter = new SendOrPostCallback(ProgressReporter);
            WorkerReportsProgress = true;
            WorkerSupportsCancellation = true;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a value indicating whether the application has requested cancellation of a background operation.
        /// </summary>
        public bool CancellationPending
        {
            get;
            private set;
        }
        /// <summary>
        /// Gets a value indicating whether the System.ComponentModel.Custom.Generic.BackgroundWorker
        /// is running an asynchronous operation.
        /// </summary>
        public bool IsBusy
        {
            get;
            private set;
        }
        /// <summary>
        /// Gets or sets a value indicating whether the System.ComponentModel.Custom.Generic.BackgroundWorker
        /// can report progress updates. The default is true.
        /// </summary>
        public bool WorkerReportsProgress
        {
            get;
            set;
        }
        /// <summary>
        /// Gets or sets a value indicating whether the System.ComponentModel.Custom.Generic.BackgroundWorker
        /// supports asynchronous cancellation. The default is true.
        /// </summary>
        public bool WorkerSupportsCancellation
        {
            get;
            set;
        }

        #endregion

        #region Methods

        private void AsyncOperationCompleted(object state)
        {
            IsBusy = false;
            CancellationPending = false;
            OnRunWorkerCompleted((RunWorkerCompletedEventArgs<T>)state);
        }
        /// <summary>
        /// Requests cancellation of a pending background operation.
        /// </summary>
        /// <returns>true, if the System.ComponentModel.Custom.Generic.BackgroundWorker supports cancellation;
        /// otherwise, false.</returns>
        public bool CancelAsync()
        {
            if (!WorkerSupportsCancellation)
                return false;
            CancellationPending = true;
            return true;
        }
        /// <summary>
        /// Raises the System.ComponentModel.Custom.Generic.BackgroundWorker.DoWork event.
        /// </summary>
        /// <param name="e">A System.ComponentModel.Custom.Generic.DoWorkEventArgs
        /// that contains the event data.</param>
        protected virtual void OnDoWork(DoWorkEventArgs<T> e)
        {
            DoWork?.Invoke(this, e);
        }
        /// <summary>
        /// Raises the System.ComponentModel.Custom.Generic.BackgroundWorker.ProgressChanged event.
        /// </summary>
        /// <param name="e">A System.ComponentModel.Custom.Generic.ProgressChangedEventArgs
        /// that contains the event data.</param>
        protected virtual void OnProgressChanged(ProgressChangedEventArgs<T> e)
        {
            ProgressChanged?.Invoke(this, e);
        }
        /// <summary>
        /// Raises the System.ComponentModel.Custom.Generic.BackgroundWorker.RunWorkerCompleted event.
        /// </summary>
        /// <param name="e">A System.ComponentModel.Custom.Generic.RunWorkerCompletedEventArgs
        /// that contains the event data.</param>
        protected virtual void OnRunWorkerCompleted(RunWorkerCompletedEventArgs<T> e)
        {
            RunWorkerCompleted?.Invoke(this, e);
        }
        private void ProgressReporter(object state)
        {
            OnProgressChanged((ProgressChangedEventArgs<T>)state);
        }
        /// <summary>
        /// Raises the System.ComponentModel.Custom.Generic.BackgroundWorker.ProgressChanged event.
        /// </summary>
        /// <param name="percentProgress">The percentage, from 0 to 100, of the background operation that is complete.</param>
        /// <returns>true, if the System.ComponentModel.Custom.Generic.BackgroundWorker reports progress;
        /// otherwise, false.</returns>
        public bool ReportProgress(int percentProgress)
        {
            return ReportProgress(percentProgress, default(T));
        }
        /// <summary>
        /// Raises the System.ComponentModel.Custom.Generic.BackgroundWorker.ProgressChanged event.
        /// </summary>
        /// <param name="percentProgress">The percentage, from MinProgress to MaxProgress,
        /// of the background operation that is complete.</param>
        /// <param name="userState">An object to be passed to the
        /// System.ComponentModel.Custom.Generic.BackgroundWorker.ProgressChanged event.</param>
        /// <returns>true, if the System.ComponentModel.Custom.Generic.BackgroundWorker reports progress;
        /// otherwise, false.</returns>
        public bool ReportProgress(int percentProgress, T userState)
        {
            if (!WorkerReportsProgress)
                return false;
            if (percentProgress < MinProgress)
                percentProgress = MinProgress;
            else if (percentProgress > MaxProgress)
                percentProgress = MaxProgress;
            ProgressChangedEventArgs<T> args = new ProgressChangedEventArgs<T>(percentProgress, userState);
            if (asyncOperation != null)
                asyncOperation.Post(progressReporter, args);
            else
                progressReporter(args);
            return true;
        }
        /// <summary>
        /// Starts execution of a background operation.
        /// </summary>
        /// <returns>true, if the System.ComponentModel.Custom.Generic.BackgroundWorker isn't busy;
        /// otherwise, false.</returns>
        public bool RunWorkerAsync()
        {
            return RunWorkerAsync(default(T));
        }
        /// <summary>
        /// Starts execution of a background operation.
        /// </summary>
        /// <param name="argument">A parameter for use by the background operation to be executed in the
        /// System.ComponentModel.Custom.Generic.BackgroundWorker.DoWork event handler.</param>
        /// <returns>true, if the System.ComponentModel.Custom.Generic.BackgroundWorker isn't busy;
        /// otherwise, false.</returns>
        public bool RunWorkerAsync(T argument)
        {
            if (IsBusy)
                return false;
            IsBusy = true;
            CancellationPending = false;
            asyncOperation = AsyncOperationManager.CreateOperation(argument);
            threadStart.BeginInvoke(null, null);
            return true;
        }
        private void WorkerThreadStart()
        {
            T workerResult = default(T);
            Exception error = null;
            bool cancelled = false;
            try
            {
                DoWorkEventArgs<T> doWorkArgs =
                    new DoWorkEventArgs<T>((T)asyncOperation.UserSuppliedState);
                OnDoWork(doWorkArgs);
                if (doWorkArgs.Cancel)
                    cancelled = true;
                else
                    workerResult = doWorkArgs.Result;
            }
            catch (Exception exception)
            {
                error = exception;
            }
            RunWorkerCompletedEventArgs<T> e = new RunWorkerCompletedEventArgs<T>(workerResult, error, cancelled);
            asyncOperation.PostOperationCompleted(operationCompleted, e);
        }

        #endregion
    }
}