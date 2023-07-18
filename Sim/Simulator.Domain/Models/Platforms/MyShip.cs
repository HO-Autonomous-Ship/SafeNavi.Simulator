using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.Maps.MapControl.WPF;

namespace SyDLab.Usv.Simulator.Domain.Models.Platforms
{
    [DataContract]
    public class MyShip : PlatformBase
    {
        private double _cogX;
        private double _inertia;
        private double _length;
        private double _mass;
        private double _rudderAngle;
        private double _rudderAngleDot;
        private double _limitOfRudderAngleDot;
        private double _limitOfRudderAngle;
        private double _initialHeading;

        [DataMember] private HullParameter _hullParameter;
        [DataMember] private ComponentProperty _componentProperty;

        // [DataMember]
        private ObservableCollection<PlatformState> _historyGPSData = new ObservableCollection<PlatformState>();

        // Radar-related
        private double radius = 3; // km
        private double interval = 2; // sec
        private double resolution = 50; // m
        private bool seaClutter;

        public MyShip(Project project) : base(project)
        {
            _initialLonLat = new Location(Project.ReferenceLatLng.X, Project.ReferenceLatLng.Y);

            ModelType = "DANV"; //VesselItemService.VesselList.First();
            IsKijimaUsed = true;

            _hullParameter = new HullParameter(ModelType);
            _length = _hullParameter.Lpp;
            _cogX = _hullParameter.xG;
            _mass = _hullParameter.rho * _length * _hullParameter.Breadth * _hullParameter.Draft * _hullParameter.Cb;
            _inertia = 1.99 * Math.Pow(10, -5) * (0.5 * _hullParameter.rho * 1000 * Math.Pow(_length, 5));

            _componentProperty = new ComponentProperty(ModelType);
            MaxRpm = _componentProperty.MaxRpm;
            MinRpm = _componentProperty.MinRpm;
        }

        #region Ship Properties
        // [DisplayName("Longitudinal Center of Gravity [m]")]
        [Browsable(false)]
        public double CogX
        {
            get => _cogX;
            set => SetProperty(ref _cogX, value);
        }

        // [DisplayName("Inertia [ton•m2]")]
        [Browsable(false)]
        public double Inertia
        {
            get => _inertia;
            set => SetProperty(ref _inertia, value);
        }

        [Category("Phyical Properties")]
        [DisplayName("Length [m]")]
        public double Length
        {
            get => _length;
            set => SetProperty(ref _length, value);
        }

        // [DisplayName("Mass [ton]")]
        [Browsable(false)]
        public double Mass
        {
            get => _mass;
            set => SetProperty(ref _mass, value);
        }

        [DataMember]
        [Browsable(false)]
        public HullParameter HullParameter
        {
            get => _hullParameter;
            set
            {
                SetProperty(ref _hullParameter, value);
                SetProperty(ref _length, value.Lpp);
            }
        }

        [DataMember]
        [Browsable(false)]
        public ComponentProperty ComponentProperty
        {
            get => _componentProperty;
            set => SetProperty(ref _componentProperty, value);
        }
        #endregion


        #region Rudder Properties
        [DataMember]
        [Category("Rudder Properties")]
        [DisplayName("Initial Angle [°]")]
        public double RudderAngle
        {
            get => _rudderAngle;
            set => SetProperty(ref _rudderAngle, value);
        }

        [DataMember]
        [DisplayName("Initial Angular Velocity [°/s]")]
        public double RudderAngleDot
        {
            get => _rudderAngleDot;
            set => SetProperty(ref _rudderAngleDot, value);
        }

        [DataMember]
        [DisplayName("Limit of Angle [°]")]
        public double LimitOfRudderAngle
        {
            get => _limitOfRudderAngle;
            set => SetProperty(ref _limitOfRudderAngle, value);
        }

        [DataMember]
        [DisplayName("Limit of Angular Velocity [°/s]")]
        public double LimitOfRudderAngleDot
        {
            get => _limitOfRudderAngleDot;
            set => SetProperty(ref _limitOfRudderAngleDot, value);
        }
        #endregion


        #region Radar Properties
        [DataMember]
        [Category("RADAR Properties")]
        [DisplayName("Range of RADAR (radius) [km]")]
        public double Radius
        {
            get => radius;
            set
            {
                SetProperty(ref radius, value);
                resolution = 50 * radius / 1.25;
                RaisePropertyChanged("Resolution");
            }
        }
        [DataMember]
        [Category("RADAR Properties")]
        [DisplayName("Interval of RADAR signal update [s]")]
        public double Interval
        {
            get => interval;
            set => SetProperty(ref interval, value);
        }
        [DataMember]
        [Category("RADAR Properties")]
        [DisplayName("Resolution of RADAR [m]")]
        public double Resolution
        {
            get => resolution;
            set => SetProperty(ref resolution, value);
        }
        [DataMember]
        [Category("RADAR Properties")]
        [DisplayName("Presence of sea clutter")]
        public bool SeaClutter
        {
            get => seaClutter;
            set => SetProperty(ref seaClutter, value);
        }
        #endregion


        #region Initail Status
        [DataMember]
        [Category("Initial Status")]
        [DisplayName("Heading (deg)")]
        public double InitialHeading
        {
            get => _initialHeading;
            set
            {
                SetProperty(ref _initialHeading, value);
                CurrentHeading = value;
            }
        }

        [DataMember]
        [DisplayName("Longitude [°]")]
        public double InitialLon
        {
            get => _initialLonLat.Longitude;
            set => SetProperty(ref _initialLonLat, new Location(_initialLonLat.Latitude, value));
        }

        [DataMember]
        [DisplayName("Latitude [°]")]
        public double InitialLat
        {
            get => _initialLonLat.Latitude;
            set => SetProperty(ref _initialLonLat, new Location(value, _initialLonLat.Longitude));
        }
        #endregion


        #region History
        [Browsable(false)]
        public ObservableCollection<PlatformState> HistoryGPSData
        {
            get => _historyGPSData;
            set => SetProperty(ref _historyGPSData, value);
        }
        #endregion




        public override string ToString()
        {
            return DisplayName;
        }

        public override void SaveState(double time)
        {
            base.SaveState(time);

            createGPSData(time, HistoryPosition.Last());
        }

        private void createGPSData(double time, PlatformState state)
        {
            PlatformState prevState = (HistoryGPSData.Count == 0) ? state : HistoryGPSData.Last();
            double prevDataTime = prevState.Time;
            double dt = time - prevDataTime;

            double timeInterval = 2;
            if (dt < timeInterval && prevState != state) { return; }
            HistoryGPSData.Add(state);
        }

        public void UpdateHullParam()
        {
            _hullParameter = new HullParameter(ModelType);
            _length = _hullParameter.Lpp;

            _componentProperty = new ComponentProperty(ModelType);
            MaxRpm = _componentProperty.MaxRpm;
            MinRpm = _componentProperty.MinRpm;
        }

        public void UpdateInitialParam()
        {
            
        }
    }
}