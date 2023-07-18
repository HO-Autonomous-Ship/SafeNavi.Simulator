using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Waf.Foundation;
using MathNet.Spatial.Euclidean;

namespace SyDLab.Usv.Simulator.Domain.Models
{
    [DataContract]
    public class Obstacle:Model
    {
        private ObservableCollection<Waypoint> _points=new ObservableCollection<Waypoint>();
        [DataMember] private ObservableCollection<double> _doublePoints=new ObservableCollection<double>();
        [DataMember] private string _name;

        public Obstacle()
        {

        }

        public Obstacle(ObservableCollection<Waypoint> points)
        {
            foreach (Waypoint point in points)
            {
                _points.Add(point);
                point.Obstacle = this;
                _doublePoints.Add(point.Location.X);
                _doublePoints.Add(point.Location.Y);
            }
        }

        [Category("Properties")]
        [DisplayName("Points")]
        public ObservableCollection<Waypoint> Points
        {
            get => _points;
            set => SetProperty(ref _points, value);
        }

        [Browsable(false)]
        public ObservableCollection<double> DoublePoints
        {
            get => _doublePoints;
            set => SetProperty(ref _doublePoints, value);
        }

        public void AddPoint(Waypoint p)
        {
            _points?.Add(p);
            _doublePoints?.Add(p.Location.X);
            _doublePoints?.Add(p.Location.Y);
            p.Obstacle = this;
        }

        [Category("Generals")]
        [DisplayName("Name")]
        public string DisplayName
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
    }
}