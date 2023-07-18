using System.ComponentModel.Composition;
using System.Waf.Applications;
using SyDLab.Usv.Simulator.Applications.Views;
using SyDLab.Usv.Simulator.Domain;
using SyDLab.Usv.Simulator.Domain.Logs;

namespace SyDLab.Usv.Simulator.Applications.ViewModels
{
    [Export]
    public class ShellViewModel : ViewModel<IShellView>
    {
        private DelegateCommand _commandConnect;
        private DelegateCommand _commandCreatePlatform;
        private DelegateCommand _commandCreateObstacle;
        private DelegateCommand _commandCreateProject;
        private DelegateCommand _commandCreateScenarioFollowPath;
        private DelegateCommand _commandCreateTaskFollowPath;
        private DelegateCommand _commandShipDynamics;
        private DelegateCommand _commandLoadProject;
        private DelegateCommand _commandPlay;
        private DelegateCommand _commandRearrangeShips;
        private DelegateCommand _commandPause;
        private DelegateCommand _commandSaveProject;
        private DelegateCommand _commandStop;
        // private DelegateCommand _commandCrConnect;
        private DelegateCommand _commSetting;
        private DelegateCommand _commandSetReference;
        private object _contentGraphView;
        private object _contentMapView;
        private object _contentRadarView;
        private object _contentModelBuilderView;
        private object _contentPropertyView;
        private object _contentTerminalView;
        private object _contentMonitorView;
        private double _currentTime;
        private bool _isConnected;
        private bool _isTcpServerConnected;
        // private bool _isCrConnected;
        private double _maxTime;
        private string _projectFileName = "Not saved";
        private string[] _simulationSpeed = new string[6] { "x0.5", "x1", "x2", "x4", "x8", "∞" };
        private string _selectedSimulationSpeed = "x1";
        private bool _externalControllerChecked = true;
        private bool _connectedVdoChecked = false;
        private bool _connectedVdmChecked = false;
        private bool _sendoutVdoChecked;
        private bool _sendoutVdmChecked;
        private bool _simpleVersionChecked;

        [ImportingConstructor]
        public ShellViewModel(IShellView view) : base(view)
        {
        }

        public DelegateCommand CommandConnect
        {
            get => _commandConnect;
            set => SetProperty(ref _commandConnect, value);
        }

        public DelegateCommand CommandCreatePlatform
        {
            get => _commandCreatePlatform;
            set => SetProperty(ref _commandCreatePlatform, value);
        }

        public DelegateCommand CommandCreateObstacle
        {
            get => _commandCreateObstacle;
            set => SetProperty(ref _commandCreateObstacle, value);
        }

        public DelegateCommand CommandCreateProject
        {
            get => _commandCreateProject;
            set => SetProperty(ref _commandCreateProject, value);
        }

        public DelegateCommand CommandCreateScenarioFollowPath
        {
            get => _commandCreateScenarioFollowPath;
            set => SetProperty(ref _commandCreateScenarioFollowPath, value);
        }
        
        public DelegateCommand CommandCreateTaskFollowPath
        {
            get => _commandCreateTaskFollowPath;
            set => SetProperty(ref _commandCreateTaskFollowPath, value);
        }

        public DelegateCommand CommandShipDynamics
        {
            get => _commandShipDynamics;
            set => SetProperty(ref _commandShipDynamics, value);
        }
        public DelegateCommand CommandLoadProject
        {
            get => _commandLoadProject;
            set => SetProperty(ref _commandLoadProject, value);
        }

        public DelegateCommand CommandPlay
        {
            get => _commandPlay;
            set => SetProperty(ref _commandPlay, value);
        }

        public DelegateCommand CommandRearrangeShips
        {
            get => _commandRearrangeShips;
            set => SetProperty(ref _commandRearrangeShips, value);
        }

        public DelegateCommand CommandStop
        {
            get => _commandStop;
            set => SetProperty(ref _commandStop, value);
        }

        public DelegateCommand CommandSaveProject
        {
            get => _commandSaveProject;
            set => SetProperty(ref _commandSaveProject, value);
        }

