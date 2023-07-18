using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Waf.Applications;
using System.Windows.Forms;
using SyDLab.Usv.Simulator.Applications.Views;
using System.Drawing;
using MathNet.Spatial.Units;
using SyDLab.Usv.Simulator.Domain;
using SyDLab.Usv.Simulator.Domain.Models;
using SyDLab.Usv.Simulator.Domain.Models.Platforms;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Documents;
using MathNet.Spatial.Euclidean;
using SyDLab.Usv.Simulator.Applications.Services;
using MathNet.Numerics.Distributions;
using System.IO;
using MathNet.Numerics.LinearAlgebra.Double;
using OpenCvSharp;


namespace SyDLab.Usv.Simulator.Applications.ViewModels
{
    public class SignalValue
    {
        public double StartAzimuth;
        public double EndAzimuth;
        public double Distance;
        public double Value;

        public SignalValue(double startAzimuth, double endAzimuth, double distance, double value)
        {
            StartAzimuth = startAzimuth;
            EndAzimuth = endAzimuth;
            Distance = distance;
            Value = value;
        }

        public double GetValue(double azimuth, double distance)
        {

            return Value;
        }
    }

    [Export]
    public class RadarViewModel : ViewModel<IRadarView>
    {
        public double Resolution
        {
            get => resolution;
            set => SetProperty(ref resolution, value);
        }

        private double resolution = 50;

        private Timer t = new Timer();

        private int width = 600, height = 600, hand = 300;
        private int marginX = 0, marginY=0;

        private int u; // degree
        private int cx, cy; // center of the circle
        private int x, y;   //HAND coordinate

        private int tx, ty, lim = 10;

        private static SolidBrush b = new SolidBrush(Color.FromArgb(100, Color.Green));
        private Pen p = new Pen(b, 1f);
        private Image baseImage;

        PointF _pt = new PointF(0F, 0F);
        PointF _pt2 = new PointF(1F, 1F);
        PointF _pt3 = new PointF(2F, 2F);

        private Bitmap radarBitmap;
        public DenseMatrix RadarSignal; // CAT240 signal
        public IDispatcherService d;

        private int numOfAzimuth = 3600; // 0.1도 간격, 360도까지
        private int numOfDataBlock = 1024; // range 관계없이 1024개 or 2048개

        public Project Project => Singleton<Project>.UniqueInstance;

        public bool SeaClutter
        {
            get => seaClutter;
            set => SetProperty(ref seaClutter, value);
        }

        private bool seaClutter;

        private object lockObject = new object();

        [ImportingConstructor]
        public RadarViewModel(IRadarView view) : base(view)
        {
            PropertyChanged += RadarViewModel_PropertyChanged;
        }

