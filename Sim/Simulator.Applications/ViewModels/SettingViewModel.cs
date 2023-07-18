using System;
using System.Collections.Generic;
using System.ComponentModel;
using SyDLab.Usv.Simulator.Applications.Views;
using System.ComponentModel.Composition;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Waf.Applications;
using System.Windows;
using System.Windows.Data;
using SyDLab.Usv.Simulator.Domain;
using SyDLab.Usv.Simulator.Domain.Models;
using SyDLab.Usv.Simulator.Domain.Models.Platforms;

namespace SyDLab.Usv.Simulator.Applications.ViewModels
{
    [Export]
    public class SettingViewModel : ViewModel<ISettingView>
    {
        public static Project Project => Singleton<Project>.UniqueInstance;

        private string _udpOutIp= ConfigurationManager.AppSettings["UdpOutIPAddress"];
        private string _udpInIp = ConfigurationManager.AppSettings["UdpInIPAddress"];
        private int _udpOutPort= int.Parse(ConfigurationManager.AppSettings["UdpOutPort"]);
        private int _udpOutPortSa = int.Parse(ConfigurationManager.AppSettings["UdpOutPortSa"]);
        private int _signalType=int.Parse(ConfigurationManager.AppSettings["SignalType"]); 
        private int _udpInPort = int.Parse(ConfigurationManager.AppSettings["UdpInPort"]);
        private int _udpInPort2 = int.Parse(ConfigurationManager.AppSettings["UdpInPortRecvSensor"]);
        private string _tcpOutIp= ConfigurationManager.AppSettings["TcpOutIPAddress"];
        private int _tcpOutPort= int.Parse(ConfigurationManager.AppSettings["TcpOutPort"]);
        private string _cmdSource = ConfigurationManager.AppSettings["CmdSource"];
        private bool _isTcpConnection = true;
        private bool _isRecvSensors = false;
        private bool _isSendout2Ca = false;

        [ImportingConstructor]
        public SettingViewModel(ISettingView view) : base(view)
        {
            PropertyChanged += SettingViewModel_PropertyChanged;

            VesselItems = new List<string>(VesselItemService.VesselList);
            SelectedVessel = VesselItems.First();

            _isCmdFromCa = _cmdSource != "NM";
            _isCmdFromNm = !_isCmdFromCa;
        }

        private void SettingViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (Project?.MyShips == null || Project?.MyShips?.Count == 0)
            {
                return;
            }


            MyShip ship = Project.MyShips[0];

            switch (e.PropertyName)
            {
                case "SelectedVessel":
                    ship.ModelType = SelectedVessel;
                    switch (_vesselItems.IndexOf(SelectedVessel))
                    {
                        case 0:
                            IsKijimaUsed = false;
                            IsKijimaEnabled = true;
                            break;
                        case 1:
                        case 2:
                            IsKijimaUsed = true;
                            IsKijimaEnabled = false;
                            break;
                    }

                    break;
                case "IsKijimaUsed":
                    ship.IsKijimaUsed = IsKijimaUsed;
                    break;
            }
        }

        #region
        private bool _isCmdFromCa;
        public bool IsCmdFromCa
        {
            get => _isCmdFromCa;
            set => SetProperty(ref _isCmdFromCa, value);
        }
        private bool _isCmdFromNm;
        public bool IsCmdFromNm
        {
            get => _isCmdFromNm;
            set => SetProperty(ref _isCmdFromNm, value);
        }
        #endregion


        #region Communication
        public string UdpInIp
        {
            get => _udpInIp;
            set => SetProperty(ref _udpInIp, value);
        }
        public string UdpOutIp
        {
            get => _udpOutIp;
            set => SetProperty(ref _udpOutIp, value);
        }
        public int UdpInPort
        {
            get => _udpInPort;
            set => SetProperty(ref _udpInPort, value);
        }
        public int UdpInPort2
        {
            get => _udpInPort2;
            set => SetProperty(ref _udpInPort2, value);
        }
        public int UdpOutPort
        {
            get => _udpOutPort;
            set => SetProperty(ref _udpOutPort, value);
        }

        public string TcpOutIp
        {
            get => _tcpOutIp;
            set => SetProperty(ref _tcpOutIp, value);
        }
        public int TcpOutPort
        {
            get => _tcpOutPort;
            set => SetProperty(ref _tcpOutPort, value);
        }

        public bool IsTcpConnection
        {
            get => _isTcpConnection;
            set => SetProperty(ref _isTcpConnection, value);
        }

        public bool IsRecvSensors
        {
            get => _isRecvSensors;
            set => SetProperty(ref _isRecvSensors, value);
        }

        public bool IsSendout2Ca
        {
            get => _isSendout2Ca;
            set => SetProperty(ref _isSendout2Ca, value);
        }
        #endregion


        #region Ship dynamics
        private bool _selectVesselEnable = true;
        private string _selectedVessel;
        private bool _isKijimaUsed = true;
        private bool _isKijimaEnabled;
        private List<string> _vesselItems;
        public bool IsKijimaUsed
        {
            get => _isKijimaUsed;
            set => SetProperty(ref _isKijimaUsed, value);
        }
        public bool SelectVesselEnable
        {
            get => _selectVesselEnable;
            set
            {
                _selectVesselEnable = value;
                RaisePropertyChanged();
            }
        }
        public bool IsKijimaEnabled
        {
            get => _isKijimaEnabled;
            set => SetProperty(ref _isKijimaEnabled, value);
        }
        public List<string> VesselItems
        {
            get => _vesselItems;
            set => _vesselItems = value;
        }
        public string SelectedVessel
        {
            get => _selectedVessel;
            set
            {
                _selectedVessel = value;
                RaisePropertyChanged(nameof(SelectedVessel));
            }
        }
        #endregion

        public void UpdateParam()
        {
            SelectedVessel = Project.MyShips[0].ModelType;
            IsKijimaUsed = Project.MyShips[0].IsKijimaUsed;
        }

        public void Show(object owner)
        {
            ViewCore.Show(owner);

            UpdateParam();
        }

        public void Close()
        {
            ViewCore.Close();
        }

        private bool? isFromNm;

        public bool? IsFromNm { get => isFromNm; set => SetProperty(ref isFromNm, value); }
    }
}
