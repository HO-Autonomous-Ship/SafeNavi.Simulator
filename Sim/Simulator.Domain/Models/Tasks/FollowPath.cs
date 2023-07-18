using System.Runtime.Serialization;

namespace SyDLab.Usv.Simulator.Domain.Models.Tasks
{
    [DataContract]
    public class FollowPath : TaskBase
    {
        public FollowPath(ScenarioBase parent) : base(parent)
        {
        }
    }
}