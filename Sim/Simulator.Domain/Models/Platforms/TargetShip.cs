using System;
using System.Collections.ObjectModel;
using MathNet.Spatial.Euclidean;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Windows;
using MathNet.Spatial.Units;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Financial;
using MathNet.Numerics.Random;
using Microsoft.Maps.MapControl.WPF;

namespace SyDLab.Usv.Simulator.Domain.Models.Platforms
{
    public class TargetShip : PlatformBase
    {
        [DataMember] private double _rangeDistance;

        private Point3D _initialPosition;

        private Angle _initialHeading;

        [DataMember] private bool _isAISExists = true;
        [DataMember] private bool _isChangeable = false;

        [DataMember] private double _stdDev = 10;

        [DataMember] private ObservableCollection<PlatformState> _historyAISData = new ObservableCollection<PlatformState>();
        [DataMember] private ObservableCollection<PlatformState> _historyRadarData = new ObservableCollection<PlatformState>();
        [DataMember] private ObservableCollection<PlatformState> _historyFusedData = new ObservableCollection<PlatformState>();

        [DataMember] private double _length;
        [DataMember] private double _desiredHeading;
        [DataMember] private double _angularVelocity;
        [DataMember] private int _mmsi;

        private double _initialX;
        private double _initialY;
        private double _initialLon;
        private double _initialLat;
        private bool _isBehindObstacle = false;

        public double DesiredSpeed;

        public TargetShip(Project project) : base(project)
        {
        }

        [Category("ID")]
        public int MMSI
        {
            get => _mmsi;
            set => SetProperty(ref _mmsi, value);
        }

        [DataMember]
        [Category("Initial Status")]
        [DisplayName("Heading [°]")]
        public double InitialHeading
        {
            get => _initialHeading.Degrees;
            set
            {
                value = value % 360;
                SetProperty(ref _initialHeading, Angle.FromDegrees(value));
                CurrentHeading = value;
                RaisePropertyChanged(nameof(Heading));
                RaisePropertyChanged(nameof(VisualizedHeading));
            }
        }

        [DataMember]
        [Category("Initial Status")]
        [DisplayName("X [m]")]
        public double InitialY
        {
            get => _initialPosition.Y;
            set
            {
                SetProperty(ref _initialPosition, new Point3D(_initialPosition.X, value, _initialPosition.Z));
                Location location = Distance.GetLatLng(_initialPosition);

                if (Math.Abs(InitialLon - location.Longitude) > 1e-7)
                    InitialLon = location.Longitude;

                if (Math.Abs(InitialLat - location.Latitude) > 1e-7)
                    InitialLat = location.Latitude;

                if (Math.Abs(InitialX - Distance.GetPosition(_initialLonLat).X) > 1e-7 ||
                    Math.Abs(InitialY - Distance.GetPosition(_initialLonLat).Y) > 1e-7)
                    SetProperty(ref _initialLonLat, location);

                CurrentY = value;
            }
        }

        [DataMember]
        [Category("Initial Status")]
        [DisplayName("Y [m]")]
        public double InitialX
        {
            get => _initialPosition.X;
            set
            {
                SetProperty(ref _initialPosition, new Point3D(value, _initialPosition.Y, _initialPosition.Z));
                Location location = Distance.GetLatLng(_initialPosition);

                if (Math.Abs(InitialLon - location.Longitude) > 1e-7)
                    InitialLon = location.Longitude;

                if (Math.Abs(InitialLat - location.Latitude) > 1e-7)
                    InitialLat = location.Latitude;

                if (Math.Abs(InitialX - Distance.GetPosition(_initialLonLat).X) > 1e-7 ||
                    Math.Abs(InitialY - Distance.GetPosition(_initialLonLat).Y) > 1e-7)
                    SetProperty(ref _initialLonLat, location);

                CurrentX = value;
            }
        }

