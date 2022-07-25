using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GPM.Middleware.Core.Models.Log
{
    public class Logger
    {
        public enum LEVEL
        {
            INFO,
            WARN,
            ERROR,
            FATAL
        }
        public static string logFolder = "Log";
        public static bool showInConsole = true;
        public static Action<string> OnLogWriting;

        public static void Initialize()
        {
            Task.Run(() => LogmsgsQueueHandler());
        }

        private static void LogmsgsQueueHandler()
        {
            while (true)
            {

                if (logmsgsQueue.Count != 0)
                {
                    LogWrite(logmsgsQueue.Dequeue());
                }

                Thread.Sleep(1);
            }
        }

        private static void LogWrite(clsLogMsgObj msgObj)
        {
            try
            {
                if (msgObj == null)
                    return;

                string folder = Path.Combine(logFolder, DateTime.Now.ToString("yyyy-MM-dd-HH"));
                Directory.CreateDirectory(folder);
                string filename = Path.Combine(folder, msgObj.level + ".txt");

                string msg = msgObj.level == LEVEL.ERROR | msgObj.level == LEVEL.FATAL ? msgObj.ex.Message : msgObj.msg;
                string writeText = $"{msgObj.time} |{msgObj.level}| {msg}";
                if (showInConsole)
                    Console.WriteLine(writeText);
                if (OnLogWriting != null)
                    OnLogWriting(writeText);
                using (StreamWriter sw = new StreamWriter(filename, true, Encoding.UTF8))
                {
                    sw.WriteLine(writeText);
                }
            }
            catch (Exception ex)
            {

            }

        }

        public static Queue<clsLogMsgObj> logmsgsQueue = new Queue<clsLogMsgObj>();

        public static void Info(string msg)
        {
            clsLogMsgObj clsLogMsgObj = new clsLogMsgObj(DateTime.Now, msg, LEVEL.INFO);
            logmsgsQueue.Enqueue(clsLogMsgObj);
        }

        public static void Warning(string msg)
        {
            clsLogMsgObj clsLogMsgObj = new clsLogMsgObj(DateTime.Now, msg, LEVEL.WARN);
            logmsgsQueue.Enqueue(clsLogMsgObj);
        }


        public static void Error(Exception ex)
        {
            clsLogMsgObj clsLogMsgObj = new clsLogMsgObj(DateTime.Now, ex, LEVEL.ERROR);
            logmsgsQueue.Enqueue(clsLogMsgObj);
        }

        public static void Fatal(Exception ex)
        {
            clsLogMsgObj clsLogMsgObj = new clsLogMsgObj(DateTime.Now, ex, LEVEL.FATAL);
            logmsgsQueue.Enqueue(clsLogMsgObj);
        }

    }
}
