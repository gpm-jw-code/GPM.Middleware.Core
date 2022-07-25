using GPM.Middleware.Core.Models.Log;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GPM.Middleware.Core.Models.SSM
{
    public static class SSMModuleManager
    {
        const string MODULES_INFO_STORE_FILE = "ssm/modules.json";
        public static Dictionary<string, clsModuleState> ModuleList = new Dictionary<string, clsModuleState>();

        public static async void InitializeAsync()
        {
            Logger.Info("SSMModuleManager Initialize start..");
            await Task.Delay(1).ContinueWith((task) =>
            {
                LoadModuleParams();
                //SSMInterfaceBuild(ModuleList);
            });

            Logger.Info("SSMModuleManager Initialize Done!");
        }

        public static clsSSMInterface GetSSMInterfaceByEndPoint(string endpoint)
        {
            if (ModuleList.TryGetValue(endpoint, out clsModuleState state))
                return state.ssmInterface;
            else
                return null;
        }

        public async static void AddModule(clsModuleinfoBase baseInfo)
        {
            if (!ModuleList.TryGetValue(baseInfo.EndPoint, out clsModuleState moduleState))
            {
                ModuleList.Add(baseInfo.EndPoint, new clsModuleState(baseInfo));
            }
            else
            {
                ModuleList[baseInfo.EndPoint].moduleInfo = baseInfo;
            }

            SaveModuleParames();

        }

        public static int Remove(clsModuleinfoBase binfo)
        {
            var kp = ModuleList.FirstOrDefault(f => f.Key == binfo.EndPoint);
            if (kp.Value == null)
                return 0;
            var ssminterface = kp.Value.ssmInterface;
            ssminterface.StopWorks();//if fetching..
            ssminterface.Disconnect();
            ModuleList.Remove(binfo.EndPoint);
            SaveModuleParames();
            return 1;
        }

        private static void LoadModuleParams()
        {
            CONFIGDirCheck();
            if (File.Exists(MODULES_INFO_STORE_FILE))
            {
                try
                {
                    ModuleList = JsonConvert.DeserializeObject<Dictionary<string, clsModuleState>>(File.ReadAllText(MODULES_INFO_STORE_FILE));
                }
                catch (JsonSerializationException ex)
                {
                    SaveModuleParames();
                }
            }
        }

        private static void SaveModuleParames()
        {
            CONFIGDirCheck();
            File.WriteAllText(MODULES_INFO_STORE_FILE, JsonConvert.SerializeObject(ModuleList, Formatting.Indented));
        }

        private static void CONFIGDirCheck()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(MODULES_INFO_STORE_FILE));

        }

        private static async void SSMInterfaceBuild(clsModuleinfoBase baseInfo)
        {
            string _ip = baseInfo.IP;
            int _port = baseInfo.Port;
            var ssmInterface = new clsSSMInterface();
            ssmInterface.ConnectAsync(_ip, _port);
        }
        private static void SSMInterfaceBuild(Dictionary<string, clsModuleState> moduleList)
        {
            foreach (var end_point in ModuleList.Keys)
            {
            }
        }

        private static void SSMInterfaceBuild(Dictionary<string, int> moduleList)
        {

        }
    }
}
