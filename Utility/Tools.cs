using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPM.Middleware.Core.Utility
{
    public class Tools
    {
        public static byte[] GetDataBytes(string str, bool addHeadWithLen = true)
        {
            byte[] data = Encoding.ASCII.GetBytes(str);
            if (!addHeadWithLen)
                return data;

            int len = data.Length;
            byte[] head = BitConverter.GetBytes(len).Reverse().ToArray();
            List<byte> outPut = new List<byte>();
            outPut.AddRange(head);
            outPut.AddRange(data);
            return outPut.ToArray();
        }

        public static long GetTimeStamp(DateTime dateTime)
        {
            DateTime dt1970 = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return (dateTime.Ticks - dt1970.Ticks) / 10000;
        }

        public static DateTime GetDateTimeFromTimeStamp(long timeStamp)
        {
            DateTime dt1970 = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return dt1970.AddTicks(timeStamp); ;
        }
    }
}
