using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Units;
using Microsoft.Maps.MapControl.WPF;
using SyDLab.Usv.Simulator.Applications.Services;
using SyDLab.Usv.Simulator.Applications.ViewModels;
using SyDLab.Usv.Simulator.Domain;
using SyDLab.Usv.Simulator.Domain.Logs;
using SyDLab.Usv.Simulator.Domain.Models;
using SyDLab.Usv.Simulator.Domain.Models.Platforms;
using SyDLab.Usv.Simulator.Domain.Models.Tasks;
using DelegateCommand = System.Waf.Applications.DelegateCommand;

namespace SyDLab.Usv.Simulator.Applications.Controllers
{
    [Export]
    public class ScenarioController
    {
        private readonly DelegateCommand _commandCreateScenarioFollowPath;
        private readonly DelegateCommand _commandCreateTaskFollowPath;
        private readonly IDispatcherService _dispatcherService;
        private readonly MapViewModel _mapViewModel;
        private readonly ShellViewModel _shellViewModel;

        [ImportingConstructor]
        public ScenarioController(
            IDispatcherService dispatcherService,
            ShellViewModel shellViewModel, MapViewModel mapViewModel)
        {
            _dispatcherService = dispatcherService;
            _shellViewModel = shellViewModel;
            _mapViewModel = mapViewModel;

            _commandCreateScenarioFollowPath = new DelegateCommand(ExecuteCreateScenarioFollowPath, CanExecuteCreateScenarioFollowPath);
            _commandCreateTaskFollowPath = new DelegateCommand(ExecuteCreateTaskFollowPath, CanExecuteCreateTaskFollowPath);
        }

        private bool CanExecuteCreateScenarioFollowPath()
        {
            return true;
        }

        private void ExecuteCreateScenarioFollowPath()
        {
            var newScenario = new ScenarioFollowPath(Project)
            {
                DisplayName = string.Format($"Follow Path {Project.Scenarios.Count}")
            };

            Project.Scenarios.Add(newScenario);

            if (Project.Scenarios.Count == 1)
                newScenario.IsSelected = true;

            _commandCreateTaskFollowPath.RaiseCanExecuteChanged();
        }

        private bool CanExecuteCreateTaskFollowPath()
        {
            return Project.SelectedScenario is ScenarioFollowPath; // 전체 선박 개수 이상으로 task를 만들 수 없음
        }

        private void ExecuteCreateTaskFollowPath()
        {
            if (Project.SelectedScenario == null || !(Project.SelectedScenario is ScenarioFollowPath))
            {
                LogManager.WriteLine("Select a scenario to create task.");
                return;
            }

            var newTask = new FollowPath(Project.SelectedScenario);

            // 첫 번째 platform 선택
            newTask.Platform =(newTask.Platforms.Count==0)? null: newTask.Platforms.First();
            newTask.DisplayName = newTask.Platform?.DisplayName + " Follows path";
           
            // Task의 Marker 생성
            _mapViewModel.CreateMarker(newTask);

            Project.SelectedScenario.Tasks.Add(newTask);
            _commandCreateTaskFollowPath.RaiseCanExecuteChanged();
        }

        private LogManager LogManager => Singleton<LogManager>.UniqueInstance;

        private Project Project => Singleton<Project>.UniqueInstance;

        private void NewTask_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            TaskBase tb = sender as TaskBase;

            switch (e.PropertyName)
            {
                case "Heading":
                    break;
                case "Platform":
                case "Object1":
                    tb.DisplayName= tb.Platform.DisplayName + " Follows path";
                    tb.UpdatePlatform();
                    _mapViewModel.UpdateRoute(tb);

                    break;
            }
        }

        public void Initialize()
        {
            Project.Scenarios.CollectionChanged += Scenarios_OnCollectionChanged;
            Scenarios_OnCollectionChanged(Project,
                new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, Project.Scenarios));

            foreach (var scenario in Project.Scenarios)
            {
                scenario.Tasks.CollectionChanged += Tasks_OnCollectionChanged;

                Tasks_OnCollectionChanged(scenario,
                    new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, scenario.Tasks));

