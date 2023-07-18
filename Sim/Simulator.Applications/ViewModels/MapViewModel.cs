using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Waf.Applications;
using System.Windows.Input;
using MathNet.Spatial.Euclidean;
using SyDLab.Usv.Simulator.Applications.Views;
using SyDLab.Usv.Simulator.Domain.Models;
using SyDLab.Usv.Simulator.Domain.Models.Platforms;
using SyDLab.Usv.Simulator.Domain.Models.Tasks;

namespace SyDLab.Usv.Simulator.Applications.ViewModels
{
    [Export]
    public class MapViewModel : ViewModel<IMapView>
    {
        private Point2D _obstaclePoint;
        private DelegateCommand _commandSetEndPoint;
        private DelegateCommand _commandSetPortPosition;
        private DelegateCommand _commandSetStartPoint;
        private bool _isHeadingFixed;

        public bool IsClickModeOn = false;

        [ImportingConstructor]
        public MapViewModel(IMapView view) : base(view)
        {
        }

        public Point2D ObstaclePoint
        {
            get => _obstaclePoint;
            set => SetProperty(ref _obstaclePoint, value);
        }

        public bool IsHeadingFixed
        {
            get => _isHeadingFixed;
            set => SetProperty(ref _isHeadingFixed, value);
        }

        public DelegateCommand CommandSetEndPoint
        {
            get => _commandSetEndPoint;
            set => SetProperty(ref _commandSetEndPoint, value);
        }

        public DelegateCommand CommandSetPortPosition
        {
            get => _commandSetPortPosition;
            set => SetProperty(ref _commandSetPortPosition, value);
        }

        public DelegateCommand CommandSetStartPoint
        {
            get => _commandSetStartPoint;
            set => SetProperty(ref _commandSetStartPoint, value);
        }

        public Point2D CurrentLatLng => ViewCore.CurrentLatLng;

        public void CreateMarker(object source)
        {
            ViewCore.CreateMarker(source);
        }

        public void UpdateMarkerPosition(object source, double x, double y)
        {
            ViewCore.UpdateMarkerPosition(source, x, y);
        }

        public void UpdateReference(Point2D latLng, bool moveFocus = false)
        {
            ViewCore.UpdateReference(latLng.X, latLng.Y, moveFocus);
        }

        public void UpdateRoute(IList<Waypoint> waypoints)
        {
            ViewCore.UpdateRoute(waypoints);
        }

        public void UpdateRoute(TaskBase task)
        {
            ViewCore.UpdateRoute(task);
        }

        public void UpdatePolygon(object source, IList<Point2D> points)
        {
            ViewCore.UpdatePolygon(source, points);
        }

        public Line2D GetRegion(Point2D startLatLng, Point2D endLatLng)
        {
            return ViewCore.GetRegion(startLatLng, endLatLng);
        }

        public double GetBearing(Point2D startLatLng, Point2D endLatLng)
        {
            return ViewCore.GetBearing(startLatLng, endLatLng);
        }

        public Point2D GetPosition(Point2D latLng)
        {
            return ViewCore.GetPosition(latLng);
        }

        public Point2D GetLatLng(Point2D position)
        {
            return ViewCore.GetLatLng(position);
        }

        public void DeleteRoute(TaskBase task)
        {
            ViewCore.DeleteRoute(task);
        }

        public void DeleteRoute(object source)
        {
            ViewCore.DeleteRoute(source);
        }

        public void DeleteWaypoint(Waypoint waypoint)
        {
            ViewCore.DeleteWaypoint(waypoint);
        }

        public void DeleteMarker(object source)
        {
            ViewCore.DeleteMarker(source);
        }

        internal void AddObstaclePoint(Waypoint waypoint)
        {
            ViewCore.AddObstaclePoint(waypoint);
        }

        internal void UpdateObstacle(Obstacle obs)
        {
            ViewCore.UpdateObstacle(obs);
        }

        internal void UpdateBearing(double bearing)
        {
            ViewCore.UpdateBearing(bearing);
        }
    }
}