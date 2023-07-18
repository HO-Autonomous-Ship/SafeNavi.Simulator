using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Waf.Applications;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using SyDLab.Usv.Simulator.Applications.Views;
using SyDLab.Usv.Simulator.Domain;
using SyDLab.Usv.Simulator.Domain.Models;
using SyDLab.Usv.Simulator.Domain.Models.Platforms;

namespace SyDLab.Usv.Simulator.Applications.ViewModels
{
    public class ItemStringService
    {
        public static readonly string[] MyShipItemString =
        {
            "Speed [knot]", //1
            "Speed Cmd [knot]",
            "Heading [deg]", //2
            "Heading Cmd [deg]",
            "Rudder Angle [deg]",
            "Rudder Angle Cmd [deg]",
            "Rpm [rpm]",
            "Rpm Cmd [rpm]",
            "-"
        };

        public static readonly string[] TargetShipItemString =
        {
            "Position (x,y) [m]", //0
            "Speed [knot]", //1
            "Heading [deg]", //2
        };

        public static readonly string[] TargetShipTypeString =
        {
            "Actual data", //0
            "AIS data", //1
            "Radar data", //2
            "Fused data", //3
        };
    }

    [Export]
    public class GraphViewModel : ViewModel<IGraphView>
    {
        [ImportingConstructor]
        public GraphViewModel(IGraphView view) : base(view)
        {
            _commandClear = new DelegateCommand(ExecuteClear);
            _commandUpdatePlot = new DelegateCommand(ExecuteUpdatePlot);
        }

        public void Initialize()
        {
            Project project = Singleton<Project>.UniqueInstance;

            if (project != null)
            {
                Targets = project.TargetShips.AsEnumerable();
            }
        }

        private IEnumerable<PlatformBase> _targets;

        public IEnumerable<PlatformBase> Targets
        {
            get => _targets;
            set
            {
                SetProperty(ref _targets, value);
                RaisePropertyChanged();
            }
        }

        private PlatformBase _selectedTarget;

        public PlatformBase SelectedTarget
        {
            get => _selectedTarget;
            set
            {
                SetProperty(ref _selectedTarget, value);
                UpdateTargetsAndItems();
            }
        }

        private DelegateCommand _commandClear;

        public DelegateCommand CommandClear
        {
            get => _commandClear;
            set => SetProperty(ref _commandClear, value);
        }

        private DelegateCommand _commandUpdatePlot;

        public DelegateCommand CommandUpdatePlot
        {
            get => _commandUpdatePlot;
            set => SetProperty(ref _commandUpdatePlot, value);
        }

        private readonly Random _random = new Random(DateTime.Now.Millisecond);

        private PlotModel _plotModel = new PlotModel();

        public PlotModel PlotModel
        {
            get => _plotModel;
            set => SetProperty(ref _plotModel, value);
        }

        private IEnumerable<string> _items;

        public IEnumerable<string> Items
        {
            get => _items;
            set => SetProperty(ref _items, value);
        }

        private IEnumerable<string> _types;

        public IEnumerable<string> Types
        {
            get => _types;
            set => SetProperty(ref _types, value);
        }

        private string _selectedItem;

