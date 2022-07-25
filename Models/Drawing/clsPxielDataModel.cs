using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPM.Middleware.Core.Models.Drawing
{
    public class clsPxielDataModel
    {
        public int _2DDataX = 0;
        public int _2DDataY = 0;
        public int temperatureRaw = 0;
        public double temperature;
        public int pixelLocationX = 0;
        public int pixelLocationY = 0;
        public clsPxielDataModel()
        {
        }
        public clsPxielDataModel(double pixelLocationX, double pixelLocationY, int _2DDataX, int _2DDataY, int temperatureRaw)
        {
            this.pixelLocationX = Convert.ToInt32(Math.Round(pixelLocationX));
            this.pixelLocationY = Convert.ToInt32(Math.Round(pixelLocationY));
            this._2DDataX = _2DDataX;
            this._2DDataY = _2DDataY;
            this.temperatureRaw = temperatureRaw;

        }
    }
}
