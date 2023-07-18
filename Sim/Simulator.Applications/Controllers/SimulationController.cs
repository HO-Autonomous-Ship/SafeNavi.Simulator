using MathNet.Spatial.Euclidean;
//using System.Windows.Media.Media3D;
using SyDLab.Usv.Simulator.Applications.Services;
using SyDLab.Usv.Simulator.Applications.ViewModels;
using SyDLab.Usv.Simulator.Domain;
using SyDLab.Usv.Simulator.Domain.Comm;
using SyDLab.Usv.Simulator.Domain.Logs;
using SyDLab.Usv.Simulator.Domain.Models;
using SyDLab.Usv.Simulator.Domain.Models.Platforms;
using SyDLab.Usv.Simulator.Domain.Models.Tasks;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Waf.Applications;
using SyDLab.Usv.Simulator.Domain.Utils;
using NmeaParser;

namespace SyDLab.Usv.Simulator.Applications.Controllers
{
    [Export]
    public class SimulationController
    {
        private readonly DelegateCommand _commandPlay;
        private readonly DelegateCommand _commandStop;
        private readonly DelegateCommand _commandPause;
        private readonly DelegateCommand _commandConnect;
        private readonly DelegateCommand _commandSetReference;
        private readonly IDispatcherService _dispatcherService;
        private readonly BackgroundWorker _playWorker;
        private readonly BackgroundWorker _playWorkerRadar;
        private readonly BackgroundWorker _playWorkerNetwork;
        private readonly BackgroundWorker _playWorkerUdpRecv;
        private readonly BackgroundWorker _playWorkerRecvSensors;
        private readonly ShellViewModel _shellViewModel;
        private readonly GraphViewModel _graphViewModel;
        private readonly RadarViewModel _radarViewModel;
        private readonly SettingViewModel _settingViewModel;
        private readonly MapViewModel _mapViewModel;
        private readonly MonitorViewModel _monitorViewModel;
        private double marginDistance = 600;
        private ulong _currentStep = 0;
        public bool isPaused = true;
        public TcpClient client;
        public NetworkStream ns;
        // public TcpListener server;
        public NMEASender NmeaSender = new NMEASender();
        public NMEASenderUdp _nmeaSenderUdp = new NMEASenderUdp();
        private readonly UdpSocketClient _udpSocketClient = new UdpSocketClient();
        private double _simualationSpeed=1;
        private ObservableCollection<string> _sentenceList = new ObservableCollection<string>();
        private bool isGotPosition = false;

        public double SimualationSpeed
        {
            get => _simualationSpeed;
            set => _simualationSpeed = value;
        }

        [ImportingConstructor]
        public SimulationController(
            IDispatcherService dispatcherService,
            ShellViewModel shellViewModel, MapViewModel mapViewModel, GraphViewModel graphViewModel, RadarViewModel radarViewModel, SettingViewModel settingViewModel, MonitorViewModel monitorViewModel)
            //ShellViewModel shellViewModel, MapViewModel mapViewModel, GraphViewModel graphViewModel, SettingViewModel settingViewModel, MonitorViewModel monitorViewModel)
        {
            _dispatcherService = dispatcherService;
            _shellViewModel = shellViewModel;
            _mapViewModel = mapViewModel;
            _graphViewModel = graphViewModel;
            _radarViewModel = radarViewModel;
            _settingViewModel = settingViewModel;
            _monitorViewModel = monitorViewModel;

            //
            // Create commands.
            //
            _commandPlay = new DelegateCommand(ExecutePlay, CanExecutePlay);
            _commandPause = new DelegateCommand(ExecutePause, CanExecutePause);
            _commandStop = new DelegateCommand(ExecuteStop, CanExecuteStop);
            _commandConnect = new DelegateCommand(ExecuteConnect);
            _commandSetReference = new DelegateCommand(ExecuteSetReference);

            //
            // Background Worker (main simulation)
            //
            _playWorker = new BackgroundWorker();
            _playWorker.WorkerReportsProgress = true;
            _playWorker.WorkerSupportsCancellation = true;
            _playWorker.DoWork += playWorker_DoWork;
            _playWorker.ProgressChanged += playWorker_ProgressChanged;
            _playWorker.RunWorkerCompleted += playWorker_RunWorkerCompleted;
            
            //
            // Background Worker (network : send out sa data to ca)
            //
            _playWorkerNetwork = new BackgroundWorker();
            _playWorkerNetwork.WorkerReportsProgress = true;
            _playWorkerNetwork.WorkerSupportsCancellation = true;
            _playWorkerNetwork.DoWork += connect_DoWork;

            //
            // Background Worker Radar
            //
            _playWorkerRadar = new BackgroundWorker();
            _playWorkerRadar.WorkerReportsProgress = true;
            _playWorkerRadar.WorkerSupportsCancellation = true;
            _playWorkerRadar.DoWork += _playWorkerRadar_DoWork;
            _playWorkerRadar.ProgressChanged += playWorker_ProgressChanged;
            _playWorkerRadar.RunWorkerCompleted += playWorker_RunWorkerCompleted;

            //
            // Background Worker Recv. Command from External Controller
            //
            _playWorkerUdpRecv = new BackgroundWorker();
            _playWorkerUdpRecv.WorkerReportsProgress = true;
            _playWorkerUdpRecv.WorkerSupportsCancellation = true;
            _playWorkerUdpRecv.DoWork += updRecvConnect_DoWork;
            _playWorkerUdpRecv.ProgressChanged += playWorker_ProgressChanged;
            _playWorkerUdpRecv.RunWorkerCompleted += playWorker_RunWorkerCompleted;

            //
            // Background Worker Recv. Sensors (External source)
            //
            _playWorkerRecvSensors = new BackgroundWorker();
            _playWorkerRecvSensors.WorkerReportsProgress = true;
            _playWorkerRecvSensors.WorkerSupportsCancellation = true;
            _playWorkerRecvSensors.DoWork += updRecvSensorsConnect_DoWork;
            _playWorkerRecvSensors.ProgressChanged += playWorker_ProgressChanged;
            _playWorkerRecvSensors.RunWorkerCompleted += playWorker_RunWorkerCompleted;


            //TimeStep = 0.1;
            TimeStep = 1;

            Project.TargetShips.CollectionChanged += TargetShipAdded;
            Project.MyShips.CollectionChanged += MyShipAdded;
            _sentenceList.CollectionChanged += SentenceCollectionChanged;
        }



