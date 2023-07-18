using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SyDLab.Usv.Simulator.Domain.Comm
{
    public class Common
    {
        // Array Size
        public static readonly int MAX_NUM_TARGET_SHIP = 200;
        public static readonly int MAX_VESSEL_NAME_LENGTH = 30;
        public static readonly int MAX_CALL_SIGN_LENGTH = 16;
        public static readonly int MAX_NUM_WAYPOINT = 100;
        public static readonly int MAX_NUM_PROPELLER = 2;
        public static readonly int SIZE_VARIANCE_ROW = 4;
        public static readonly int SIZE_VARIANCE_COL = 4;

        public static readonly int SIZE_BUFF = 65536;
        public static readonly int BACKLOG = 5;

        public static readonly int MAX_NMEA_LENGTH = 60;

        //Size of primitive
        public static readonly int SIZE_DOUBLE = 8;
        public static readonly int SIZE_INT = 4;
        public static readonly int SIZE_SHORT = 2;
        public static readonly int SIZE_BOOL = 4; // 1;
        public static readonly int SIZE_BOOL1BYTE = 1;
        public static readonly int SIZE_FLOAT = 4;
        public static readonly int SIZE_UINT = 4;
        public static readonly int SIZE_CHAR = 1;


        public static readonly int SIZE_ST_POS = SIZE_FLOAT * 3;
        public static readonly int SIZE_ST_DGPS_POS = SIZE_FLOAT * 6;
        public static readonly int SIZE_ST_VESSEL_VELOCITY = SIZE_FLOAT * 3;
        public static readonly int SIZE_ST_BASE_STATE = SIZE_FLOAT * 4;
        public static readonly int SIZE_ST_VOYAGE_PLAN = SIZE_INT * 2
                                                         + SIZE_ST_POS * MAX_NUM_WAYPOINT
                                                         + SIZE_FLOAT * MAX_NUM_WAYPOINT * 3;
        public static readonly int SIZE_ST_ENVIRONMENT = SIZE_FLOAT * 10 + SIZE_INT * 2;
        public static readonly int SIZE_ST_VESSEL_INFO = SIZE_UINT * 3 +
                                                         SIZE_CHAR * (MAX_CALL_SIGN_LENGTH + MAX_VESSEL_NAME_LENGTH) +
                                                         SIZE_UINT + SIZE_FLOAT * 2;
        public static readonly int SIZE_ST_VOYAGE_INFO = SIZE_ST_DGPS_POS * 2 + SIZE_BOOL + SIZE_ST_VOYAGE_PLAN * 2 + SIZE_UINT * 2 + SIZE_INT;
        public static readonly int SIZE_ST_VESSEL_MOTION = SIZE_FLOAT * 2;

        public static readonly int SIZE_ST_OWNSHIP_INFO = SIZE_UINT + SIZE_ST_VESSEL_INFO + SIZE_INT + SIZE_FLOAT * 2 +
                                                          SIZE_ST_BASE_STATE + SIZE_FLOAT +
                                                          SIZE_ST_POS + SIZE_FLOAT * 2 + SIZE_FLOAT * 4 +
                                                          SIZE_ST_VESSEL_MOTION + SIZE_ST_VOYAGE_INFO;

        public static readonly int SIZE_ST_OBJECT_INFO = SIZE_ST_VESSEL_INFO + SIZE_ST_BASE_STATE + SIZE_ST_POS + SIZE_UINT +
                                                         SIZE_FLOAT * Common.SIZE_VARIANCE_ROW * Common.SIZE_VARIANCE_COL;

        public static readonly int SIZE_ST_DATA_SA_OUT = SIZE_ST_ENVIRONMENT + SIZE_INT + SIZE_ST_OWNSHIP_INFO +
                                                         SIZE_ST_OBJECT_INFO * MAX_NUM_TARGET_SHIP;

        public static readonly int SIZE_ST_CR_STATE = SIZE_FLOAT * 4 + SIZE_INT + SIZE_FLOAT * 3;
        public static readonly int SIZE_ST_DATA_CR_OUT = SIZE_ST_DATA_SA_OUT + SIZE_ST_CR_STATE * MAX_NUM_TARGET_SHIP;
        public static readonly int SIZE_ST_CA_COMMAND = SIZE_FLOAT * 2;
        public static readonly int SIZE_ST_DATA_CA_OUT = SIZE_ST_DATA_CR_OUT + SIZE_ST_CA_COMMAND;
        public static readonly int SIZE_DATA_OP_TO_NM = SIZE_INT;
        public static readonly int SIZE_DATA_AUTO_PILOT = SIZE_FLOAT * 2;
        public static readonly int SIZE_DATA_NM_OUT = SIZE_ST_DATA_CA_OUT + SIZE_DATA_OP_TO_NM + SIZE_DATA_AUTO_PILOT;
        public static readonly int SIZE_DATA_REMOTE_COMMAND = SIZE_BOOL + SIZE_FLOAT * 4;


        // Comm. Interval
        public static readonly int UDP_TIMER_INTERVAL_MS = 1000; // 1SEC
        public static readonly int TCP_TIMER_INTERVAL_MS = 1000; // 1SEC
        public static readonly int TIMEOUT_CONNECT_TCP_MSEC = 3000; // 3SEC
        public static readonly int CR_COMM_IN_TIMER_INTERVAL_MS = 1000; // 1SEC
        public static readonly int CR_COMM_OUT_TIMER_INTERVAL_MS = 1000; // 1SEC

        // Sim
        public static readonly int SIM_TIMER_INTERVAL_MS = 1000; // 1 SEC
        public static readonly int SIM_COMM_OUT_TIMER_INTERVAL_MS = 2000; // 2 SEC

        // Comm. Port
        public static readonly int PORT_UDP_SA_TO_CR = 9001;
        public static readonly int PORT_UDP_CR_TO_CA = 9002;
        public static readonly int PORT_UDP_CA_TO_VSV = 9003;
        public static readonly int PORT_UDP_CA_TO_SIM = 9007;
        public static readonly int PORT_UDP_SIM_TO_CA = PORT_UDP_CR_TO_CA;
        public static readonly int PORT_UDP_SIM_TO_VSV = PORT_UDP_CA_TO_VSV;
    }

    public enum ShipType { LNGC, BULK, CRUS, CNTR, TANK, FISH, TUG }
    public enum WeatherHarshness { Calm, Medium, Severe }
    public enum WeatherCase { Clear, Rain, Snow, Typhoon }
}
