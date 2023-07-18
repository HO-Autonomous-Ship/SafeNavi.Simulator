using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.Serialization;
using System.Waf.Foundation;
using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Units;
using PropertyTools.DataAnnotations;
using SyDLab.Usv.Simulator.Domain.Models.Platforms;

namespace SyDLab.Usv.Simulator.Domain.Models.Tasks
{
    [DataContract]
    public class TaskBase : Model
    {
        private Angle _heading;
        private PlatformBase _platform;
        public int TargetWaypoint = 0;

        public TaskBase(ScenarioBase parent)
        {
            Parent = parent;
            Waypoints = new ObservableCollection<Waypoint>();
            Waypoints.CollectionChanged += WaypointsOnCollectionChanged;
        }

        private void WaypointsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (Waypoint waypoint in e.NewItems)
                    {
                        waypoint.Task = this;
                    }

                    break;
            }
        }

        [DataMember]
        [System.ComponentModel.Browsable(false)]
        public double Heading
        {
            get => _heading.Degrees;
            set
            {
                SetProperty(ref _heading, Angle.FromDegrees(value));

                // if (Platform != null)
                //     Platform.CurrentHeading = _heading.Degrees;

                if (Platform is MyShip)
                    ((MyShip)Platform).InitialHeading = Heading;
                else if (Platform is TargetShip)
                    ((TargetShip)Platform).InitialHeading = Heading;
            }
        }

        [DataMember] [System.ComponentModel.Browsable(false)] public ScenarioBase Parent { get; set; }

        [System.ComponentModel.Browsable(false)]
        public IList<PlatformBase> Platforms
        {
            get
            {
                IList<PlatformBase> pList = new List<PlatformBase>();

                foreach (PlatformBase myShip in Project.MyShips)
                {
                    pList.Add(myShip);
                }
                foreach (PlatformBase myShip in Project.TargetShips)
                {
                    pList.Add(myShip);
                }

                foreach (TaskBase scenarioTask in Project.SelectedScenario.Tasks)
                {
                    pList.Remove(scenarioTask.Platform);
                }

                return pList;
            }
        }

        private Project Project => Singleton<Project>.UniqueInstance;

        // [DataMember] private Group _object1;

        [DataMember]
        [ItemsSourceProperty("Platforms")]
        public PlatformBase Platform
        {
            get => Object1 as PlatformBase;
            set => Object1 = value;
        }

        [DataMember] private Group _object1;

        [PropertyTools.DataAnnotations.Browsable(false)]
        public Group Object1
        {
            get => _object1;
            set => SetProperty(ref _object1, value);
        }

        [DataMember] public ObservableCollection<Waypoint> Waypoints { get; }

        public void UpdatePlatform()
        {
            // if (Waypoints.Count > 0)
            // {
            //     Platform.CurrentY = Waypoints[0].Location.Y;
            //     Platform.CurrentX = Waypoints[0].Location.X;
            // }

            if (Waypoints.Count == 0)
                return;

            if (Platform is MyShip)
                ((MyShip) Platform).InitialHeading = Heading;
            else if (Platform is TargetShip)
                ((TargetShip) Platform).InitialHeading = Heading;

        }

        [DataMember]
        private string _name;

        [Category("Generals")]
        [DisplayName("Name")]
        public string DisplayName
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        [OnDeserialized]
        internal void OnDeserialized(StreamingContext context)
        {
            Waypoints.CollectionChanged += WaypointsOnCollectionChanged;

            foreach (Waypoint waypoint in Waypoints)
            {
                waypoint.Task = this;
            }
        }
    }
}