        private void _playWorkerRadar_DoWork(object sender, DoWorkEventArgs e)
        {
            double time = _shellViewModel.CurrentTime;
            while (true)
            {
                if (_playWorkerRadar.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }
                if (_shellViewModel.CurrentTime - time < 4)
                    continue;
                time = _shellViewModel.CurrentTime;

                
                //SendImageData();  //via TCP
                SendRadarImage();   //via UDP
            }
        }

        private void connect_DoWork(object sender, DoWorkEventArgs e)
        {
            var sw = new Stopwatch();

            while (true)
            {
                sw.Restart();

                #region UDP-Comm : Send out data

                try
                {
                    if (Project != null && _settingViewModel.IsSendout2Ca)
                    {
                        ST_DATA_SA_OUT stDataCrOut = DataManager.RetrieveData(new ST_DATA_SA_OUT());
                        byte[] sendPacket0 = new byte[0];
                        stDataCrOut.Serialize(ref sendPacket0);
                        _udpSocketClient.SendDataSaOut(sendPacket0);


                        // ST_DATA_NM_OUT stDataNmOut = DataManager.RetrieveData(new ST_DATA_NM_OUT());
                        // byte[] sendPacket = new byte[0];
                        // stDataNmOut.Serialize(ref sendPacket);
                        // _udpSocketClient.SendData(sendPacket);
                        // _udpSocketClient.SendData3(sendPacket);
                    }
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception);
                    throw;
                }


                #endregion
            }
        }

        private void updRecvConnect_DoWork(object sender, DoWorkEventArgs e)
        {
            // var sw = new Stopwatch();

            while (true)
            {
                // sw.Restart();

                try
                {
                    if (_shellViewModel.ExternalControllerChecked)
                    {
                        int offset = 0;
                        if (_settingViewModel.IsCmdFromNm)
                        {
                            var recvPacket = _udpSocketClient?.RecvUdpData2(Common.SIZE_DATA_REMOTE_COMMAND);
                            ST_DATA_REMOTE_COMMAND response = new ST_DATA_REMOTE_COMMAND();
                            response.Deserialize(recvPacket, ref offset);
                            DataManager.DeployData(response);
                        }
                        else
                        {
                            // var recvPacket = _udpSocketClient?.RecvUdpData2(Common.SIZE_ST_CA_COMMAND);
                            // ST_CA_COMMAND response = new ST_CA_COMMAND();
                            // response.Deserialize(recvPacket, ref offset);
                            // DataManager.DeployData_StCaCom(response);
                        }
                        
                    }
                }
                catch (SocketException se)
                {
                    _dispatcherService.Invoke(() =>
                    {
                        LogManager.AddLog("No connection with udp client...");
                        //LogManager.AddLog(se.Message);
                    });
                }
                
                // sw.Stop();
                //
                // var waitDuration = Math.Max(0, Common.SIM_COMM_OUT_TIMER_INTERVAL_MS - (int)sw.ElapsedMilliseconds);
                // if (waitDuration != 0)
                //     System.Threading.Thread.Sleep(waitDuration);
            }
        }

