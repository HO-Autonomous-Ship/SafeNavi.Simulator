using System;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using SyDLab.Usv.Simulator.Applications.Views;
using SyDLab.Usv.Simulator.Domain.Models.Platforms;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Waf.Applications;
using OxyPlot.Legends;
using SyDLab.Usv.Simulator.Domain;
using SyDLab.Usv.Simulator.Domain.Models;

namespace SyDLab.Usv.Simulator.Applications.ViewModels
{
    [Export]
    public class MonitorViewModel : ViewModel<IMonitorView>
    {
        public static readonly string[] ConfigItemString =
        {
            "Direction",
            "Speed",
            "Speed & Heading",
            "Rpm & Rudder"
        };

        private static readonly string[] ConfigSet1 =
        {
            "Rudder Angle [deg]",
            "Rudder Angle Cmd [deg]",
            "Heading [deg]",
            "Heading Cmd [deg]",
            "-",
            "-"
        };
        private static readonly string[] ConfigSet2 =
        {
            "Speed [knot]",
            "Speed Cmd [knot]",
            "Rpm [rpm]",
            "Rpm Cmd [rpm]",
            "-",
            "-"
        };
        private static readonly string[] ConfigSet3 =
        {
            "Speed [knot]",
            "Speed Cmd [knot]",
            "Heading [deg]",
            "Heading Cmd [deg]",
            "-",
            "-"
        };
        private static readonly string[] ConfigSet4 =
        {
            "Rpm [rpm]",
            "Rpm Cmd [rpm]",
            "Rudder Angle [deg]",
            "Rudder Angle Cmd [deg]",
            "-",
            "-"
        };


        [ImportingConstructor]
        public MonitorViewModel(IMonitorView view): base(view)
        {
        }

        private Project Project => Singleton<Project>.UniqueInstance;

        private IEnumerable<string> _configItems;
        public IEnumerable<string> ConfigItems
        {
            get => _configItems;
            set => _configItems = value;    
        }

        private string _selectedConfigItem;
        public string SelectedConfigItem
        {
            get => _selectedConfigItem;
            set
            {
                SetProperty(ref _selectedConfigItem, value);
                UpdatePlotItems();
            } 
        }

        private IEnumerable<string> _plotItems;
        public IEnumerable<string> PlotItems
        {
            get => _plotItems;
            set => _plotItems = value;
        }

        private string _selectedPlotItem11;
        public string SelectedPlotItem11
        {
            get=> _selectedPlotItem11;
            set
            {
                SetProperty(ref _selectedPlotItem11, value);
                UpdatePlot1();
            }
        }
        private string _selectedPlotItem12;
        public string SelectedPlotItem12
        {
            get => _selectedPlotItem12;
            set
            {
                lock (_plotModel1.SyncRoot)
                {
                    _selectedPlotItem12 = value == _selectedPlotItem11 ? "-" : value;
                }
                RaisePropertyChanged();
                UpdatePlot1();
            }
        }
        private string _selectedPlotItem21;
        public string SelectedPlotItem21
        {
            get => _selectedPlotItem21;
            set
            {
                SetProperty(ref _selectedPlotItem21, value);
                UpdatePlot2();
            }
        }
        private string _selectedPlotItem22;
        public string SelectedPlotItem22
        {
            get => _selectedPlotItem22;
            set
            {
                lock (_plotModel2.SyncRoot)
                {
                    _selectedPlotItem22 = value == _selectedPlotItem21 ? "-" : value;
                }
                RaisePropertyChanged(nameof(SelectedPlotItem22));
                UpdatePlot2();
            }
        }
        private string _selectedPlotItem31;
        public string SelectedPlotItem31
        {
            get => _selectedPlotItem31;
            set
            {
                SetProperty(ref _selectedPlotItem31, value);
                UpdatePlot3();
            }
        }
        private string _selectedPlotItem32;
        public string SelectedPlotItem32
        {
            get => _selectedPlotItem32;
            set
            {
                lock (_plotModel3.SyncRoot)
                {
                    _selectedPlotItem32 = value == _selectedPlotItem31 ? "-" : value;
                }
                RaisePropertyChanged(nameof(SelectedPlotItem32));
                UpdatePlot3();
            }
        }

