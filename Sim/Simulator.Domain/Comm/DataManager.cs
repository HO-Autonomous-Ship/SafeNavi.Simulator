using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Spatial.Euclidean;
using Microsoft.Maps.MapControl.WPF;
using SyDLab.Usv.Simulator.Domain.Models;
using SyDLab.Usv.Simulator.Domain.Models.Platforms;

namespace SyDLab.Usv.Simulator.Domain.Comm
{
    public class DataManager
    {
        public Project Project => Singleton<Project>.UniqueInstance;

        #region ST_DATA_SA_OUT
        public ST_DATA_SA_OUT RetrieveData(ST_DATA_SA_OUT stData)
        {
            MyShip myShip = Project.MyShips[0];
            List<TargetShip> targetShips = Project.TargetShips.ToList();

            ST_POS stOsPos = new ST_POS()
            {
                Lat = (float)myShip.CurrentLat,
                Lon = (float)myShip.CurrentLon,
                Bearing = 0
            };

            ST_DGPS_POS stOsDgpsPos = new ST_DGPS_POS();
            stOsDgpsPos.LatSign = myShip.CurrentLat > 0 ? 1 : -1;
            stOsDgpsPos.LatDeg = (float)((int)myShip.CurrentLat);
            stOsDgpsPos.LatMin = (float)(myShip.CurrentLat - (int)myShip.CurrentLat);
            stOsDgpsPos.LonSign = myShip.CurrentLat > 0 ? 1 : -1;
            stOsDgpsPos.LonDeg = (float)((int)myShip.CurrentLon);
            stOsDgpsPos.LonMin = (float)(myShip.CurrentLon - (int)myShip.CurrentLon);

            ST_VESSEL_VELOCITY stOsVelocity = new ST_VESSEL_VELOCITY()
            {
                Sog = (float)(myShip.Speed*Distance.KN2MS),
                Cog = (float)myShip.CurrentHeading,
                Rot = (float)myShip.AngularVelocity,
            };

            ST_VESSEL_MOTION stOsMotion = new ST_VESSEL_MOTION()
            {
                Roll = 0,
                Pitch = 0
            };

            ST_BASE_STATE stOsBaseState = new ST_BASE_STATE()
            {
                PosX = (float)myShip.CurrentY,
                PosY = (float)myShip.CurrentX,
                Sog = (float)(myShip.Speed*Distance.KN2MS),
                Cog = (float)myShip.CurrentHeading,
            };


            List<Waypoint> waypoints = new List<Waypoint>();
            try
            {
                waypoints = Project.Scenarios[0].Tasks[0].Waypoints.ToList();
            }
            catch (Exception)
            {
            }
            ST_VOYAGE_PLAN stVoyagePlan = new ST_VOYAGE_PLAN()
            {
                StPos = new ST_POS[Common.MAX_NUM_WAYPOINT],
                NumWaypoints = waypoints.Count
            };
            foreach (var item in waypoints.Select((value, index) => (value, index)))
            {
                var value = item.value;
                var index = item.index;
                Location location = Distance.GetLatLng(new Point3D(value.Y, value.X, 0));

                stVoyagePlan.StPos[index] = new ST_POS()
                {
                    Lat = (float)location.Latitude,
                    Lon = (float)location.Longitude
                };
            }

            ST_VOYAGE_PLAN stAvoidPlan = new ST_VOYAGE_PLAN();

            ST_VOYAGE_INFO stVoyageInfo = new ST_VOYAGE_INFO()
            {
                StPosDeparture = new ST_DGPS_POS(),
                StPosDestination = new ST_DGPS_POS(),
                CollisionSituation = false,
                StVoyagePlan = stVoyagePlan,
                StAvoidPlan = stAvoidPlan,
                Rta = 0,
                Eta = 0,
                PosIndex = Project.Scenarios[0].Tasks[0].TargetWaypoint
            };

            ST_ENVIRONMENT stEnv = new ST_ENVIRONMENT()
            {
                AirTemp = 0,
                WaterTemp = 0,
                WaterDepth = 0,
                CurrentDir = 0,
                CurrentSpeed = 0,
                WindDir = 0,
                WindSpeed = 0,
                WaveDir = 0,
                WaveHeight = 0,
                WavePeriod = 0,
                WeatherHarshness = (int)WeatherHarshness.Medium,
                WeatherCase = (int)WeatherCase.Clear
            };

            ST_VESSEL_INFO stOwnshipInfo = new ST_VESSEL_INFO()
            {
                ShipId = 0,
                ImoNum = 0,
                Mmsi = 12345678,
                CallSign = new char[Common.MAX_CALL_SIGN_LENGTH],
                VesselName = new char[Common.MAX_VESSEL_NAME_LENGTH],
                VesselType = (uint)ShipType.FISH,
                Lpp = (float)myShip.Length,
                Breadth = (float)myShip.Length / 4
            };
            foreach (var item in myShip.DisplayName.Select((value, index) => (value, index)))
            {
                stOwnshipInfo.VesselName[item.index] = item.value;
            }


            ST_OWNSHIP_INFO stOsInfo = new ST_OWNSHIP_INFO()
            {
                Time = 0,
                StVesselInfo = stOwnshipInfo,
                NumPropeller = 2,
                MinWaterDepth = 0,
                MaxVesselSpeed = 0,
                StBaseState = stOsBaseState,
                Heading = (float)myShip.CurrentHeading,
                Rot = (float)myShip.AngularVelocity,
                StPos = stOsPos,
                DraftFwd = 0,
                DraftAft = 0,
                PropellerRpm = new float[2] { 0, 0 },
                RudderAngle = new float[2] { 0, 0 },
                StVesselMotion = stOsMotion,
                StVoyageInfo = stVoyageInfo
            };

            ST_OBJECT_INFO[] stObjectState = new ST_OBJECT_INFO[Common.MAX_NUM_TARGET_SHIP];
            int numTargetship = 0;
            Random rand = new Random();
            foreach (var ship in targetShips)
            {
                ST_VESSEL_INFO stVesselInfo = new ST_VESSEL_INFO()
                {
                    ShipId = (uint)numTargetship,
                    ImoNum = 0,
                    Mmsi = (uint)ship.MMSI,
                    CallSign = new char[Common.MAX_CALL_SIGN_LENGTH],
                    VesselName = new char[Common.MAX_VESSEL_NAME_LENGTH],
                    VesselType = (uint)rand.Next(0,6),
                    Lpp = (float)ship.Length,
                    Breadth = (float)ship.Length/4.0f,
                };
                foreach (var item in ship.DisplayName.Select((value, index)=>(value, index)))
                {
                    stVesselInfo.VesselName[item.index] = item.value;
                }

                ST_BASE_STATE stBaseState = new ST_BASE_STATE()
                {
                    PosX = (float)ship.CurrentY,
                    PosY = (float)ship.CurrentX,
                    Sog = (float)(ship.CurrentSpeed * Distance.KN2MS),
                    Cog = (float)ship.CurrentHeading
                };
                var latlngTarget = Distance.GetLatLng(new Point3D(ship.CurrentX, ship.CurrentY, 0));
                ST_POS stPos = new ST_POS()
                {
                    Lat = (float)latlngTarget.Latitude,
                    Lon = (float)latlngTarget.Longitude,
                    Bearing = 0,
                };
                ST_OBJECT_INFO stObj = new ST_OBJECT_INFO()
                {
                    StVesselInfo = stVesselInfo,
                    StBaseState = stBaseState,
                    StPos = stPos,
                    Variance = new float[Common.SIZE_VARIANCE_ROW, Common.SIZE_VARIANCE_COL],
                    CameraTag = 9999
                };
                for (int i = 0; i < Common.SIZE_VARIANCE_ROW; i++)
                    for (int j = 0; j < Common.SIZE_VARIANCE_COL; j++)
                        stObj.Variance[i, j] = (float)0;

                stObjectState[numTargetship++] = stObj;
            }

            ST_DATA_SA_OUT stDataSaOut = new ST_DATA_SA_OUT()
            {
                StEnvironment = stEnv,
                NumObjectFused = numTargetship,
                StOwnshipInfo = stOsInfo,
                StObjectInfo = stObjectState,
            };

            return stDataSaOut;
        }
        #endregion


        public void DeployData(ST_DATA_REMOTE_COMMAND stRemoteCmd)
        {
            MyShip myShip = Project.MyShips[0];

            if (stRemoteCmd.CourseCommand)
            {
                myShip.HeadingDesired = stRemoteCmd.HdgCmd;
                myShip.SpeedDesired = stRemoteCmd.SpdCmd*Distance.MS2KN;
            }
            else
            {
                myShip.RudderDesired = stRemoteCmd.RuddCmd;
                myShip.RpmDesired = stRemoteCmd.RpmCmd;
            }
        }
    }
}