        public string SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetProperty(ref _selectedItem, value);
                AddPlot(SelectedTarget, SelectedType,SelectedItem);
            }
        }

        private string _selectedType;

        public string SelectedType
        {
            get => _selectedType;
            set
            {
                SetProperty(ref _selectedType, value);
                SelectedItem = string.Empty;
                Items = null;
                Items = new List<string>(ItemStringService.TargetShipItemString);
            }
        }

        private double _startTime = 0;

        public double StartTime
        {
            get => _startTime;
            set => SetProperty(ref _startTime, value);
        }

        private double _endTime = 10800;

        public double EndTime
        {
            get => _endTime;
            set => SetProperty(ref _endTime, value);
        }

        public ObservableCollection<Output> Outputs { get; } = new ObservableCollection<Output>();

        public void Reset()
        {
            Targets = null;
            Types = null;
            Items = null;

            ExecuteClear();
            UpdateTargetsAndItems();
        }

        private void ExecuteClear()
        {
            PlatformBase tmpTarget = SelectedTarget;
            SelectedTarget = tmpTarget;

            Outputs.Clear();
            PlotModel.Series.Clear();
            PlotModel.InvalidatePlot(true);
        }

        private void UpdateTargetsAndItems()
        {
            if (SelectedTarget is TargetShip)
            {
                Types = null;
                Items = null;
                Types=new List<string>(ItemStringService.TargetShipTypeString);
                Items = new List<string>(ItemStringService.TargetShipItemString);
            }

            SelectedType = string.Empty;
            //SelectedItem = string.Empty;
        }

        private void AddPlot(PlatformBase model,string type, string item)
        {
            List<double> xdata = new List<double>();
            List<double> ydata = new List<double>();
            List<double> timedata = new List<double>();
            bool isDotData = false;

            if (model is TargetShip target)
            {
                int index = 0;
                int index2 = 0;

                ObservableCollection<PlatformState> targetState=new ObservableCollection<PlatformState>();

                if (type == ItemStringService.TargetShipTypeString[index2++]) // Actual data //0
                    targetState = target.HistoryPosition;
                else if (type == ItemStringService.TargetShipTypeString[index2++]) // AIS data //1
                {
                    targetState = target.HistoryAISData;
                    isDotData = true;
                }
                else if (type == ItemStringService.TargetShipTypeString[index2++]) // RADAR data //2
                {
                    targetState = target.HistoryRadarData;
                    isDotData = true;
                }
                else if (type == ItemStringService.TargetShipTypeString[index2++]) // Fused data //3
                    targetState = target.HistoryFusedData;


                if (item == ItemStringService.TargetShipItemString[index++]) // Position [x,y] //0
                {
                    foreach (PlatformState state in targetState)
                    {
                        xdata.Add(state.Position.X);
                        ydata.Add(state.Position.Y);
                        timedata.Add(state.Time);
                    }
                }
                else if (item == ItemStringService.TargetShipItemString[index++]) // Speed [knot] //1
                {
                    foreach (PlatformState state in targetState)
                    {
                        ydata.Add(state.Speed);
                        timedata.Add(state.Time);
                    }
                }
                else if (item == ItemStringService.TargetShipItemString[index++]) // heading [deg] //2
                {
                    foreach (PlatformState state in targetState)
                    {
                        if (state.Heading < 0)
                            ydata.Add(state.Heading + 360);
                        else
                            ydata.Add(state.Heading);
                        timedata.Add(state.Time);
                    }
                }
               
            }

            if (ydata.Count > 0)
            {
                LineSeries lineSeries = new LineSeries();
                int startIndex = timedata.FindIndex(e => StartTime <= e && e < EndTime);
                int endIndex = timedata.FindLastIndex(e => StartTime <= e && e < EndTime);

                if (startIndex == -1 || endIndex == -1 || startIndex > endIndex)
                    return;

                if(xdata.Count==0) // speed인 경우 x축이 시간
                {
                    if (PlotModel.Axes.Any(e => e.Title== "X [m]"))
                    {
                        Outputs.Clear();
                        PlotModel.Series.Clear();
                        PlotModel.Axes.Clear();
                        PlotModel.InvalidatePlot(true);
                    }

                    if (!PlotModel.Axes.Any(e => e.Title == "Time [sec]"))
                    {
                        PlotModel.Axes.Add(new LinearAxis
                        {
                            Title = "Time [sec]",
                            Position = AxisPosition.Bottom,
                        });

                        PlotModel.Axes.Add(new LinearAxis
                        {
                            Title = "Speed [knot]",
                            Position = AxisPosition.Left,
                        });
                    }

                    for (int i = startIndex; i < endIndex; i++)
                    {
                        lineSeries.Points.Add(new DataPoint(timedata[i], ydata[i]));
                    }
                }
                else
                {
                    for (int i = startIndex; i < endIndex; i++)
                    {
                        lineSeries.Points.Add(new DataPoint(xdata[i], ydata[i]));
                    }
                    if (PlotModel.Axes.Any(e => e.Title == "Time [sec]"))
                    {
                        Outputs.Clear();
                        PlotModel.Series.Clear();
                        PlotModel.Axes.Clear();
                        PlotModel.InvalidatePlot(true);
                    }

                    if (!PlotModel.Axes.Any(e => e.Title == "X [m]"))
                    {
                        PlotModel.Axes.Add(new LinearAxis
                        {
                            Title = "Y [m]",
                            Position = AxisPosition.Bottom,
                        });

                        PlotModel.Axes.Add(new LinearAxis
                        {
                            Title = "X [m]",
                            Position = AxisPosition.Left,
                        });
                    }
                }

                //lineSeries.Color = OxyColor.FromRgb((byte)_random.Next(0, 255), (byte)_random.Next(0, 255), (byte)_random.Next(0, 255));
                // 그래프 색깔 고정
                if (item == ItemStringService.TargetShipItemString[0])
                    //lineSeries.Color = OxyColor.FromRgb(0, 0, 255);
                    lineSeries.Color = OxyColor.FromRgb((byte)_random.Next(0, 255), (byte)_random.Next(0, 255), (byte)_random.Next(0, 255));
                else if (item == ItemStringService.TargetShipItemString[1])
                    //lineSeries.Color = OxyColor.FromRgb(100, 0, 200);
                    lineSeries.Color = OxyColor.FromRgb((byte)_random.Next(0, 255), (byte)_random.Next(0, 255), (byte)_random.Next(0, 255));
                else if (item == ItemStringService.TargetShipItemString[2])
                    //lineSeries.Color = OxyColor.FromRgb(0, 200, 0);
                    lineSeries.Color = OxyColor.FromRgb((byte)_random.Next(0, 255), (byte)_random.Next(0, 255), (byte)_random.Next(0, 255));
                else if (item == ItemStringService.TargetShipItemString[3])
                    lineSeries.Color = OxyColor.FromRgb((byte)_random.Next(0, 255), (byte)_random.Next(0, 255), (byte)_random.Next(0, 255));
                //lineSeries.Color = OxyColor.FromRgb(255, 0, 0);
                else if (item == ItemStringService.TargetShipItemString[4])
                    lineSeries.Color = OxyColor.FromRgb((byte)_random.Next(0, 255), (byte)_random.Next(0, 255), (byte)_random.Next(0, 255));
                //lineSeries.Color = OxyColor.FromRgb(0, 0, 255);
                else if (item == ItemStringService.TargetShipItemString[5])
                    lineSeries.Color = OxyColor.FromRgb((byte)_random.Next(0, 255), (byte)_random.Next(0, 255), (byte)_random.Next(0, 255));

                //lineSeries.Color = OxyColor.FromRgb(255, 0, 0);

                PlotModel.Series.Add(lineSeries);
                lineSeries.Title = model.DisplayName + "_" + item;

                // 그래프 색깔 고정
                if (isDotData)
                {
                    lineSeries.MarkerSize = 2;
                    lineSeries.MarkerType = OxyPlot.MarkerType.Circle;
                    lineSeries.StrokeThickness = 0;
                }

                Output output = new Output()
                {
                    Target = model,
                    Item = item,
                    Type = type,
                    Mean = (ydata.Count > 0) ? ydata.Average() : 0,
                    Min = (ydata.Count > 0) ? ydata.Min() : 0,
                    Max = (ydata.Count > 0) ? ydata.Max() : 0
                };

                string[] rgb = lineSeries.Color.ToByteString().Split(',');
                output.Color = Color.FromRgb(byte.Parse(rgb[0]), byte.Parse(rgb[1]), byte.Parse(rgb[2]));
                Outputs.Add(output);

                PlotModel.InvalidatePlot(true);

                // string path = "C:\\Users\\Hyewon\\Desktop\\Result";
                //
                //
                // double error = 0;
                // double avError = 0;
                //
                // foreach (TargetShip tar in Targets)
                // {
                //     string filePath = path + "\\" + tar.DisplayName + "fused position"+ ".csv";
                //     StreamWriter sw = new StreamWriter(filePath);
                //     sw.WriteLine("time, x_fused, y_fused");
                //
                //     string filePath2 = path + "\\" + tar.DisplayName + "actual position" + ".csv";
                //     StreamWriter sw2 = new StreamWriter(filePath2);
                //     sw2.WriteLine("time, x_actual, y_actual");
                //
                //     error = 0;
                //
                //     for (int i = 0; i < tar.HistoryFusedData.Count; i++)
                //     {
                //         PlatformState p=tar.HistoryPosition.First(e => (e.Time- tar.HistoryFusedData[i].Time<1e-2));
                //         double dist = (tar.HistoryFusedData[i].Position.X - p.Position.X) *
                //                       (tar.HistoryFusedData[i].Position.X - p.Position.X)
                //                       + (tar.HistoryFusedData[i].Position.Y - p.Position.Y) *
                //                       (tar.HistoryFusedData[i].Position.Y - p.Position.Y);
                //         error += Math.Sqrt(dist);
                //
                //         sw.Write(tar.HistoryFusedData[i].Time);
                //         sw.Write(",");
                //         sw.Write(tar.HistoryFusedData[i].Position.X);
                //         sw.Write(",");
                //         sw.Write(tar.HistoryFusedData[i].Position.Y);
                //         sw.Write("\n");
                //     }
                //
                //     for (int i = 0; i < tar.HistoryPosition.Count; i++)
                //     {
                //         sw2.Write(tar.HistoryPosition[i].Time);
                //         sw2.Write(",");
                //         sw2.Write(tar.HistoryPosition[i].Position.X);
                //         sw2.Write(",");
                //         sw2.Write(tar.HistoryPosition[i].Position.Y);
                //         sw2.Write("\n");
                //
                //     }
                //
                //
                //     sw.Close();
                //     sw2.Close();
                //     error /= tar.HistoryFusedData.Count;
                //     avError += error;
                // }
                //
                // avError /= Targets.Count();

            }
        }

        private void ExecuteUpdatePlot()
        {
            // 다 지우고 새로 그림.
            PlotModel.Series.Clear();

            int num = Outputs.Count;

            for (int i = 0; i < num; i++)
            {
                AddPlot(Outputs[0].Target, Outputs[0].Type, Outputs[0].Item);
                Outputs.RemoveAt(0);
            }

            PlotModel.InvalidatePlot(true);
        }
    }
};