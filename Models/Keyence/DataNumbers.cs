using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPM.Middleware.Core.Keyence.DataNumbers
{
    public enum DATA_NUMBERS
    {
        Zero_Shift_Request = 001,
        Sensor_Amp_Error = 033,
        Judgment_output_Alarm_output = 036,
        Internal_measurement_value = 038,
        Analog_Output_Value = 042,
        Current_system_parameters = 056,
        Product_Code = 193,
        Connected_sensor_head = 195
    }
}
