using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Text;
using System.Waf.Applications;
using System.Windows;
using MathNet.Spatial.Euclidean;
using Microsoft.Win32;
using SyDLab.Usv.Simulator.Applications.Services;
using SyDLab.Usv.Simulator.Applications.ViewModels;
using SyDLab.Usv.Simulator.Domain;
using SyDLab.Usv.Simulator.Domain.Logs;
using SyDLab.Usv.Simulator.Domain.Models;
using SyDLab.Usv.Simulator.Domain.Models.Platforms;
using SyDLab.Usv.Simulator.Domain.Models.Tasks;

namespace SyDLab.Usv.Simulator.Applications.Controllers
{
    [Export]
    public class FileController
    {
        private readonly DelegateCommand _commandCreateProject;
        private readonly DelegateCommand _commandLoadProject;
        private readonly DelegateCommand _commandSaveProject;
        private readonly IDispatcherService _dispatcherService;
        private readonly ShellViewModel _shellViewModel;
        private readonly MapViewModel _mapViewModel;
        private ShipDynamicsViewModel _shipDynamicsViewModel;

        [ImportingConstructor]
        public FileController(IDispatcherService dispatcherService, ShellViewModel shellViewModel, MapViewModel mapViewModel, ShipDynamicsViewModel shipDynamicsViewModel)
        {
            _dispatcherService = dispatcherService;
            _shellViewModel = shellViewModel;
            _mapViewModel = mapViewModel;
            _shipDynamicsViewModel = shipDynamicsViewModel;
            _commandCreateProject = new DelegateCommand(ExecuteCreateProject, CanExecuteCreateProject);
            _commandLoadProject = new DelegateCommand(ExecuteLoadProject, CanExecuteLoadProject);
            _commandSaveProject = new DelegateCommand(ExecuteSaveProject, CanExecuteSaveProject);
            _shipDynamicsViewModel.PropertyChanged += ShipDynamicsViewModelOnPropertyChanged;
        }

        private void ShipDynamicsViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            foreach (MyShip myShip in Project.MyShips)
            {
                myShip.Length = _shipDynamicsViewModel.Length;
                myShip.Mass = _shipDynamicsViewModel.M;
                myShip.Inertia = _shipDynamicsViewModel.Izz;
                myShip.Speed = _shipDynamicsViewModel.UNorm;
                myShip.CogX = _shipDynamicsViewModel.Xg;
                myShip.CurrentX = 0;
                myShip.CurrentY = 0;
                myShip.LimitOfRudderAngle = _shipDynamicsViewModel.DelSat;
                myShip.LimitOfRudderAngleDot = _shipDynamicsViewModel.DelSatDot;
                myShip.Coefficient = _shipDynamicsViewModel.Coefficient;
            }
        }


        private LogManager LogManager => Singleton<LogManager>.UniqueInstance;
        public Project Project => Singleton<Project>.UniqueInstance;

        private bool CanExecuteSaveProject()
        {
            return true;
        }

        private void ExecuteSaveProject()
        {
            var fileName = "";
            var filePath = "";

            if (!string.IsNullOrEmpty(_shellViewModel.ProjectFileName))
            {
                fileName = Path.GetFileName(_shellViewModel.ProjectFileName);
                filePath = Path.GetFullPath(_shellViewModel.ProjectFileName);
            }

            var dlg = new SaveFileDialog
            {
                DefaultExt = ".json",
                Filter = "Project File(.json)|*.json",
                FileName = fileName,
                InitialDirectory = filePath
            };

            if (dlg.ShowDialog() == true)
            {
                JsonSerializerService.FileSerializer<Project>(dlg.FileName, Project);
                _shellViewModel.ProjectFileName = dlg.SafeFileName;
                LogManager.WriteLine($"Succeed to save current project to the file '{_shellViewModel.ProjectFileName}'.");
            }

            // 오차 확인용 데이터 저장, 파일이 열려있으면 저장안됨
            try
            {
                string dataPath = filePath + "Data.csv";
                string textLabel = "track data";
                File.WriteAllText(dataPath, textLabel, Encoding.Default);
                foreach (var item in Project.TargetShips)
                {
                    File.AppendAllText(dataPath, "\n" + item.DisplayName + "\n", Encoding.Default);
                    File.AppendAllText(dataPath, "Path\n", Encoding.Default);
                    foreach (var state in item.HistoryPosition)
                    {
                        File.AppendAllText(dataPath, state.Time + ", " + state.Position.X + ", " + state.Position.Y + "\n", Encoding.Default);
                    }
                    File.AppendAllText(dataPath, "AIS Data\n", Encoding.Default);
                    foreach (var state in item.HistoryAISData)
                    {
                        File.AppendAllText(dataPath, state.Time + ", " + state.Position.X + ", " + state.Position.Y + "\n", Encoding.Default);
                    }
                    File.AppendAllText(dataPath, "Radar Data\n", Encoding.Default);
                    foreach (var state in item.HistoryRadarData)
                    {
                        File.AppendAllText(dataPath, state.Time + ", " + state.Position.X + ", " + state.Position.Y + "\n", Encoding.Default);
                    }
                    File.AppendAllText(dataPath, "Fused Data\n", Encoding.Default);
                    foreach (var state in item.HistoryFusedData)
                    {
                        File.AppendAllText(dataPath, state.Time + ", " + state.Position.X + ", " + state.Position.Y + "\n", Encoding.Default);
                    }
                }
            }
            catch
            {
            }
        }

