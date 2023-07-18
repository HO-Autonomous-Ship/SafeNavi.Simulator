using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Waf.Applications;
using SyDLab.Usv.Simulator.Applications.Controllers;
using SyDLab.Usv.Simulator.Applications.Views;
using SyDLab.Usv.Simulator.Domain;
using SyDLab.Usv.Simulator.Domain.Models;
using SyDLab.Usv.Simulator.Domain.Models.Platforms;

namespace SyDLab.Usv.Simulator.Applications.ViewModels
{
    [Export]
    public class ShipDynamicsViewModel : ViewModel<IShipDynamicsView>
    {
        [ImportingConstructor]
        public ShipDynamicsViewModel(IShipDynamicsView view) : base(view)
        {
            Coefficient = new ShipCoefficient();
            Izz = 1.99 * Math.Pow(10, -5) * (0.5 * RhoSea * Math.Pow(Length, 5));
            _commandSave = new DelegateCommand(ExecuteSave);
            PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "RhoSea":
                    Izz = 1.99 * Math.Pow(10, -5) * (0.5 * RhoSea * Math.Pow(Length, 5));
                    break;
                case "Length":
                    Izz = 1.99 * Math.Pow(10, -5) * (0.5 * RhoSea * Math.Pow(Length, 5));
                    break;
            }
        }

        private void ExecuteSave()
        {
            RaisePropertyChanged();
        }

        private DelegateCommand _commandSave;
        public DelegateCommand CommandSave
        {
            get => _commandSave;
            set => SetProperty(ref _commandSave, value);
        }
        public void ShowDialog(object owner)
        {
            ViewCore.ShowDialog(owner);
        }

        private void ExecuteClose()
        {
            ViewCore.Close();
        }

        private double _length = 320;
        public double Length
        {
            get => _length;
            set => SetProperty(ref _length, value);
        }

        private double _rhoSea = 1014.0;
        public double RhoSea
        {
            get => _rhoSea;
            set => SetProperty(ref _rhoSea, value);
        }

        private double _uNorm = 8;
        public double UNorm
        {
            get => _uNorm;
            set => SetProperty(ref _uNorm, value);
        }

        private double _xg = 11.2;
        public double Xg
        {
            get => _xg;
            set => SetProperty(ref _xg, value);
        }

        private double _m = 312622000;
        public double M
        {
            get => _m;
            set => SetProperty(ref _m, value);
        }

        private double _izz ;
        public double Izz
        {
            get => _izz;
            set => SetProperty(ref _izz, value);
        }

        private double _delSat = 35;
        public double DelSat
        {
            get => _delSat;
            set => SetProperty(ref _delSat, value);
        }

        private double _delSatDot = 5;
        public double DelSatDot
        {
            get => _delSatDot;
            set => SetProperty(ref _delSatDot, value);
        }

        private ShipCoefficient _coefficient;
        public ShipCoefficient Coefficient
        {
            get => _coefficient;
            set => SetProperty(ref _coefficient, value);
        }

    }
}