using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Text;
using System.Waf.Applications;
using System.Windows.Forms;
using SyDLab.Usv.Simulator.Applications.Views;
using SyDLab.Usv.Simulator.Domain;
using SyDLab.Usv.Simulator.Domain.Models;
using SyDLab.Usv.Simulator.Domain.Models.Platforms;
using SyDLab.Usv.Simulator.Domain.Models.Tasks;
using SaveFileDialog = Microsoft.Win32.SaveFileDialog;

namespace SyDLab.Usv.Simulator.Applications.ViewModels
{
    [Export]
    public class ModelBuilderViewModel : ViewModel<IModelBuilderView>
    {
        [ImportingConstructor]
        public ModelBuilderViewModel(IModelBuilderView view) : base(view)
        {
            Waypoints = new ObservableCollection<Waypoint>();
            _commandDelete = new DelegateCommand(ExecuteDelete);
            _commandSaveTrack = new DelegateCommand(ExecuteSaveTrack);
        }

        public ObservableCollection<Waypoint> Waypoints { get; }

        //public object Usv { get; set; }
        private ObservableCollection<object> _selectedObjects;

        public ObservableCollection<object> SelectedObjects
        {
            get { return _selectedObjects; }
            set { SetProperty(ref _selectedObjects, value); }
        }

        public ObservableCollection<object> Nodes { get; set; } = new ObservableCollection<object>();

        private Project Project => Singleton<Project>.UniqueInstance;

        private DelegateCommand _commandDelete;

        public DelegateCommand CommandDelete
        {
            get => _commandDelete;
            set => SetProperty(ref _commandDelete, value);
        }

        private DelegateCommand _commandSaveTrack;

        public DelegateCommand CommandSaveTrack
        {
            get => _commandSaveTrack;
            set => SetProperty(ref _commandSaveTrack, value);
        }

        private void ExecuteDelete()
        {
            DialogResult dialogResult = MessageBox.Show("Are you sure to continue?", "Remove", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning);
            if (dialogResult == DialogResult.OK)
            {
                for (int i = 0; i < SelectedObjects.Count;)
                {
                    if (SelectedObjects[i] is TaskBase task)
                    {
                        foreach (ScenarioBase scenario in Project.Scenarios)
                        {
                            if (scenario.Tasks.Contains(task))
                                scenario.Tasks.Remove(task);
                        }
                    }
                    else if (SelectedObjects[i] is TargetShip target)
                    {
                        if (Project.TargetShips.Contains(target))
                        {
                            Project.TargetShips.Remove(target);
                        }
                    }
                    else if (SelectedObjects[i] is Obstacle obstacle)
                    {
                        if (Project.Obstacles.Contains(obstacle))
                        {
                            Project.Obstacles.Remove(obstacle);
                        }
                    }
                    else
                    {
                        i++;
                    }
                }
            }
        }

