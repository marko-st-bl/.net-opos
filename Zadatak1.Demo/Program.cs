using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Zadatak1.Demo
{
    class Program
    {

        static void Main(string[] args)
        {
            //NonPreemptive(15, 4);
            //PreemtiveWithDeadlock();
            //PreemptiveUsingStandardScheduler(10, 3);
            
        }

        public static void NonPreemptive(int numOfTasks, int numOfThreads)
        {
            MyOposScheduler osch = new MyOposScheduler(numOfThreads, MyOposScheduler.SchedulingMode.NonPreemptive);
            Console.WriteLine(osch.Mode);
            Console.WriteLine("===========================");
            MyOposScheduler.NewTaskStart[] tasks = new MyOposScheduler.NewTaskStart[numOfTasks];
            for (int i = 0; i < numOfTasks; i++)
            {
                int val = i;
                tasks[i] = x => printf(x, i > 10 ? 9 : 3, val);
                osch.ScheduleTask(tasks[i], i % 10, 5000);
            }

            while (osch.CurrentTaskCount > 0)
                Task.Delay(1000).Wait();
        }
        public static void PreemtiveWithDeadlock()
        {
            const int numOfTask = 4;
            MyOposScheduler osch = new MyOposScheduler(1, MyOposScheduler.SchedulingMode.Preemptive);
            Console.WriteLine(osch.Mode);
            Console.WriteLine("===========================");
            MyOposScheduler.NewTaskStart[] tasks = new MyOposScheduler.NewTaskStart[numOfTask];

            tasks[0] = x => printf1(x, 3, 1);
            tasks[1] = x => printf1(x, 3, 2);
            tasks[2] = x => printf(x, 3, 3);
            tasks[3] = x => printf(x, 3, 4);
            osch.ScheduleTask(tasks[0], 1, 10000);
            osch.ScheduleTask(tasks[1], 9, 10000);
            osch.ScheduleTask(tasks[2], 4, 10000);
            osch.ScheduleTask(tasks[3], 5, 10000);

            while (osch.CurrentTaskCount > 0)
                Task.Delay(1000).Wait();
        }

        public static void PreemptiveUsingStandardScheduler(int numOfTasks, int numOfThreads)
        {
            MyOposScheduler osch = new MyOposScheduler(numOfThreads, MyOposScheduler.SchedulingMode.Preemptive);
            Console.WriteLine(osch.Mode);
            Console.WriteLine("===========================");
            MyOposScheduler.NewTaskStart[] tasks = new MyOposScheduler.NewTaskStart[numOfTasks];
            for (int i = 0; i < numOfTasks; i++)
            {
                int val = i;
                OposTask otask = new OposTask(() => { });
                otask.Priority = i % 10;
                otask.MaxDuration = 5000;
                otask.nts = x => printf(x, i > 10 ? 3 : 7, val);
                otask.Start(osch);
            }

            while (osch.CurrentTaskCount > 0)
                Task.Delay(1000).Wait();
        }

        public static object locker = new object();
        public static void printf(TaskController taskController, int maxDuration, int val)
        {
            for (int i = 0; i < maxDuration; i++)
            {
                lock (taskController._locker)
                {
                    while (taskController.IsPaused)
                    {
                        Console.WriteLine("Task-{0} is WAITING.", val);
                        Monitor.Wait(taskController._locker);
                    }
                }
                if (taskController.IsCancelled)
                {
                    Console.WriteLine("Task {0} canceled after {1} seconds", val, i);
                    break;
                }
                Console.WriteLine("Task {0} is executing... i={1}", val, i);
                Task.Delay(1000).Wait();
                if (i == (maxDuration - 1))
                    Console.WriteLine("Task {0} Done!", val);
            }
        }
        public static void printf1(TaskController taskController, int maxDuration, int val)
        {
            taskController.Enter(locker);
            for (int i = 0; i < maxDuration; i++)
            {
                lock (taskController._locker)
                {
                    Console.WriteLine("Task {0} is executing... i={1}", val, i);
                    while (taskController.IsPaused)
                    {
                        Console.WriteLine("Task-{0} is WAITING.", val);
                        Monitor.Wait(taskController._locker);
                    }
                }
                if (taskController.IsCancelled)
                {
                    Console.WriteLine("Task {0} canceled after {1} seconds", val, i);
                    taskController.Exit(locker);
                    break;
                }
                Task.Delay(1000).Wait();
                if (i == (maxDuration - 1))
                    Console.WriteLine("Task {0} Done!", val);
            }
            taskController.Exit(locker);
        }
    }
}
