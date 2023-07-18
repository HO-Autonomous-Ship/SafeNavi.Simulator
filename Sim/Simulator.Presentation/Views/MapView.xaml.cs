using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Waf.Applications;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsPresentation;
using MathNet.Spatial.Euclidean;
using SyDLab.Usv.Simulator.Applications.ViewModels;
using SyDLab.Usv.Simulator.Applications.Views;
using SyDLab.Usv.Simulator.Domain.Models;
using SyDLab.Usv.Simulator.Domain.Models.Platforms;
using SyDLab.Usv.Simulator.Domain.Models.Tasks;
using SyDLab.Usv.Simulator.Presentation.Resources.Markers;
using MapViewModel = SyDLab.Usv.Simulator.Applications.ViewModels.MapViewModel;
using Path = System.Windows.Shapes.Path;

namespace SyDLab.Usv.Simulator.Presentation.Views
{
    [Export(typeof(IMapView))]
    public partial class MapView : IMapView
    {
        private readonly Dispatcher _dispatcher;

        //        private readonly GMapRoute _mapRoute;
        private readonly Lazy<MapViewModel> _viewModel;

        private readonly GMapRoute _waypointRoute;
        private PointLatLng _referencePoint;
        private Dictionary<object, GMapRoute> _mapRoutes = new Dictionary<object, GMapRoute>();
        private object _myShip;
        public bool IsSpaceFixed;

        public MapView()
        {
            InitializeComponent();

            _viewModel = new Lazy<MapViewModel>(this.GetViewModel<MapViewModel>);

            // config map
            //Map.MapProvider = GMapProviders.OpenStreetMap;
            Map.MapProvider = GMap.NET.MapProviders.GoogleMapProvider.Instance;
            Map.Position = new PointLatLng(37.0000, 125.5000);
            Map.Manager.Mode = AccessMode.ServerAndCache;
            //if (File.Exists("OpenStreetMapCache.gmdb"))
            //Singleton<GMaps>.Instance.ImportFromGMDB("OpenStreetMapCache.gmdb");

            Map.DragButton = MouseButton.Middle;

            // map events
            Map.OnPositionChanged += Map_OnCurrentPositionChanged;
            Map.OnTileLoadComplete += Map_OnTileLoadComplete;
            Map.OnTileLoadStart += Map_OnTileLoadStart;
            Map.OnMapTypeChanged += Map_OnMapTypeChanged;
            Map.OnMapZoomChanged += Map_OnMapZoomChanged;
            Map.MouseMove += Map_MouseMove;
            //Map.MouseLeftButtonDown += Map_MouseLeftButtonDown;
            Map.MouseEnter += Map_MouseEnter;

            if (Map.Markers.Count > 1)
                Map.ZoomAndCenterMarkers(null);
            else
                Map.Zoom = 16;

            _dispatcher = Dispatcher.CurrentDispatcher;

            //            _mapRoute = new GMapRoute(new List<PointLatLng>()) { ZIndex = 29 };
            //            Map.Markers.Add(_mapRoute);

            _waypointRoute = new GMapRoute(new List<PointLatLng>()) { ZIndex = 9 };
            Map.Markers.Add(_waypointRoute);
        }

        public void UpdateHeading(PlatformBase platform, double visualizedHeading)
        {

        }