        // UdpInPortRecvSensor
        private void updRecvSensorsConnect_DoWork(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                try
                {
                    if (_settingViewModel.IsRecvSensors)
                    {
                        byte[] buff = new byte[3000];
                        if (_udpSocketClient.RecvSensorData(ref buff))
                        {

                            if (_shellViewModel.ConnectedVdmChecked)
                            {
                                string str = Encoding.ASCII.GetString(buff);
                                if (str.Contains("VDM"))
                                    _udpSocketClient.SendUdpData(buff);
                            }
                            
                            AddBuffer(buff);    //checksum 확인 후, 리스트에 등록
                        }
                    }
                }
                catch (SocketException se)
                {
                    _dispatcherService.Invoke(() =>
                    {
                        LogManager.AddLog("Failed to recv sensors...");
                        //LogManager.AddLog(se.Message);
                    });
                }
            }
        }

        private void AddBuffer(byte[] buff)
        {
            string stringLine = Encoding.ASCII.GetString(buff);
            stringLine = stringLine.TrimEnd();
            stringLine = stringLine.TrimEnd('\0');
            string[] separators = { "\r\n\r\n", "\r\n", "\0" };
            string[] sentences = stringLine.Split(separators, StringSplitOptions.RemoveEmptyEntries);

            foreach (var sentence in sentences)     
            {
                if (!DefineFunction.CheckSum(sentence))
                    continue;

                _sentenceList.Add(sentence);
            }
        }

        private void SentenceCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                {
                    foreach (string sentence in e.NewItems)
                    {
                        Parser.StringParser(sentence, out SensorData parsedData);

                        if (parsedData == null)
                            continue;

                        if (parsedData.SentenceId == SentenceID.VDO)
                        {
                            if (!_shellViewModel.ConnectedVdoChecked)
                                continue;

                            // myship
                            Project.MyShips[0].CurrentLat = parsedData.Lat;
                            Project.MyShips[0].CurrentLon = parsedData.Lon;
                            Project.MyShips[0].CurrentSpeed = parsedData.Sog;
                            Project.MyShips[0].CurrentHeading = parsedData.Cog;
                            Point3D osPos = Distance.GetPosition(
                                new Microsoft.Maps.MapControl.WPF.Location(parsedData.Lat, parsedData.Lon));
                            Project.MyShips[0].CurrentX = osPos.X;
                            Project.MyShips[0].CurrentY = osPos.Y;
                            isGotPosition = false;
                        }
                        else if (parsedData.SentenceId == SentenceID.VDM)
                        {
                            if (!_shellViewModel.ConnectedVdmChecked)
                                    continue;

                            // traffic ship
                            _dispatcherService.Invoke(() =>
                            {
                                List<TargetShip> targetShips = Project.TargetShips.ToList();
                                if (targetShips.All(i => i.MMSI != parsedData.Mmsi))
                                {
                                    var newShip = new TargetShip(Project)
                                    {
                                        CurrentLat = parsedData.Lat,
                                        CurrentLon = parsedData.Lon,
                                        CurrentHeading = parsedData.Cog,
                                        CurrentSpeed = parsedData.Sog,
                                        DisplayName = string.Format("{0}", parsedData.Mmsi),
                                        MMSI = (int)parsedData.Mmsi
                                    };
                                    Point3D newPos = Distance.GetPosition(
                                        new Microsoft.Maps.MapControl.WPF.Location(parsedData.Lat, parsedData.Lon));
                                    newShip.CurrentX = newPos.X;
                                    newShip.CurrentY = newPos.Y;
                                    Project.TargetShips.Add(newShip);
                                    _mapViewModel.UpdateMarkerPosition(newShip, newShip.CurrentX, newShip.CurrentY);
                                }
                                else
                                {
                                    int index = targetShips.FindIndex(i => i.MMSI == parsedData.Mmsi);
                                    Project.TargetShips[index].CurrentLat = parsedData.Lat;
                                    Project.TargetShips[index].CurrentLon = parsedData.Lon;
                                    Project.TargetShips[index].CurrentHeading = parsedData.Cog;
                                    Project.TargetShips[index].CurrentSpeed = parsedData.Sog;
                                    Point3D newPos = Distance.GetPosition(
                                        new Microsoft.Maps.MapControl.WPF.Location(parsedData.Lat, parsedData.Lon));
                                    Project.TargetShips[index].CurrentX = newPos.X;
                                    Project.TargetShips[index].CurrentY = newPos.Y;
                                }
 
                            });
                        }
                    }
                } 
                    break;
            }
        }

        private LogManager LogManager => Singleton<LogManager>.UniqueInstance;
        public Project Project => Singleton<Project>.UniqueInstance;

        public DataManager DataManager => Singleton<DataManager>.UniqueInstance;

        public double TimeStep { get; }

        public ulong CurrentStep
        {
            get { return _currentStep; }
            set
            {
                _currentStep = value;
                _shellViewModel.CurrentTime = _currentStep * TimeStep;
            }
        }

        private void playWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _graphViewModel.EndTime = _shellViewModel.CurrentTime;
            _graphViewModel.Initialize();
        }

        private void playWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
        }

        private void playWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var sw = new Stopwatch();

            int cat240Degree = 0;

            while (true)
            {
                sw.Restart();

                if (_playWorker.CancellationPending)
                {
                    e.Cancel = true;
                    break;
                }

                // 시간 증가
                CurrentStep++;
                if (!_shellViewModel.ConnectedVdoChecked)
                {
                    foreach (MyShip myShip in Project.MyShips)
                    {
                        TaskBase task = Project.SelectedScenario.Tasks.FirstOrDefault(tb => tb.Platform == myShip);
                        if (task == null)
                            continue; // task가 없는 경우


                        if (task.Waypoints.Count == 0)
                        {
                            SolveManeuver(myShip);
                            continue;
                        }

                        if (task.TargetWaypoint >= task.Waypoints.Count)
                            continue; // 도착한 경우

                        if (task.TargetWaypoint + 1 == task.Waypoints.Count)
                            marginDistance = 20;
                        else
                            marginDistance = 700;

                        // Start simulation!

                        Waypoint prevPoint;
                        if (task.TargetWaypoint == 0)
                            prevPoint = new Waypoint(0, 0);
                        else
                            prevPoint = task.Waypoints[task.TargetWaypoint - 1];

                        SolveManeuver(myShip, prevPoint, task.Waypoints[task.TargetWaypoint]);

                        // is arrived at waypoint
                        if ((task.Waypoints[task.TargetWaypoint].Location.DistanceTo(new Point2D(myShip.CurrentX, myShip.CurrentY)) - myShip.CurrentSpeed * TimeStep) < marginDistance) // 600 m 근처 왔으면 다음 point 추종
                        {
                            task.TargetWaypoint++;
                        }
                    }
                }

                foreach (TargetShip targetShip in Project.TargetShips.ToList())
                {
                    TaskBase task = Project.SelectedScenario.Tasks.FirstOrDefault(tb => tb.Platform == targetShip);
                    if (task == null)
                    {
                        SolveManeuver(targetShip); // task가 없는 경우
                        continue;
                    }

                    if (task.TargetWaypoint >= task.Waypoints.Count)
                        continue; // 도착한 경우

                    marginDistance = task.TargetWaypoint + 1 == task.Waypoints.Count ? 10 : 100;

                    // Start simulation!
                    Waypoint prevPoint;
                    if (task.TargetWaypoint == 0)
                        prevPoint = new Waypoint(targetShip.InitialX, targetShip.InitialY);
                    else
                        prevPoint = task.Waypoints[task.TargetWaypoint - 1];

                    SolveManeuver(targetShip, prevPoint, task.Waypoints[task.TargetWaypoint]);

                    // is arrived at waypoint

                    if ((task.Waypoints[task.TargetWaypoint].Location.DistanceTo(new Point2D(targetShip.CurrentX, targetShip.CurrentY)) - targetShip.CurrentSpeed * TimeStep) < marginDistance) // 600 m 근처 왔으면 다음 point 추종
                    {
                        task.TargetWaypoint++;
                    }
                }


                _dispatcherService.Invoke(() =>
                {
                    if (Project.MyShips[0] != null)
                        _monitorViewModel.AddValues(Project.MyShips[0]);
                });

                sw.Stop();

                double interval = Project.MyShips[0].Interval;
                bool isTargetUpdated = (((int)CurrentStep * TimeStep * 1000 % interval * 1000) < 1);


                _dispatcherService.Invoke(() =>
                {
                    _radarViewModel.CreateSignal(); // CAT240 data 생성
                    _radarViewModel.UpdateHand(isTargetUpdated);
                });

                //_radarViewModel.RadarSignal
                // 구현중인 부분: CAT240 데이터 생성
                // for (int i = 0; i < 1000; i++)
                // {
                //     NmeaSender.SendCat240(_radarViewModel.RadarSignal, (cat240Degree + i)%3600, ns, client);
                // }
                //
                // cat240Degree += 1000;
                // if (cat240Degree > 3600) { cat240Degree -= 3600; }


                var waitDuration = Math.Max(0, (int)(TimeStep * 1000 * _simualationSpeed) - (int)sw.ElapsedMilliseconds);
                if (waitDuration != 0)
                    Thread.Sleep(waitDuration);
            }
        }


        // target ship 생성 될 시 ais 전송 함수에 연결
        private void TargetShipAdded(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (TargetShip item in e.NewItems)
                    {
                        item.HistoryAISData.CollectionChanged += SendSensorData;
                        //item.HistoryRadarData.CollectionChanged += SendSensorData;
                        //NmeaSender.SendVDM5(item, ns, client);
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (TargetShip item in e.OldItems)
                    {
                        item.HistoryAISData.CollectionChanged -= SendSensorData;
                        //item.HistoryRadarData.CollectionChanged -= SendSensorData;
                    }
                    break;

                default:
                    break;
            }
        }

        #region 이미지 전송 vis Tcp
        private void SendImageData()
        {
            // 시간 먼저 전송
            NmeaSender.SendTime(_shellViewModel.CurrentTime, ns, client);

            object lockObject = new object();

            string sentenceTypeString = "#IMAGE,";

            MemoryStream memoryStream = new MemoryStream();

            try
            {
                lock (lockObject)
                {
                    _radarViewModel.SaveImage(memoryStream);
                }
            }
            catch (Exception e)
            {
                return;
            }

            memoryStream.Position = 0;
            byte[] imageBuffer = memoryStream.ToArray();
            memoryStream.Close();
            string imageBase64 = Convert.ToBase64String(imageBuffer);
            string lengthBase64 = imageBase64.Length + ",";
            // SendImageData 함수에 radar range를 입력하여 NMEA 문장에 보냄
            string radarRangeString = "," + Project.MyShips[0].Radius * 1000;
            byte[] byteBuffer = Encoding.ASCII.GetBytes(sentenceTypeString + lengthBase64 + imageBase64 + radarRangeString);
            if (ns != null && ns.CanWrite && client.Connected)
            {
                try
                {
                    ns.Write(byteBuffer, 0, byteBuffer.Length);
                }
                catch (Exception exception)
                {
                }
            }
        }
        #endregion

        #region Send radar image via Udp
        private void SendRadarImage()
        {
            // 시간 먼저 전송
            _nmeaSenderUdp.SendTime(_shellViewModel.CurrentTime, _udpSocketClient);

            object lockObject = new object();
            string sentenceTypeString = "#IMAGE,";
            MemoryStream memoryStream = new MemoryStream();

            try
            {
                //lock (lockObject)
                //{
                //    _radarViewModel.SaveImage(memoryStream);
                //}
                _dispatcherService.Invoke(() =>
                {
                    _radarViewModel.SaveImage(memoryStream);
                });
            }
            catch (Exception e)
            {
                throw;
            }

            memoryStream.Position = 0;
            byte[] imageBuffer = memoryStream.ToArray();
            memoryStream.Close();
            string imageBase64 = Convert.ToBase64String(imageBuffer);
            string lengthBase64 = imageBase64.Length + ",";
            // SendImageData 함수에 radar range를 입력하여 NMEA 문장에 보냄
            string radarRangeString = "," + Project.MyShips[0].Radius * 1000;
            byte[] byteBuffer = Encoding.ASCII.GetBytes(sentenceTypeString + lengthBase64 + imageBase64 + radarRangeString);
            try
            {
                _udpSocketClient?.SendUdpData(byteBuffer);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
        #endregion

        private void MyShipAdded(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (MyShip item in e.NewItems)
                    {
                        item.HistoryGPSData.CollectionChanged += SendSensorData;
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (MyShip item in e.OldItems)
                    {
                        item.HistoryGPSData.CollectionChanged -= SendSensorData;
                    }
                    break;

                default:
                    break;
            }
        }

        // target ship ais data 전송
        private void SendSensorData(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (PlatformState item in e.NewItems)
                    {
                        // NmeaSender.SendData(item, _shellViewModel.CurrentTime, ns, client);
                        if ( (_shellViewModel.SendoutVdoChecked && item.Platform is MyShip) || (_shellViewModel.SendoutVdmChecked && item.Platform is TargetShip) )
                            _nmeaSenderUdp.SendData(item, _shellViewModel.CurrentTime, _udpSocketClient);
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    break;
                     
                default:
                    break;
            }
        }

        //        public static void UpdateMyShipMMG(PlatformBase ship, double[] setpoints, double dt, bool isSpeedHeadingCtrl)
        private void SolveManeuver(PlatformBase ship, Waypoint wayPoint_prev, Waypoint wayPoint_next)
        {
            if (ship is MyShip)
            {
                double[] setPoints = {ship.HeadingDesired, ship.SpeedDesired};
                //
                // ManeouvringEquation.UpdateMyShip(ship, wayPoint_prev, wayPoint_next, TimeStep);

                if (_shellViewModel.SimpleVersionChecked)
                {
                    double delx = ship.CurrentSpeed * 0.5144 * TimeStep * Math.Sin(ship.CurrentHeading / 180.0 * Math.PI);
                    double dely = ship.CurrentSpeed * 0.5144 * TimeStep * Math.Cos(ship.CurrentHeading / 180.0 * Math.PI);
                    ship.CurrentY += ship.CurrentSpeed * 0.5144 * TimeStep * Math.Cos(ship.CurrentHeading / 180.0 * Math.PI);
                    ship.CurrentX += ship.CurrentSpeed * 0.5144 * TimeStep * Math.Sin(ship.CurrentHeading / 180.0 * Math.PI);
                    ship.CurrentHeading = ship.HeadingDesired;
                    ship.Velocity = new System.Windows.Media.Media3D.Vector3D(ship.CurrentX, ship.CurrentY, 0);
                    ship.AngularVelocity = 0;
                    ship.Rpm = 0;
                    ship.Rudder[0] = 0;
                    ship.Rudder[1] = 0;
                    ship.Speed = Math.Sqrt(Math.Pow(ship.Velocity.X, 2) + Math.Pow(ship.Velocity.Y, 2));  //(knots)
                }
                else
                    ManeouvringEquation.UpdateMyShipMMG(ship, TimeStep, _shellViewModel.ExternalControllerChecked);
            }
                
                
            else if (ship is TargetShip)
            {
                Vector2D distNow = wayPoint_next.Location.ToVector2D() - new Vector2D(ship.CurrentX, ship.CurrentY);

                if (ship.CurrentSpeed + wayPoint_next.Speed < 1e-10)
                    return;

                double ttg = 2 * distNow.Length / (ship.CurrentSpeed + wayPoint_next.Speed);
                distNow = distNow / distNow.Length;
                ship.CurrentSpeed += (wayPoint_next.Speed - ship.CurrentSpeed) / ttg * TimeStep;
                if (ship.CurrentSpeed < 0) ship.CurrentSpeed = 0;

                double bearing = Math.PI * 0.5 - Math.Atan2(distNow.Y, distNow.X);
                double headingDiff = bearing / Math.PI * 180 - ship.CurrentHeading;
                if (headingDiff > 180)
                    headingDiff -= 360;
                else if (headingDiff < -180)
                    headingDiff += 360;

                double headingVel = headingDiff / TimeStep;

                // 사용자가 입력한 max heading velocity에 따라 회전
                if ((ship as TargetShip).IsChangeable)
                {
                    if (Math.Abs(headingVel) > (ship as TargetShip).HeadingVelocity)
                    {
                        if (headingDiff > 0)
                            headingVel = (ship as TargetShip).HeadingVelocity;
                        else
                            headingVel = -(ship as TargetShip).HeadingVelocity;
                    }
                }
                
                ship.CurrentHeading += headingVel * TimeStep;

                ship.CurrentX += Math.Sin(ship.CurrentHeading / 180 * Math.PI) * ship.CurrentSpeed * 0.5144 * TimeStep;
                ship.CurrentY += Math.Cos(ship.CurrentHeading / 180 * Math.PI) * ship.CurrentSpeed * 0.5144 * TimeStep;
                //ship.CurrentHeading = bearing / Math.PI * 180;
            }

            _dispatcherService.Invoke(() =>
             {
                 if (_mapViewModel.IsHeadingFixed)
                     ship.VisualizedHeading = 0;
                 else
                     ship.VisualizedHeading = ship.CurrentHeading;
             });

            ship.SaveState(_shellViewModel.CurrentTime);
        }

        private void SolveManeuver(PlatformBase ship)
        {
            if (ship is MyShip)
            {

                // double[] setPoints = { ship.RpmDesired, ship.RudderDesired };
                // if (_shellViewModel.ExternalControllerChecked)
                // {
                //     setPoints = new double[] { ship.RpmDesired, ship.RudderDesired };
                // }    
                // ManeouvringEquation.UpdateMyShipMMG(ship, setPoints, TimeStep, _shellViewModel.ExternalControllerChecked);
                if (_shellViewModel.SimpleVersionChecked)
                {
                    double delx = ship.CurrentSpeed * 0.5144 * TimeStep * Math.Sin(ship.CurrentHeading / 180.0 * Math.PI);
                    double dely = ship.CurrentSpeed * 0.5144 * TimeStep * Math.Cos(ship.CurrentHeading / 180.0 * Math.PI);
                    ship.CurrentY += ship.CurrentSpeed * 0.5144 * TimeStep * Math.Cos(ship.CurrentHeading / 180.0 * Math.PI); 
                    ship.CurrentX += ship.CurrentSpeed * 0.5144 * TimeStep * Math.Sin(ship.CurrentHeading / 180.0 * Math.PI);
                    ship.CurrentHeading = ship.HeadingDesired;
                    ship.Velocity = new System.Windows.Media.Media3D.Vector3D(ship.CurrentX, ship.CurrentY, 0);
                    ship.AngularVelocity = 0;
                    ship.Rpm = 0;
                    ship.Rudder[0] = 0;
                    ship.Rudder[1] = 0;
                    ship.Speed = Math.Sqrt(Math.Pow(ship.Velocity.X, 2) + Math.Pow(ship.Velocity.Y, 2));  //(knots)
                }
                else
                    ManeouvringEquation.UpdateMyShipMMG(ship, TimeStep, _shellViewModel.ExternalControllerChecked);
            }

            if (ship is TargetShip tship)
            {
                // var tship = ship as TargetShip;
                // Waypoint 없는 경우 임의로 운동

                if (tship.IsChangeable)
                {
                    if ((int)(CurrentStep * TimeStep) % 60 == 1) // 60초에 한번씩 desired heading 바꾸기
                    {
                        Random ran = new Random((int)DateTime.Now.Ticks);
                        int num = ran.Next();

                        if (num % 5 == 0)
                            tship.DesiredHeading = tship.CurrentHeading + 30;
                        else if (num % 5 == 1)
                            tship.DesiredHeading = tship.CurrentHeading - 30;
                        else
                            tship.DesiredHeading = tship.CurrentHeading;

                        num = ran.Next();

                        if (num % 10 == 0)
                            tship.DesiredSpeed = tship.CurrentSpeed + 2;
                        else if (num % 10 == 1)
                            tship.DesiredSpeed = tship.CurrentSpeed - 2;
                        else
                            tship.DesiredSpeed = tship.CurrentSpeed;

                        if (tship.DesiredSpeed < 0)
                            tship.DesiredSpeed = 0;
                        else if (tship.DesiredSpeed > 12)
                            tship.DesiredSpeed = 12;
                    }

                    if (Math.Abs(tship.DesiredHeading - tship.CurrentHeading) < 1e-2)
                        ;
                    else if (tship.DesiredHeading > tship.CurrentHeading)
                        tship.CurrentHeading += tship.HeadingVelocity * TimeStep; // 1초에 0.5도씩 틀도록 설정
                    else if (tship.DesiredHeading < tship.CurrentHeading)
                        tship.CurrentHeading -= tship.HeadingVelocity * TimeStep;

                    if (Math.Abs(tship.DesiredSpeed - tship.CurrentSpeed) < 1e-2)
                        ;
                    else if (tship.DesiredSpeed > tship.CurrentSpeed)
                        tship.CurrentSpeed += 0.1 * TimeStep; // 1초에 0.1 m/s씩 변하도록 설정
                    else if (tship.DesiredSpeed < tship.CurrentSpeed)
                        tship.CurrentSpeed -= 0.1 * TimeStep;
                }

                double delx = tship.CurrentSpeed * 0.5144 * TimeStep * Math.Sin(tship.CurrentHeading / 180.0 * Math.PI);
                double dely = tship.CurrentSpeed * 0.5144 * TimeStep * Math.Cos(tship.CurrentHeading / 180.0 * Math.PI);

                tship.CurrentX += delx;
                tship.CurrentY += dely;

                _dispatcherService.Invoke(() =>
                {
                    if (_mapViewModel.IsHeadingFixed)
                        tship.VisualizedHeading = tship.CurrentHeading - Project.MyShips[0].CurrentHeading;
                    else
                        (ship as TargetShip).VisualizedHeading = tship.CurrentHeading;
                });

                tship.SaveState(_shellViewModel.CurrentTime);
            }
        }

        public void Initialize()
        {
            _shellViewModel.MaxTime = Project.MaxTime;

            //
            // Connect commands to view model.
            //
            _shellViewModel.CommandPlay = _commandPlay;
            _shellViewModel.CommandPause = _commandPause;
            _shellViewModel.CommandStop = _commandStop;
            _shellViewModel.CommandConnect = _commandConnect;
            _shellViewModel.CommandSetReference = _commandSetReference;

            // Event
            //_mapViewModel.PropertyChanged += _mapViewModel_PropertyChanged;
        }

        private void ExecuteConnect()
        {
            // UDP (Recv.) start
            // IPAddress udpInipAddress = IPAddress.Parse(_settingViewModel.UdpInIp);
            int udpInport = _settingViewModel.UdpInPort;
            int udpInport2 = _settingViewModel.UdpInPort2;
            IPAddress udpOutIpAddress = IPAddress.Parse(_settingViewModel.UdpOutIp);
            int udpOutport = _settingViewModel.UdpOutPort;
            // TCP
            IPAddress ipAddress = IPAddress.Parse(_settingViewModel.TcpOutIp);
            int port = _settingViewModel.TcpOutPort;

            // Udp connection for Nmea
            if (_shellViewModel.IsConnected)
            {
                _udpSocketClient?.Stop("");

                _dispatcherService.Invoke(() =>
                {
                    _shellViewModel.IsConnected = false;
                    LogManager.AddLog("Udp comm. stopped.");
                });

                if (_playWorkerUdpRecv.IsBusy && _playWorkerUdpRecv.WorkerSupportsCancellation)
                    _playWorkerUdpRecv.CancelAsync();


                if (_playWorkerNetwork.IsBusy && _playWorkerNetwork.WorkerSupportsCancellation)
                    _playWorkerNetwork.CancelAsync();

                if (_playWorkerRecvSensors.IsBusy && _playWorkerRecvSensors.WorkerSupportsCancellation)
                    _playWorkerRecvSensors.CancelAsync();

                if (_playWorkerRadar.IsBusy && _playWorkerRadar.WorkerSupportsCancellation)
                    _playWorkerRadar.CancelAsync();
            }
            else
            {
                _udpSocketClient.Start(udpInport, udpInport2, udpOutIpAddress, udpOutport);

                _dispatcherService.Invoke(() =>
                {
                    _shellViewModel.IsConnected = true;
                    LogManager.AddLog("Udp comm. started...");
                });
                
                //Run thread
                if (!_playWorkerUdpRecv.IsBusy)
                    _playWorkerUdpRecv.RunWorkerAsync();
                
                if (!_playWorkerNetwork.IsBusy)
                    _playWorkerNetwork.RunWorkerAsync();

                if (!_playWorkerRecvSensors.IsBusy && _settingViewModel.IsRecvSensors)
                    _playWorkerRecvSensors.RunWorkerAsync();

                if (!_playWorkerRadar.IsBusy)
                    _playWorkerRadar.RunWorkerAsync();
            }


            // Tcp connection for sending Radar image
            if (_settingViewModel.IsTcpConnection)
            {
                if (_shellViewModel.IsTcpServerConnected)
                {
                    // server?.Stop();
                    client?.Close();
                    ns?.Close();

                    _dispatcherService.Invoke(() =>
                    {
                        _shellViewModel.IsTcpServerConnected = false;
                        LogManager.AddLog("Tcp server stopped...");
                    });

                    if (_playWorkerRadar.IsBusy && _playWorkerRadar.WorkerSupportsCancellation)
                        _playWorkerRadar.CancelAsync();
                }
                // else if (server != null)
                // {
                // }
                else
                {
                    try
                    {
                        // TCP connection
                        client = new TcpClient(ipAddress.ToString(), port);
                        ns = client.GetStream();

                        _dispatcherService.Invoke(() =>
                        {
                            _shellViewModel.IsTcpServerConnected = true;
                            LogManager.AddLog("Tcp server started...");
                        });

                        // //Run thread
                        // if (!_playWorkerRadar.IsBusy)
                        //     _playWorkerRadar.RunWorkerAsync();
                    }
                    catch (Exception e)
                    {
                        _dispatcherService.Invoke(() =>
                        {
                            _shellViewModel.IsTcpServerConnected = false;
                            LogManager.AddLog(e.ToString());
                        });
                    }
                }
            }
        }

        private bool CanExecutePause()
        {
            if (!isPaused)
                return true;

            _settingViewModel.SelectVesselEnable = true;
            return false;
        }

        public void ExecutePause()
        {
            if (_playWorker.IsBusy && _playWorker.WorkerSupportsCancellation)
                _playWorker.CancelAsync();

            isPaused = true;

            _commandPlay.RaiseCanExecuteChanged();
            _commandStop.RaiseCanExecuteChanged();
            _commandPause.RaiseCanExecuteChanged();

            // radar 이미지 전송 중지
            if (_playWorkerRadar.WorkerSupportsCancellation)
                _playWorkerRadar.CancelAsync();
        }

        private bool CanExecuteStop()
        {
            if (isPaused && CurrentStep > 0)
                return true;
  
            _settingViewModel.SelectVesselEnable = true;
            return false;
        }

        private void ExecuteStop()
        {
            CurrentStep = 0;
            _shellViewModel.CurrentTime = 0;

            // USV 위치 초기화 코드 추가
            foreach (TaskBase task in Project.SelectedScenario.Tasks)
            {
                task.TargetWaypoint = 0;
                task.UpdatePlatform();
                _mapViewModel.DeleteRoute(task.Platform);
            }

            foreach (MyShip myShip in Project.MyShips)
            {
                myShip.HistoryPosition.Clear();
                double u = myShip.Speed * Math.Cos(myShip.InitialHeading * Distance.D2R);
                double v = myShip.Speed * Math.Sin(myShip.InitialHeading * Distance.D2R);
                myShip.Velocity = new System.Windows.Media.Media3D.Vector3D(u, v, 0);
                myShip.Speed = Math.Sqrt(Math.Pow(myShip.Velocity.X, 2) + Math.Pow(myShip.Velocity.Y, 2));
                myShip.CurrentHeading = myShip.InitialHeading;
                myShip.HeadingDesired = myShip.InitialHeading;
                
                myShip.AngularVelocity = 0;
                myShip.Rudder = new double[2];
                myShip.TempVar = new double[3];
                _mapViewModel.DeleteRoute(myShip);
                if (_mapViewModel.IsHeadingFixed)
                    _mapViewModel.UpdateBearing(myShip.CurrentHeading);
                else
                    _mapViewModel.UpdateBearing(0);

                myShip.CurrentX = 0;
                myShip.CurrentY = 0;
            }
            foreach (TargetShip targetShip in Project.TargetShips)
            {
                targetShip.CurrentX = targetShip.InitialX;
                targetShip.CurrentY = targetShip.InitialY;
                targetShip.CurrentHeading = targetShip.InitialHeading;
                targetShip.CurrentSpeed = targetShip.Speed;
                targetShip.HistoryPosition.Clear();
                targetShip.HistoryAISData.Clear();
                targetShip.HistoryRadarData.Clear();
                targetShip.HistoryFusedData.Clear();
                Project.MyShips[0].HistoryGPSData.Clear();

                _mapViewModel.DeleteRoute(targetShip);
            }

            // _radarViewModel.Reset();
            _commandStop.RaiseCanExecuteChanged();

            _monitorViewModel.PlotModelClear();
            _monitorViewModel.Initialize();
        }

        private bool CanExecutePlay()
        {
            if (isPaused)
                return true;

            _settingViewModel.SelectVesselEnable = false;
            return false;
        }

        private void ExecutePlay()
        {
            if (Project.SelectedScenario == null)
            {
                LogManager.AddLog("Select Scenario.");
                return;
            }

            isPaused = false;
            _commandPause.RaiseCanExecuteChanged();
            _commandStop.RaiseCanExecuteChanged();

            if (CurrentStep == 0)
            {
            }

            // play
            CurrentStep = 0;

            //_radarViewModel.Load();
            _playWorker.RunWorkerAsync();
            _commandPlay.RaiseCanExecuteChanged();

            // // radar 이미지 전송
            // _playWorkerRadar.RunWorkerAsync();
        }

        private void ExecuteSetReference()
        {
            // MyShip myShip = Project.MyShips[0];
            //
            // Project.ReferenceLat = Project.MyShips[0].CurrentLat;
            // Project.ReferenceLon = Project.MyShips[0].CurrentLon;
            //
            // Project.ReferenceLatLng = new Point2D(Project.MyShips[0].CurrentLat, Project.MyShips[0].CurrentLon);
            //
            // myShip.CurrentX = 0;
            // myShip.CurrentY = 0;

            MyShip myShip = Project.MyShips[0];

            myShip.InitialLat = myShip.CurrentLat;
            myShip.InitialLon = myShip.CurrentLon;
        }


        public void ThreadShutDown()
        {
            if (_playWorker?.WorkerSupportsCancellation == true)
            {
                //스레드 취소(종료), 이 메소드를 호출하면 CancellationPending 속성이 true로 set됩니다.
                _playWorker.CancelAsync();
            }
            if (_playWorkerRadar?.WorkerSupportsCancellation == true)
            {
                //스레드 취소(종료), 이 메소드를 호출하면 CancellationPending 속성이 true로 set됩니다.
                _playWorkerRadar.CancelAsync();
            }
            //if (_playWorkerNetwork?.WorkerSupportsCancellation == true)
            //{
            //    //스레드 취소(종료), 이 메소드를 호출하면 CancellationPending 속성이 true로 set됩니다.
            //    _playWorkerNetwork.CancelAsync();
            //}
        }
    }
}