using GPM.Middleware.Core.Models.Log;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPM.Middleware.Core.Models.Drawing
{
    public class clsSelectRegion
    {

        public class clsReginData
        {
            public List<double> timeStamps = new List<double>();
            public List<double> maxValueList = new List<double>();
            public List<double> minValueList = new List<double>();

            public void Add(DateTime time, double max, double min)
            {
                timeStamps.Add(time.ToOADate());
                maxValueList.Add(max);
                minValueList.Add(min);

                if (timeStamps.Count >= 150)
                {
                    timeStamps.RemoveAt(0);
                    maxValueList.RemoveAt(0);
                    minValueList.RemoveAt(0);
                }
            }

            public void Clear()
            {
                timeStamps.Clear();
                maxValueList.Clear();
                minValueList.Clear();
            }
        }

        public clsReginData seriesData = new clsReginData();

        public clsReginData global_seriesData = new clsReginData();
        private ushort[,] _thermalData { get; set; }
        public clsRegionPoints regionPoints { get; set; } = new clsRegionPoints();

        private Bitmap _showingBitmap;
        public Bitmap CopyBitmap;
        public Bitmap showingBitmap
        {
            get => _showingBitmap;
            set
            {
                _showingBitmap = value;
                CopyBitmap = (Bitmap)value.Clone();
            }
        }

        public int PictureBoxHeight { get; set; }
        public int PictureBoxWidth { get; set; }

        public DateTime Time { get; private set; }
        public double MaxTemp { get; set; }
        public double MinTemp { get; set; }
        public double MeanTemp { get; set; }

        public double global_MaxTemp { get; set; }
        public double global_MinTemp { get; set; }
        public double global_MeanTemp { get; set; }


        public ushort[,] thermalData
        {
            get
            {
                return _thermalData;
            }
            set
            {
                _thermalData = value;
                Calculate();
            }
        }

        double rlWidth => (double)PictureBoxWidth / (double)_showingBitmap.Width;  // 3000  300
        double rlHeight => (double)PictureBoxHeight / (double)_showingBitmap.Height;  // 3000  300


        public void UpdatePoints(Point SelectPoint_Start, Point SelectPoint_End)
        {
            seriesData.Clear();
            var stx = SelectPoint_Start.X;
            var sty = SelectPoint_Start.Y;
            var endx = SelectPoint_End.X;
            var endy = SelectPoint_End.Y;

            if (stx > endx)
            {
                if (sty > endy)
                {
                    regionPoints.StartPoint = SelectPoint_End;
                    regionPoints.EndPoint = SelectPoint_Start;
                }
                else
                {
                    regionPoints.StartPoint = new Point(endx, sty);
                    regionPoints.EndPoint = new Point(stx, endy);
                }
            }
            else
            {

                if (sty < endy)
                {
                    regionPoints.StartPoint = SelectPoint_Start;
                    regionPoints.EndPoint = SelectPoint_End;
                }
                else
                {
                    regionPoints.StartPoint = new Point(stx, endy);
                    regionPoints.EndPoint = new Point(endx, sty);
                }
            }

            ///
            /// 


            string json = JsonConvert.SerializeObject(regionPoints, Formatting.Indented);
            File.WriteAllText("select_region.json", json);
            Logger.Info($"框選區域變更 {JsonConvert.SerializeObject(regionPoints)}");

        }
        public clsPxielDataModel maxTempData = new clsPxielDataModel();
        public clsPxielDataModel global_maxTempData = new clsPxielDataModel();
        private void Calculate()
        {
            if (thermalData == null | thermalData.Length == 0)
            {
                MaxTemp = MinTemp = -1;
                return;
            }

            var _startX = regionPoints.StartPoint.X;
            var _startY = regionPoints.StartPoint.Y;
            var _endX = regionPoints.EndPoint.X;
            var _endY = regionPoints.EndPoint.Y;

            if (_startX == 0 && _startY == 0 && _endX == 0 && _endY == 0)
                return;

            List<clsPxielDataModel> ls = new List<clsPxielDataModel>();
            List<clsPxielDataModel> globlaDatals = new List<clsPxielDataModel>();
            var row = _thermalData.GetLength(0);
            var col = _thermalData.GetLength(1);

            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < col; j++)
                {
                    double w = i * rlWidth;
                    double h = j * rlHeight;
                    var pxData = new clsPxielDataModel(w, h, j, i, _thermalData[i, j]);

                    if (w >= Math.Abs(_startX) && w <= Math.Abs(_endX) && h >= Math.Abs(_startY) && h <= Math.Abs(_endY))
                    {
                        ls.Add(pxData);
                    }
                    globlaDatals.Add(pxData);
                }
            }

            if (ls.Count == 0)
                return;

            Time = DateTime.Now;

            IEnumerable<int> tempRawList = ls.Select(d => d.temperatureRaw);
            var _MaxTempRaw = tempRawList.Max();
            var _MinTempRaw = tempRawList.Min();
            var _MeanTempRaw = tempRawList.Select(d => (int)d).ToList().Average();

            IEnumerable<int> global_tempRawList = globlaDatals.Select(d => d.temperatureRaw);
            var global_MaxTempRaw = global_tempRawList.Max();
            var global_MinTempRaw = global_tempRawList.Min();
            var global_MeanTempRaw = global_tempRawList.Select(d => (int)d).ToList().Average();

            maxTempData = ls.FirstOrDefault(d => d.temperatureRaw == _MaxTempRaw);
            global_maxTempData = globlaDatals.FirstOrDefault(d => d.temperatureRaw == global_MaxTempRaw);
            //ls.FirstOrDefault(d=>d.temperatureRaw==_MinTempRaw);

            MaxTemp = _MaxTempRaw / 10.0;
            MinTemp = _MinTempRaw / 10.0;
            MeanTemp = _MeanTempRaw / 10.0;

            global_MaxTemp = global_MaxTempRaw / 10.0;
            global_MinTemp = global_MinTempRaw / 10.0;
            global_MeanTemp = global_MeanTempRaw / 10.0;

            var time = DateTime.Now;
            seriesData.Add(time, MaxTemp, MinTemp);
            global_seriesData.Add(time, global_MaxTemp, global_MinTemp);
        }

        public void UpdateThermalData(mypicturebox.clsThermalData thermalData)
        {
            var time = DateTime.Now;
            this.Time = time;
            MaxTemp = maxTempData.temperature = thermalData.maxTemperature;
            MinTemp = thermalData.minTemperature;
            MeanTemp = thermalData.meanTemperature;
            seriesData.Add(time, thermalData.maxTemperature, thermalData.minTemperature);
            global_seriesData.Add(time, thermalData.global_maxTemperature, thermalData.global_minTemperature);

        }
    }
}