        public void CreateMarker(object source)
        {
            // 임의 위치에 생성
            var pos = new PointLatLng(0.0, 0.0);
            //var pos = new PointLatLng(36.0917, 124.5000);

            if (source is MyShip)
            {
                _myShip = source;
                var marker = new GMapMarker(pos)
                {
                    ZIndex = 50,
                    Tag = source,
                    Shape = new MyShipMarker(source)
                };
                Map.Markers.Add(marker);

                //                _mapRoute.Clear();
                //                _mapRoute.RegenerateShape(Map);

                var mapRoute = new GMapRoute(new List<PointLatLng>()) { ZIndex = 29 };
                Map.Markers.Add(mapRoute);
                _mapRoutes[source] = mapRoute;
            }
            else if (source is TargetShip)
            {
                var marker = new GMapMarker(pos)
                {
                    ZIndex = 30,
                    Tag = source,
                    Shape = new TargetShipMarker(source)
                };
                Map.Markers.Add(marker);

                //                _mapRoute.Clear();
                //                _mapRoute.RegenerateShape(Map);

                var mapRoute = new GMapRoute(new List<PointLatLng>()) { ZIndex = 29 };
                Map.Markers.Add(mapRoute);
                _mapRoutes[source] = mapRoute;
            }
            else if (source is Waypoint)
            {
                var marker = new GMapMarker(pos)
                {
                    ZIndex = 10,
                    Tag = source,
                    Shape = new WaypointMarker(source)
                };
                Map.Markers.Add(marker);
            }
            else if (source is FollowPath)
            {
                var platformMarker = new GMapMarker(pos)
                {
                    ZIndex = 30,
                    Tag = source,
                    Shape = new WaypointMarker(source)
                };
                Map.Markers.Add(platformMarker);

                var routeMarker = new GMapRoute(new List<PointLatLng>())
                {
                    ZIndex = 9,
                    Tag = source
                };
                Map.Markers.Add(routeMarker);
            }
            else if (source is Obstacle obs)
            {
                List<PointLatLng> pList=new List<PointLatLng>();

                foreach (Waypoint point in obs.Points)
                {
                    var pointLatLon = GetLatLng(point.Location);
                    pList.Add(new PointLatLng(pointLatLon.X, pointLatLon.Y));
                }

                GMapPolygon polygon=new GMapPolygon(pList)
                {
                    ZIndex = 40,
                    Tag = source,
                };

                polygon.RegenerateShape(Map);
                (polygon.Shape as Path).Fill=new SolidColorBrush(Color.FromArgb(255, Colors.Green.R, Colors.Green.G, Colors.Green.B));
                (polygon.Shape as Path).StrokeThickness = 1;
                (polygon.Shape as Path).Stroke=new SolidColorBrush(Color.FromArgb(255, Colors.DarkGreen.R, Colors.DarkGreen.G, Colors.DarkGreen.B));
                (polygon.Shape as Path).Opacity = 1;

                Map.Markers.Add(polygon);
            }
        }

        public void UpdateObstacle(Obstacle obs)
        {
            List<Point2D> points=new List<Point2D>();
            foreach (Waypoint point in obs.Points)
            {
                points.Add(new Point2D(point.Location.X, point.Location.Y));
            }

            var dLat = 0.01;
            var dLng = 0.01;

            // Longitude 거리
            var dX =
                GMapProviders.EmptyProvider.Projection.GetDistance(
                    new PointLatLng(_referencePoint.Lat, _referencePoint.Lng - dLng / 2.0),
                    new PointLatLng(_referencePoint.Lat, _referencePoint.Lng + dLng / 2.0));
            // Latitude 거리
            var dY =
                GMapProviders.EmptyProvider.Projection.GetDistance(
                    new PointLatLng(_referencePoint.Lat - dLat / 2.0, _referencePoint.Lng),
                    new PointLatLng(_referencePoint.Lat + dLat / 2.0, _referencePoint.Lng));

            var latLngs = points.Select(i =>
            {
                var lat = _referencePoint.Lat + i.Y / 1000.0 / dY * dLat;
                var lng = _referencePoint.Lng + i.X / 1000.0 / dX * dLng;
                return new PointLatLng(lat, lng);
            });

            var res = Map.Markers.Where(i => i.Tag == obs);
            var portMarkers = res as GMapMarker[] ?? res.ToArray();
            if (portMarkers.Any())
                foreach (var item in portMarkers)
                    Map.Markers.Remove(item);

            var polygon = new GMapPolygon(latLngs)
            {
                Tag = obs,
                ZIndex = 40
            };

            polygon.RegenerateShape(Map);
            (polygon.Shape as Path).Fill = new SolidColorBrush(Color.FromArgb(255, Colors.Green.R, Colors.Green.G, Colors.Green.B));
            (polygon.Shape as Path).StrokeThickness = 1;
            (polygon.Shape as Path).Stroke = new SolidColorBrush(Color.FromArgb(255, Colors.DarkGreen.R, Colors.DarkGreen.G, Colors.DarkGreen.B));
            (polygon.Shape as Path).Opacity = 1;

            Map.Markers.Add(polygon);
        }

        public void AddObstaclePoint(Waypoint pXY)
        {
            var p = GetLatLng(pXY.Location);
            var pll=new PointLatLng(p.X, p.Y);
            var startMarker = new GMapMarker(pll)
            {
                ZIndex = 20,
                Tag = pXY,
                Shape = new StartMarker()
            };
            Map.Markers.Add(startMarker);
        }

