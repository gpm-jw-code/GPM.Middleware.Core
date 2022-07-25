using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPM.Middleware.Core.Models.Drawing
{
    public class clsRegionPoints
    {
        public Point StartPoint { get; set; }
        public Point EndPoint { get; set; }

        public int Width => EndPoint.X - StartPoint.X;
        public int Height => EndPoint.Y - StartPoint.Y;
    }
}