        private void UpdatePlot1()
        {
            lock (_plotModel1.SyncRoot)
            {
                _plotModel1.Axes.Clear();
                _plotModel1.Series.Clear();
                _plotModel1.Axes.Add(new LinearAxis {Position=AxisPosition.Bottom, MajorGridlineStyle=LineStyle.Solid, Title="Time [sec]"});
                
                if (_selectedPlotItem11 != "-" && _selectedPlotItem11 != null)
                {
                    GetItem(_selectedPlotItem11, out _, out var la, out var ls);
                    if (ls == null || la == null) return;
                    _plotModel1.Axes.Add(la);
                    _plotModel1.Series.Add(ls);
                }

                if (_selectedPlotItem12 != "-" && _selectedPlotItem12 != null)
                {
                    GetItem(_selectedPlotItem12, out _, out var la, out var ls);
                    if (ls == null || la == null) return;
                    if (_plotModel1.Axes.All(i => i.Key != la.Key))
                    {
                        la.Position = AxisPosition.Right;
                        _plotModel1.Axes.Add(la);
                    }

                    if (_plotModel1.Series.All(i => i.Title != ls.Title))
                        _plotModel1.Series.Add(ls);
                }
            }
            _plotModel1.InvalidatePlot(true);
        }

        private void UpdatePlot2()
        {
            lock (_plotModel2.SyncRoot)
            {
                _plotModel2.Axes.Clear();
                _plotModel2.Series.Clear();
                _plotModel2.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, MajorGridlineStyle = LineStyle.Solid, Title = "Time [sec]" });

                if (_selectedPlotItem21 != "-" && _selectedPlotItem21 != null)
                {
                    GetItem(_selectedPlotItem21, out _, out var la, out var ls);
                    if (ls == null || la == null) return;
                    _plotModel2.Axes.Add(la);
                    _plotModel2.Series.Add(ls);
                }

