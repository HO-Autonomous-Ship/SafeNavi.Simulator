using System;
using System.Collections.Generic;
using GMap.NET;
using GMap.NET.MapProviders;
using MathNet.Spatial.Euclidean;
using Microsoft.Maps.MapControl.WPF;
using SyDLab.Usv.Simulator.Domain.Models;

namespace SyDLab.Usv.Simulator.Domain
{
    public enum GeographicalFormula
    {
        Vincenty = 0,
        Haversine = 1
    }
    public class Distance
    {
        private const double EarthRadius = 6378137; //R
        private const double MajorAxis = 6378137; //a
        private const double MinorAxis = 6356752.314245; //b
        private const double EarthFlattening = 1 / 298.257223563; //f
        public GeographicalFormula DistanceFormula = GeographicalFormula.Haversine;
        public Location Start = new Location();
        public Location Stop = new Location();
        public double Nnode = 0;
        public int AdjustLon = 0;
        public double GreatCircleDistance = 0;
        public double GreatCircleAzimuth = 0;
        public List<Location> RouteLocations = new List<Location>();
        public List<double> RouteAzimuths = new List<double>();
        public static double KN2MS = 0.514444;
        public static double MS2KN = 1.943844;
        public static double NM2KM = 1.852;
        public const double D2R = Math.PI / 180;
        public const double R2D = 180 / Math.PI;

        private static Project Project => Singleton<Project>.UniqueInstance;


        public Distance(Location start, Location stop, double nnode)
        {
            Start = new Location(start.Latitude, start.Longitude);
            Stop = new Location(stop.Latitude, stop.Longitude);

            //출발지, 도착지 관련 조정
            if (System.Math.Abs(start.Longitude - stop.Longitude) <= 180) //출발지, 도착지 관련 이상 없음, Longitude는 [-180,180)으로 들어가 있음
            {
                AdjustLon = 1; //출발지, 도착지 경도 변화x
            }
            else if (start.Longitude < 0) //출발지의 longitude + 360 하여 지도를 옮김.
            {
                Start.Longitude = Start.Longitude + 360;
                AdjustLon = 0; //출발지의 경도 변화o
            }
            else if (stop.Longitude < 0) //도착지의 longitude + 360 하여 지도를 옮김.
            {
                Stop.Longitude = Stop.Longitude + 360;
                AdjustLon = 0; //도착지의 경도 변화o
            }

            if (nnode != 0)
                Nnode = nnode;
            else
                Nnode = 10;
        }

        public void GreatCircleRoute()
        {
            LonLat2Distance(Start, Stop, ref GreatCircleDistance, ref GreatCircleAzimuth);
            double nodeDistance = GreatCircleDistance / Nnode;
            RouteLocations.Add(Start);
            RouteAzimuths.Add(GreatCircleAzimuth);

            for (int i = 0; i < Nnode; i++)
            {
                Location nextPoint = new Location();
                double tmpDistance = 0;
                double tmpAzimuth = 0;
                Distance2LonLat(RouteLocations[i], ref nextPoint, nodeDistance, RouteAzimuths[i], DistanceFormula);

                if ((AdjustLon == 0) && (nextPoint.Longitude < -90))
                    nextPoint.Longitude += 360;

                RouteLocations.Add(nextPoint);
                LonLat2Distance(nextPoint, Stop, ref tmpDistance, ref tmpAzimuth);
                RouteAzimuths.Add(tmpAzimuth);
            }
            RouteLocations.Add(Stop);
            RouteAzimuths.Add(0);
        }

        /// <summary>
        /// 미터 좌표계를 위경도 좌표계로 반환
        /// 기준 위치는 Map의 Reference Point
        /// </summary>
        /// <param name="position">미터 기준 좌표</param>
        /// <returns>위경도 좌표</returns>
        public static Location GetLatLng(Point3D position)
        {
            PointLatLng _referencePoint = new PointLatLng(Project.ReferenceLatLng.X, Project.ReferenceLatLng.Y);

            if (Project.MyShips.Count != 0)
                _referencePoint = new PointLatLng(Project.MyShips[0].InitialLonLat.Latitude,
                    Project.MyShips[0].InitialLonLat.Longitude);

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

            return new Location(lat, lng);
        }

        /// <summary>
        /// 위경도 좌표계를 미터 좌표계로 반환
        /// 기준 위치는 Map의 Reference Point
        /// </summary>
        /// <param name="latLng">위경도 기준 좌표</param>
        /// <returns>미터 좌표</returns>
        public static Point3D GetPosition(Location latLng)
        {
            PointLatLng _referencePoint = new PointLatLng(Project.ReferenceLatLng.X, Project.ReferenceLatLng.Y);

            if (Project.MyShips.Count > 0)
                _referencePoint = new PointLatLng(Project.MyShips[0].InitialLonLat.Latitude,
                    Project.MyShips[0].InitialLonLat.Longitude);

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
            var y = (latLng.Latitude- _referencePoint.Lat) * 1000.0 * dY / dLat;
            var x = (latLng.Longitude - _referencePoint.Lng) * 1000.0 * dX / dLng;

            return new Point3D(x, y,0);
        }