        public void DeleteMarker(object source)
        {
            if (Map.Markers.All(i => i.Tag != source))
                return;

            var markers = Map.Markers.Where(i => i.Tag == source);
            var copyMakers = markers.ToList();
            foreach (var marker in copyMakers)
            {
                Map.Markers.Remove(marker);
            }
        }

        public Point2D CurrentLatLng { get; set; }

        public double GetBearing(Point2D startLatLng, Point2D endLatLng)
        {
            var start = new PointLatLng(startLatLng.X, startLatLng.Y);
            var end = new PointLatLng(endLatLng.X, endLatLng.Y);
            return GMapProviders.EmptyProvider.Projection.GetBearing(start, end);
        }

        /// <summary>
        /// 미터 좌표계를 위경도 좌표계로 반환
        /// 기준 위치는 Map의 Reference Point
        /// </summary>
        /// <param name="position">미터 기준 좌표</param>
        /// <returns>위경도 좌표</returns>
        public Point2D GetLatLng(Point2D position)
        {
            var dLat = 0.01;
            var dLng = 0.01;

            //
            // 단위 위경도(0.01 기준) 당 길이(m) 계산
            // 기준점은 Map의 Reference Point
            //

            // Longitude 거리
            var dX =
                GMapProviders.EmptyProvider.Projection.GetDistance(
                    new PointLatLng(_referencePoint.Lat, _referencePoint.Lng - dLng / 2.0),
                    new PointLatLng(_referencePoint.Lat, _referencePoint.Lng + dLng / 2.0));
            // Latitude 거리
            var dY =
                GMapProviders.EmptyProvider.Projection.GetDistance(
                    new PointLatLng(_referencePoint.Lat - dLat / 2.0, _referencePoint.Lng),
                    new PointLatLng(_referencePoint.Lat + dLat / 2.0, _referencePoint.Lng));

            //
            // 미터 단위 좌표를 위경도 좌표로 변환
            //
            var lat = _referencePoint.Lat + position.Y / 1000.0 / dY * dLat;
            var lng = _referencePoint.Lng + position.X / 1000.0 / dX * dLng;

            return new Point2D(lat, lng);
        }

        /// <summary>
        /// 위경도 좌표계를 미터 좌표계로 반환
        /// 기준 위치는 Map의 Reference Point
        /// </summary>
        /// <param name="latLng">위경도 기준 좌표</param>
        /// <returns>미터 좌표</returns>
        public Point2D GetPosition(Point2D latLng)
        {
            var dLat = 0.01;
            var dLng = 0.01;

            //
            // 단위 위경도(0.01 기준) 당 길이(m) 계산
            // 기준점은 Map의 Reference Point
            //

            // Longitude 거리
            var dX =
                GMapProviders.EmptyProvider.Projection.GetDistance(
                    new PointLatLng(_referencePoint.Lat, _referencePoint.Lng - dLng / 2.0),
                    new PointLatLng(_referencePoint.Lat, _referencePoint.Lng + dLng / 2.0));
            // Latitude 거리
            var dY =
                GMapProviders.EmptyProvider.Projection.GetDistance(
                    new PointLatLng(_referencePoint.Lat - dLat / 2.0, _referencePoint.Lng),
                    new PointLatLng(_referencePoint.Lat + dLat / 2.0, _referencePoint.Lng));

            //
            // 미터 단위 좌표를 위경도 좌표로 변환
            //
            var y = (latLng.X - _referencePoint.Lat) * 1000.0 * dY / dLat;
            var x = (latLng.Y - _referencePoint.Lng) * 1000.0 * dX / dLng;

            return new Point2D(x, y);
        }

