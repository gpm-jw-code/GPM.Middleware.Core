using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPM.Middleware.Core.Models.SSM
{
    public class clsModuleState
    {
        private clsModuleinfoBase _moduleInfo;
        public clsModuleinfoBase moduleInfo
        {
            get => _moduleInfo;
            set
            {
                _moduleInfo = value;
                if (ssmInterface != null)
                {
                    ssmInterface.StopWorks();
                    ssmInterface.Disconnect();
                }
                ssmInterface = new clsSSMInterface();
                ssmInterface.ConnectAsync(_moduleInfo.IP, _moduleInfo.Port);

            }
        }
        [NonSerialized]
        public clsSSMInterface ssmInterface;

        public clsModuleState(clsModuleinfoBase moduleInfo)
        {
            this.moduleInfo = moduleInfo;
        }
    }
}
