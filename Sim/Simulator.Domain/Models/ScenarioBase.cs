using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Waf.Foundation;
using SyDLab.Usv.Simulator.Domain.Models.Tasks;

namespace SyDLab.Usv.Simulator.Domain.Models
{
    [DataContract]
    public class ScenarioBase : Model
    {
        private string _name;

        public ScenarioBase(Project parent)
        {
            Parent = parent;
            Tasks = new ObservableCollection<TaskBase>();
        }

        [DataMember]
        [DisplayName("Name")]
        public string DisplayName
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        [DataMember]
        [Browsable(false)]
        public bool IsSelected
        {
            get => Parent != null && Parent.SelectedScenario == this;
            set
            {
                if (Parent == null)
                    return;

                if (value && Parent.SelectedScenario != this)
                {
                    var oldItem = Parent.SelectedScenario;
                    Parent.SelectedScenario = this;
                    oldItem?.UpdateProperty("IsSelected");
                }
                else if (!value && Parent.SelectedScenario == this)
                {
                    Parent.SelectedScenario = null;
                }

                RaisePropertyChanged("IsSelected");
            }
        }

        [DataMember]
        [Browsable(false)]
        public Project Parent { get; }

        [DataMember]
        [Browsable(false)]
        public ObservableCollection<TaskBase> Tasks { get; }

        public void UpdateProperty(string propertyName)
        {
            RaisePropertyChanged(propertyName);
        }

        [OnDeserialized]
        internal void OnDeserialized(StreamingContext context)
        {
            foreach (TaskBase task in Tasks)
            {
                task.Parent = this;
            }
        }
    }

    [DataContract]
    public class ScenarioDetectMine : ScenarioBase
    {
        public ScenarioDetectMine(Project parent) : base(parent)
        {
        }
    }

    [DataContract]
    public class ScenarioEnterPort : ScenarioBase
    {
        public ScenarioEnterPort(Project parent) : base(parent)
        {
        }
    }

    [DataContract]
    public class ScenarioFollowPath : ScenarioBase
    {
        public ScenarioFollowPath(Project parent) : base(parent)
        {
        }
    }
}