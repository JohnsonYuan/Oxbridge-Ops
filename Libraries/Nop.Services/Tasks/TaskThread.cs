using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nop.Services.Tasks
{
    /// <summary>
    /// Represents task thread
    /// </summary>
    public partial class TaskThread : IDisposable
    {
        private static readonly TaskThread _taskThread = new TaskThread();

        private Timer _timer;
        private bool _disposed;
        private readonly Dictionary<string, ITask> _tasks;

        private TaskThread()
        {
            this._tasks = new Dictionary<string, ITask>();
            this.Seconds = 10 * 60;
        }

        private void Run()
        {
            if (Seconds <= 0)
                return;

            this.StartedUtc = DateTime.UtcNow;
            this.IsRunning = true;
            foreach (ITask task in this._tasks.Values)
            {
                task.Execute();
            }
            this.IsRunning = false;
        }

        private void TimerHandler(object state)
        {
            this._timer.Change(-1, -1);

            // 增加catch
            try
            {
                this.Run();

                if (this.RunOnlyOnce)
                {
                    this.Dispose();
                }
                else
                {
                    this._timer.Change(this.Interval, this.Interval);
                }
            }
            catch
            {
                this.Dispose();
            }
        }

        /// <summary>
        /// Disposes the instance
        /// </summary>
        public void Dispose()
        {
            if ((this._timer != null) && !this._disposed)
            {
                lock (this)
                {
                    this._timer.Dispose();
                    this._timer = null;
                    this._disposed = true;
                }
            }
        }

        /// <summary>
        /// Inits a timer
        /// </summary>
        public void InitTimer()
        {
            if (this._timer == null)
            {
                this._timer = new Timer(new TimerCallback(this.TimerHandler), null, this.Interval, this.Interval);
            }
        }

        /// <summary>
        /// Adds a task to the thread
        /// </summary>
        /// <param name="task">The task to be added</param>
        public void AddTask(ITask task)
        {
            if (!this._tasks.ContainsKey(task.Name))
            {
                this._tasks.Add(task.Name, task);
            }
        }

        /// <summary>
        /// Gets the task thread instance
        /// </summary>
        public static TaskThread Instance
        {
            get
            {
                return _taskThread;
            }
        }

        /// <summary>
        /// Gets or sets the interval in seconds at which to run the tasks
        /// </summary>
        public int Seconds { get; set; }

        /// <summary>
        /// Get or sets a datetime when thread has been started
        /// </summary>
        public DateTime StartedUtc { get; private set; }

        /// <summary>
        /// Get or sets a value indicating whether thread is running
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Get a list of tasks
        /// </summary>
        public IList<ITask> Tasks
        {
            get
            {
                var list = new List<ITask>();
                foreach (var task in this._tasks.Values)
                {
                    list.Add(task);
                }
                return new ReadOnlyCollection<ITask>(list);
            }
        }

        /// <summary>
        /// Gets the interval (in milliseconds) at which to run the task
        /// </summary>
        public int Interval
        {
            get
            {
                //if somobody entered more than "2147483" seconds, then an exception could be thrown (exceeds int.MaxValue)
                int interval = this.Seconds * 1000;
                if (interval <= 0)
                    interval = int.MaxValue;
                return interval;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the thread whould be run only once (per appliction start)
        /// </summary>
        public bool RunOnlyOnce { get; set; }
    }
}
