using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Waf.Foundation;
using System.Windows.Media.Media3D;
using MathNet.Spatial.Units;
using Microsoft.Maps.MapControl.WPF;
using Point3D = MathNet.Spatial.Euclidean.Point3D;

namespace SyDLab.Usv.Simulator.Domain.Models.Platforms
{
    [DataContract]
    public class PlatformBase : Group
    {
        private Angle _currentHeading;
        private Point3D _currentPosition = new Point3D(0, 0, 0);
        private double _currentSpeed;
        protected Location _currentLonLat = new Location();
        private double _rpm=3000;
        private Vector3D _velocity = new Vector3D(20, 0, 0);


        private string _name;
        private int _shipType;

        private ObservableCollection<PlatformState> _historyPosition=new ObservableCollection<PlatformState>();

        private double _visualizedHeading;

        private double _initialSpeed=11;    //(knots)
        protected Location _initialLonLat = new Location();
        

        // control setpoint
        private double _headingDesired;
        private double _speedDesired=11;    //(knots)

        private double _rpmDesired=2500;
        private double _rudderDesired;

        // p(i)d controller gain parameters
        private double _headingControlGainP = 1;
        private double _headingControlGainD = 10;
        private double _speedControlGainP = 1;
        private double _speedControlGainI = 3;
        private double _speedControlGainD = 10;

        // ship properties
        private double _maxRpm = 80;
        private double _minRpm = 30;

        // [DataMember]
        [Browsable(false)]
        public Location InitialLonLat => _initialLonLat;

        [Browsable(false)]
        public Location CurrentLonLat => _currentLonLat;

        [Browsable(false)]
        public ShipCoefficient Coefficient = new ShipCoefficient();

        private string _modelType;
        private bool _isKijimaUsed;

        public PlatformBase(Project project)
        {
            Project = project;
            _speedDesired = _initialSpeed;
        }


        #region General Information
        [DataMember]
        [Category("Generals Info.")]
        [DisplayName("Name")]
        public string DisplayName
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
        [DataMember]
        [DisplayName("Type info")]
        public int ShipType
        {
            get => _shipType;
            set => SetProperty(ref _shipType, value);
        }
        #endregion


        #region Current Status
        [Category("Current Status")]
        [DisplayName("Heading (°)")]
        [ReadOnly(true)]
        public double CurrentHeading
        {
            get => _currentHeading.Degrees;
            set
            {
                SetProperty(ref _currentHeading, Angle.FromDegrees(value));
                RaisePropertyChanged(nameof(Heading));
                RaisePropertyChanged(nameof(VisualizedHeading));
            }
        }

        [Browsable(false)]
        public Point3D CurrentPosition => _currentPosition;

        [Browsable(false)]
        public double VisualizedHeading
        {
            get => _visualizedHeading;
            set => SetProperty(ref _visualizedHeading, value);
        }
        [DisplayName("X (m)")]
        [ReadOnly(true)]
        public double CurrentY
        {
            get => _currentPosition.Y;
            set
            {
                SetProperty(ref _currentPosition, new Point3D(_currentPosition.X, value, _currentPosition.Z));
                Location location = Distance.GetLatLng(_currentPosition);

                CurrentLon = location.Longitude;
                CurrentLat = location.Latitude;
                SetProperty(ref _currentLonLat , location);
            }
        }
        [DisplayName("Y (m)")]
        [ReadOnly(true)]
        public double CurrentX
        {
            get => _currentPosition.X;
            set
            {
                SetProperty(ref _currentPosition, new Point3D(value, _currentPosition.Y, _currentPosition.Z));
                Location location = Distance.GetLatLng(_currentPosition);

                CurrentLon = location.Longitude;
                CurrentLat = location.Latitude;
                SetProperty(ref _currentLonLat, location);
            }
        }
        [DisplayName("Longitude [°]")]
        [ReadOnly(true)]
        public double CurrentLon
        {
            get => _currentLonLat.Longitude;
            set => SetProperty(ref _currentLonLat, new Location(_currentLonLat.Latitude, value));
        }
        [DisplayName("Latitude [°]")]
        [ReadOnly(true)]
        public double CurrentLat
        {
            get => _currentLonLat.Latitude;
            set => SetProperty(ref _currentLonLat, new Location(value, _currentLonLat.Longitude));
        }

        [Browsable(false)]
        [ReadOnly(true)]
        public double CurrentZ
        {
            get => _currentPosition.Z;
            set => SetProperty(ref _currentPosition, new Point3D(_currentPosition.X, _currentPosition.Y, value));
        }

