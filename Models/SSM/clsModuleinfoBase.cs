using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPM.Middleware.Core.Models.SSM
{
    public class clsModuleinfoBase
    {
        public string IP { get; set; }
        public int Port { get; set; }

        /// <summary>
        /// IP格式是否錯誤
        /// </summary>
        public bool isIPIllegal
        {
            get => IP == null ? false : IP.Split('.').Length != 4;
        }

        public string EndPoint => $"{IP}:{Port}";
    }
}
