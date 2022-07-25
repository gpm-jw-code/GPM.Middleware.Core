using GPM.Middleware.Core.Models.Communication;
using GPM.Middleware.Core.Models.Drawing;
using GPM.Middleware.Core.Models.System;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using GPM.Middleware.Core.Keyence.Emulator;
using GPM.Middleware.Core.Keyence;
using GPM.Middleware.Core.Models.Communication.Middleware;

namespace GPM.Middleware.Core.Utility
{
    public class StaUtility
    {
        public static clsSysParams SysParams = new clsSysParams();
        public static clsSelectRegion selectRegionModel = new clsSelectRegion();

        public static clsServer server = new clsServer(clsServer.Client.CLIENT_TYPE.IR);
        public static clsServer LRserver = new clsServerLR(clsServer.Client.CLIENT_TYPE.LR);
        public static clsServer SSMserver = new clsServerLR(clsServer.Client.CLIENT_TYPE.SSM);

        public static ControlMiddleware ControlMiddleware = new ControlMiddleware();
        public static clsDLRS1AEmulator dlRS1AEmulator = new clsDLRS1AEmulator();
        public static RS232Interface dlRS1Interface = new RS232Interface(new List<string>() { "00" });

        public static clsDeviceConnection DevicesConnectionStates = new clsDeviceConnection();

        public struct Methods
        {
            internal static string sysParamJsonFile = "system_configs.json";

            public static void LoadSysParams()
            {

                if (File.Exists(sysParamJsonFile))
                    SysParams = JsonConvert.DeserializeObject<clsSysParams>(File.ReadAllText(sysParamJsonFile));
                SaveSysParams();
            }

            public static void SaveSysParams()
            {
                File.WriteAllText(sysParamJsonFile, JsonConvert.SerializeObject(SysParams, Formatting.Indented));
            }
        }

    }
}