        [Category("Initial Status")]
        [DisplayName("Longitude [°]")]
        public double InitialLon
        {
            get => _initialLonLat.Longitude;
            set
            {
                SetProperty(ref _initialLonLat, new Location(_initialLonLat.Latitude, value));

                if (Math.Abs(InitialX - Distance.GetPosition(_initialLonLat).X) > 1e-7)
                    InitialX = Distance.GetPosition(_initialLonLat).X;

                if (Math.Abs(InitialY - Distance.GetPosition(_initialLonLat).Y) > 1e-7)
                    InitialY = Distance.GetPosition(_initialLonLat).Y;

                if (Math.Abs(InitialX - Distance.GetPosition(_initialLonLat).X) > 1e-7 ||
                    Math.Abs(InitialY - Distance.GetPosition(_initialLonLat).Y) > 1e-7)
                    SetProperty(ref _initialPosition, Distance.GetPosition(_initialLonLat));

                CurrentX = _initialPosition.X;
            }
        }

        [Category("Initial Status")]
        [DisplayName("Latitude [°]")]
        public double InitialLat
        {
            get => _initialLonLat.Latitude;
            set
            {
                SetProperty(ref _initialLonLat, new Location(value, _initialLonLat.Longitude));

                if (Math.Abs(InitialX - Distance.GetPosition(_initialLonLat).X) > 1e-7)
                    InitialX = Distance.GetPosition(_initialLonLat).X;

                if (Math.Abs(InitialY - Distance.GetPosition(_initialLonLat).Y) > 1e-7)
                    InitialY = Distance.GetPosition(_initialLonLat).Y;

                if (Math.Abs(InitialX - Distance.GetPosition(_initialLonLat).X) > 1e-7 ||
                    Math.Abs(InitialY - Distance.GetPosition(_initialLonLat).Y) > 1e-7)
                    SetProperty(ref _initialPosition, Distance.GetPosition(_initialLonLat));

                CurrentY = _initialPosition.Y;
            }
        }

        [Category("Initial Status")]
        [DisplayName("Range [m]")]
        public double RangeDistance
        {
            get => _rangeDistance;
            set
            {
                SetProperty(ref _rangeDistance, value);
                // var r = new Random((int)DateTime.Now.Ticks);
                // InitialX = r.NextDouble() * 2 * value - value;
                // InitialY = r.NextDouble() * 2 * value - value;
                // RaisePropertyChanged();
            }
        }

        [Category("Phyical Properties")]
        [DisplayName("Length [m]")]
        public double Length
        {
            get => _length;
            set => SetProperty(ref _length, value);
        }

        [Category("Phyical Properties")]
        [DisplayName("Maximum Angular velocity [deg/s]")]
        public double HeadingVelocity
        {
            get => _angularVelocity;
            set => SetProperty(ref _angularVelocity, value);
        }

        [Category("Sensor Properties")]
        [DisplayName("IS AIS exists?")]
        public bool IsAISExists
        {
            get => _isAISExists;
            set => SetProperty(ref _isAISExists, value);
        }

        [Category("Sensor Properties")]
        [DisplayName("Standard Deviation of AIS Noise")]
        public double StdDev
        {
            get => _stdDev;
            set => SetProperty(ref _stdDev, value);
        }

        [Category("Sensor Properties")]
        [DisplayName("Are Sensor Properties Changeable?")]
        public bool IsChangeable
        {
            get => _isChangeable;
            set => SetProperty(ref _isChangeable, value);
        }

        [Browsable(false)]
        public bool IsBehindObstacle
        {
            get => _isBehindObstacle;
            set => SetProperty(ref _isBehindObstacle, value);
        }

        [Browsable(false)]
        public ObservableCollection<PlatformState> HistoryAISData
        {
            get => _historyAISData;
            set => SetProperty(ref _historyAISData, value);
        }

        [Browsable(false)]
        public ObservableCollection<PlatformState> HistoryRadarData
        {
            get => _historyRadarData;
            set => SetProperty(ref _historyRadarData, value);
        }

        [Browsable(false)]
        public ObservableCollection<PlatformState> HistoryFusedData
        {
            get => _historyFusedData;
            set => SetProperty(ref _historyFusedData, value);
        }

        [Browsable(false)]
        public double DesiredHeading
        {
            get => _desiredHeading;
            set => SetProperty(ref _desiredHeading, value);
        }

        public override string ToString()
        {
            return DisplayName;
        }

