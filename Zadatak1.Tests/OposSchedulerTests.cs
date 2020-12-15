using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static Zadatak1.MyOposScheduler;

namespace Zadatak1.Tests
{
    [TestClass]
    public class OposSchedulerTests
    {

        [TestMethod]
        public void TasksCountTests()
        {
            const int numOfTasks = 15;
            const int numOfThreads = 4;

            MyOposScheduler sch = new MyOposScheduler(numOfThreads);

            for(int i=0; i<numOfTasks; i++)
            {
                sch.ScheduleTask(x => Task.Delay(1000), i % 10, 5000);
            }
            Assert.AreEqual(numOfThreads, sch.CurrentTaskCount);
            Assert.AreEqual(SchedulingMode.NonPreemptive, sch.Mode);
            Assert.AreEqual(0, sch.PausedTasksCount);
        }

        [TestMethod]
        public void TestPausedTasksCount()
        {
            const int numOfTasks = 10;
            const int numOfThreads = 4;

            MyOposScheduler sch = new MyOposScheduler(numOfThreads, SchedulingMode.Preemptive);

            for(int i=0; i<numOfTasks; i++)
            {
                int val = i;
                sch.ScheduleTask(x => printf(x, 3, val), i % 10, 5000);
            }
            Thread.Sleep(1000);
            Assert.AreEqual(numOfThreads, sch.CurrentTaskCount);
            Assert.AreEqual(SchedulingMode.Preemptive, sch.Mode);
            Assert.AreEqual(6, sch.PausedTasksCount);
        }

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
    }
}
