using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using PropertyTools.DataAnnotations;

namespace SyDLab.Usv.Simulator.Domain.Models
{
    [DataContract]
    public class Group : ModelBase
    {
        public Group()
        {
        }

        #region TreeViewItems

        [DataMember]
        private string _remarks;

        [System.ComponentModel.Browsable(false)]
        public string Remarks
        {
            get => _remarks;
            set => SetProperty(ref _remarks, value);
        }

        [DataMember]
        private bool _isExpanded;

        [System.ComponentModel.Browsable(false)]
        public bool IsExpanded
        {
            get => _isExpanded;
            set => SetProperty(ref _isExpanded, value);
        }

        [DataMember]
        private bool _isSelected;

        [System.ComponentModel.Browsable(false)]
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        [IgnoreDataMember]
        private bool _isEditing;

        [System.ComponentModel.Browsable(false)]
        public bool IsEditing
        {
            get => _isEditing;
            set => SetProperty(ref _isEditing, value);
        }

        [System.ComponentModel.Browsable(false)]
        
        #endregion TreeViewItems

        [DataMember]
        private bool _isVisualized = true;

        [DataMember]
        protected Group _parrent = null;

        [System.ComponentModel.Browsable(false)]
        public Group Parrent
        {
            get => _parrent;
            set => _parrent = value;
        }

        [DataMember]
        protected ObservableCollection<Group> _children = new ObservableCollection<Group>();

        [System.ComponentModel.Browsable(false)]
        public ObservableCollection<Group> Children => _children;

        public bool HasParrent() => _parrent != null;

        public bool IsChild(Group child) => _children.Contains(child);

        [System.ComponentModel.Browsable(false)]
        public int NumOfChildren => _children.Count;

        public override void Initialize()
        {
            foreach (var child in Children)
            {
                child.Initialize();
            }

            base.Initialize();
        }

        public List<Group> GetAll()
        {
            List<Group> list = new List<Group> { this };

            foreach (Group child in Children)
            {
                list.AddRange(child.GetAll());
            }

            return list;
        }

        public List<T> GetList<T>()
        {
            List<T> list = new List<T>();

            if (this is T)
                list.Add((T)(object)this);

            foreach (var child in Children)
            {
                list.AddRange(child.GetList<T>());
            }

            return list;
        }
    }
}