        public static double Degree2Radian(double degree)
        {
            return degree * Math.PI / 180;
        }

        public static double Radian2Degree(double radian)
        {
            return radian * 180 / Math.PI;
        }

        public static void Distance2LonLat(Location point, ref Location next, double distance, double heading, GeographicalFormula type=GeographicalFormula.Vincenty)
        {
            Location start = new Location(Degree2Radian(point.Latitude), Degree2Radian(point.Longitude));
            double azimuth = Degree2Radian(heading);
            switch (type)
            {
                default:
                case GeographicalFormula.Vincenty: //direct solutioin
                    {
                        double sinAlpha1 = Math.Sin(azimuth);
                        double cosAlpha1 = Math.Cos(azimuth);

                        double tanU1 = (1 - EarthFlattening) * Math.Tan(start.Latitude), cosU1 = 1 / Math.Sqrt((1 + tanU1 * tanU1)), sinU1 = tanU1 * cosU1;
                        double sigma1 = Math.Atan2(tanU1, cosAlpha1);
                        double sinAlpha = cosU1 * sinAlpha1;
                        double cosSqAlpha = 1 - sinAlpha * sinAlpha;
                        double uSq = cosSqAlpha * (MajorAxis * MajorAxis - MinorAxis * MinorAxis) / (MinorAxis * MinorAxis);
                        double A = 1 + uSq / 16384 * (4096 + uSq * (-768 + uSq * (320 - 175 * uSq)));
                        double B = uSq / 1024 * (256 + uSq * (-128 + uSq * (74 - 47 * uSq)));

                        double sigma = distance / (MinorAxis * A), sigma0;
                        double cos2SigmaM;
                        double sinSigma;
                        double cosSigma;
                        double delSigma;

                        do
                        {
                            cos2SigmaM = Math.Cos(2 * sigma1 + sigma);
                            sinSigma = Math.Sin(sigma);
                            cosSigma = Math.Cos(sigma);
                            delSigma = B * sinSigma * (cos2SigmaM + B / 4 * (cosSigma * (-1 + 2 * cos2SigmaM * cos2SigmaM) - B / 6 * cos2SigmaM * (-3 + 4 * sinSigma * sinSigma) * (-3 + 4 * cos2SigmaM * cos2SigmaM)));
                            sigma0 = sigma;
                            sigma = distance / (MinorAxis * A) + delSigma;
                        } while (Math.Abs(sigma - sigma0) > 1e-12);

                        double tmp = sinU1 * sinSigma - cosU1 * cosSigma * cosAlpha1;
                        next.Latitude = Radian2Degree(Math.Atan2(sinU1 * cosSigma + cosU1 * sinSigma * cosAlpha1, (1 - EarthFlattening) * Math.Sqrt(sinAlpha * sinAlpha + tmp * tmp)));
                        double lamda = Math.Atan2(sinSigma * sinAlpha1, cosU1 * cosSigma - sinU1 * sinSigma * cosAlpha1);
                        double C = EarthFlattening / 16 * cosSqAlpha * (4 + EarthFlattening * (4 - 3 * cosSqAlpha));
                        double L = lamda - (1 - C) * EarthFlattening * sinAlpha * (sigma + C * sinSigma * (cos2SigmaM + C * cosSigma * (-1 + 2 * cos2SigmaM * cos2SigmaM)));
                        next.Longitude = Radian2Degree((start.Longitude + L + 3 * Math.PI) % (2 * Math.PI) - Math.PI); // normalise to -180...+180

                        break;
                    }
                case GeographicalFormula.Haversine:
                    {
                        //angle과 distance를 가지고 다음 위경도를 계산하자
                        double delLat = 0;
                        double delLon = 0;
                        double angle = distance / EarthRadius; //radian

                        delLat = Radian2Degree(Math.Tan(angle) * Math.Cos(azimuth));

                        if (azimuth == 0 || azimuth == 180)
                        {
                            delLon = 0;
                        }
                        else if (azimuth < 180)
                        {
                            delLon = Radian2Degree(Math.Pow(2 * (1 - Math.Cos(angle) - 0.5 * Math.Tan(angle) * Math.Sin(angle) * Math.Cos(azimuth) * Math.Cos(azimuth)), 0.5));
                        }
                        else
                        {
                            delLon = -Radian2Degree(Math.Pow(2 * (1 - Math.Cos(angle) - 0.5 * Math.Tan(angle) * Math.Sin(angle) * Math.Cos(azimuth) * Math.Cos(azimuth)), 0.5));
                        }

                        //찾은 지점 넣기

                        next.Longitude = point.Longitude + delLon;
                        next.Latitude = point.Latitude + delLat;
                        break;
                    }
            }
            next.Longitude = (next.Longitude + 180) % 360 - 180; //normalize
        }

