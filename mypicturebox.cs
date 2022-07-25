using GPM.Middleware.Core.Models.Drawing;
using GPM.Middleware.Core.Models.Log;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GPM.Middleware.Core
{
    public class mypicturebox : PictureBox
    {

        public class clsThermalData
        {
            public ushort[,] _RawData = new ushort[388, 286];
            public ushort[,] RawData
            {
                get => _RawData;
                set
                {
                    _RawData = value;
                    GetListDataInRegion(out List<ushort> globalData, out List<ushort> selectedRegionData);
                    if (selectedRegionData.Count == 0)
                    {
                        maxTemperature = minTemperature = meanTemperature = -1;
                        return;
                    }
                    maxTemperature = (selectedRegionData.Max() - 1000) / 10.0;
                    minTemperature = (selectedRegionData.Min() - 1000) / 10.0;
                    meanTemperature = (selectedRegionData.Select(i => (double)i).Average() - 1000) / 10.0;


                    if (globalData.Count == 0)
                    {
                        global_maxTemperature = global_minTemperature = global_meanTemperature = -1;
                        return;
                    }
                    global_maxTemperature = (globalData.Max() - 1000) / 10.0;
                    global_minTemperature = (globalData.Min() - 1000) / 10.0;
                    global_meanTemperature = (globalData.Select(i => (double)i).Average() - 1000) / 10.0;

                }
            }

            public Point SelectedStartPixel;
            public Point SelectedEndPixel;

            public double maxTemperature { get; private set; }
            public double minTemperature { get; private set; }
            public double meanTemperature { get; private set; }

            public double global_maxTemperature { get; private set; }
            public double global_minTemperature { get; private set; }
            public double global_meanTemperature { get; private set; }


            public Point maxValPoint { get; private set; }
            public Point minValPoint { get; private set; }
            public Point global_maxValPoint { get; private set; }
            public Point global_minValPoint { get; private set; }

            private void GetListDataInRegion(out List<ushort> globalDataLs, out List<ushort> selectedDataLs)
            {
                var startRow = SelectedStartPixel.X;
                var endRow = SelectedEndPixel.X;
                var startCol = SelectedStartPixel.Y;
                var endCol = SelectedEndPixel.Y;

                ushort max = ushort.MinValue;
                ushort min = ushort.MaxValue;
                ushort global_max = ushort.MinValue;
                ushort global_min = ushort.MaxValue;

                Point maxValPoint = Point.Empty;
                Point minValPoint = Point.Empty;
                Point global_maxValPoint = Point.Empty;
                Point global_minValPoint = Point.Empty;

                selectedDataLs = new List<ushort>();
                globalDataLs = new List<ushort>();
                try
                {
                    int rowCunt = RawData.GetLength(0);
                    int colCunt = RawData.GetLength(1);

                    for (var x = 0; x < rowCunt; x++)
                    {
                        for (var y = 0; y < colCunt; y++)
                        {
                            var dat = RawData[x, y];
                            if (x >= startRow && x <= endRow && y >= startCol && y <= endCol)
                            {
                                bool isMaxValChange = dat > max;
                                bool isMinValChange = dat < min;
                                max = isMaxValChange ? dat : max;
                                min = isMinValChange ? dat : min;
                                if (isMaxValChange)
                                    maxValPoint = new Point(x, y);
                                if (isMinValChange)
                                    minValPoint = new Point(x, y);
                                selectedDataLs.Add(dat);
                            }


                            bool isGlobalMaxValChange = dat > global_max;
                            bool isGlobalMinValChange = dat < global_min;
                            global_max = isGlobalMaxValChange ? dat : global_max;
                            global_min = isGlobalMinValChange ? dat : global_min;
                            if (isGlobalMaxValChange)
                                global_maxValPoint = new Point(x, y);
                            if (isGlobalMinValChange)
                                global_minValPoint = new Point(x, y);

                            globalDataLs.Add(dat);
                        }
                    }

                    this.maxValPoint = maxValPoint;
                    this.minValPoint = minValPoint;
                    this.global_maxValPoint = global_maxValPoint;
                    this.global_minValPoint = global_minValPoint;

                }
                catch (IndexOutOfRangeException ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            public void UpdateSelectedPixel(Point startPixel, Point endPixel)
            {
                Point _px, _py;
                UpdateSelectedPixel(startPixel, endPixel, out _px, out _py);
            }
            public void UpdateSelectedPixel(Point startPixel, Point endPixel, out Point finalStartP, out Point finalEndP)
            {
                if (startPixel.X < endPixel.X && startPixel.Y < endPixel.Y)
                {
                    SelectedStartPixel = startPixel;
                    SelectedEndPixel = endPixel;
                }
                else if (startPixel.X > endPixel.X && startPixel.Y > endPixel.Y)
                {
                    SelectedStartPixel = endPixel;
                    SelectedEndPixel = startPixel;
                }
                else if (startPixel.X < endPixel.X && startPixel.Y > endPixel.Y)
                {
                    SelectedStartPixel = new Point(startPixel.X, endPixel.Y);
                    SelectedEndPixel = new Point(endPixel.X, startPixel.Y);
                }
                else
                {
                    SelectedStartPixel = new Point(endPixel.X, startPixel.Y);
                    SelectedEndPixel = new Point(startPixel.X, endPixel.Y);
                }

                finalStartP = SelectedStartPixel;
                finalEndP = SelectedEndPixel;
            }
        }


        public event EventHandler<bool> SelectModeOnchanged;

        bool isMouseDown;
        bool isMouseDrag;
        private bool _isSelectedMode = false;
        public bool isSelectedMode
        {
            get => _isSelectedMode;
            set
            {
                _isSelectedMode = value;
                Invalidate();
            }
        }
        public SHAPE_TYPE selectShapeType { get; set; } = SHAPE_TYPE.RECTANGLE;
        Rectangle rect = Rectangle.Empty;
        Rectangle pictrueModeBorder
        {
            get
            {
                return new Rectangle(-1, -1, this.Width - 2, this.Height - 2);
            }
        }

        public enum SHAPE_TYPE
        {
            RECTANGLE, CIRCLE
        }
        public Point startPixel { get; set; } = new Point();
        public Point endPixel { get; set; } = new Point();

        double factorX => Image == null ? 1 : (double)Width / Image.Width;
        double factorY => Image == null ? 1 : (double)Width / Image.Width;

        ContextMenu contextMenu = new ContextMenu();

        public mypicturebox() : base()
        {
            MouseWheel += PictureBox1_MouseWheel;
            MouseDown += pictureBox1_MouseDown;
            MouseUp += pictureBox1_MouseUp;
            MouseMove += pictureBox1_MouseMove;
            MouseLeave += pictureBox1_MouseLeave;
            Paint += pictureBox1_Paint;
            MouseClick += Mypicturebox_MouseClick;

            contextMenu.MenuItems.Add(new MenuItem("顯示", new EventHandler(ShowMenuItemOnclick)) { Enabled = false });
            contextMenu.MenuItems.Add(new MenuItem("變更框選區域", new EventHandler((sender, arg) =>
            {
                isSelectedMode = !isSelectedMode;
                SelectModeOnchanged?.Invoke(this, isSelectedMode);
                MenuItem menuItem = sender as MenuItem;
                menuItem.Checked = isSelectedMode;
                menuItem.Text = isSelectedMode ? "鎖定選取" : "變更框選區域";
            })));
            contextMenu.MenuItems.Add(new MenuItem("恢復成原大小", new EventHandler(RestoreToOriSizeHandle)));
            contextMenu.MenuItems.Add(new MenuItem("儲存影像", new EventHandler(SaveImageHandle)));
            ContextMenu = contextMenu;
        }

        private void SaveImageHandle(object sender, EventArgs e)
        {
            try
            {
                Image image = (Image)Image.Clone();

                SaveFileDialog saveFileDialog = new SaveFileDialog()
                {
                    Filter = "PNG|*.png|Bmp|*.bmp|JPG|*.jpg"
                };
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filename = saveFileDialog.FileName;
                    var exten = Path.GetExtension(filename);
                    ImageFormat format = ImageFormat.Png;
                    if (exten == ".jpg")
                        format = ImageFormat.Jpeg;
                    else if (exten == ".png")
                        format = ImageFormat.Png;
                    else if (exten == ".bmp")
                        format = ImageFormat.Bmp;

                    image.Save(filename, format);
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void RestoreToOriSizeHandle(object sender, EventArgs e)
        {
            Size = new Size(382, 288);
            Invalidate();
        }

        public void UpdateSelectPixelData(clsRegionPoints regionPoints)
        {
            startPixel = regionPoints.StartPoint;
            endPixel = regionPoints.EndPoint;
            thermalData.UpdateSelectedPixel(startPixel, endPixel);
            Invalidate();
        }

        Control oriParent;
        Size oriSize;
        DockStyle oriDockStyle;
        Point oriLocation;
        private void ShowMenuItemOnclick(object sender, EventArgs e)
        {
            MenuItem menuItem = sender as MenuItem;
            menuItem.Enabled = false;
            oriParent = Parent;
            oriDockStyle = Dock;
            oriSize = Size;
            oriLocation = Location;

            Form newForm = new Form()
            {
                AutoSize = true,
            };
            newForm.Size = newForm.MinimumSize = new Size(oriSize.Width, oriSize.Height + 25);
            Panel panel = new Panel() { Size = oriSize, Dock = DockStyle.Fill };
            newForm.FormClosing += NewForm_FormClosing;
            panel.Controls.Add(this);
            newForm.Controls.Add(panel);
            this.Dock = DockStyle.Fill;
            newForm.Show();
            newForm.SizeChanged += NewForm_SizeChanged;
        }

        private void NewForm_SizeChanged(object sender, EventArgs e)
        {
            Form form = (Form)sender;
            form.Size = new Size(form.Width, form.Width * oriSize.Height / oriSize.Width);
            Invalidate();
        }

        private void NewForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            contextMenu.MenuItems[0].Enabled = true;
            Size = oriSize;
            Location = oriLocation;
            Dock = oriDockStyle;

            oriParent.Controls.Add(this);
            (sender as Form).Dispose();
        }

        private void Mypicturebox_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {

            }
        }

        public clsThermalData thermalData { get; private set; } = new clsThermalData();

        private void PictureBox1_MouseWheel(object sender, MouseEventArgs e)
        {
            if (Width <= 388 && e.Delta < 0)
                return;
            int incX = e.Delta / 5;
            int incY = incX * 286 / 388;
            Size = new Size(Size.Width + incX, Size.Height + incY);
            Invalidate();
        }


        bool _isDragableRegion = false;
        bool isDragableRegion
        {
            get => _isDragableRegion;
            set
            {
                _isDragableRegion = value;
                Cursor = _isDragableRegion ? Cursors.Hand : Cursors.Default;
                BorderStyle = _isDragableRegion ? BorderStyle.Fixed3D : BorderStyle.FixedSingle;
            }
        }


        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (!isSelectedMode)
                return;

            isMouseDown = true;
            if (e.Button == MouseButtons.Left)
            {
                startPixel = new Point((int)(e.X / factorX), (int)(e.Y / factorY));
            }
        }
        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (!isSelectedMode)
                return;

            if (isMouseDown)
            {
                isMouseDrag = true;
                endPixel = new Point((int)(e.X / factorX), (int)(e.Y / factorY));
                Invalidate();
            }
            else
            {
                isMouseDown = false;
            }

        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (!isSelectedMode)
                return;

            isMouseDrag = isMouseDown = false;
            thermalData.UpdateSelectedPixel(startPixel, endPixel, out Point relPstart, out Point relPend);
            clsRegionPoints regionPoints = new clsRegionPoints()
            {
                StartPoint = relPstart,
                EndPoint = relPend
            };
            string json = JsonConvert.SerializeObject(regionPoints, Formatting.Indented);
            File.WriteAllText("select_region.json", json);
            Logger.Info($"框選區域變更 {JsonConvert.SerializeObject(regionPoints)}");
            Invalidate();
        }


        private void pictureBox1_MouseLeave(object sender, EventArgs e)
        {
            if (!isMouseDown)
            {
                isDragableRegion = false;
            }
        }
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            if (startPixel.X < endPixel.X)
            {
                rect.X = startPixel.X;
                rect.Width = endPixel.X - startPixel.X;
            }
            else
            {
                rect.X = endPixel.X;
                rect.Width = startPixel.X - endPixel.X;
            }
            if (startPixel.Y < endPixel.Y)
            {
                rect.Y = startPixel.Y;
                rect.Height = endPixel.Y - startPixel.Y;
            }
            else
            {
                rect.Y = endPixel.Y;
                rect.Height = startPixel.Y - endPixel.Y;
            }
            rect.X = (int)(rect.X * factorX);
            rect.Y = (int)(rect.Y * factorY);
            rect.Width = (int)(rect.Width * factorX);
            rect.Height = (int)(rect.Height * factorY);
            if (isMouseDrag)
            {
                if (selectShapeType == SHAPE_TYPE.RECTANGLE)
                    g.DrawRectangle(new Pen(Brushes.Gray, 2) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dot }, rect);
                else
                    g.DrawEllipse(new Pen(Brushes.Gray, 2) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dot }, rect);
            }
            else
            {
                if (selectShapeType == SHAPE_TYPE.RECTANGLE)
                    g.DrawRectangle(new Pen(Brushes.Red, 2), rect);
                else
                    g.DrawEllipse(new Pen(Brushes.Red, 2), rect);
            }
            g.DrawRectangle(new Pen(isSelectedMode ? Brushes.OrangeRed : Brushes.Transparent, 5) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash }, pictrueModeBorder);



            Pen redPen = new Pen(Brushes.Red, 2);
            ///選取區域
            var maxPx = (int)(thermalData.maxValPoint.X * factorX);
            var maxPy = (int)(thermalData.maxValPoint.Y * factorY);

            g.DrawEllipse(redPen, new Rectangle(maxPx, maxPy, 3, 3));
            g.DrawString($"Local.{thermalData.maxTemperature}", new Font("微軟正黑體", 12), Brushes.Red, maxPx, maxPy);

            ///Global
            var global_maxPx = (int)(thermalData.global_maxValPoint.X * factorX);
            var global_maxPy = (int)(thermalData.global_maxValPoint.Y * factorY);
            g.DrawEllipse(redPen, new Rectangle(global_maxPx, global_maxPy, 3, 3));
            g.DrawString($"Global.{thermalData.global_maxTemperature}", new Font("微軟正黑體", 12), Brushes.OrangeRed, global_maxPx, global_maxPy);
        }


    }
}
