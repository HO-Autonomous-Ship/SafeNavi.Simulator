using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Waf.Applications;
using MathNet.Spatial.Euclidean;
using SyDLab.Usv.Simulator.Applications.ViewModels;
using SyDLab.Usv.Simulator.Domain;
using SyDLab.Usv.Simulator.Domain.Models;

namespace SyDLab.Usv.Simulator.Applications.Controllers
{
    [Export]

    public class ObstacleController
    {
        private readonly ObstacleEditorViewModel _obstacleEditorViewModel;
        private readonly ShellViewModel _shellViewModel;
        private readonly MapViewModel _mapViewModel;

        private DelegateCommand _commandSetObstacle;
        private DelegateCommand _commandCancel;
        private DelegateCommand _commandDelete;
        private readonly DelegateCommand _commandCreateObstacle;
        private Project Project => Singleton<Project>.UniqueInstance;

        [ImportingConstructor]
        ObstacleController(ShellViewModel shellViewModel, ObstacleEditorViewModel obstacleEditorViewModel, MapViewModel mapViewModel)
        {
            _obstacleEditorViewModel = obstacleEditorViewModel;
            _shellViewModel = shellViewModel;
            _mapViewModel = mapViewModel;

            _commandCreateObstacle = new DelegateCommand(ExecuteCreateObstacle);
            _commandSetObstacle = new DelegateCommand(ExecuteSetObstacle);
            _commandCancel = new DelegateCommand(ExecuteCancel);
            _commandDelete = new DelegateCommand(DeletePoint);
        }

        public void Initialize()
        {
            // Connect commands
            _shellViewModel.CommandCreateObstacle = _commandCreateObstacle;

            _obstacleEditorViewModel.CommandCreateObstacle = _commandSetObstacle;
            _obstacleEditorViewModel.CommandClose = _commandCancel;
            _obstacleEditorViewModel.CommandDelete = _commandDelete;

            // Events
            _obstacleEditorViewModel.ObstaclePoints.CollectionChanged += ObstaclePoints_CollectionChanged;
            _mapViewModel.PropertyChanged += _mapViewModel_PropertyChanged;
            Project.Obstacles.CollectionChanged += Obstacles_CollectionChanged;
        }

        private void Obstacles_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (Obstacle newItem in e.NewItems)
                    {
                        // Marker 생성
                        _mapViewModel.CreateMarker(newItem);
                        newItem.Points.CollectionChanged += Points_CollectionChanged;
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (Obstacle oldItem in e.OldItems)
                    {
                        _mapViewModel.DeleteMarker(oldItem);
                        oldItem.Points.CollectionChanged -= Points_CollectionChanged;
                    }
                    break;
            }
        }

        private void Points_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            var s = sender as ObservableCollection<Waypoint>;
            var obs = s[0].Obstacle as Obstacle;

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                {
                    foreach (Waypoint item in e.NewItems)
                    {
                        item.Obstacle = obs;
                        _mapViewModel.UpdateObstacle(obs);
                        item.PropertyChanged += ObstaclePointPropertyChanged;
                    }
                }
                    break;

                case NotifyCollectionChangedAction.Remove:
                {
                    foreach (Waypoint item in e.OldItems)
                    {
                        _mapViewModel.UpdateObstacle(item.Obstacle);

                        //item.PropertyChanged -= ObstaclePointPropertyChanged;
                    }
                }
                    break;
            }
        }

        private void _mapViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            MapViewModel mvm = sender as MapViewModel;

            switch (e.PropertyName)
            {
                case "ObstaclePoint":
                    _obstacleEditorViewModel.ObstaclePoints.Add(new Waypoint(mvm.ObstaclePoint.X, mvm.ObstaclePoint.Y)
                        {Index = (uint) (_obstacleEditorViewModel.ObstaclePoints.Count)});
                    break;
            }
        }

        private void ExecuteCancel()
        {
            foreach (Waypoint point in _obstacleEditorViewModel.ObstaclePoints)
                _mapViewModel.DeleteMarker(point);

            _mapViewModel.IsClickModeOn = false;
            _obstacleEditorViewModel.ObstaclePoints.Clear();
            _commandSetObstacle.RaiseCanExecuteChanged();
            _obstacleEditorViewModel.Close();
        }

        private void DeletePoint()
        {
            for (int i = 0; i < _obstacleEditorViewModel.SelectedPoints.Count;)
            {
                var point = _obstacleEditorViewModel.SelectedPoints[i];
                if (_obstacleEditorViewModel.ObstaclePoints.Contains(point))
                {
                    _obstacleEditorViewModel.ObstaclePoints.Remove(point);
                }
                else
                    i++;
            }
        }

        private void ExecuteSetObstacle()
        {
            Obstacle obs = new Obstacle(_obstacleEditorViewModel.ObstaclePoints);
            obs.DisplayName = "Obstacle " + Project.Obstacles.Count;

            Project.Obstacles.Add(obs);

            while (_obstacleEditorViewModel.ObstaclePoints.Count > 0)
            {
                _obstacleEditorViewModel.ObstaclePoints.RemoveAt(0);
            }
           
            _mapViewModel.IsClickModeOn = false;
            _obstacleEditorViewModel.Close();
        }

        private void ExecuteCreateObstacle()
        {
            _mapViewModel.IsClickModeOn = true;
            _obstacleEditorViewModel.Show(_shellViewModel.View);
        }

        private void ObstaclePoints_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    {
                        foreach (Waypoint item in e.NewItems)
                        {
                            _mapViewModel.AddObstaclePoint(item);

                            item.PropertyChanged += ObstaclePointPropertyChanged;
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    {
                        foreach (Waypoint item in e.OldItems)
                        {
                            _mapViewModel.DeleteWaypoint(item);

                            //item.PropertyChanged -= ObstaclePointPropertyChanged;
                        }
                    }
                    break;
            }
        }

        private void ObstaclePointPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Waypoint wp = sender as Waypoint;

            switch (e.PropertyName)
            {
                case "X":
                case "Y":
                    if (wp.Obstacle == null)
                        _mapViewModel.UpdateMarkerPosition(wp, wp.Location.X, wp.Location.Y);
                    else
                        _mapViewModel.UpdateObstacle(wp.Obstacle);
                    break;
            }
        }
    }
}