using SyDLab.Usv.Simulator.Applications.ViewModels;
using SyDLab.Usv.Simulator.Domain;
using SyDLab.Usv.Simulator.Domain.Logs;
using SyDLab.Usv.Simulator.Domain.Models;
using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Waf.Applications;

namespace SyDLab.Usv.Simulator.Applications.Controllers
{
    [Export(typeof(IModuleController)), Export]
    internal class ModuleController : IModuleController
    {
        private readonly DelegateCommand _commandExecute;
        private readonly DelegateCommand _commSetting;
        private readonly MapViewModel _mapViewModel;
        private readonly ModelBuilderViewModel _modelBuilderViewModel;
        private readonly PropertyViewModel _propertyViewModel;
        private readonly ShellViewModel _shellViewModel;
        private readonly SimulationController _simulationController;
        private readonly SensorController _sensorController;
        private readonly PlatformController _platformController;
        private readonly TerminalViewModel _terminalViewModel;
        private readonly RadarViewModel _radarViewModel;
        private readonly GraphViewModel _graphViewModel;
        private readonly FileController _fileController;
        private readonly ScenarioController _scenarioController;
        private readonly ObstacleController _obstacleController;
        private readonly CollisionRiskController _collisionRiskController;
        private readonly SettingViewModel _settingViewModel;
        private readonly MonitorViewModel _monitorViewModel;



        [ImportingConstructor]
        public ModuleController(
            // Services
            // Controllers
            SimulationController simulationController, PlatformController platformController, FileController fileController, ScenarioController scenarioController,
            // ViewModels
            ShellViewModel shellViewModel, ModelBuilderViewModel modelBuilderViewModel, PropertyViewModel propertyViewModel,
            MapViewModel mapViewModel, TerminalViewModel terminalViewModel, GraphViewModel graphViewModel, RadarViewModel radarViewModel,
            SensorController sensorController, ObstacleController obstacleController, CollisionRiskController collisionRiskController, SettingViewModel settingViewModel,
            MonitorViewModel monitorViewModel)
        {
            _shellViewModel = shellViewModel;
            _modelBuilderViewModel = modelBuilderViewModel;
            _propertyViewModel = propertyViewModel;
            _mapViewModel = mapViewModel;
            _terminalViewModel = terminalViewModel;
            _graphViewModel = graphViewModel;
            _radarViewModel = radarViewModel;
            _settingViewModel = settingViewModel;
            _monitorViewModel = monitorViewModel;

            _simulationController = simulationController;
            _platformController = platformController;
            _fileController = fileController;
            _scenarioController = scenarioController;
            _sensorController = sensorController;
            _obstacleController = obstacleController;
            _collisionRiskController = collisionRiskController;
            

            _commandExecute = new DelegateCommand(ExecuteExecuteNewLine);
            _commSetting = new DelegateCommand(ExecuteCommSetting);


            SelectedObjects.CollectionChanged += SelectedObjects_OnCollectionChanged;
            _shellViewModel.PropertyChanged += OnShellViewPropertyChanged;
        }

        private void OnShellViewPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            string str;
            switch (e.PropertyName)
            {
                case "SelectedSimulationSpeed":
                    str = (sender as ShellViewModel)?.SelectedSimulationSpeed;
                    double rate=1;
                    switch (str)
                    {
                        case "x0.5":
                            rate = 0.5;
                            break;
                        case "x1":
                            rate = 1;
                            break;
                        case "x2":
                            rate = 2;
                            break;
                        case "x4":
                            rate = 4;
                            break;
                        case "x8":
                            rate = 8;
                            break;
                        case "∞":
                            rate = 100;
                            break;
                    }
                    _simulationController.SimualationSpeed = 1/rate;
                    break;
            }
        }

        private void SelectedObjects_OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (SelectedObjects != null)
                Debug.WriteLine("SelectedObjects: {0}", SelectedObjects.Count);

            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    object lastSelectedObject = null;

                    foreach (var item in e.NewItems)
                    {
                        if (item is System.Waf.Foundation.Model)
                            lastSelectedObject = item;
                    }

                    if (lastSelectedObject != null)
                        _propertyViewModel.LastSelectedObject = lastSelectedObject;
                    break;

                case NotifyCollectionChangedAction.Remove:
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

        internal Project Project { get; private set; }
        internal LogManager LogManager { get; private set; }
        public ObservableCollection<object> SelectedObjects { get; } = new ObservableCollection<object>();

        public void Initialize()
        {
            Project = Singleton<Project>.UniqueInstance;
            Project.PropertyChanged += _scenarioController.ProjectOnPropertyChanged;

            LogManager = Singleton<LogManager>.UniqueInstance;

            //
            // ViewModel 연결
            //
            _terminalViewModel.CommandExecute = _commandExecute;
            _shellViewModel.CommSetting = _commSetting;

            _propertyViewModel.SelectedObjects = SelectedObjects;

            _modelBuilderViewModel.Nodes.Add(Project);
            _modelBuilderViewModel.SelectedObjects = SelectedObjects;

            //
            // 하위 Controller 초기화
            //
            // 순서 주의
            _simulationController.Initialize();
            _platformController.Initialize();
            _fileController.Initialize();
            _scenarioController.Initialize();
            _sensorController.Initialize();
            _obstacleController.Initialize();
            _collisionRiskController.Initialize();
            _monitorViewModel.Initialize();
       }

        private void ExecuteCommSetting()
        {
            _settingViewModel.Show(_shellViewModel);
        }

        

        public void Run()
        {
            _shellViewModel.ContentModelBuilderView = _modelBuilderViewModel.View;
            _shellViewModel.ContentPropertyView = _propertyViewModel.View;
            _shellViewModel.ContentMapView = _mapViewModel.View;
            _shellViewModel.ContentTerminalView = _terminalViewModel.View;
            _shellViewModel.ContentGraphView = _graphViewModel.View;
            _shellViewModel.ContentRadarView = _radarViewModel.View;
            _shellViewModel.ContentMonitorView = _monitorViewModel.View;

            _shellViewModel.Show();
        }

        public void Shutdown()
        {
            _simulationController.ThreadShutDown();
            _shellViewModel.Close();
        }

        private void ExecuteExecuteNewLine(object obj)
        {
            var logManager = Singleton<LogManager>.UniqueInstance;
            logManager.AddLog(obj.ToString());
        }
    }
}