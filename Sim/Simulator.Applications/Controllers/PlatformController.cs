using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Waf.Applications;
using MathNet.Spatial.Euclidean;
using Microsoft.Maps.MapControl.WPF;
using SyDLab.Usv.Simulator.Applications.ViewModels;
using SyDLab.Usv.Simulator.Domain;
using SyDLab.Usv.Simulator.Domain.Models;
using SyDLab.Usv.Simulator.Domain.Models.Platforms;

namespace SyDLab.Usv.Simulator.Applications.Controllers
{
    [Export]
    public class PlatformController
    {
        private readonly DelegateCommand _commandCreatePlatform;
        private readonly DelegateCommand _commandRearrangeShips;

        private readonly MapViewModel _mapViewModel;
        private readonly ExportFactory<PlatformEditorViewModel> _platformEditorViewModelFactory;
        private readonly ShellViewModel _shellViewModel;
        private readonly RadarViewModel _radarViewModel;
        private ShipDynamicsViewModel _shipDynamicsViewModel;
        private readonly SimulationController _simulationController;
        private DelegateCommand _commandShipDynamics;

        [ImportingConstructor]
        public PlatformController(ShellViewModel shellViewModel, MapViewModel mapViewModel,
            ExportFactory<PlatformEditorViewModel> platformEditorViewModelFactory, RadarViewModel radarViewModel,
            ObstacleEditorViewModel obstacleEditorViewModel, ShipDynamicsViewModel shipDynamicsViewModel, SimulationController simulationController)
        {
            _shellViewModel = shellViewModel;
            _mapViewModel = mapViewModel;
            _platformEditorViewModelFactory = platformEditorViewModelFactory;
            //_obstacleEditorViewModelFactory = obstacleEditorViewModelFactory;
            _radarViewModel = radarViewModel;
            _simulationController = simulationController;
            _shipDynamicsViewModel = shipDynamicsViewModel;
            _shipDynamicsViewModel.PropertyChanged += shipDynamicsPropertyChanged;
            // Create commands
            _commandCreatePlatform = new DelegateCommand(ExecuteCreatePlatform, CanExecuteCreatePlatform);
            _commandRearrangeShips = new DelegateCommand(ExecuteRearrangeShips, CanExecuteRearrangeShips);
            _commandShipDynamics = new DelegateCommand(ChangeShipDynamics);
        }

        private void shipDynamicsPropertyChanged(object sender, PropertyChangedEventArgs e)
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

        private void ChangeShipDynamics()
        {
            _shipDynamicsViewModel.ShowDialog(_shellViewModel.View);
        }

        private Project Project => Singleton<Project>.UniqueInstance;

        private bool CanExecuteCreatePlatform()
        {
            return true;
        }

        private void ExecuteCreatePlatform()
        {
            var rho = 1014.0;
            var L = 48.0;
            var U_norm = 8.0; // knot
            var xg = -3.38;
            var m = 634.9 * 1e-5 * (0.5 * rho * Math.Pow(L, 3.0));
            var Izz = 2.63 * 1e-5 * (0.5 * rho * Math.Pow(L, 5.0));
            var range = 1000;

            var r = new Random((int)DateTime.Now.Ticks);

            var newPlatform = new TargetShip(Project)
            {
                DisplayName = string.Format($"Target Ship {Project.TargetShips.Count}"),
                Speed = U_norm,
                Length = L,
                InitialX = r.NextDouble() * 2 * range - range,
                InitialY = r.NextDouble() * 2 * range - range,
                InitialHeading = r.NextDouble() * 360,
                RangeDistance = range,
                HeadingVelocity = 0.3 + r.NextDouble() * 0.4,
                MMSI=r.Next(100000000,999999999),
            };

            var platformInitializerViewModel = _platformEditorViewModelFactory.CreateExport().Value;

            platformInitializerViewModel.Platform = newPlatform;

            if (platformInitializerViewModel.ShowDialog(_shellViewModel.View))
            {
                Project.TargetShips.Add(platformInitializerViewModel.Platform as TargetShip);
                platformInitializerViewModel.Platform.VisualizedHeading = platformInitializerViewModel.Platform.InitialHeading;


                _mapViewModel.UpdateMarkerPosition(newPlatform, newPlatform.CurrentX, newPlatform.CurrentY);
                // 이전 점과 현재 이동된 점을 연결하는 버그 임시 수정.
                _mapViewModel.DeleteRoute(newPlatform);

                _shellViewModel.CommandRearrangeShips.RaiseCanExecuteChanged();
            }
        }