        /// <summary>
        ///     기뢰 탐색 영역 설정 후 시작 위치와 종료 위치의 위경도 정보를 활용하여 시작 위치와 종료 위치의 좌표(미터 기준)를 반환 Line2D 클래스의 인스턴스로 반환
        /// </summary>
        /// <param name="startLatLng">시작 위치의 위경도 정보</param>
        /// <param name="endLatLng">종료 위치의 위경도 정보</param>
        /// <returns>시작 위치와 종료 위치의 좌표(미터 기준), 원점(0, 0)은 시작 위치와 종료 위치의 중점</returns>
        public Line2D GetRegion(Point2D startLatLng, Point2D endLatLng)
        {
            var dLat = 0.01;
            var dLng = 0.01;
            //var dLat = Math.Abs(startLatLng.X - endLatLng.X);
            //var dLng = Math.Abs(startLatLng.Y - endLatLng.Y);

            // Longitude 거리
            var dX =
                GMapProviders.EmptyProvider.Projection.GetDistance(
                    new PointLatLng(_referencePoint.Lat, _referencePoint.Lng - dLng / 2.0),
                    new PointLatLng(_referencePoint.Lat, _referencePoint.Lng + dLng / 2.0));
            // Latitude 거리
            var dY =
                GMapProviders.EmptyProvider.Projection.GetDistance(
                    new PointLatLng(_referencePoint.Lat - dLat / 2.0, _referencePoint.Lng),
                    new PointLatLng(_referencePoint.Lat + dLat / 2.0, _referencePoint.Lng));

            var startPoint = new Point2D(
                (startLatLng.Y - _referencePoint.Lng) / dLng * dX * 1000.0,
                (startLatLng.X - _referencePoint.Lat) / dLat * dY * 1000.0);
            var endPoint = new Point2D(
                (endLatLng.Y - _referencePoint.Lng) / dLng * dX * 1000.0,
                (endLatLng.X - _referencePoint.Lat) / dLat * dY * 1000.0);

            return new Line2D(startPoint, endPoint);
        }

        /// <summary>
        ///     Marker의 위치를 갱신
        /// </summary>
        /// <param name="source">Marker 구분용 소스, Marker 생성 시 Tag에 연결한 객체</param>
        /// <param name="x">갱신할 위치의 X 좌표, 미터 기준</param>
        /// <param name="y">갱신할 위치의 Y 좌표, 미터 기준</param>
        public void UpdateMarkerPosition(object source, double x, double y)
        {
            var dLat = 0.01;
            var dLng = 0.01;

            //
            // 단위 위경도(0.01 기준) 당 길이(m) 계산
            // 기준점은 Map의 Reference Point
            //

            // Longitude 거리
            var dX =
                GMapProviders.EmptyProvider.Projection.GetDistance(
                    new PointLatLng(_referencePoint.Lat, _referencePoint.Lng - dLng / 2.0),
                    new PointLatLng(_referencePoint.Lat, _referencePoint.Lng + dLng / 2.0));
            // Latitude 거리
            var dY =
                GMapProviders.EmptyProvider.Projection.GetDistance(
                    new PointLatLng(_referencePoint.Lat - dLat / 2.0, _referencePoint.Lng),
                    new PointLatLng(_referencePoint.Lat + dLat / 2.0, _referencePoint.Lng));

            //
            // 미터 단위 좌표를 위경도 좌표로 변환
            //
            var lat = _referencePoint.Lat + y / 1000.0 / dY * dLat;
            var lng = _referencePoint.Lng + x / 1000.0 / dX * dLng;

            //
            // USV 이동 궤적 업데이트
            //
            if (Map.Markers.Any(i => i.Tag == source && !(i is GMapRoute)))
            {
                var res = Map.Markers.First(i => i.Tag == source && !(i is GMapRoute));

                _dispatcher.Invoke(() =>
                {
                    res.Position = new PointLatLng(lat, lng);

                    if (!(source is MyShip) && !(source is TargetShip)) return;
                    
                    if (_mapRoutes.ContainsKey(source))
                    {
                        if (source is TargetShip)
                        {
                            if ((source as TargetShip).HistoryPosition.Count>0 &&
                                (source as TargetShip).HistoryPosition.Last().Time > 180 &&
                                _mapRoutes[source].Points.Count > 0)
                                _mapRoutes[source].Points.RemoveAt(0);
                        }

                        _mapRoutes[source].Points.Add(new PointLatLng(lat, lng));
                        _mapRoutes[source].RegenerateShape(Map);

                        if (source is MyShip)
                        {
                            if (ShipHeadingFixed.IsChecked.HasValue && ShipHeadingFixed.IsChecked.Value)
                                UpdateBearing((source as MyShip).CurrentHeading);
                            if (MapCenterFixed.IsChecked.HasValue && MapCenterFixed.IsChecked.Value)
                                UpdateCenter(lat, lng);
                        }
                    }
                });
            }
        }

        public void UpdateBearing(double bearing)
        {
            Map.Bearing = (float)bearing;
        }

