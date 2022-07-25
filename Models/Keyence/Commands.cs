using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPM.Middleware.Core.Keyence.Commands
{
    public enum READ_COMMANDS
    {
        SR,
        M0,
        MS,
        DRQinput
    }

    public enum WRITE_COMMANDS
    {
        SW, AW
    }
}