        public void Initialize()
        {
            // Connect commands
            _shellViewModel.CommandCreatePlatform = _commandCreatePlatform;
            _shellViewModel.CommandRearrangeShips = _commandRearrangeShips;
            _shellViewModel.CommandShipDynamics = _commandShipDynamics;
            // Event
            Project.MyShips.CollectionChanged += PlatformsOnCollectionChanged;
            Project.TargetShips.CollectionChanged += PlatformsOnCollectionChanged;

            // Add my ship (default)
            var newPlatform = new MyShip(Project)
            {
                DisplayName = string.Format($"Own ship"),
            };

            Project.MyShips.Add(newPlatform);

            _mapViewModel.UpdateReference(Project.ReferenceLatLng, true);
            _mapViewModel.UpdateMarkerPosition(newPlatform, 0, 0);
            // 이전 점과 현재 이동된 점을 연결하는 버그 임시 수정.
            _mapViewModel.DeleteRoute(newPlatform);

            _shellViewModel.CommandRearrangeShips.RaiseCanExecuteChanged();
        }

        private bool CanExecuteRearrangeShips()
        {
            return true;
        }

        private void ExecuteRearrangeShips()
        {
            var r = new Random((int)DateTime.Now.Ticks);

            if (Project.TargetShips.Count == 0)
                return;

            foreach (var item in Project.TargetShips)
            {
                item.InitialHeading = r.NextDouble() * 360;
                item.InitialX = r.NextDouble() * 2*item.RangeDistance - item.RangeDistance;
                item.InitialY = r.NextDouble() * 2 * item.RangeDistance - item.RangeDistance;
                _mapViewModel.UpdateMarkerPosition(item, item.CurrentX, item.CurrentY);

                // 이전 점과 현재 이동된 점을 연결하는 버그 임시 수정.
                item.HistoryPosition.Clear();
                item.HistoryAISData.Clear();
                _mapViewModel.DeleteRoute(item);
            }
        }

        private void PlatformsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (PlatformBase newItem in e.NewItems)
                    {
                        if (newItem is MyShip)
                            newItem.PropertyChanged += MyShip_OnPropertyChanged;
                        else
                            newItem.PropertyChanged += Platform_OnPropertyChanged;

                        // Marker 생성
                        _mapViewModel.CreateMarker(newItem);
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (PlatformBase oldItem in e.OldItems)
                    {
                        if (oldItem is MyShip)
                            oldItem.PropertyChanged -= MyShip_OnPropertyChanged;
                        else
                            oldItem.PropertyChanged -= Platform_OnPropertyChanged;

                        _mapViewModel.DeleteMarker(oldItem);
                    }
                    break;
            }
        }

        private void MyShip_OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(sender is MyShip platform))
                return;

            switch (e.PropertyName)
            {
                case "InitialLon":
                case "InitialLat":
                    _mapViewModel.UpdateReference(new Point2D(platform.InitialLat, platform.InitialLon), true);
                    _mapViewModel.UpdateMarkerPosition(platform, 0, 0);
                    // 이전 점과 현재 이동된 점을 연결하는 버그 임시 수정.
                    _mapViewModel.DeleteRoute(platform);

                    foreach (TargetShip ship in Project.TargetShips)
                    {
                        UpdateXY(ship);
                    }
                    break;

                case "Resolution":
                    _radarViewModel.Resolution = (sender as MyShip).Resolution;
                    break;

                case "SeaClutter":
                    _radarViewModel.SeaClutter = (sender as MyShip).SeaClutter;
                    break;

                case "CurrentX":
                case "CurrentY":
                {
                    //UpdateMyShipMarker();
                    _mapViewModel.UpdateMarkerPosition(platform, platform.CurrentX, platform.CurrentY);

                    if (_simulationController.CurrentStep == 0)
                        // 이전 점과 현재 이동된 점을 연결하는 버그 임시 수정.
                        _mapViewModel.DeleteRoute(platform);
                }
                    break;
                case "Heading":
                {
                    UpdateVisualizedHeading(platform);
                }
                    break;
                case "ModelType":
                    platform.UpdateHullParam();
                    break;
            }
        }

        private void UpdateLonLat(TargetShip ship)
        {
            Location location = Distance.GetLatLng(new Point3D(ship.InitialX, ship.InitialY,  0));

            ship.InitialLat = location.Latitude;
            ship.InitialLon = location.Longitude;
        }

        private void UpdateXY(TargetShip ship)
        {
            Point3D p = Distance.GetPosition(ship.InitialLonLat);

            ship.InitialX = p.X;
            ship.InitialY = p.Y;
        }

        private void Platform_OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!(sender is PlatformBase platform))
                return;

            switch (e.PropertyName)
            {
                case "CurrentX":
                case "CurrentY":
                {
                    //UpdateMyShipMarker();
                    _mapViewModel.UpdateMarkerPosition(platform, platform.CurrentX, platform.CurrentY);

                    if (_simulationController.CurrentStep == 0)
                        // 이전 점과 현재 이동된 점을 연결하는 버그 임시 수정.
                        _mapViewModel.DeleteRoute(platform);
                }
                    break;
                case "Heading":
                {
                    UpdateVisualizedHeading(platform);
                }
                    break;

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
    }
}