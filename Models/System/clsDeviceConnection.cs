using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPM.Middleware.Core.Models.System
{
    public class clsDeviceConnection
    {
        public bool IsIRConnected { get; set; } = false;
        public bool IsLRConnected { get; set; } = false;
    }
}
