using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using System.Waf.Foundation;
using MathNet.Spatial.Euclidean;
using SyDLab.Usv.Simulator.Domain.Models.Platforms;
using SyDLab.Usv.Simulator.Domain.Models.Tasks;

namespace SyDLab.Usv.Simulator.Domain.Models
{
    [DataContract]
    public class Project : Model
    {
        private double _maxTime = 1000.0;
        private string _name = "Empty Project";
        private Point2D _referenceLatLng;
        private ScenarioBase _selectedScenario;


        public Project()
        {
            MyShips = new ObservableCollection<MyShip>();
            TargetShips = new ObservableCollection<TargetShip>();
            Obstacles = new ObservableCollection<Obstacle>();
            Scenarios = new ObservableCollection<ScenarioBase>();
            _referenceLatLng = new Point2D(37.0000, 125.5000);
        }

        [IgnoreDataMember]
        [Browsable(false)]

        [DataMember]
        [DisplayName("Name")]
        public string DisplayName
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        [IgnoreDataMember]
        [Browsable(false)]
        public double MaxTime
        {
            get => _maxTime;
            set => SetProperty(ref _maxTime, value);
        }

        [DataMember]
        [Browsable(false)]
        public ObservableCollection<MyShip> MyShips { get; }

        [DataMember]
        [Browsable(false)]
        public ObservableCollection<TargetShip> TargetShips { get; }

        [DataMember]
        [Browsable(false)]
        public ObservableCollection<Obstacle> Obstacles { get; }

        [DataMember]
        [DisplayName("Reference Latitude")]
        public double ReferenceLat
        {
            get => ReferenceLatLng.X;
            set
            {
                ReferenceLatLng = new Point2D(ReferenceLat, ReferenceLon);
            }
        }

        [DataMember]
        [DisplayName("Reference Longitude")]
        public double ReferenceLon
        {
            get => ReferenceLatLng.Y;
            set
            {
                ReferenceLatLng = new Point2D(ReferenceLat, ReferenceLon);
            }
        }

        [Browsable(false)]
        public Point2D ReferenceLatLng
        {
            get => _referenceLatLng;
            set
            {
                SetProperty(ref _referenceLatLng, value);
                RaisePropertyChanged(nameof(ReferenceLatLng));
            }
        }

        [DataMember]
        [Browsable(false)]
        public ObservableCollection<ScenarioBase> Scenarios { get; }

        [IgnoreDataMember]
        public ScenarioBase SelectedScenario
        {
            get => _selectedScenario;
            set
            {
                if (_selectedScenario == value)
                    return;

                _selectedScenario = value;
                RaisePropertyChanged(nameof(SelectedScenario));

                foreach (var item in Scenarios)
                    item.UpdateProperty("IsSelected");
            }
        }

        [OnDeserialized]
        internal void OnDeserialized(StreamingContext context)
        {
            foreach (ScenarioBase scenario in Scenarios)
            {
                if (scenario.IsSelected)
                    SelectedScenario = scenario;
            }
        }

