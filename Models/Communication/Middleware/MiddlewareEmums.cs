using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPM.Middleware.Core.Models.Communication.Middleware
{
    public class MiddlewareEmums
    {
        public enum DATA_FORMAT
        {
            ASCII, HEX
        }


        public enum COMMAND_TYPE
        {
            /// <summary>
            /// Read
            /// </summary>
            SR,
            /// <summary>
            /// Write
            /// </summary>
            SW,
            Known = 999
        }

        public enum SENSOR_TYPE
        {
            IR, LR, SYS,
            Known = 999
        }

        public enum IR_READONLY_DATA_TYPE
        {
            READ_MAX_VAL_OF_OVERALL = 0,
            READ_MIN_VAL_OF_OVERALL = 1,
            READ_MEAN_VAL_OF_OVERALL = 2,
            READ_MAX_VAL_OF_SELECTED_ROI = 3,
            READ_MIN_VAL_OF_SELECTED_ROI = 4,
            READ_MEAN_VAL_OF_SELECTED_ROI = 5,
            Known

        }
        public enum LR_READONLY_DATA_TYPE
        {
            READ_CURRENT_MES_VALUE = 0,
            Known = 999
        }

        public enum SYS_READONLY_DATA_TYPE
        {
            READ_SYS_TIME = 102,
            Known = 999
        }

        public enum ERROR_CODES
        {
            COMMAND_STRUCT_ERROR = 400,
            COMMAND_TYPE_NOT_EXIST,
            SENSOR_TYPE_NOT_EXIST,
            DATA_TYPE_NOT_EXIST,
        }


        public enum SSM_DATA_TYPES
        {
            RAW = 001,
            P2P = 002,
            RMS = 003,
            ENERGY = 004,
            ALL = 005,
            Known = 999
        }

        public static List<int> SSM_DATA_TYPES_INTS => Enum.GetValues(typeof(SSM_DATA_TYPES)).Cast<SSM_DATA_TYPES>().Select(i => (int)i).ToList();

        internal static bool TryGetSSMDataType(int dataType, out SSM_DATA_TYPES dataTypeEnum)
        {
            dataTypeEnum = SSM_DATA_TYPES.Known;
            if (!SSM_DATA_TYPES_INTS.Contains(dataType))
                return false;
            if (dataType == 1)
                dataTypeEnum = SSM_DATA_TYPES.RAW;
            else if (dataType == 2)
                dataTypeEnum = SSM_DATA_TYPES.P2P;
            else if (dataType == 3)
                dataTypeEnum = SSM_DATA_TYPES.RMS;
            else if (dataType == 4)
                dataTypeEnum = SSM_DATA_TYPES.ENERGY;
            else if (dataType == 5)
                dataTypeEnum = SSM_DATA_TYPES.ALL;
            return true;
        }
    }
}
