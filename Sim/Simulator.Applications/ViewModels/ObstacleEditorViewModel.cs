using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Waf.Applications;
using System.Windows.Input;
using MathNet.Spatial.Euclidean;
using SyDLab.Usv.Simulator.Applications.Views;
using SyDLab.Usv.Simulator.Domain.Models;

namespace SyDLab.Usv.Simulator.Applications.ViewModels
{
    [Export]
    public class ObstacleEditorViewModel: ViewModel<IObstacleEditorView>
    {
        private DelegateCommand _commandCreateObstacle;
        private DelegateCommand _commandClose;
        private DelegateCommand _commandDelete;
        private ObservableCollection<Waypoint> _obstaclePoints = new ObservableCollection<Waypoint>();

        public ObservableCollection<Waypoint> SelectedPoints = new ObservableCollection<Waypoint>();


        [ImportingConstructor]
        public ObstacleEditorViewModel(IObstacleEditorView view) : base(view)
        {
        }


        public DelegateCommand CommandCreateObstacle
        {
            get => _commandCreateObstacle;
            set => SetProperty(ref _commandCreateObstacle, value);
        }

        public DelegateCommand CommandDelete
        {
            get => _commandDelete;
            set => SetProperty(ref _commandDelete, value);
        }

        public DelegateCommand CommandClose
        {
            get => _commandClose;
            set => SetProperty(ref _commandClose, value);
        }

        public ObservableCollection<Waypoint> ObstaclePoints
        {
            get => _obstaclePoints;
            set => SetProperty(ref _obstaclePoints, value);
        }

        public void Show(object owner)
        {
            ViewCore.Show(owner);
            //return _dialogResult;
        }

        public void Close()
        {
            ViewCore.Close();
        }
    }
}