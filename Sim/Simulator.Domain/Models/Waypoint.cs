using System.ComponentModel;
using System.Runtime.Serialization;
using System.Security.AccessControl;
using MathNet.Spatial.Euclidean;
using System.Waf.Foundation;
using SyDLab.Usv.Simulator.Domain.Models.Tasks;

namespace SyDLab.Usv.Simulator.Domain.Models
{
    [DataContract]
    public class Waypoint : Model
    {
        private uint _index;
        private Point2D _location = new Point2D(0, 0);
        
        [DataMember]
        private double _y;
        [DataMember]
        private double _x;
        [DataMember]
        private double _speed;

        [DataMember]
        private Obstacle _obstacle;

        public Waypoint()
        {
        }

        public Waypoint(double x, double y)
        {
            Location = new Point2D(x, y);
        }

        [DataMember]
        [Browsable(false)]
        public uint Index
        {
            get => _index;
            set => SetProperty(ref _index, value);
        }

        [IgnoreDataMember]
        [Browsable(false)]
        public Point2D Location
        {
            get => _location;
            set
            {
                SetProperty(ref _location, value);
                RaisePropertyChanged("Y");
                RaisePropertyChanged("X");
                _x = _location.X;
                _y = _location.Y;
            }
        }

        /// <summary>
        /// 동쪽이 Y, 북쪽이 X
        /// </summary>
        public double X
        {
            get => Location.Y;
            set
            {
                Location = new Point2D(Location.X, value);
                RaisePropertyChanged("Y");
            }
        }

        /// <summary>
        /// 동쪽이 Y, 북쪽이 X
        /// </summary>
        public double Y
        {
            get => Location.X;
            set
            {
                Location = new Point2D(value, Location.Y);
                RaisePropertyChanged("X");
            }
        }

        public double Speed
        {
            get => _speed;
            set
            {
                if (value < 0)
                    value = 0;

                SetProperty(ref _speed, value);
            }
        }

        public override string ToString()
        {
            return $"{Index}: {Location.X}, {Location.Y}";
        }

        [Browsable(false)]
        public Obstacle Obstacle
        {
            get => _obstacle;
            set => SetProperty(ref _obstacle, value);
        }

        [IgnoreDataMember]
        [Browsable(false)]
        public TaskBase Task { get; set; }

        [OnDeserialized]
        internal void OnDeserialized(StreamingContext context)
        {
            _location = new Point2D(_x, _y);
        }
    }
}