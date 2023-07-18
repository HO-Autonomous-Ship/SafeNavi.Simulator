using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Waf.Applications;
using MathNet.Spatial.Euclidean;
using SyDLab.Usv.Simulator.Applications.Services;
using SyDLab.Usv.Simulator.Applications.ViewModels;
using SyDLab.Usv.Simulator.Domain;
using SyDLab.Usv.Simulator.Domain.Logs;
using SyDLab.Usv.Simulator.Domain.Models;
using SyDLab.Usv.Simulator.Domain.Models.Platforms;

namespace SyDLab.Usv.Simulator.Applications.Controllers
{
    [Export]
    public class CollisionRiskController
    {
        private readonly BackgroundWorker _playWorkerNetwork;
        private IDispatcherService _dispatcherService;
        private DelegateCommand _commandConnect;
        public TcpClient client;
        public NetworkStream ns;
        public TcpListener server;
        private ShellViewModel _shellViewModel;
        private LogManager LogManager => Singleton<LogManager>.UniqueInstance;
        [ImportingConstructor]
        public CollisionRiskController(IDispatcherService dispatcherService, ShellViewModel shellViewModel)
        {
            _dispatcherService = dispatcherService;
            _shellViewModel = shellViewModel;
            _commandConnect = new DelegateCommand(ExecuteConnect);

            // Background Worker (network)
            //
            _playWorkerNetwork = new BackgroundWorker();
            _playWorkerNetwork.WorkerReportsProgress = true;
            _playWorkerNetwork.WorkerSupportsCancellation = true;
            _playWorkerNetwork.DoWork += connect_DoWork;

            _commandConnect.Execute(null);
            // _shellViewModel.IsCrConnected = true;
        }

        private void connect_DoWork(object sender, DoWorkEventArgs e)
        {
            // while (true)
            // {
            //     if (server != null)
            //     {
            //         client = server.AcceptTcpClient();
            //         ns = client.GetStream();
            //         _dispatcherService.Invoke(() => { LogManager.AddLog("CollisionRisk connected."); });
            //     }
            //     byte[] receiver = new byte[12];
            //     while (client.Connected && ns != null && ns.CanRead)
            //     {
            //         try
            //         {
            //             int numOfBytesRead = ns.Read(receiver, 0, receiver.Length);
            //             if (client.Connected && numOfBytesRead > 0)
            //             {
            //                 // fused data로 변환해서 받기
            //                 // 1. byte를 double, int로 변환
            //                 int shipID = BitConverter.ToInt32(receiver, 0);
            //                 double collisionRisk = BitConverter.ToDouble(receiver, 4);
            //             
            //             }
            //         }
            //         catch (Exception exception)
            //         {
            //             continue;
            //         }
            //     }
            //     _dispatcherService.Invoke(() => { LogManager.AddLog("CollisionRisk disconnected."); });
            // }
        }

        private void ExecuteConnect()
        {
        //     IPAddress ipAddress = IPAddress.Parse(_shellViewModel.CRIpAddress);
        //     if (_shellViewModel.IsCrConnected)
        //     {
        //         server.Stop();
        //         client.Close();
        //         ns.Close();
        //         _dispatcherService.Invoke(() => { _shellViewModel.IsCrConnected = false; });
        //
        //         if (_playWorkerNetwork.IsBusy && _playWorkerNetwork.WorkerSupportsCancellation)
        //             _playWorkerNetwork.CancelAsync();
        //
        //         server = null;
        //
        //         return;
        //     }
        //     if (server != null)
        //         return;
        //
        //     server = new TcpListener(ipAddress, _shellViewModel.CRPort);
        //     server.Start();
        //     _dispatcherService.Invoke(() =>
        //     {
        //         _shellViewModel.IsCrConnected = true;
        //         LogManager.AddLog("Server started. Waiting for the client...");
        //     });
        //
        //     _playWorkerNetwork.RunWorkerAsync();
        }

        public void Initialize()
        {
            // _shellViewModel.CommandCrConnect = _commandConnect;
        }
    }
}