        public void Clone(Project project)
        {
            // Clone Project
            DisplayName = project.DisplayName;
            ReferenceLatLng = project.ReferenceLatLng;



            // Clone platform
            Dictionary<PlatformBase, PlatformBase> dicPlatform = new Dictionary<PlatformBase, PlatformBase>();
            foreach (MyShip platform in project.MyShips)
            {
                MyShip newUsv = new MyShip(this);
                newUsv.CogX = platform.CogX;
                newUsv.Inertia = platform.Inertia;
                newUsv.Length = platform.Length;
                newUsv.LimitOfRudderAngle = platform.LimitOfRudderAngle;
                newUsv.LimitOfRudderAngleDot = platform.LimitOfRudderAngleDot;
                newUsv.InitialLon = platform.InitialLon;
                newUsv.InitialLat = platform.InitialLat;
                newUsv.InitialHeading = platform.InitialHeading;
                double u = platform.Speed * Math.Cos(platform.InitialHeading * Distance.D2R);
                double v = platform.Speed * Math.Sin(platform.InitialHeading * Distance.D2R);
                newUsv.Velocity = new System.Windows.Media.Media3D.Vector3D(u,v,0);
                newUsv.Mass = platform.Mass;
                newUsv.RudderAngle = platform.RudderAngle;
                newUsv.RudderAngleDot = platform.RudderAngleDot;
                newUsv.Speed = platform.Speed;
                newUsv.Rpm = platform.RpmDesired;
                newUsv.RpmDesired = platform.RpmDesired;
                newUsv.MaxRpm = platform.MaxRpm;
                newUsv.CurrentHeading = platform.InitialHeading;
                newUsv.SpeedDesired = platform.Speed;
                newUsv.HeadingDesired = platform.InitialHeading;
                newUsv.RudderDesired = 0;
                newUsv.CurrentX = platform.CurrentX;
                newUsv.CurrentY = platform.CurrentY;
                newUsv.CurrentZ = platform.CurrentZ;
                newUsv.DisplayName = platform.DisplayName;
                newUsv.ModelType = platform.ModelType;
                newUsv.HullParameter = platform.HullParameter;
                newUsv.ComponentProperty = platform.ComponentProperty;
                newUsv.HeadingControlGainP = platform.HeadingControlGainP;
                newUsv.HeadingControlGainD = platform.HeadingControlGainD;
                newUsv.SpeedControlGainP = platform.SpeedControlGainP;
                newUsv.SpeedControlGainD = platform.SpeedControlGainD;
                newUsv.SpeedControlGainI = platform.SpeedControlGainI;

                MyShips.Add(newUsv);
                dicPlatform[platform] = newUsv;
            }

            foreach (TargetShip platform in project.TargetShips)
            {
                TargetShip newUsv = new TargetShip(this);
                newUsv.Speed = platform.Speed;
                newUsv.Length = platform.Length;
                newUsv.InitialX = platform.InitialX;
                newUsv.InitialY = platform.InitialY;
                newUsv.InitialHeading = platform.InitialHeading;
                newUsv.RangeDistance = platform.RangeDistance;
                newUsv.DisplayName = platform.DisplayName;
                newUsv.AngularVelocity = platform.AngularVelocity;
                newUsv.IsAISExists = platform.IsAISExists;
                newUsv.HeadingVelocity = platform.HeadingVelocity;
                newUsv.MMSI = platform.MMSI;
                newUsv.SpeedDesired = platform.Speed;
                newUsv.HeadingDesired = platform.InitialHeading;
                newUsv.Rpm = platform.RpmDesired;
                newUsv.RpmDesired = platform.RpmDesired;
                TargetShips.Add(newUsv);
                dicPlatform[platform] = newUsv;
            }

            // Clone Obstacle
            foreach (Obstacle obs in project.Obstacles)
            {
                ObservableCollection<Waypoint> pointList = new ObservableCollection<Waypoint>();

                for (int i = 0; i < obs.DoublePoints.Count; i += 2)
                {
                    pointList.Add(new Waypoint(obs.DoublePoints[i], obs.DoublePoints[i + 1]));
                }

                Obstacle newObs = new Obstacle(pointList);
                newObs.DisplayName = obs.DisplayName;

                Obstacles.Add(newObs);
            }

            // Clone Scenario.
            foreach (ScenarioBase scenario in project.Scenarios)
            {
                ScenarioBase newScenario = null;

                if (scenario is ScenarioDetectMine)
                    newScenario = new ScenarioDetectMine(this);
                else if (scenario is ScenarioEnterPort)
                    newScenario = new ScenarioEnterPort(this);
                else if (scenario is ScenarioFollowPath)
                    newScenario = new ScenarioFollowPath(this);

                if (newScenario == null)
                    continue;

                newScenario.DisplayName = scenario.DisplayName;
                newScenario.IsSelected = scenario.IsSelected;

                Scenarios.Add(newScenario);

                foreach (TaskBase task in scenario.Tasks)
                {
                    TaskBase newTask = null;

                   if (task is FollowPath)
                        newTask = new FollowPath(newScenario);

                    if (newTask == null)
                        continue;

                    newTask.Heading = task.Heading;
                    newTask.Platform = dicPlatform[task.Platform];
                    newTask.DisplayName = task.DisplayName;

                    foreach (Waypoint waypoint in task.Waypoints)
                    {
                        Waypoint newWaypoint = new Waypoint(waypoint.Location.X, waypoint.Location.Y);
                        newWaypoint.Speed = waypoint.Speed;
                        newWaypoint.Task = newTask;
                        newWaypoint.Index = waypoint.Index;
                        newTask.Waypoints.Add(newWaypoint);
                    }

                    // waypoint를 넣어준 다음 task 리스트에 추가해줌. 이벤트 처리 방식에 차이.
                    newScenario.Tasks.Add(newTask);
                }
            }
        }
    }
}