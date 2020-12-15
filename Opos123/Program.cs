using System;
using System.Collections.Generic;
using System.Linq;

namespace Opos123
{
    class Program
    {

        static void Main(string[] args)
        {
            List<(string task, int priority, int maxDuration)> pendingTasks = new List<(string, int, int)>();
            pendingTasks.Add(("zadatak1", 10, 20));
            pendingTasks.Add(("zadatak2", 5, 20));
            pendingTasks.Add(("zadatak3", 1, 20));

            (string task, int priority, int maxDuration) t1 = pendingTasks.Where(x => x.maxDuration == 20).Max();
            Console.WriteLine(t1);
        }
    }
}