        private bool CanExecuteLoadProject()
        {
            return true;
        }

        private void ExecuteLoadProject()
        {
            var fileName = "";
            var filePath = "";

            if (!string.IsNullOrEmpty(_shellViewModel.ProjectFileName))
            {
                fileName = Path.GetFileName(_shellViewModel.ProjectFileName);
                filePath = Path.GetFullPath(_shellViewModel.ProjectFileName);
            }

            var dlg = new OpenFileDialog
            {
                DefaultExt = ".json",
                Filter = "Project File(.json)|*.json",
                FileName = fileName,
                InitialDirectory = filePath
            };

            // 설정된 파일 저장
            if (dlg.ShowDialog() == true)
            {
                ResetProject();
                var project = JsonSerializerService.FileDeserializer<Project>(dlg.FileName);
                Project.Clone(project);
                _shellViewModel.ProjectFileName = dlg.SafeFileName;
                LogManager.WriteLine($"Succeed to load current project to the file '{_shellViewModel.ProjectFileName}'.");

                foreach (MyShip ship in Project.MyShips)
                {
                    _mapViewModel.UpdateReference(new Point2D(ship.InitialLat, ship.InitialLon), true);
                    _mapViewModel.UpdateMarkerPosition(ship, 0, 0);
                    // 이전 점과 현재 이동된 점을 연결하는 버그 임시 수정.
                    _mapViewModel.DeleteRoute(ship);
                    UpdateVisualizedHeading(ship);

                    ship.UpdateHullParam();
                    ship.UpdateInitialParam();
                }

                foreach (TargetShip ship in Project.TargetShips)
                {
                    _mapViewModel.UpdateMarkerPosition(ship, ship.CurrentX, ship.CurrentY);
                    UpdateVisualizedHeading(ship);
                }

                foreach (Obstacle obstacle in Project.Obstacles)
                {
                    _mapViewModel.UpdateObstacle(obstacle);
                }
            }
        }

        private void UpdateVisualizedHeading(PlatformBase platform)
        {
            if (_mapViewModel.IsHeadingFixed)
            {
                if (platform is MyShip)
                    platform.VisualizedHeading = 0;
                else if (platform is TargetShip)
                    platform.VisualizedHeading = platform.CurrentHeading - Project.MyShips[0].CurrentHeading;
            }
            else
            {
                platform.VisualizedHeading = platform.CurrentHeading;
            }
        }

        private bool CanExecuteCreateProject()
        {
            return true;
        }

        private void ExecuteCreateProject()
        {
            var result = MessageBox.Show("Are you sure to create new project without saving current project?", "Create New Project", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result == MessageBoxResult.No)
                return;

            // Reset file name
            _shellViewModel.ProjectFileName = "Not saved";

            // Reset model
            ResetProject();
            AddMyShip();
        }

        private void AddMyShip()
        {
            var newPlatform = new MyShip(Project)
            {
                DisplayName = string.Format($"Own ship"),
            };

            Project.MyShips.Add(newPlatform);
            _mapViewModel.UpdateReference(new Point2D(37.0, 125.5), true);
            _mapViewModel.CreateMarker(newPlatform);
            _mapViewModel.UpdateMarkerPosition(newPlatform, 0, 0);
            // 이전 점과 현재 이동된 점을 연결하는 버그 임시 수정.
            _mapViewModel.DeleteRoute(newPlatform);
        }

        private void ResetProject()
        {
            // Reset project
            Project.DisplayName = "Empty Project";

            // Reset platforms
            while (Project.MyShips.Count > 0)
            {
                Project.MyShips.RemoveAt(0);
            }

            while (Project.TargetShips.Count > 0)
            {
                Project.TargetShips.RemoveAt(0);
            }

            while (Project.Obstacles.Count > 0)
            {
                Project.Obstacles.RemoveAt(0);
            }

            // Reset scenario
            while (Project.Scenarios.Count > 0)
            {
                Project.Scenarios.RemoveAt(0);
            }
        }

        public void Initialize()
        {
            _shellViewModel.CommandCreateProject = _commandCreateProject;
            _shellViewModel.CommandLoadProject = _commandLoadProject;
            _shellViewModel.CommandSaveProject = _commandSaveProject;
        }
    }
}