        public DelegateCommand CommandPause
        {
            get => _commandPause;
            set => SetProperty(ref _commandPause, value);
        }

        public DelegateCommand CommSetting
        {
            get => _commSetting;
            set => SetProperty(ref _commSetting, value);
        }

        public DelegateCommand CommandSetReference
        {
            get => _commandSetReference;
            set => SetProperty(ref _commandSetReference, value);
        }

        public string ConnectedOrNot => IsConnected ? "Disconnect" : "Connect";

        // public string CRConnectedOrNot => IsCrConnected ? "Disconnect" : "Connect";

        public object ContentGraphView
        {
            get => _contentGraphView;
            set => SetProperty(ref _contentGraphView, value);
        }

        public object ContentMapView
        {
            get => _contentMapView;
            set => SetProperty(ref _contentMapView, value);
        }

        public object ContentRadarView
        {
            get => _contentRadarView;
            set => SetProperty(ref _contentRadarView, value);
        }

        public object ContentModelBuilderView
        {
            get => _contentModelBuilderView;
            set => SetProperty(ref _contentModelBuilderView, value);
        }

        public object ContentPropertyView
        {
            get => _contentPropertyView;
            set => SetProperty(ref _contentPropertyView, value);
        }

        public object ContentTerminalView
        {
            get => _contentTerminalView;
            set => SetProperty(ref _contentTerminalView, value);
        }

        public double CurrentTime
        {
            get => _currentTime;
            set => SetProperty(ref _currentTime, value);
        }

        public bool IsConnected
        {
            get => _isConnected;
            set
            {
                SetProperty(ref _isConnected, value);
                RaisePropertyChanged(nameof(ConnectedOrNot));
            }
        }
        
        public bool IsTcpServerConnected
        {
            get { return _isTcpServerConnected; }
            set
            {
                SetProperty(ref _isTcpServerConnected, value);
            }
        }

        // public bool IsCrConnected
        // {
        //     get => _isCrConnected;
        //     set
        //     {
        //         SetProperty(ref _isCrConnected, value);
        //         RaisePropertyChanged(nameof(CRConnectedOrNot));
        //     }
        // }

        public bool IsLineWrapped
        {
            get => Singleton<LogManager>.UniqueInstance.CanMultiline;
            set => Singleton<LogManager>.UniqueInstance.CanMultiline = value;
        }

        public double MaxTime
        {
            get => _maxTime;
            set => SetProperty(ref _maxTime, value);
        }

        public string ProjectFileName
        {
            get => _projectFileName;
            set => SetProperty(ref _projectFileName, value);
        }

        public string[] SimulationSpeed
        {
            get => _simulationSpeed;
            set => SetProperty(ref _simulationSpeed, value);
        }

        public string SelectedSimulationSpeed
        {
            get => _selectedSimulationSpeed;
            set => SetProperty(ref _selectedSimulationSpeed, value);
        }

        public bool ExternalControllerChecked
        {
            get => _externalControllerChecked;
            set => SetProperty(ref _externalControllerChecked, value);
        }
 
        public object ContentMonitorView
        {
            get => _contentMonitorView; 
            set => SetProperty(ref _contentMonitorView, value);
        }
 
        public bool ConnectedVdoChecked
        {
            get => _connectedVdoChecked;
            set => SetProperty(ref _connectedVdoChecked, value);
        }
        public bool ConnectedVdmChecked
        {
            get => _connectedVdmChecked;
            set => SetProperty(ref _connectedVdmChecked, value);
        }
        public bool SendoutVdoChecked
        {
            get => _sendoutVdoChecked;
            set => SetProperty(ref _sendoutVdoChecked, value);
        }

        public bool SendoutVdmChecked
        {
            get => _sendoutVdmChecked;
            set => SetProperty(ref _sendoutVdmChecked, value);
        }

        public bool SimpleVersionChecked
        {
            get => _simpleVersionChecked;
            set => SetProperty(ref _simpleVersionChecked, value);
        }
        public void Show()
        {
            ViewCore.Show();
        }

        public void Close()
        {
            ViewCore.Close();
        }
    }
}