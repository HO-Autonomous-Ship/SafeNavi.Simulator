using System.Collections.Generic;
using System.Waf.Applications;
using MathNet.Spatial.Euclidean;
using SyDLab.Usv.Simulator.Applications.ViewModels;
using SyDLab.Usv.Simulator.Domain.Models;
using SyDLab.Usv.Simulator.Domain.Models.Platforms;
using SyDLab.Usv.Simulator.Domain.Models.Tasks;

namespace SyDLab.Usv.Simulator.Applications.Views
{
    public interface IMapView : IView
    {
        Point2D CurrentLatLng { get; }

        void CreateMarker(object source);

        double GetBearing(Point2D startLatLng, Point2D endLatLng);

        Line2D GetRegion(Point2D startLatLng, Point2D endLatLng);

        void UpdateMarkerPosition(object source, double x, double y);

        void UpdateReference(double lat, double lng, bool moveFocus);

        void UpdateRoute(IList<Waypoint> waypoints);

        void UpdatePolygon(object source, IList<Point2D> points);

        void UpdateRoute(TaskBase task);

        void AddObstaclePoint(Waypoint pXY);

        Point2D GetLatLng(Point2D position);

        Point2D GetPosition(Point2D latLng);

        void DeleteRoute(TaskBase task);

        void DeleteWaypoint(Waypoint waypoint);

        void UpdateObstacle(Obstacle obs);

        void DeleteRoute(object source);

        void DeleteMarker(object source);

        void UpdateBearing(double bearing);
    }
}