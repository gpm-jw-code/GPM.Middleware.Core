using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GPM.Middleware.Core.Models.Log.Logger;

namespace GPM.Middleware.Core.Models.Log
{
    public class clsLogMsgObj
    {
        public DateTime time { get; }
        public Exception ex { get; }
        public string msg { get; }
        public LEVEL level { get; }

        public clsLogMsgObj(DateTime time, string msg, LEVEL level)
        {
            this.time = time;
            this.msg = msg;
            this.level = level;
        }

        public clsLogMsgObj(DateTime time, Exception ex, LEVEL level)
        {
            this.time = time;
            this.ex = ex;
            this.level = level;
        }
    }
}