        private void RadarViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "Resolution":
                    Project.MyShips[0].Resolution = this.resolution;
                    break;
            }
        }

        public void Load()
        {
            //create Bitmap
            Bitmap bmp = new Bitmap(width + 2*marginX, height + 2*marginY);

            //center
            cx = width / 2 + marginX;
            cy = height / 2 + marginY;

            //initial degree of HAND
            u = 0;

            //graphics
            Graphics g = Graphics.FromImage(bmp);

            ////draw circle
            //g.DrawEllipse(p, marginX, marginY, width, height);  //bigger circle
            //g.DrawEllipse(p, marginX + width / 4, marginY + height / 4, width / 2, height / 2);    //smaller circle

            ////draw perpendiculat line
            //g.DrawLine(p, new Point(cx, marginY), new Point(cx, height + marginY)); //up-down
            //g.DrawLine(p, new Point(marginX, cy), new Point(width + marginX, cy)); //up-down

            // release the graphics object
            g.Dispose();

            // update the base image
            baseImage = bmp;
            // //timer
            // t.Interval = 2000/360; //in millisecond
            // t.Tick += T_Tick;
            // t.Start();

            UpdateImage(bmp);
        }

        public void CreateSignal()
        {
            Random r = new Random((int)DateTime.Now.Ticks);
            Normal distribution = new Normal(100, 40, r); // noise distribution (0~255)

            RadarSignal = DenseMatrix.CreateRandom(numOfAzimuth, numOfDataBlock, distribution); // noise 추가
            
        }

        public void UpdateHand(bool isTargetUpdated)
        {
            //graphics

            lock (lockObject)
            {
                Bitmap RadarBitmapTemp = new Bitmap(baseImage);
                Graphics gTemp = Graphics.FromImage(RadarBitmapTemp);
                Random r = new Random((int) DateTime.UtcNow.Ticks);

                // Noise 추가
                for (int i = 0; i < width / 2; i++)
                {
                    for (int j = 0; j < height / 2; j++)
                    {
                        int rand = r.Next();
                        if (rand % 20 != 0)
                            continue;
                
                        // GraphicsPath gp = new GraphicsPath();
                        // gp.AddEllipse(2 * i, 2 * j, 5, 5);
                        //
                        // PathGradientBrush pgb = new PathGradientBrush(gp);
                        // pgb.CenterPoint = new PointF(2 * i, 2 * j);
                        // pgb.CenterColor = Color.FromArgb(r.Next(0, 100), Color.Green);
                        // pgb.FocusScales = new PointF(0.7f, 0.7f);
                        // pgb.SurroundColors = new Color[] {Color.Empty};
                        //
                        // gTemp.FillPath(pgb, gp);
                
                        RadarBitmapTemp.SetPixel(2 * i, 2 * j, Color.FromArgb(r.Next(0, 150), 0, r.Next(0, 255), 0));
                
                        // SolidBrush b = new SolidBrush(Color.FromArgb(r.Next(0, 150), Color.Green));
                        //
                        // gTemp.FillEllipse(b, 2 * i, 2 * j, 2, 2);
                    }
                }

                if (seaClutter)
                {
                    int rnum = r.Next(300, 600);
                    double radius = Project.MyShips[0].Radius * 1000; //m

                    float scaleX = Convert.ToSingle(width / (2 * radius));
                    float scaleY = Convert.ToSingle(height / (2 * radius));

                    for (int i = 0; i < rnum; i++)
                    {
                        float pX = (float)(marginX + width * 0.5 + Normal.Sample(r, 0, 120) * scaleX);
                        float pY = (float)(marginY + height * 0.5 - Normal.Sample(r, 0, 120) * scaleY);

                        GraphicsPath gp = new GraphicsPath();
                        gp.AddEllipse(pX, pY, r.Next(2, 8), r.Next(2, 8));

                        PathGradientBrush pgb = new PathGradientBrush(gp);
                        pgb.CenterPoint = new PointF(pX, pY);
                        pgb.CenterColor = Color.FromArgb(r.Next(150, 250), 0, 255, 0);
                        pgb.FocusScales = new PointF(0.7f, 0.7f);
                        pgb.SurroundColors = new Color[] { Color.Empty };

                        gTemp.FillPath(pgb, gp);
                    }
                }
                
                ViewCore.UpdateImage(RadarBitmapTemp);

                if (true)
                    UpdateTargets(RadarBitmapTemp);
            }
        }

        public void UpdateTargets(Bitmap image)
        {
            lock (lockObject)
            {
                Random r = new Random((int)DateTime.Now.Ticks);

                Graphics gTemp = Graphics.FromImage(image);

                double cx = Project.MyShips[0].CurrentX;
                double cy = Project.MyShips[0].CurrentY;
                double radius = Project.MyShips[0].Radius * 1000; //m
                double scale = width / (radius * 2);
                double myHeading = Project.MyShips[0].CurrentHeading;

                //int size = (int)((width / 2) * (double)resolution / (double)Project.MyShips[0].Radius / 1000.0);

                double x, y, x1, y1, x2, y2;

                foreach (TargetShip targetShip in Project.TargetShips)
                {
                    targetShip.IsBehindObstacle = false;
                }

                foreach (Obstacle ob in Project.Obstacles)
                {
                    double myX = Project.MyShips[0].CurrentPosition.X;
                    double myY = Project.MyShips[0].CurrentPosition.Y;

                    List<double[]> obPointsRel = new List<double[]>();
                    List<double[]> obVisualized = new List<double[]>();

                    Dictionary<double[], double> DicDistance = new Dictionary<double[], double>();
                    Dictionary<double[], double> DicAzimuth = new Dictionary<double[], double>();

                    double minAzimuth = double.MaxValue;
                    double maxAzimuth = double.MinValue;

                    int max = -1;
                    int min = -1;

                    bool isCrossingBase = false;

                    double azimuthPrev = 0;

                    foreach (Waypoint point in ob.Points)
                    {
                        double tempX = point.Location.X - myX;
                        double tempY = point.Location.Y - myY;
                        double[] relPoint = { tempX, tempY };
                        double distance = Math.Sqrt(tempX * tempX + tempY * tempY);
                        double azimuth = Math.Atan2(tempY, tempX) / Math.PI * 180;

                        if (azimuth < 0)
                            azimuth += 360;

                        obPointsRel.Add(relPoint);
                        DicDistance[relPoint] = distance;
                        DicAzimuth[relPoint] = azimuth;


                        if (ob.Points.IndexOf(point) != 0)
                        {
                            if (Math.Abs(azimuth - azimuthPrev) > 180)
                            {
                                isCrossingBase = true;
                                continue;
                            }
                        }

                        if (minAzimuth > azimuth)
                        {
                            minAzimuth = azimuth;
                            min = obPointsRel.Count - 1;
                        }

                        if (azimuth > maxAzimuth)
                        {
                            maxAzimuth = azimuth;
                            max = obPointsRel.Count - 1;
                        }

                        azimuthPrev = azimuth;
                    }

                    ///// 기준 축을 지나는 면이 있는 경우, 외적을 통해 최대, 최소각 결정
                    List<int> minmaxPoint = new List<int>();

                    if (isCrossingBase)
                    {
                        foreach (Waypoint point in ob.Points)
                        {
                            Vector3D pointVec = new Vector3D(point.Location.X, point.Location.Y, 0);

                            Vector3D prevVector = new Vector3D(0, 0, 0);

                            bool isConCave = false;

                            foreach (Waypoint pointOthers in ob.Points)
                            {
                                Vector3D pointOtherVec = new Vector3D(pointOthers.Location.X, pointOthers.Location.Y, 0);

                                if (point == pointOthers)
                                    continue;
                                else
                                {
                                    Vector3D outVec = pointVec.CrossProduct(pointOtherVec);

                                    if (outVec.DotProduct(prevVector) < 0)
                                    {
                                        isConCave = true;
                                        break;
                                    }

                                    prevVector = outVec;
                                }
                            }

                            if (!isConCave) // 장애물이 오목다각형이며, 선박이 그 내에 위치하는 경우 추가 알고리즘 필요..
                                minmaxPoint.Add(ob.Points.IndexOf(point));
                        }

                        min = minmaxPoint[0];
                        max = minmaxPoint[1];
                    }

                    int small;// = (max > min) ? min : max; // 둘 중 작은 index
                    int big;// = (max > min) ? max : min; // 둘 중 큰 index


                    if (max > min)
                    {
                        small = min;
                        big = max;
                    }
                    else
                    {
                        small = max;
                        big = min;
                    }

                    Vector2D pNext = new Vector2D(obPointsRel[small + 1]);


                    Vector2D pPrev = (small == 0) ? new Vector2D(obPointsRel.Last()) : new Vector2D(obPointsRel[small - 1]);
                    Vector2D pSmall = new Vector2D(obPointsRel[small]);

                    Vector2D vecNext = (pNext - pSmall).Normalize();
                    Vector2D vecPrev = (pPrev - pSmall).Normalize();

                    if ((pSmall + vecNext).Length < (pSmall + vecPrev).Length)
                    {
                        for (int i = small; i <= big; i++)
                        {
                            obVisualized.Add(obPointsRel[i]);
                        }
                    }
                    else
                    {
                        for (int i = small; i >= 0; i--)
                        {
                            obVisualized.Add(obPointsRel[i]);
                        }

                        for (int i = obPointsRel.Count - 1; i >= big; i--)
                        {
                            obVisualized.Add(obPointsRel[i]);
                        }
                    }

                    for (int i = 0; i < obVisualized.Count - 1; i++)
                    {
                        PointF pt1 = AzEl2XY(-DicAzimuth[obVisualized[i]] + 90, DicDistance[obVisualized[i]] * scale);
                        PointF pt2 = AzEl2XY(-DicAzimuth[obVisualized[i + 1]] + 90, DicDistance[obVisualized[i + 1]] * scale);

                        Vector2D next = new Vector2D(obVisualized[i + 1]) - new Vector2D(obVisualized[i]);

                        double interval = 5;
                        for (int j = 0; j < next.Length / interval; j++)
                        {
                            Vector2D nextPoint = new Vector2D(obVisualized[i]) + next.Normalize() * interval * j;

                            double distance = nextPoint.Length;
                            double azimuth = Math.Atan2(nextPoint.Y, nextPoint.X) / Math.PI * 180;

                            if (azimuth < 0)
                                azimuth += 360;

                            foreach (TargetShip targetShip in Project.TargetShips)
                            {
                                if (targetShip.IsBehindObstacle) // 이미 다른 장애물 안에 있으면 넘어감
                                    continue;

                                x = targetShip.CurrentX - cx;
                                y = targetShip.CurrentY - cy;

                                double distTarget = Math.Sqrt(x * x + y * y);
                                double bearing = Math.Atan2(y, x) / Math.PI * 180; //degree

                                if (bearing < 0)
                                    bearing += 360;

                                if (!isCrossingBase) // 장애물이 기준 축을 지나지 않으면, bearing이 장애물 내에 있지 않으면 넘어감
                                    if ((DicAzimuth[obPointsRel[small]] - bearing) * (DicAzimuth[obPointsRel[big]] - bearing) > 0)
                                        continue;

                                double error = Math.Abs(bearing - azimuth);
                                if (error > 360)
                                    error -= 360;

                                if (error < 5)
                                    if (distTarget > distance)
                                        targetShip.IsBehindObstacle = true;
                            }

                            //gp.AddCurve(new PointF[] {pt11, pt12});
                            SolidBrush b = new SolidBrush(Color.FromArgb(250, 0, 255,0));
                            Pen p = new Pen(b);
                            p.EndCap = LineCap.Round;

                            // Pen pen=new Pen(b);
                            // pen.Width = 1;
                            //

                            distance += Normal.Sample(r, 0, 5);

                            if (distance > radius)
                                continue;

                            GraphicsPath gp = new GraphicsPath(FillMode.Winding);
                            gp.AddCurve(CreateSignal(90 - (azimuth) - myHeading, distance, 8));
                            //gp.AddCurve(new PointF[] { pt2, q2, pt1 });
                            
                            PathGradientBrush pgb = new PathGradientBrush(gp);
                            pgb.CenterPoint = AzEl2XY(azimuth, distance * scale);
                            pgb.CenterColor = Color.FromArgb(230, 0,255,0);
                            pgb.FocusScales = new PointF(0.7f, 0.7f);
                            pgb.SurroundColors = new Color[] { Color.Empty };
                            
                            gTemp.FillPath(pgb, gp);


                            // double noise = 50 / radius * Math.Sqrt(distance) + resolution / 150.0;
                            // PointF pt11 = AzEl2XY(azimuth - noise, distance * scale);
                            // PointF pt12 = AzEl2XY(azimuth + noise, distance * scale);
                            //
                            // gTemp.DrawLine(pen, pt11, pt12);
                            gTemp.FillClosedCurve(b, CreateSignal(90 - (azimuth) - myHeading, distance));

                            double distance2 = distance;
                            int a = 250;

                            int count = 0;

                            while (true)
                            {
                                if (r.Next() % 3 == 0)
                                    break;

                                //20.0/radius*1000
                                distance2 += Normal.Sample(r, 20.0 / radius * 1000, 5);
                                double azimuth2 = azimuth + Normal.Sample(r, 0, 2);
                                //a -= (int)(Normal.Sample(r, 10, 5));
                                a = (int) (a * Normal.Sample(r, 90, 5));

                                if (a > 255)
                                    a = 255;
                                if (a < 30)
                                    break;

                                if (distance2 > radius)
                                    break;

                                SolidBrush b2 = new SolidBrush(Color.FromArgb(a, 0,255,0));
                                Pen p2 = new Pen(b2);
                                p2.EndCap = LineCap.Round;
                                p2.Width = 5;

                                double breadth = 4 * resolution / 50;
                                //
                                // gp = new GraphicsPath(FillMode.Winding);
                                // gp.AddCurve(CreateSignal(90 - (azimuth) - myHeading, distance2, 2));
                                //
                                // pgb = new PathGradientBrush(gp);
                                // pgb.CenterPoint = AzEl2XY(90 - (azimuth) - myHeading, distance2 * scale);
                                // pgb.CenterColor = Color.FromArgb(a, Color.Green);
                                // pgb.FocusScales = new PointF(0.7f, 0.7f);
                                // pgb.SurroundColors = new Color[] { Color.Empty };
                                //
                                // gTemp.FillPath(pgb, gp);
                                //

                                gTemp.FillClosedCurve(b2, CreateSignal(90 - (azimuth2) - myHeading, distance2, breadth));

                                count++;
                            }

                        }
                    }
                }

                foreach (TargetShip targetShip in Project.TargetShips)
                {
                    //DrawPoint(g, targetShip.CurrentX-cx, targetShip.CurrentY-cy);


                    if (targetShip.IsBehindObstacle) // 장애물 뒤에 있으면 가시화하지 않음
                        continue;

                    double headingRad = (targetShip.Heading - myHeading) / 180.0 * Math.PI;

                    x = targetShip.CurrentX - cx;
                    y = targetShip.CurrentY - cy;

                    double dist = Math.Sqrt(x * x + y * y);
                    if (dist > radius)
                        continue;

                    x1 = x + targetShip.Length * Math.Sin(headingRad);
                    y1 = y + targetShip.Length * Math.Cos(headingRad);

                    x2 = x - targetShip.Length * Math.Sin(headingRad);
                    y2 = y - targetShip.Length * Math.Cos(headingRad);
                    
                    double length = targetShip.Length + resolution;
                    double breadth = length * 0.2;
                    double bearing = Math.Atan(x / y) / Math.PI * 180 - myHeading; //degree
                    double bearing1 = Math.Atan(x1 / y1) / Math.PI * 180 - myHeading; //degree
                    double bearing2 = Math.Atan(x2 / y2) / Math.PI * 180 - myHeading; //degree
                    //
                    //
                    if (Math.Abs(bearing2 - bearing) > 90)
                        if (bearing2 < bearing)
                            bearing2 += 180;
                        else
                            bearing2 -= 180;

                    if (Math.Abs(bearing1 - bearing) > 90)
                        if (bearing1 < bearing)
                            bearing1 += 180;
                        else
                            bearing1 -= 180;

                    if (y < 0)
                    {
                        if (bearing < 0)
                        {
                            bearing += 180;
                            bearing1 += 180;
                            bearing2 += 180;
                        }
                        else
                        {
                            bearing -= 180;
                            bearing1 -= 180;
                            bearing2 -= 180;
                        }
                    }

                    PointF pt = AzEl2XY(bearing, dist * scale);

                    GraphicsPath gp = new GraphicsPath(FillMode.Winding);
                    gp.AddCurve(CreateSignal(bearing1, bearing2, dist, breadth));
                    //gp.AddCurve(new PointF[] { pt2, q2, pt1 });

                    PathGradientBrush pgb = new PathGradientBrush(gp);
                    pgb.CenterPoint = pt;
                    pgb.CenterColor = Color.FromArgb(250, 0,255,0);
                    pgb.FocusScales = new PointF(0.7f, 0.7f);
                    pgb.SurroundColors = new Color[] { Color.Empty };

                    gTemp.FillPath(pgb, gp);
                }

                UpdateImage(image);
            }
        }

        private PointF[] CreateSignal(double azimuth, double distance, double breadth = 0)
        {
            Random r = new Random((int)DateTime.Now.Ticks);

            double radius = Project.MyShips[0].Radius * 1000; //m
            double noise = 50 / radius * Math.Sqrt(distance) + resolution / 150.0;
            double noise2 = noise * 0.5;
            if (breadth != 0)
                noise2 = breadth;
            double scale = width / (radius * 2);

            PointF pt = AzEl2XY(azimuth, distance * scale);
            PointF pt11 = AzEl2XY(azimuth - noise, distance * scale);
            PointF pt115 = AzEl2XY(azimuth, (distance + noise2) * scale);
            PointF pt12 = AzEl2XY(azimuth + noise, distance * scale);
            PointF pt125 = AzEl2XY(azimuth, (distance - noise2) * scale);

            int startAzimuth = (int) ((azimuth - noise) * 10);
            int endAzimuth = (int) (azimuth + noise) * 10;

            if (startAzimuth < 0)
                startAzimuth += 3600;
            if (endAzimuth < 0)
                endAzimuth += 3600;

            if (startAzimuth > endAzimuth)
            {
                int temp;
                temp = endAzimuth;
                endAzimuth = startAzimuth;
                startAzimuth = temp;
            }

            for (int i = startAzimuth; i < endAzimuth; i++)
            {
                for (int j = (int)(RadarSignal.ColumnCount * (distance-noise2) / radius); j < (int)(RadarSignal.ColumnCount * (distance+noise2) / radius); j++)
                {
                    if (j > RadarSignal.ColumnCount-1 || j<0)
                        continue;

                    RadarSignal[i, j] = (Normal.Sample(r, 200, 20) > 255) ? 255 : Normal.Sample(r, 200, 20);
                }
            }

            return new PointF[] {pt11, pt115, pt12, pt125};
        }


        private PointF[] CreateSignal(double azimuth1, double azimuth2, double distance, double breadth)
        {
            Random r = new Random((int)DateTime.Now.Ticks);

            double radius = Project.MyShips[0].Radius * 1000; //m
            double noise = 50 / radius * Math.Sqrt(distance) + resolution / 150.0;
            double scale = width / (radius * 2);

            noise = (azimuth1 < azimuth2) ? noise : -noise;

            double azimuth = (azimuth1 + azimuth2) * 0.5;

            PointF pt11 = AzEl2XY(azimuth1 - noise, (distance + breadth * 0.5) * scale);
            PointF pt12 = AzEl2XY(azimuth1 - noise, (distance - breadth * 0.5) * scale);
            PointF pt21 = AzEl2XY(azimuth2 + noise, (distance + breadth * 0.5) * scale);
            PointF pt22 = AzEl2XY(azimuth2 + noise, (distance - breadth * 0.5) * scale);
            PointF pt = AzEl2XY(azimuth, distance * scale);
            PointF q1 = AzEl2XY(azimuth, (distance + breadth) * scale);
            PointF q2 = AzEl2XY(azimuth, (distance - breadth) * scale);

            int startAzimuth = (int)((azimuth1 - noise) * 10);
            int endAzimuth = (int)(azimuth2 + noise) * 10;

            if (startAzimuth < 0)
                startAzimuth += 3600;
            if (endAzimuth < 0)
                endAzimuth += 3600;

            if (startAzimuth > endAzimuth)
            {
                int temp;
                temp = endAzimuth;
                endAzimuth = startAzimuth;
                startAzimuth = temp;
            }

            for (int i = startAzimuth; i < endAzimuth; i++)
            {
                for (int j = (int)(RadarSignal.ColumnCount * (distance - breadth) / radius); j < (int)(RadarSignal.ColumnCount * (distance + breadth) / radius); j++)
                {
                    if (j > RadarSignal.ColumnCount - 1 || j < 0)
                        continue;

                    RadarSignal[i, j] = (Normal.Sample(r, 200, 20) > 255) ? 255 : Normal.Sample(r, 200, 20);
                }
            }
            
             return new PointF[] { pt11, q1, pt21, pt22, q2, pt12, pt11 };
        }

        internal void SaveImage(MemoryStream memoryStream)
        {
            if (radarBitmap == null)
                return;

            //get image dimension
            int width = radarBitmap.Width;
            int height = radarBitmap.Height;

            //color of pixel
            Color p;

            //grayscale
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    //get pixel value
                    p = radarBitmap.GetPixel(x, y);

                    //extract pixel component ARGB
                    int a = p.A;
                    int r = p.R;
                    int g = p.G;
                    int b = p.B;

                    //find average
                    int avg = (r + g + b) / 3;

                    //set new pixel value
                    radarBitmap.SetPixel(x, y, Color.FromArgb(a, avg, avg, avg));
                }
            }

            radarBitmap.Save(memoryStream, ImageFormat.Jpeg);
            //radarBitmap.Save(memoryStream, ImageFormat.Bmp);
        }

        public void UpdateImage(Bitmap image)
        {
            
            try
            {
                //lock (lockObject)
                //{
                    ViewCore.UpdateImage(image);
                    radarBitmap = image;                    
                //}
            }
            catch (Exception e)
            {
                return;
            }
        }

        public void Reset()
        {
            //g.Clear(Color.Black);

            Load();
        }

        private PointF AzEl2XY(double azimuth, double distance)
        {
            // rotate coords... 90deg W = 180deg trig
            double angle = 90 - azimuth;

            // turn into radians
            angle *= 0.0174532925d;

            double x, y;

            // determine the length of the radius

            double dx = distance * Math.Cos(angle);
            double dy = -distance * Math.Sin(angle);

            x = ((double)marginX + width * 0.5d) + dx;
            y = ((double)marginY + height * 0.5d) + dy;

            return new PointF((float)x, (float)y);
        }

        private PointF Pos2XY(double x, double y)
        {
            double radius = Project.MyShips[0].Radius * 1000; //m

            double scaleX = width / (2*radius);
            double scaleY = height / (2*radius);

            float pX = (float)(marginX + width *0.5 + x * scaleX);
            float pY = (float)(marginY + height *0.5- y * scaleY);

            return new PointF(pX, pY);
        }

        private void DrawPoint(Graphics g, double x, double y)
        {
            SolidBrush b = new SolidBrush(Color.FromArgb(200, Color.Green));
            PointF pt=Pos2XY(x,y);
            int size = (int) ((width / 2) * (double)resolution / (double)Project.MyShips[0].Radius/1000.0);

            if (size < 1)
                size = 1;

            g.FillEllipse(b, pt.X- size, pt.Y- size, size*2, size*2);
        }

    }
}