        public static void Point2LonLat(Location point, ref Location next, double x, double y)
        {
            double distance = Math.Sqrt(x * x + y * y);
            double azimuth = Radian2Degree(Math.Atan2(y, x));

            Distance2LonLat(point, ref next, distance, azimuth);
        }

        public static void LonLat2Point(Location start, Location stop, ref double x, ref double y) // [m]
        {
            double distance = 0, azimuth = 0;
            LonLat2Distance(start, stop,ref distance, ref azimuth);

            x = distance * Math.Cos(Degree2Radian(azimuth));
            y= distance * Math.Sin(Degree2Radian(azimuth));
        }

        public static void LonLat2Point(Location start, Location stop, ref Point3D point) // [m]
        {
            double distance = 0, azimuth = 0;
            LonLat2Distance(start, stop, ref distance, ref azimuth);

            double x = distance * Math.Cos(Degree2Radian(azimuth));
            double y = distance * Math.Sin(Degree2Radian(azimuth));

            point = new Point3D(x, y, 0);
        }


        public static void LonLat2Distance(Location start, Location stop, ref double distance, ref double azimuth) // [m]
        {
            double L = Degree2Radian(stop.Longitude) - Degree2Radian(start.Longitude);
            double tanU1 = (1.0 - EarthFlattening) * Math.Tan(Degree2Radian(start.Latitude));
            double cosU1 = 1.0 / Math.Sqrt((1.0 + tanU1 * tanU1));
            double sinU1 = tanU1 * cosU1;
            double tanU2 = (1.0 - EarthFlattening) * Math.Tan(Degree2Radian(stop.Latitude));
            double cosU2 = 1.0 / Math.Sqrt((1.0 + tanU2 * tanU2));
            double sinU2 = tanU2 * cosU2;

            double ramda = L;
            double ramda2;
            int iterationLimit = 100;

            double sinramda;
            double cosramda;
            double sinSqSigma;
            double sinSigma;
            double cosSigma;
            double sigma;
            double sinAlpha;
            double cosSqAlpha;
            double cos2SigmaM;
            double C;

            do
            {
                sinramda = Math.Sin(ramda);
                cosramda = Math.Cos(ramda);
                sinSqSigma = (cosU2 * sinramda) * (cosU2 * sinramda) + (cosU1 * sinU2 - sinU1 * cosU2 * cosramda) * (cosU1 * sinU2 - sinU1 * cosU2 * cosramda);
                sinSigma = Math.Sqrt(sinSqSigma);

                if (Math.Abs(sinSigma) < 1e-5)
                {
                    distance = 0;
                    azimuth = 0;
                    return;
                } //return 0; // co-incident points

                cosSigma = sinU1 * sinU2 + cosU1 * cosU2 * cosramda;
                sigma = Math.Atan2(sinSigma, cosSigma);
                sinAlpha = cosU1 * cosU2 * sinramda / sinSigma;
                cosSqAlpha = 1.0 - sinAlpha * sinAlpha;
                cos2SigmaM = cosSigma - 2.0 * sinU1 * sinU2 / cosSqAlpha;

                if (double.IsNaN(cos2SigmaM))
                    cos2SigmaM = 0; // equatorial line: cosSqα=0 (§6)

                C = EarthFlattening / 16.0 * cosSqAlpha * (4.0 + EarthFlattening * (4.0 - 3.0 * cosSqAlpha));
                ramda2 = ramda;
                ramda = L + (1 - C) * EarthFlattening * sinAlpha * (sigma + C * sinSigma * (cos2SigmaM + C * cosSigma * (-1 + 2 * cos2SigmaM * cos2SigmaM)));
            } while (Math.Abs(ramda - ramda2) > 1e-6 && --iterationLimit > 0);

            if (iterationLimit < 0)
                throw new ArgumentException("SimulataneousOptimization.LonLat2Distance: Formula failed to converge");

            double uSq = cosSqAlpha * (MajorAxis * MajorAxis - MinorAxis * MinorAxis) / (MinorAxis * MinorAxis);
            double A = 1.0 + uSq / 16384.0 * (4096.0 + uSq * (-768.0 + uSq * (320.0 - 175.0 * uSq)));
            double B = uSq / 1024.0 * (256.0 + uSq * (-128.0 + uSq * (74.0 - 47.0 * uSq)));
            double delSigma = B * sinSigma * (cos2SigmaM + B / 4.0 * (cosSigma * (-1.0 + 2.0 * cos2SigmaM * cos2SigmaM) - B / 6.0 * cos2SigmaM * (-3.0 + 4.0 * sinSigma * sinSigma) * (-3.0 + 4.0 * cos2SigmaM * cos2SigmaM)));

            distance = MinorAxis * A * (sigma - delSigma);

            double fwdAz = Math.Atan2(cosU2 * sinramda, cosU1 * sinU2 - sinU1 * cosU2 * cosramda);

            azimuth = (Radian2Degree(fwdAz) + 360) % 360;
        }
    }
}