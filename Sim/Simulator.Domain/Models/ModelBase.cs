using System;
using System.Runtime.Serialization;
using System.Waf.Foundation;
using PropertyTools.DataAnnotations;

namespace SyDLab.Usv.Simulator.Domain.Models
{
    [DataContract]
    public class ModelBase : Model
    {
        [DataMember]
        protected string _name = string.Empty;

        [DataMember]
        protected Guid _id = Guid.NewGuid();

        [System.ComponentModel.Category("Name")]
        [ReadOnly(true)]
        [SortIndex(999)]
        public virtual Guid ID
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        public virtual void Initialize()
        {
        }

        public virtual void Import(string filePath)
        {
        }

        public virtual void Export(string filePath, double timeStep)
        {
        }
    }
}