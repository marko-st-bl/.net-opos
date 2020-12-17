using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace Zadatak2
{
    public sealed class NotificationBackgroundTask : IBackgroundTask
    {

        private BackgroundTaskDeferral deferal;
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            deferal = taskInstance.GetDeferral();



            deferal.Complete();
        }
    }
}
