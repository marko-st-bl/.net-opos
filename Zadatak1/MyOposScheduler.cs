using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Zadatak1
{
    public class MyOposScheduler : TaskScheduler
    {
        public delegate void NewTaskStart(TaskController taskController);
        private object _lock = new object();

        readonly List<(NewTaskStart task, int priority, int maxDuration)> pendingTasks = new List<(NewTaskStart task, int priority, int maxDuration)>();
        readonly (NewTaskStart task, TaskController taskController, Task executingTask, Task callback, CancellationTokenSource cancelationTolenSource, int priority)?[] executingTasks;
        readonly List<(NewTaskStart task, TaskController taskController, Task executingTask, Task callback, CancellationTokenSource cancellationTokenSource, int priority)> pausedTasks = new List<(NewTaskStart, TaskController, Task, Task, CancellationTokenSource, int)>();

        public MyOposScheduler(int maxParallelTasks, SchedulingMode mode=SchedulingMode.NonPreemptive)
        {
            executingTasks = new (NewTaskStart task, TaskController taskController, Task executingTask, Task callback, CancellationTokenSource cancelationTolenSource, int priority)?[maxParallelTasks];
            Mode = mode;
        }

        /// <summary>
        /// Represents number of available threads, therefore it represent number of task that could run concurently
        /// </summary>
        public int MaxParallelTasks => executingTasks.Length;

        /// <summary>
        /// Represent number of task that are currently executing
        /// </summary>
        public int CurrentTaskCount => executingTasks.Count(x => x.HasValue);

        /// <summary>
        /// Enum type representig mode of MyOposScheduler (Preemptive or NonPreemptive)
        /// </summary>
        public SchedulingMode Mode { get; private set; }

        /// <summary>
        /// Represents number of paused tasks
        /// </summary>
        public int PausedTasksCount => pausedTasks.Count;

        /// <summary>
        /// Represents number of tasks that are waiting for scheduling
        /// </summary>
        public int PendingTasksCount => pendingTasks.Count;

        /// <summary>
        /// Method for passing function with priority and maximum time allowed for execution to scheduler. Scheduler schedule them based on mode selected in constructor
        /// </summary>
        /// <param name="task"></param>
        /// <param name="priority"></param>
        /// <param name="maxDuration"></param>
        public void ScheduleTask(NewTaskStart task, int priority, int maxDuration)
        {
            lock(pendingTasks)
            {
                pendingTasks.Add((task, priority, maxDuration));
            }
            SchedulePendingTasks();
        }

        private void ExecuteTasksPreemptive()
        {
        Monitor.Enter(_lock);
        int[] availableThreads = executingTasks.Select((value, i) => (value, i)).Where(x => !x.value.HasValue).Select(x => x.i).ToArray();
        foreach (int freeThread in availableThreads)
        {
            (NewTaskStart task, int priority, int maxDuration) pending;
            (NewTaskStart task, TaskController taskController, Task executingTask, Task callback, CancellationTokenSource cancelationTolenSource, int priority) paused;

            pending = pendingTasks.OrderBy(x => x.priority).LastOrDefault();
            paused = pausedTasks.OrderBy(x => x.priority).LastOrDefault();

            if(pendingTasks.Count > 0 && pausedTasks.Count > 0)
            {
                if (pending.priority > paused.priority)
                {
                    Console.WriteLine("Task dequed with priority: {0}", pending.priority);
                    pendingTasks.Remove(pending);

                    StartTask(pending, freeThread);
                }
                else
                {
                    executingTasks[freeThread] = paused;
                    pausedTasks.Remove(paused);
                    paused.taskController.Resume();
                    Console.WriteLine("Task with priority: {0} RESUMED.", paused.priority);
                }
            }
            else if (pendingTasks.Count > 0)
            {
                Console.WriteLine("Task dequed with priority: {0}", pending.priority);
                pendingTasks.Remove(pending);

                StartTask(pending, freeThread);  
                }
            else if (pausedTasks.Count > 0)
            {
                executingTasks[freeThread] = paused;
                pausedTasks.Remove(paused);
                paused.taskController.Resume();
                Console.WriteLine("Task with priority: {0} RESUMED.", paused.priority);
            }
                
        }
        (NewTaskStart task, int priority, int maxDuration) nextPending;
        (NewTaskStart task, TaskController taskController, Task executingTask, Task callback, CancellationTokenSource cancelationTolenSource, int priority) nextPaused;
            nextPending = pendingTasks.OrderBy(x => x.priority).LastOrDefault(); 
            nextPaused = pausedTasks.OrderBy(x => x.priority).LastOrDefault();
        if (nextPending.priority > nextPaused.priority )
        {
            int minPriorityExecuting = executingTasks.Select((value, i) => (value, i)).OrderBy(x => x.value.Value.priority).Select(x => x.i).First();

            if (nextPending.priority > executingTasks[minPriorityExecuting].Value.priority)
            {
                pendingTasks.Remove(nextPending);
                executingTasks[minPriorityExecuting].Value.taskController.Pause();
                Console.WriteLine("Task with priority: {0} paused task with priority: {1}", nextPending.priority, executingTasks[minPriorityExecuting].Value.priority);
                pausedTasks.Add(executingTasks[minPriorityExecuting].Value);
                StartTask(nextPending, minPriorityExecuting);
            }
                
            else
            {
                if(nextPaused.priority > executingTasks[minPriorityExecuting].Value.priority)
                {
                    pausedTasks.Add(executingTasks[minPriorityExecuting].Value);
                    pausedTasks.Remove(nextPaused);
                    executingTasks[minPriorityExecuting] = nextPaused;
                    nextPaused.taskController.Resume();
                    Console.WriteLine("Task with priority: {0} RESUMED", nextPaused.priority);
                }
            }
        }

        Monitor.Exit(_lock);
        CheckForDeadlocks();
        }

        private void SchedulePendingTasks()
        {
            AbortTasksOverQuota();
            if (Mode.Equals(SchedulingMode.NonPreemptive))
                ExecuteTasksNonPreemptive();
            else
                ExecuteTasksPreemptive();
        }

        private void ExecuteTasksNonPreemptive()
        {
            Monitor.Enter(_lock);
            int[] availableThreads = executingTasks.Select((value, i) => (value, i)).Where(x => !x.value.HasValue).Select(x => x.i).ToArray();
            foreach(int freeThread in availableThreads)
            {
                if (pendingTasks.Count > 0)
                { 
                    var next = pendingTasks.OrderBy(x => x.priority).Last();
                    Console.WriteLine("Task dequed with priority: {0}", next.priority);
                    pendingTasks.Remove(next);
                    StartTask(next, freeThread);                                          
                }
            }
            Monitor.Exit(_lock);
        }

        private void AbortTasksOverQuota()
        {
            for (int i = 0; i < MaxParallelTasks; ++i)
            {
                Monitor.Enter(_lock);
                if (executingTasks[i].HasValue)
                {
                    (NewTaskStart task, TaskController taskController, Task executingTask, _, CancellationTokenSource cancellationTokenSource, int priority) = executingTasks[i].Value;
                    if (cancellationTokenSource.IsCancellationRequested || executingTask.IsCanceled || executingTask.IsCompleted)
                    {
                        executingTasks[i] = null;
                        taskController.Cancel();
                    }
                }
                Monitor.Exit(_lock);
            }
        }

        protected override IEnumerable<Task> GetScheduledTasks() => pendingTasks.Select(x => new Task(() => x.task(new TaskController()))).ToArray();

        protected override void QueueTask(Task task)
        {
            OposTask otask = (OposTask)task;
            Monitor.Enter(_lock);
            pendingTasks.Add((otask.nts, otask.Priority, otask.MaxDuration));
            Monitor.Exit(_lock);
            SchedulePendingTasks();
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return !taskWasPreviouslyQueued && TryExecuteTask(task);
        }

        private void CheckForDeadlocks()
        {
            foreach (var paused in pausedTasks.ToArray())
            {
                if(!paused.Equals(default))
                {
                    foreach(var locker in paused.taskController.Locks.ToList())
                    {
                        for(int i=0; i<MaxParallelTasks; i++)
                        {
                            if(executingTasks[i].HasValue && executingTasks[i].Value.taskController.Locks.Contains(locker) && paused.priority < executingTasks[i].Value.priority)
                            {
                                Console.WriteLine("WARNING: DEADLOCK DETECTED: Task with priority {0} blocks task with priority {1}", paused.priority, executingTasks[i].Value.priority);
                                pausedTasks.Add(executingTasks[i].Value);
                                var pcp = paused;
                                pausedTasks.Remove(pcp);
                                pcp.priority = executingTasks[i].Value.priority;
                                executingTasks[i] = pcp;
                                pcp.taskController.Resume();

                            }
                        }
                    }
                }
            }
        }

        private (NewTaskStart, int, int) GetNextPendingTask()
        {
            (NewTaskStart, int, int) nextPending;    
            lock (pendingTasks)
            {
                nextPending = pendingTasks.OrderBy(x => x.priority).LastOrDefault();
                if(!nextPending.Equals(default))
                    Console.WriteLine("NEXT PENDING PRIORITY-{0}", nextPending.Item2);
            }
            return nextPending;
        }

        private (NewTaskStart, TaskController, Task, Task, CancellationTokenSource, int) GetNextPaused()
        {
            (NewTaskStart, TaskController, Task, Task, CancellationTokenSource, int) nextPaused;
            lock(pausedTasks)
            {
                nextPaused = pausedTasks.OrderBy(x => x.priority).LastOrDefault();
            }
            return nextPaused;
        }

        private void StartTask((NewTaskStart task, int priority, int maxDuration) next, int freeThread)
        {
            CancellationTokenSource cts = new CancellationTokenSource(next.maxDuration);
            TaskController taskController = new TaskController();
            Task scheduledTask = Task.Factory.StartNew(() => next.task(taskController), cts.Token);
            Task callback = next.maxDuration > 0 ? Task.Factory.StartNew(() =>
            {
                Task.Delay(next.maxDuration).Wait();
                if (!taskController.IsPaused)
                {
                    taskController.Cancel();
                    cts.Cancel();
                }
                scheduledTask.Wait();
                SchedulePendingTasks();
            }) : Task.Factory.StartNew(() =>
            {
                scheduledTask.Wait();
                SchedulePendingTasks();
            });
            executingTasks[freeThread] = (next.task, taskController, scheduledTask, callback, cts, next.priority);
        }

        public enum SchedulingMode
        {
            Preemptive,
            NonPreemptive
        }
    }
}