        public void DeleteRoute(object source)
        {
            //
            // USV 이동 궤적 업데이트
            //
            foreach (GMapRoute route in _mapRoutes.Values)
            {
                route.Points.Clear();
                route.RegenerateShape(Map);
            }
        }

        public void UpdatePolygon(object source, IList<Point2D> points)
        {
            var dLat = 0.01;
            var dLng = 0.01;

            // Longitude 거리
            var dX =
                GMapProviders.EmptyProvider.Projection.GetDistance(
                    new PointLatLng(_referencePoint.Lat, _referencePoint.Lng - dLng / 2.0),
                    new PointLatLng(_referencePoint.Lat, _referencePoint.Lng + dLng / 2.0));
            // Latitude 거리
            var dY =
                GMapProviders.EmptyProvider.Projection.GetDistance(
                    new PointLatLng(_referencePoint.Lat - dLat / 2.0, _referencePoint.Lng),
                    new PointLatLng(_referencePoint.Lat + dLat / 2.0, _referencePoint.Lng));

            var latLngs = points.Select(i =>
            {
                var lat = _referencePoint.Lat + i.Y / 1000.0 / dY * dLat;
                var lng = _referencePoint.Lng + i.X / 1000.0 / dX * dLng;
                return new PointLatLng(lat, lng);
            });

            var res = Map.Markers.Where(i => i.Tag == source);
            var portMarkers = res as GMapMarker[] ?? res.ToArray();
            if (portMarkers.Any())
                foreach (var item in portMarkers)
                    Map.Markers.Remove(item);

            var polygon = new GMapPolygon(latLngs)
            {
                Tag = source,
                ZIndex = 5
            };
            Map.Markers.Add(polygon);

            var pt = Point2D.MidPoint(points.First(), points.Last());
            var latLng = new PointLatLng(
                _referencePoint.Lat + pt.Y / 1000.0 / dY * dLat,
                _referencePoint.Lng + pt.X / 1000.0 / dX * dLng);

            var marker = new GMapMarker(latLng)
            {
                ZIndex = 8,
                Tag = source,
                Shape = new PortMarker(source)
            };
            Map.Markers.Add(marker);
        }

        public void UpdateReference(double lat, double lng, bool moveFocus = false)
        {
            _referencePoint = new PointLatLng(lat, lng);
            if (moveFocus)
                Map.Position = _referencePoint;
        }

        private void UpdateCenter(double lat, double lng)
        {
            Map.Position = new PointLatLng(lat, lng);
        }

        public void UpdateRoute(TaskBase task)
        {
            var marker = Map.Markers.First(i => i.Tag == task && i is GMapRoute);
            if (!(marker is GMapRoute route))
                return;

            route.Points.Clear();

            if (task.Platform != null)
            {
                Point2D pll = GetLatLng(new Point2D(task.Platform.CurrentX, task.Platform.CurrentY));
                PointLatLng p = new PointLatLng(pll.X, pll.Y);

                route.Points.Add(p);
            }

            foreach (var item in task.Waypoints)
            {
                if (Map.Markers.All(i => i.Tag != item))
                    continue;

                var res = Map.Markers.First(i => i.Tag == item);

                _dispatcher.Invoke(() => { route.Points.Add(res.Position); });
            }

            route.RegenerateShape(Map);
            var path = route.Shape as System.Windows.Shapes.Path;
            if (path != null)
            {
                //if (addBlurEffect)
                //{
                //	BlurEffect ef = new BlurEffect();
                //	{
                //		ef.KernelType = KernelType.Gaussian;
                //		ef.Radius = 3.0;
                //		ef.RenderingBias = RenderingBias.Performance;
                //	}

                //	path.Effect = ef;
                //}

                path.Stroke = Brushes.White;
                path.StrokeThickness = 3;
                //path.StrokeLineJoin = PenLineJoin.Round;
                //path.StrokeStartLineCap = PenLineCap.Triangle;
                //path.StrokeEndLineCap = PenLineCap.Square;

                path.Opacity = 0.6;
                //path.IsHitTestVisible = false;
            }
        }

        public void DeleteRoute(TaskBase task)
        {
            // Route 삭제
            var marker = Map.Markers.First(i => i.Tag == task && i is GMapRoute);
            if (!(marker is GMapRoute route))
                return;

            route.Points.Clear();
            Map.Markers.Remove(marker);

            // Waypoint 삭제
            foreach (Waypoint waypoint in task.Waypoints)
            {
                DeleteWaypoint(waypoint);
            }

            // USV 삭제
            var markerUsv = Map.Markers.First(i => i.Tag == task);

            if (markerUsv != null)
                Map.Markers.Remove(markerUsv);
        }