                if (_selectedPlotItem22 != "-" && _selectedPlotItem22 != null)
                {
                    GetItem(_selectedPlotItem22, out _, out var la, out var ls);
                    if (ls == null || la == null) return;
                    if (_plotModel2.Axes.All(i => i.Key != la.Key))
                    {
                        la.Position = AxisPosition.Right;
                        _plotModel2.Axes.Add(la);
                    }

                    if (_plotModel2.Series.All(i => i.Title != ls.Title))
                        _plotModel2.Series.Add(ls);
                }
            }
            _plotModel2.InvalidatePlot(true);
        }
        
        private void UpdatePlot3()
        {
            lock (_plotModel3.SyncRoot)
            {
                _plotModel3.Axes.Clear();
                _plotModel3.Series.Clear();
                _plotModel3.Axes.Add(new LinearAxis { Position = AxisPosition.Bottom, MajorGridlineStyle = LineStyle.Solid, Title = "Time [sec]" });

                if (_selectedPlotItem31 != "-" && _selectedPlotItem31 != null)
                {
                    GetItem(_selectedPlotItem31, out _, out var la, out var ls);
                    if (ls == null || la == null) return;
                    _plotModel3.Axes.Add(la);
                    _plotModel3.Series.Add(ls);
                }

                if (_selectedPlotItem32 != "-" && _selectedPlotItem32 != null)
                {
                    GetItem(_selectedPlotItem32, out _, out var la, out var ls);
                    if (ls == null || la == null) return;
                    if (_plotModel3.Axes.All(i => i.Key != la.Key))
                    {
                        la.Position = AxisPosition.Right;
                        _plotModel3.Axes.Add(la);
                    }
                    if (_plotModel3.Series.All(i => i.Title != ls.Title))
                        _plotModel3.Series.Add(ls);
                }
            }
            _plotModel3.InvalidatePlot(true);
        }

        private void UpdatePlotItems()
        {
            switch (_selectedConfigItem)
            {
                case "Direction":
                    SelectedPlotItem11 = ConfigSet1[0];
                    SelectedPlotItem12 = ConfigSet1[1];
                    SelectedPlotItem21 = ConfigSet1[2];
                    SelectedPlotItem22 = ConfigSet1[3];
                    SelectedPlotItem31 = ConfigSet1[4];
                    SelectedPlotItem32 = ConfigSet1[5];
                    break;
                case "Speed":
                    SelectedPlotItem11 = ConfigSet2[0];
                    SelectedPlotItem12 = ConfigSet2[1];
                    SelectedPlotItem21 = ConfigSet2[2];
                    SelectedPlotItem22 = ConfigSet2[3];
                    SelectedPlotItem31 = ConfigSet2[4];
                    SelectedPlotItem32 = ConfigSet2[5];
                    break;
                case "Speed & Heading":
                    SelectedPlotItem11 = ConfigSet3[0];
                    SelectedPlotItem12 = ConfigSet3[1];
                    SelectedPlotItem21 = ConfigSet3[2];
                    SelectedPlotItem22 = ConfigSet3[3];
                    SelectedPlotItem31 = ConfigSet3[4];
                    SelectedPlotItem32 = ConfigSet3[5];
                    break;
                case "Rpm & Rudder":
                    SelectedPlotItem11 = ConfigSet4[0];
                    SelectedPlotItem12 = ConfigSet4[1];
                    SelectedPlotItem21 = ConfigSet4[2];
                    SelectedPlotItem22 = ConfigSet4[3];
                    SelectedPlotItem31 = ConfigSet4[4];
                    SelectedPlotItem32 = ConfigSet4[5];
                    break;
            }
        }


        public void Initialize()
        {
            PlotItems = new List<string>(ItemStringService.MyShipItemString);
            ConfigItems = new List<string>(ConfigItemString);

            SelectedConfigItem = ConfigItems.First();


            #region === Plot1 ==========
            _plotModel1.Axes.Clear();
            _plotModel1.Series.Clear();

            LinearAxis yAxis = new LinearAxis()
            {
                Position = AxisPosition.Left,
                MajorGridlineStyle = LineStyle.Solid,
                Minimum = -40,
                Maximum = 40,
                Title = SelectedPlotItem11,
                Key = "RudderAngle",
            };
            LinearAxis xAxis = new LinearAxis()
            {
                Position = AxisPosition.Bottom,
                MajorGridlineStyle = LineStyle.Solid,
                Title = "Time [sec]"
            };
            _plotModel1.Axes.Add(xAxis);
            _plotModel1.Axes.Add(yAxis);
            LineSeries ls1 = new LineSeries()
            {
                Title = _selectedPlotItem11,
                Color = OxyColors.Blue,
                StrokeThickness = 2,
                YAxisKey = "RudderAngle"
            };
            LineSeries ls2 = new LineSeries()
            {
                Title = _selectedPlotItem12,
                Color = OxyColors.DarkGray,
                StrokeThickness = 2,
                LineStyle = LineStyle.Dash,
                YAxisKey = "RudderAngle"
            };
            _plotModel1.Series.Add(ls1);
            _plotModel1.Series.Add(ls2);
            var l = new Legend
            {
                LegendLineSpacing = 2,
                // LegendPlacement = LegendPlacement.Outside,
                LegendPosition = LegendPosition.RightTop,
                // LegendOrientation = LegendOrientation.Horizontal,
                // LegendBackground = OxyColor.FromAColor(200, OxyColors.White),
                // LegendBorder = OxyColors.Black,
                FontSize=5
            };
            _plotModel1.Legends.Add(l);
            _plotModel1.InvalidatePlot(true);
            #endregion


            #region === Plot2 ===========
            _plotModel2.Axes.Clear();
            _plotModel2.Series.Clear();

            GetItem(_selectedPlotItem21, out _, out var yAxis2, out var ls21);
            LinearAxis xAxis2 = new LinearAxis()
            {
                Position = AxisPosition.Bottom,
                MajorGridlineStyle = LineStyle.Solid,
                Title = "Time [sec]"
            };
            _plotModel2.Axes.Add(xAxis2);
            _plotModel2.Axes.Add(yAxis2);
            GetItem(_selectedPlotItem22, out _, out _, out var ls22);
            _plotModel2.Series.Add(ls21);
            _plotModel2.Series.Add(ls22);
            var l2 = new Legend
            {
                LegendPosition = LegendPosition.RightTop,
                FontSize = 5
            };
            _plotModel2.Legends.Add(l2);
            _plotModel2.InvalidatePlot(true);
            #endregion


            #region === Plot3 ==========
            _plotModel3.Axes.Clear();
            _plotModel3.Series.Clear();

            GetItem(_selectedPlotItem31, out _, out var yAxis3, out var ls31);
            LinearAxis xAxis3 = new LinearAxis()
            {
                Position = AxisPosition.Bottom,
                MajorGridlineStyle = LineStyle.Solid,
                Title = "Time [sec]"
            };
            _plotModel3.Axes.Add(xAxis3);
            _plotModel3.Axes.Add(yAxis3);
            GetItem(_selectedPlotItem32, out _, out var yAxis32, out var ls32);
            yAxis32.Position = AxisPosition.Right;
            _plotModel3.Axes.Add(yAxis32);
            _plotModel3.Series.Add(ls31);
            _plotModel3.Series.Add(ls32);
            var l3 = new Legend
            {
                LegendPosition = LegendPosition.RightTop,
                FontSize = 5
            };
            _plotModel3.Legends.Add(l3);
            _plotModel3.InvalidatePlot(true);
            #endregion
        }


        public void AddValues(MyShip myShip)
        {
            #region PlotModel 1
            lock (_plotModel1.SyncRoot)
            {
                var s = new LineSeries();
                double value = 0;
                LinearAxis linearAxis = null;
                LineSeries lineSeries = null;
                if (_selectedPlotItem11 != "-")
                {
                    GetItem(_selectedPlotItem11, out value, out linearAxis, out lineSeries);
                    s = (LineSeries)PlotModel1.Series[0];
                    s?.Points.Add(_selectedPlotItem11 == "-"
                        ? new DataPoint(s.Points.Count+1, 0)
                        : new DataPoint(s.Points.Count + 1, value));
                }
                
                var s2 = new LineSeries();
                if (_selectedPlotItem12 != "-")
                {
                    GetItem(_selectedPlotItem12, out value, out linearAxis, out lineSeries);
                    s2 = (LineSeries)PlotModel1.Series?[1];
                    s2?.Points.Add(_selectedPlotItem12 == "-"
                        ? new DataPoint(s2.Points.Count + 1, 0)
                        : new DataPoint(s2.Points.Count + 1, value));
                }
                
                if (s?.Points.Count > 15)
                {
                    if (s.Points[s.Points.Count - 1].X - s.Points[0].X > 250)
                    {
                        s?.Points.Remove(s.Points[0]);
                        s2?.Points.Remove(s2.Points[0]);
                    }
                }
            }
            _plotModel1.InvalidatePlot(true);
            #endregion


            #region PlotModel 2
            lock (_plotModel2.SyncRoot)
            {
                var s = new LineSeries();
                double value = 0;
                if (_selectedPlotItem21 != "-")
                {
                    GetItem(_selectedPlotItem21, out value, out var linearAxis, out var lineSeries);
                    s = (LineSeries)PlotModel2.Series[0];
                    s?.Points.Add(_selectedPlotItem21 == "-"
                        ? new DataPoint(s.Points.Count + 1, 0)
                        : new DataPoint(s.Points.Count + 1, value));
                }

                var s2 = new LineSeries();
                if (_selectedPlotItem22 != "-")
                {
                    GetItem(_selectedPlotItem22, out value, out var linearAxis, out var lineSeries);
                    s2 = (LineSeries)PlotModel2.Series?[1];
                    s2?.Points.Add(_selectedPlotItem22 == "-"
                        ? new DataPoint(s2.Points.Count + 1, 0)
                        : new DataPoint(s2.Points.Count + 1, value));
                }

                if (s?.Points.Count > 15)
                {
                    if (s.Points[s.Points.Count - 1].X - s.Points[0].X > 250)
                    {
                        s?.Points.Remove(s.Points[0]);
                        s2?.Points.Remove(s2.Points[0]);
                    }
                }
            }
            _plotModel2.InvalidatePlot(true);
            #endregion


            #region PlotModel 3
            lock (_plotModel3.SyncRoot)
            {
                var s = new LineSeries();
                double value = 0;
                if (_selectedPlotItem31 != "-")
                {
                    GetItem(_selectedPlotItem31, out value, out var linearAxis, out var lineSeries);
                    s = (LineSeries)PlotModel3.Series[0];
                    s?.Points.Add(_selectedPlotItem31 == "-"
                        ? new DataPoint(s.Points.Count + 1, 0)
                        : new DataPoint(s.Points.Count + 1, value));
                }

                var s2 = new LineSeries();
                if (_selectedPlotItem32 != "-")
                {
                    GetItem(_selectedPlotItem32, out value, out var linearAxis, out var lineSeries);
                    s2 = (LineSeries)PlotModel3.Series?[1];
                    s2?.Points.Add(_selectedPlotItem32 == "-"
                        ? new DataPoint(s2.Points.Count + 1, 0)
                        : new DataPoint(s2.Points.Count + 1, value));
                }

                if (s?.Points.Count > 15)
                {
                    if (s.Points[s.Points.Count - 1].X - s.Points[0].X > 250)
                    {
                        s?.Points.Remove(s.Points[0]);
                        s2?.Points.Remove(s2.Points[0]);
                    }
                }
            }
            _plotModel3.InvalidatePlot(true);
            #endregion
        }

        private readonly PlotModel _plotModel1 = new PlotModel();
        public PlotModel PlotModel1 => _plotModel1;

        private readonly PlotModel _plotModel2 = new PlotModel();
        public PlotModel PlotModel2 => _plotModel2;

        private readonly PlotModel _plotModel3 = new PlotModel();
        public PlotModel PlotModel3 => _plotModel3;


        private void GetItem(object obj, out double value, out LinearAxis la, out LineSeries ls)
        {
            MyShip myShip = Project.MyShips[0];
            string item = obj.ToString();
            int index = 0;
            value = 0;
            la = new LinearAxis();
            ls = new LineSeries();
            if (item == ItemStringService.MyShipItemString[index++])
            {
                value = myShip.Speed;
                ls = new LineSeries()
                {
                    Title = item,
                    YAxisKey = "Speed",
                };
                la = new LinearAxis()
                {
                    Title = item,
                    // AbsoluteMinimum = 0,
                    // AbsoluteMaximum = 50,
                    Minimum = 0,
                    Maximum = 25,
                    Key = "Speed"
                };
            }
            else if (item == ItemStringService.MyShipItemString[index++])
            {
                value = myShip.SpeedDesired;
                ls = new LineSeries()
                {
                    Title = item,
                    YAxisKey = "Speed",
                    Tag = "Reference"
                };
                la = new LinearAxis()
                {
                    Title = item,
                    // AbsoluteMinimum = 0,
                    // AbsoluteMaximum = 50,
                    Minimum = 0,
                    Maximum = 25,
                    Key = "Speed"
                };
            }
            else if (item == ItemStringService.MyShipItemString[index++])
            {
                value = myShip.Heading;
                ls = new LineSeries()
                {
                    Title = item,
                    YAxisKey = "Heading"
                };
                la = new LinearAxis()
                {
                    Title = item,
                    Key = "Heading"
                };
            }
            else if (item == ItemStringService.MyShipItemString[index++])
            {
                value = myShip.HeadingDesired;
                ls = new LineSeries()
                {
                    Title = item,
                    YAxisKey = "Heading",
                    Tag = "Reference"
                };
                la = new LinearAxis()
                {
                    Title = item,
                    Key = "Heading"
                };
            }
            else if (item == ItemStringService.MyShipItemString[index++])
            {
                value = myShip.Rudder[0] * Distance.R2D;
                ls = new LineSeries()
                {
                    Title = item,
                    YAxisKey = "Rudder"
                };
                la = new LinearAxis()
                {
                    Position = AxisPosition.Left,
                    MajorGridlineStyle = LineStyle.Solid,
                    Title = item,
                    // AbsoluteMinimum = -100,
                    // AbsoluteMaximum = 100,
                    Minimum = -40,
                    Maximum = 40,
                    Key = "Rudder"
                };
            }
            else if (item == ItemStringService.MyShipItemString[index++])
            {
                value = myShip.RudderDesired * Distance.R2D;
                ls = new LineSeries()
                {
                    Title = item,
                    YAxisKey = "Rudder"
                };
                la = new LinearAxis()
                {
                    Title = item,
                    // AbsoluteMinimum = -100,
                    // AbsoluteMaximum = 100,
                    Minimum = -40,
                    Maximum = 40,
                    Key = "Rudder"
                };
            }
            else if (item == ItemStringService.MyShipItemString[index++])
            {
                value = myShip.Rpm;
                ls = new LineSeries()
                {
                    Title = item,
                    YAxisKey = "Rpm"
                };
                la = new LinearAxis()
                {
                    Title = item,
                    // AbsoluteMinimum = -100,
                    // AbsoluteMaximum = 100,
                    // Minimum = -80,
                    // Maximum = 80,
                    Key = "Rpm"
                };
            }
            else if (item == ItemStringService.MyShipItemString[index++])
            {
                value = myShip.RpmDesired;
                ls = new LineSeries()
                {
                    Title = item,
                    YAxisKey = "Rpm"
                };
                la = new LinearAxis()
                {
                    Title = item,
                    // AbsoluteMinimum = -100,
                    // AbsoluteMaximum = 100,
                    // Minimum = -80,
                    // Maximum = 80,
                    Key = "Rpm"
                };
            }
 
            if (ls != null)
            {
                ls.Color = OxyColors.Blue;
                ls.LineStyle = LineStyle.Solid;
                ls.StrokeThickness = 2;
            }
            if (la != null)
            {
                la.Position = AxisPosition.Left;
                la.MajorGridlineStyle = LineStyle.Solid;
            }
            
            if (obj.Equals(_selectedPlotItem12) || obj.Equals(_selectedPlotItem22) ||
                obj.Equals(_selectedPlotItem32))
            {
                if (ls?.Tag != null && ls.Tag.ToString().Contains("Reference"))
                {
                    ls.Color = OxyColors.LightGray;
                    ls.LineStyle = LineStyle.LongDash;
                }
                else
                {
                    ls.Color = OxyColors.Red;
                    ls.LineStyle = LineStyle.Solid;
                }

                if (la == null) return;
                la.MajorGridlineColor = OxyColors.DarkGray;
                la.MajorGridlineStyle = LineStyle.Dash;
            }

        }


        public void PlotModelClear()
        {
            _plotModel1.Series.Clear();
            _plotModel2.Series.Clear();
            _plotModel3.Series.Clear();
            _plotModel1.InvalidatePlot(true);
            _plotModel2.InvalidatePlot(true);
            _plotModel3.InvalidatePlot(true);
        }
    }



    public class MonitorParameters
    {
        public string Title;
        public string LeftAxisTitle;
        public string LeftAxisKey;
        public string BottomAxisTitle;
        public string LineTitle;
        public string YaxisKey;

        public MonitorParameters(string title, string leftAxisTitle, string leftAxisKey,
                                string bottomAxisTitle, string lineTitle, string yAxisKey)
        {
            Title = title;
            LeftAxisTitle = leftAxisTitle;
            LeftAxisKey = leftAxisKey;
            BottomAxisTitle = bottomAxisTitle;
            LineTitle = lineTitle;
            YaxisKey = yAxisKey;
        }
    }

}
