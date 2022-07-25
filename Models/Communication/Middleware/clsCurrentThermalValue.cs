using Newtonsoft.Json;

namespace GPM.Middleware.Core.Models.Communication.Middleware
{
    public class clsCurrentThermalValue
    {
        public double timeStamp { get; set; }
        public double maxTemp { get; set; }
        public double minTemp { get; set; }
        internal string json => JsonConvert.SerializeObject(this);
    }
}
