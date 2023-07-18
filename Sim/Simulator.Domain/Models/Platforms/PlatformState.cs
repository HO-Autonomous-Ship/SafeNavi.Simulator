using System.Runtime.Serialization;
using System.Waf.Foundation;
using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Units;

namespace SyDLab.Usv.Simulator.Domain.Models.Platforms
{
    public enum SensorType
    {
        NONE = -1,
        AIS = 0,
        RADAR = 1
    }

    public class PlatformState:Model
    {
        [DataMember]
        public double Heading;

        [DataMember]
        public Point3D Position; // knot

        [DataMember]
        public double Speed;

        [DataMember]
        public double Time;

        [DataMember] 
        public PlatformBase Platform;

        [DataMember] 
        public SensorType Type = SensorType.NONE;

        [DataMember] public double Rpm;
        [DataMember] public double RudderAngle;
    }
}