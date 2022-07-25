using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPM.Middleware.Core.Models.System
{
    public class clsSysParams
    {
        public bool IsSimulation { get; set; } = true;
        public string ServerIP { get; set; } = "0.0.0.0";
        public int IRServerPort { get; set; } = 1330;
        public int LRServerPort { get; set; } = 1331;

        public bool AutoKickOutNoActiveTcpClient { get; set; } = false;

        public int NoAcitveAllowTime { get; set; } = 60;//sencond

        public string Server => $"{ServerIP}:{IRServerPort}";
        public string LRComPort { get; set; } = "COM16";
        public int LRBaudRate { get; set; } = 9600;
    }
}
