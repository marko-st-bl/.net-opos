using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Zadatak1
{
    public class TaskController
    {
        private static Semaphore semaphore = new Semaphore(1,100);
        public object _locker { get; private set; } = new object();

        /// <summary>
        /// Flag for checking is task canceled. It's used for cooperation between tasks.
        /// </summary>
        public bool IsCancelled { get; private set; }

        /// <summary>
        /// Flag for checking is task canceled. It's used for cooperation between tasks.
        /// </summary>
        public bool IsPaused { get; private set; }

        /// <summary>
        /// Represents List of locks that task aquired. You SHOULDN'T use this Property, it's used by MyOposScheduler class only
        /// </summary>
        public List<object> Locks { get; } = new List<object>();

        /// <summary>
        /// Method that is used to lock resource before entering critical section.
        /// </summary>
        /// <param name="locker"></param>
        public void Enter(object locker)
        {
            Locks.Add(locker);
            Monitor.Enter(locker);
        }

        /// <summary>
        /// Method to realese locked resource used in critical section
        /// </summary>
        /// <param name="locker"></param>
        public void Exit(object locker)
        {
            Locks.Remove(locker);
            Monitor.Exit(locker);
        }

        /// <summary>
        /// Method for canceling task. Sets IsCancalled flag to true
        /// </summary>
        public void Cancel() => IsCancelled = true;

        /// <summary>
        /// Method for pausing task. Sets IsPaused flag to true
        /// </summary>
        public void Pause() => IsPaused = true;

        /// <summary>
        /// Method used to resume paused task. This method is used by MyOposScheduler to resume paused tasks. Don't use this method in your function!!!
        /// </summary>
        public void Resume()
        {
            lock(_locker)
            {
                IsPaused = false;
                Monitor.Pulse(_locker);
            }
        } 
    }
}
