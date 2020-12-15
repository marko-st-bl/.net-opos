using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Zadatak1.MyOposScheduler;

namespace Zadatak1
{
    public class OposTask : Task, IComparable<OposTask>
    {
        /// <summary>
        /// Represents priority of task that must be specified before starting task
        /// </summary>
        public int Priority { get { return _priority; } set { _priority = value; } }
        private int _priority;

        /// <summary>
        /// Represent maximum duration for task to execute
        /// </summary>
        public int MaxDuration { get { return _maxDuration; } set { _maxDuration = value; } }
        private int _maxDuration;

        /// <summary>
        /// Represent a delegete of function to be scheduled.
        /// </summary>
        public NewTaskStart nts { get; set; }
        public OposTask(Action action) : base(action)
        {
        }

        public OposTask(Action action, CancellationToken cancellationToken) : base(action, cancellationToken)
        {
        }

        public OposTask(Action action, TaskCreationOptions creationOptions) : base(action, creationOptions)
        {
        }

        public OposTask(Action<object> action, object state) : base(action, state)
        {
        }

        public OposTask(Action action, CancellationToken cancellationToken, TaskCreationOptions creationOptions) : base(action, cancellationToken, creationOptions)
        {
        }

        public OposTask(Action<object> action, object state, CancellationToken cancellationToken) : base(action, state, cancellationToken)
        {
        }

        public OposTask(Action<object> action, object state, TaskCreationOptions creationOptions) : base(action, state, creationOptions)
        {
        }

        public OposTask(Action<object> action, object state, CancellationToken cancellationToken, TaskCreationOptions creationOptions) : base(action, state, cancellationToken, creationOptions)
        {
        }

        public int CompareTo(OposTask other)
        {
            return other.Priority.CompareTo(this.Priority);
        }
    }
}
