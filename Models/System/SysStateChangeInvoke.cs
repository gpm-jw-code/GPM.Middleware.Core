using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPM.Middleware.Core.Models.System
{
    public class SysStateChangeInvoke
    {
        public static event EventHandler SystemStateOnChanged;
        internal static async Task Invoke()
        {
            await Task.Run(() => SystemStateOnChanged?.Invoke(null, null));

        }
    }
}