        public override void SaveState(double time)
        {
            base.SaveState(time);

            if (IsAISExists)
            {
                createAISData(time, HistoryPosition.Last());
                //createRadarData(time, HistoryPosition.Last()); // isRadarExists 추후에 추가할 필요 있음
            }
        }

        private void createAISData(double time, PlatformState state)
        {
            // 시간과 속도에 따라 생성할 지 말 지 결정
            // 조건1. isAISExist: history data 없었으면 당장 생성
            // 조건2. state.speed와 time 차이에 따라
            // 조건3. 방향 변경중일시
            PlatformState prevState = (HistoryAISData.Count == 0) ? state : HistoryAISData.Last();
            double prevDataTime = prevState.Time;
            double prevDataSpeed = prevState.Speed;
            double dt = time - prevDataTime;

            // 배의 속도에 따라 AIS 데이터 생성 간격 결정 (이동시 2~10초, 정지시 분 단위)
            double timeInterval = 2;
            // if (prevDataSpeed > 11.5 && state.Speed > 11.5) { timeInterval = 2; }
            // else if (prevDataSpeed > 7 && state.Speed > 7) { timeInterval = 10; }
            // else if (prevDataSpeed > 1.5 || state.Speed > 1.5) { timeInterval = 10; }
            // else { timeInterval = 180; }
            //충분한 시간이 흐르지 않으면 데이터 생성 하지 않음
            //if (dt < timeInterval && prevState != state) { return; }

            // AIS data 생성
            // x, y좌표에 각각 표준편차 10m의 가우시안 분포 오차 추가
            //double dev = _stdDev;
            //var random = new MersenneTwister((int)time * 100 + 1); // 난수 고정
            //double r1 = Normal.Sample(random, 0, dev);
            //double r2 = Normal.Sample(random, 0, dev);
            // if (Math.Abs(time - 90) <= 3.5) // 90초 쯤에 outlier 발생
            // {
            //     r1 += 200 * Math.Cos(ContinuousUniform.Sample(random, 0, 2 * Math.PI));
            //     r2 += 200 * Math.Sin(ContinuousUniform.Sample(random, 0, 2 * Math.PI));
            // }
            // Point3D newPosition = new Point3D(state.Position.X + r1, state.Position.Y + r2, state.Position.Z);


            // // Speed fluctuation (임시)
            // Random ran2 = new Random((int)DateTime.Now.Ticks);
            // int num2 = ran2.Next();
            // double rate = 0.1;
            // if (num2 % 50 == 0)
            //     rate = 0.3;
            //
            // double newSpeed = CurrentSpeed * (1 + ((ran2.NextDouble() * rate - rate * 0.5)));

            PlatformState newState = new PlatformState()
            {
                Position = state.Position,
                Heading = state.Heading,
                Speed = state.Speed,
                Time = state.Time,
                Platform = this,
                Type = SensorType.AIS
            };

            // AIS data 저장 (HistoryAISData)
            // if (ran2.Next(6) < 1)
            //     return;

            HistoryAISData.Add(newState);
        }

        private void createRadarData(double time, PlatformState state)
        {
            //충분한 시간이 흐르지 않으면 데이터 생성 하지 않음
            PlatformState prevState = (HistoryRadarData.Count == 0) ? state : HistoryRadarData.Last();
            double timeInterval = 3;
            double dt = time - prevState.Time;
            if (dt < timeInterval && prevState != state) { return; }

            // 퓨전 알고리즘 확인용 임시 레이더 데이터 생성
            double dev = 1.2 * _stdDev;
            var random = new MersenneTwister(((int)time * 100)); // 난수 고정
            double r3 = Normal.Sample(random, 0, dev);
            double r4 = Normal.Sample(random, 0, dev);
            Point3D newPosition = new Point3D(state.Position.X + r3, state.Position.Y + r4, state.Position.Z);
            PlatformState newState = new PlatformState()
            {
                Position = newPosition,
                Heading = state.Heading,
                Speed = state.Speed,
                Time = state.Time,
                Platform = this,
                Type = SensorType.RADAR
            };

            // Radar data 저장 (HistoryRadarData)
            HistoryRadarData.Add(newState);
        }
    }
}