        private void ExecuteSaveTrack()
        {
            // Track 결과 저장
            var fileName = "";
            var filePath = "";

            if (!string.IsNullOrEmpty(Directory.GetCurrentDirectory()))
            {
                fileName = Directory.GetCurrentDirectory();
                filePath = Directory.GetCurrentDirectory();
            }

            var dlg = new SaveFileDialog
            {
                DefaultExt = ".csv",
                Filter = "comma-separated value(.csv)|*.csv",
                FileName = fileName,
                InitialDirectory = filePath
            };

            if (dlg.ShowDialog() == true)
            {
                string dataPath = dlg.FileName;//filePath;
                string textLabel = "track data";
                File.WriteAllText(dataPath, textLabel, Encoding.Default);

                for (int i = 0; i < SelectedObjects.Count; i++)
                {
                    if (SelectedObjects[i] is TargetShip ship)
                    {
                        File.AppendAllText(dataPath, "\nAIS");
                        foreach (var item in ship.HistoryAISData)
                        {
                            PlatformState gt = ship.HistoryPosition.Last(n => n.Time - item.Time < 0.1);
                            double error = Math.Sqrt(Math.Pow(gt.Position.X - item.Position.X, 2) + Math.Pow(gt.Position.Y - item.Position.Y, 2));
                            File.AppendAllText(dataPath,
                                "\n" + item.Time + "," + gt.Position.X + "," + gt.Position.Y + "," + item.Position.X +
                                "," + item.Position.Y + "," + error, Encoding.Default);
                        }

                        File.AppendAllText(dataPath, "\nRadar");
                        foreach (var item in ship.HistoryRadarData)
                        {
                            PlatformState gt = ship.HistoryPosition.Last(n => n.Time - item.Time < 0.1);
                            double error = Math.Sqrt(Math.Pow(gt.Position.X - item.Position.X, 2) + Math.Pow(gt.Position.Y - item.Position.Y, 2));
                            File.AppendAllText(dataPath,
                                "\n" + item.Time + "," + gt.Position.X + "," + gt.Position.Y + "," + item.Position.X +
                                "," + item.Position.Y + "," + error, Encoding.Default);
                        }

                        File.AppendAllText(dataPath, "\nFusion");
                        foreach (var item in ship.HistoryFusedData)
                        {
                            PlatformState gt = ship.HistoryPosition.Last(n => n.Time - item.Time < 0.1);
                            double error = Math.Sqrt(Math.Pow(gt.Position.X - item.Position.X, 2) + Math.Pow(gt.Position.Y - item.Position.Y, 2));
                            File.AppendAllText(dataPath,
                                "\n" + item.Time + "," + gt.Position.X + "," + gt.Position.Y + "," + item.Position.X +
                                "," + item.Position.Y + "," + error, Encoding.Default);
                        }

                        File.AppendAllText(dataPath, "\nSpeed/Heading");
                        File.AppendAllText(dataPath, "\nAIS");
                        foreach (var item in ship.HistoryAISData)
                        {
                            PlatformState gt = ship.HistoryPosition.Last(n => n.Time - item.Time < 0.1);
                            double errorV = Math.Sqrt(Math.Pow(gt.Speed - item.Speed, 2));
                            double errorH = Math.Sqrt(Math.Pow(gt.Heading - item.Heading, 2));
                            File.AppendAllText(dataPath,
                                "\n" + item.Time + "," + gt.Speed + "," + item.Speed + "," + errorV + "," + gt.Heading +
                                "," + item.Heading + "," + errorH, Encoding.Default);
                        }

                        File.AppendAllText(dataPath, "\nRadar");
                        foreach (var item in ship.HistoryRadarData)
                        {
                            PlatformState gt = ship.HistoryPosition.Last(n => n.Time - item.Time < 0.1);
                            double errorV = Math.Sqrt(Math.Pow(gt.Speed - item.Speed, 2));
                            double errorH = Math.Sqrt(Math.Pow(gt.Heading - item.Heading, 2));
                            File.AppendAllText(dataPath,
                                "\n" + item.Time + "," + gt.Speed + "," + item.Speed + "," + errorV + "," + gt.Heading +
                                "," + item.Heading + "," + errorH, Encoding.Default);
                        }
                        File.AppendAllText(dataPath, "\nFusion");
                        foreach (var item in ship.HistoryFusedData)
                        {
                            PlatformState gt = ship.HistoryPosition.Last(n => n.Time - item.Time < 0.1);
                            double errorV = Math.Sqrt(Math.Pow(gt.Speed - item.Speed, 2));
                            double errorH = Math.Sqrt(Math.Pow(gt.Heading - item.Heading, 2));
                            File.AppendAllText(dataPath,
                                "\n" + item.Time + "," + gt.Speed + "," + item.Speed + "," + errorV + "," + gt.Heading +
                                "," + item.Heading + "," + errorH, Encoding.Default);
                        }
                    }
                }
            }
        }
    }
}