        public void DeleteWaypoint(Waypoint waypoint)
        {
            var res = Map.Markers.First(i => i.Tag == waypoint);

            if (res == null)
                return;

            Map.Markers.Remove(res);
        }

        public void UpdateRoute(IList<Waypoint> waypoints)
        {
            _waypointRoute.Points.Clear();

            foreach (var item in waypoints)
            {
                var res = Map.Markers.First(i => i.Tag == item);

                _dispatcher.Invoke(() => { _waypointRoute.Points.Add(res.Position); });
            }

            _waypointRoute.RegenerateShape(Map);
            var path = _waypointRoute.Shape as System.Windows.Shapes.Path;
            if (path != null)
            {
                //if (addBlurEffect)
                //{
                //	BlurEffect ef = new BlurEffect();
                //	{
                //		ef.KernelType = KernelType.Gaussian;
                //		ef.Radius = 3.0;
                //		ef.RenderingBias = RenderingBias.Performance;
                //	}

                //	path.Effect = ef;
                //}

                path.Stroke = Brushes.White;
                path.StrokeThickness = 3;
                //path.StrokeLineJoin = PenLineJoin.Round;
                //path.StrokeStartLineCap = PenLineCap.Triangle;
                //path.StrokeEndLineCap = PenLineCap.Square;

                path.Opacity = 0.6;
                //path.IsHitTestVisible = false;
            }
        }

        private void Map_MouseEnter(object sender, MouseEventArgs e)
        {
        }

        private void Map_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!(DataContext as MapViewModel).IsClickModeOn)
                return;

            var p = e.GetPosition(Map);
            var latLng = Map.FromLocalToLatLng((int)p.X, (int)p.Y);

            var pMeter = GetPosition(new Point2D(latLng.Lat, latLng.Lng));

            (DataContext as MapViewModel).ObstaclePoint = new Point2D(Math.Round(pMeter.X, 2), Math.Round(pMeter.Y, 2));
        }

        private void Map_MouseMove(object sender, MouseEventArgs e)
        {
            var p = e.GetPosition(Map);
            var latLng = Map.FromLocalToLatLng((int)p.X, (int)p.Y);
            CurrentLatLngTextBlock.Text = latLng.ToString();
        }

        private void Map_OnCurrentPositionChanged(PointLatLng point)
        {
        }

        private void Map_OnMapTypeChanged(GMapProvider type)
        {
        }

        private void Map_OnMapZoomChanged()
        {
            // 확대/축소에 따른 marker의 가시화 변경
            //foreach (var item in Map.Markers)
            //{
            //	(item.Shape as IShapeUpdatable)?.UpdateShape(Map);
            //}
        }

        private void Map_OnPreviewMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            var p = e.GetPosition(Map);
            var currentLatLng = Map.FromLocalToLatLng((int)p.X, (int)p.Y);
            CurrentLatLng = new Point2D(currentLatLng.Lat, currentLatLng.Lng);
        }

        private void MapCenterFixed_OnClick(object sender, RoutedEventArgs e)
        {
            if (Map.Markers.Any(i => i.Tag == _myShip && !(i is GMapRoute)))
            {
                var res = Map.Markers.First(i => i.Tag == _myShip && !(i is GMapRoute));
                UpdateCenter(res.Position.Lat, res.Position.Lng);
            }
        }

        private void Map_OnTileLoadComplete(long elapsedmilliseconds)
        {
        }

        private void Map_OnTileLoadStart()
        {
        }

        private void ZoomInButton_OnClick(object sender, RoutedEventArgs e)
        {
            Map.Zoom += 1;
        }

        private void ZoomOutButton_OnClick(object sender, RoutedEventArgs e)
        {
            Map.Zoom -= 1;
        }

        private void toggleButton_IsCheckedChanged(object sender, RoutedEventArgs e)
        {
            if (ShipHeadingFixed.IsChecked.HasValue && ShipHeadingFixed.IsChecked.Value)
            {
                UpdateBearing((_myShip as MyShip).Heading);
            }
            else
            {
                UpdateBearing(0);
            }
        }
    }
}