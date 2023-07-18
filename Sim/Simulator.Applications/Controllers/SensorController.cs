using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Drawing;
using SyDLab.Usv.Simulator.Applications.Services;
using SyDLab.Usv.Simulator.Applications.ViewModels;

namespace SyDLab.Usv.Simulator.Applications.Controllers
{
    [Export]
    public class SensorController
    {
        // private readonly IDispatcherService _dispatcherService;
        private readonly RadarViewModel _radarViewModel;
        //private BackgroundWorker _playWorker;

        [ImportingConstructor]
        public SensorController(
            IDispatcherService dispatcherService, RadarViewModel radarViewModel)
        {
            ////_dispatcherService = dispatcherService;
            _radarViewModel = radarViewModel;

            ////_radarViewModel.d = dispatcherService;

            //// Background Worker
            ////
            ////_playWorker = new BackgroundWorker();
            ////_playWorker.WorkerReportsProgress = true;
            ////_playWorker.WorkerSupportsCancellation = true;
            ////_playWorker.DoWork += _playWorker_DoWork;    
        }

        public void Initialize()
        {
            _radarViewModel.Load();
        }

        ////private void _playWorker_DoWork(object sender, DoWorkEventArgs e)
        ////{
        ////    var sw = new Stopwatch();

        ////    while (true)
        ////    {
        ////        //_radarViewModel.UpdateRadar();

        ////        var waitDuration = Math.Max(0, (int)(2000) - (int)sw.ElapsedMilliseconds);
        ////        if (waitDuration != 0)
        ////            System.Threading.Thread.Sleep(waitDuration);
        ////        else
        ////        {
                    
        ////        }
        ////    }
        ////}
    }
}