                foreach (var task in scenario.Tasks)
                {
                    task.Waypoints.CollectionChanged += Waypoints_OnCollectionChanged;
                    Waypoints_OnCollectionChanged(task,
                        new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, task.Waypoints));
                }
            }

            Project.PropertyChanged += Project_OnPropertyChanged;

            _shellViewModel.CommandCreateScenarioFollowPath = _commandCreateScenarioFollowPath;
            _shellViewModel.CommandCreateTaskFollowPath = _commandCreateTaskFollowPath;
        }

        private void Project_OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "SelectedScenario":
                    _commandCreateTaskFollowPath.RaiseCanExecuteChanged();
                    break;
                case "ReferenceLatLng":
                    _mapViewModel.UpdateReference(Project.ReferenceLatLng);
                    foreach (MyShip ship in Project.MyShips)
                    {
                        _mapViewModel.UpdateMarkerPosition(ship, 0, 0);
                        _mapViewModel.DeleteRoute(ship);
                    }

                    foreach (TargetShip ship in Project.TargetShips)
                    {
                        _mapViewModel.UpdateMarkerPosition(ship, ship.CurrentX, ship.CurrentY);

                        Location temp = Distance.GetLatLng(new Point3D(ship.CurrentX, ship.CurrentY, 0));
                        ship.CurrentLat = temp.Latitude;
                        ship.CurrentLon = temp.Longitude;
                    }

                    foreach (Obstacle obstacle in Project.Obstacles)
                    {
                        _mapViewModel.UpdateObstacle(obstacle);
                    }
                    break;
            }
        }

        private void Tasks_OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (TaskBase newItem in e.NewItems)
                    {
                        // Task의 Marker 생성
                        _mapViewModel.CreateMarker(newItem);

                        newItem.PropertyChanged += NewTask_PropertyChanged;
                        newItem.Waypoints.CollectionChanged += Waypoints_OnCollectionChanged;
                        Waypoints_OnCollectionChanged(newItem,
                            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItem.Waypoints));

                        if (newItem.Waypoints.Count > 0)
                        {
                            // newItem.Platform.CurrentX = newItem.Waypoints[0].Location.X;
                            // newItem.Platform.CurrentY = newItem.Waypoints[0].Location.Y;
                            newItem.Platform.CurrentHeading = newItem.Heading;
                            //_mapViewModel.UpdateMarkerPosition(newItem.Platform, newItem.Waypoints[0].Location.X, newItem.Waypoints[0].Location.Y);
                        }
                    }

                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (TaskBase oldItem in e.OldItems)
                    {
                        _mapViewModel.DeleteRoute(oldItem);
                        oldItem.Waypoints.CollectionChanged -= Waypoints_OnCollectionChanged;
                        oldItem.PropertyChanged -= NewTask_PropertyChanged;
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    break;

                case NotifyCollectionChangedAction.Move:
                    break;

                case NotifyCollectionChangedAction.Reset:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Scenarios_OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (ScenarioBase newItem in e.NewItems)
                    {
                        newItem.Tasks.CollectionChanged += Tasks_OnCollectionChanged;
                        Tasks_OnCollectionChanged(newItem,
                            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, newItem.Tasks));
                    }

                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (ScenarioBase oldItem in e.OldItems)
                    {
                        while (oldItem.Tasks.Count > 0)
                        {
                            oldItem.Tasks.RemoveAt(0);
                        }
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    break;

                case NotifyCollectionChangedAction.Move:
                    break;

                case NotifyCollectionChangedAction.Reset:
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void Waypoints_OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (Waypoint newItem in e.NewItems)
                        _dispatcherService.Invoke(() =>
                        {
                            var currentTask = newItem.Task;
                            if (currentTask == null)
                                return;

                            //
                            // 경유점
                            //

                            // 경유점 생성
                            _mapViewModel.CreateMarker(newItem);

                            // 경유점 위치 조정
                            _mapViewModel.UpdateMarkerPosition(newItem, newItem.Location.X, newItem.Location.Y);

                            //
                            // 경유점 경로 다시 그리기
                            //
                            _mapViewModel.UpdateRoute(currentTask);

                            //
                            // USV 초기 위치 설정
                            //
                            // if (currentTask.Waypoints.IndexOf(newItem) == 0)
                            // {
                            //     currentTask.UpdatePlatform();
                            //
                            //     // 이전 점과 현재 이동된 점을 연결하는 버그 임시 수정.
                            //     _mapViewModel.DeleteRoute(currentTask.Platform);
                            //
                            //     //_mapViewModel.UpdateMarkerPosition(currentTask.Platform, newItem.Location.X, newItem.Location.Y);
                            //     // newItem.Offset 에 맞게 usv 초기 각도 변경
                            // }

                            // Waypoint PropertyChanged 이벤트 처리
                            newItem.PropertyChanged += WaypointOnPropertyChanged;
                        });

                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (Waypoint oldItem in e.OldItems)
                    {
                        //// 검색
                        //var res = _modelBuilderViewModel.Waypoints.Where(i => i == oldItem);

                        //// 제거
                        //foreach (var item in res)
                        //	_modelBuilderViewModel.Waypoints.Remove(item);

                        var currentTask = oldItem.Task;
                        if (currentTask == null)
                            return;

                        _mapViewModel.DeleteWaypoint(oldItem);
                        oldItem.PropertyChanged -= WaypointOnPropertyChanged;

                        //
                        // 경유점 경로 다시 그리기
                        //
                        _mapViewModel.UpdateRoute(currentTask);
                    }

                    break;

                case NotifyCollectionChangedAction.Replace:
                    break;

                case NotifyCollectionChangedAction.Move:
                    break;

                case NotifyCollectionChangedAction.Reset:
                    //_modelBuilderViewModel.Waypoints.Clear();
                    break;

                default:
                    break;
            }
        }

        private void WaypointOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Waypoint waypoint = sender as Waypoint;

            switch (e.PropertyName)
            {
                case "Location":
                    _mapViewModel.UpdateMarkerPosition(waypoint, waypoint.Location.X, waypoint.Location.Y);
                    _mapViewModel.UpdateRoute(waypoint.Task);

                    if (waypoint.Task.Waypoints.IndexOf(waypoint) == 0)
                    {
                        if (waypoint.Task.Waypoints.Count ==0)
                            break;

                        Vector2D platformPosition = new Vector2D(waypoint.Task.Platform.CurrentX, waypoint.Task.Platform.CurrentY);
                        Vector2D dist = waypoint.Task.Waypoints[0].Location.ToVector2D() - platformPosition;
                        double bearing = Math.Atan2(dist.X, dist.Y);
                        waypoint.Task.Heading = bearing * 180 / Math.PI;
                    } 
                    break;
            }
        }

        internal void ProjectOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "SelectedScenario":
                    _commandCreateTaskFollowPath.RaiseCanExecuteChanged();

                    if (Project.SelectedScenario == null)
                        return;

                    foreach (TaskBase task in Project.SelectedScenario.Tasks)
                    {
                        task.UpdatePlatform();

                        // 이전 점과 현재 이동된 점을 연결하는 버그 임시 수정.
                        _mapViewModel.DeleteRoute(task.Platform);
                    }
                    break;
            }
        }
    }
}