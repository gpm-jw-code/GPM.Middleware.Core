using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GPM.Middleware.Core.Models.System
{
    public class PCStatesHelper
    {
        public class clsPCState
        {
            public double cpu { get; set; } = 0.0;
            public double ram { get; set; } = 0.0;
        }

        public static clsPCState pcState { get; private set; } = new clsPCState();

        public static void StartFetch(TimeSpan interval)
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    pcState.ram = DateTime.Now.Second;
                    pcState.cpu = DateTime.Now.Millisecond / 100.0;
                    await Task.Delay(interval);
                    SysStateChangeInvoke.Invoke();
                }
            });
        }

    }
}