        [DisplayName("Speed [knot]")]
        [ReadOnly(true)]
        public double CurrentSpeed
        {
            get => _currentSpeed*Distance.MS2KN;
            set => SetProperty(ref _currentSpeed, value*Distance.KN2MS);
        }

        [Browsable(false)]
        public double Heading => CurrentHeading;

        public double AngularVelocity = 0;
        #endregion


        public double[] TempVar = new double[3];
        public double[] Rudder = new double[2];


        [DataMember]
        [Browsable(false)]
        public Project Project { get; }


        [DataMember]
        [Category("Initial Status")]
        [DisplayName("Speed [knot]")]
        public double Speed
        {
            get => _initialSpeed;
            set
            {
                if (value < 0)
                    value = 0;

                SetProperty(ref _initialSpeed, value);
                CurrentSpeed = value;
            }
        }

        public double Rpm
        {
            get => _rpm;
            set => SetProperty(ref _rpm, value);
        }

        [DataMember]
        public double MaxRpm
        {
            get => _maxRpm;
            set => SetProperty(ref _maxRpm, value);
        }
        [DataMember]
        public double MinRpm
        {
            get => _minRpm;
            set => SetProperty(ref _minRpm, value);
        }


        #region Ship control
        [Category("Current Status")]
        public double RpmDesired
        {
            get => _rpmDesired;
            set => SetProperty(ref _rpmDesired, value);
        }
        [DisplayName("Rudder Desired. (rad)")]
        public double RudderDesired
        {
            get => _rudderDesired;
            set => SetProperty(ref _rudderDesired, value);
        }
        [DisplayName("Heading Desired. (deg/s)")]
        public double HeadingDesired
        {
            get => _headingDesired;
            set => SetProperty(ref _headingDesired, value);
        }
        [DisplayName("Speed Desired. (knots)")]
        public double SpeedDesired
        {
            get => _speedDesired;
            set => SetProperty(ref _speedDesired, value);
        }

        [Category("Autopilot Controller")]
        [DataMember]
        [DisplayName("Heading Control Gain P")]
        public double HeadingControlGainP
        {
            get => _headingControlGainP;
            set => SetProperty(ref _headingControlGainP, value);
        }
        [DataMember]
        [DisplayName("Heading Control Gain D")]
        public double HeadingControlGainD
        {
            get => _headingControlGainD;
            set => SetProperty(ref _headingControlGainD, value);
        }
        [DataMember]
        [DisplayName("Speed Control Gain P")]
        public double SpeedControlGainP
        {
            get => _speedControlGainP;
            set => SetProperty(ref _speedControlGainP, value);
        }
        [DataMember]
        [DisplayName("Speed Control Gain I")]
        public double SpeedControlGainI
        {
            get => _speedControlGainI;
            set => SetProperty(ref _speedControlGainI, value);
        }
        [DataMember]
        [DisplayName("Speed Control Gain D")]
        public double SpeedControlGainD
        {
            get => _speedControlGainD;
            set => SetProperty(ref _speedControlGainD, value);
        }
        #endregion


        [Browsable(false)]
        public Vector3D Velocity
        {
            get => _velocity;
            set => SetProperty(ref _velocity, value);
        }

        [DataMember]
        [Browsable(false)]
        public string ModelType
        {
            get => _modelType;
            set => SetProperty(ref _modelType, value);
        }

        [DataMember]
        [Browsable(false)]
        public bool IsKijimaUsed
        {
            get => _isKijimaUsed;
            set => _isKijimaUsed = value;
        }

        #region History
        [Browsable(false)]
        public ObservableCollection<PlatformState> HistoryPosition
        {
            get => _historyPosition;
            set => SetProperty(ref _historyPosition, value);
        }
        #endregion


        public override string ToString()
        {
            return DisplayName;
        }

        public bool IsStopped(double x, double y, double theta)
        {
            if (Math.Abs(CurrentX - x) < 1e-2 && Math.Abs(CurrentY - y) < 1e-2 && Math.Abs(CurrentHeading - theta) < 1e-2)
                return true;
            return false;
        }

        public virtual void SaveState(double time)
        {
            HistoryPosition.Add(new PlatformState()
            {
                Heading = CurrentHeading,
                Position = CurrentPosition,
                Time = time,
                Speed = CurrentSpeed,
                Rpm = Rpm,
                RudderAngle = Rudder[0],
                Platform = this
            });
        }
    }
}