using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AisParser;
using AisParser.Messages;
using Microsoft.Maps.MapControl.WPF;

namespace NmeaParser
{
    public class Parser
    {
        #region Variables

        internal static System.Globalization.NumberFormatInfo numberFormat_EnUS =
            new System.Globalization.CultureInfo("en-US", false).NumberFormat;
        private static DateTime _timeOfFix;
        private static byte _noOfSats;
        private static double _dilution;
        private static double _altitude;
        private static char _altitudeUnits;
        private static double _heightOfGeoid;
        private static double _longitude;
        private static double _latitude;
        private static int _year;
        private static int _day;
        private static int _month;
        private static uint _mmsi;
        private static uint _imoNo;
        private static string _callSign;
        private static string _shipName;
        private static int _commState;
        private static int _rot;
        private static uint _timestamp;
        private static double _cog;
        private static uint _trueHeading;
        private static int _regApplication;
        private static double _speed;
        private static ShipType _aisShipType;
        private static AisMessageType _messageType;
        private static NavigationStatus _navigationalStatus;
        private static PositionAccuracy _positionAccuracy;
        private static NavigationalAidType _navigationAidType;
        #endregion

        public static void StringParser(string strLine, out SensorData parsedData)
        {
            strLine = strLine.Trim('\0');   //null 문자열 제거
            strLine = strLine.Replace("\r", string.Empty);
            strLine = strLine.Replace("\n", string.Empty);
            string[] stringSeparators = new string[] { ",", "*" };
            string[] splits = strLine.Split(stringSeparators, StringSplitOptions.None);

            parsedData = null;

            if (splits[0].Contains("GGA"))
            {
                try
                {
                    long time = 0;
                    if (splits[1].Length >= 6)
                    {
                        var YY = DateTime.UtcNow.Year;
                        var MM = DateTime.UtcNow.Month;
                        var DD = DateTime.UtcNow.Day;

                        DateTime dt = new DateTime(YY, MM, DD, intTryParse(splits[1].Substring(0, 2)), intTryParse(splits[1].Substring(2, 2)), intTryParse(splits[1].Substring(4, 2)));
                        time = DateTime.UtcNow.Ticks;
                        if (time - dt.Ticks > 1e8)    //timespan이 10s 이상이면 update하지 않음
                            return;
                    }

                    //N or E 기준으로 양수.음수
                    _longitude = DM2Decimal(splits[4], splits[5]); //Longitude, E or W
                    _latitude = DM2Decimal(splits[2], splits[3]); //Latitude, N or S

                    parsedData = new SensorData(new Location(_latitude, _longitude), SentenceID.GGA);
                }
                catch (Exception e)
                {
                    Debug.Print("Failed to parse GGA...");
                }
            }
            else if (splits[0].Contains("GLL"))
            {

            }
            else if (splits[0].Contains("VTG"))
            {
                try
                {
                    var cogT = double.Parse(splits[1]);
                    var cogM = double.Parse(splits[3]);
                    var sogKn = double.Parse(splits[5]);
                    var sog = double.Parse(splits[7]) * Defined.KMPH2MS;   //km/h->m/s

                    // parsedData = new SensorData()
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }
            else if (splits[0].Contains("RMC"))
            {

            }
            else if (splits[0].Contains("ZDA"))
            {

            }
            else if (splits[0].Contains("WPL"))
            {

            }
            else if (splits[0].Contains("RTE"))
            {

            }
            else if (splits[0].Contains("HDT"))
            {

            }
            else if (splits[0].Contains("THS"))
            {

            }
            else if (splits[0].Contains("DTM"))
            {

            }
            else if (splits[0].Contains("RPM"))
            {
                try
                {
                    // P3RPM,E,1,695.0,,A*11
                    // P3RPM,E,2,,,V * 21
                    if (!string.IsNullOrEmpty(splits[3]))
                    {
                        var side = int.Parse(splits[2]); // 1: stbd, 2: port
                        var rpm = double.Parse(splits[3]); //rpm
                        parsedData = new SensorData(side, rpm, SentenceID.RPM);
                    }
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Failed to parse RPM !!!");
                }
            }
            else if (splits[0].Contains("RSA"))
            {
                try
                {
                    double stbd = 0;
                    bool isStbdValid = false;
                    double port = 0;
                    bool isPortValid = false;
                    if (splits[2] == "A")
                    {
                        isStbdValid = true;
                        stbd = double.Parse(splits[1]);
                    }

                    if (splits[4] == "A")
                    {
                        isPortValid = true;
                        port = double.Parse(splits[3]);
                    }

                    parsedData = new SensorData(isStbdValid, stbd, isPortValid, port, SentenceID.RSA);
                }
                catch (Exception e)
                {
                    //Debug.Assert(false);
                    Debug.WriteLine("Failed to parse RSA !!!");
                }
            }
            else if (splits[0].Contains("MWV"))
            {

            }
            else if (splits[0].Contains("VDM") || splits[0].Contains("VDO"))
            {
                try
                {
                    var aisParser = new AisParser.Parser();
                    var result = aisParser.Parse(strLine);
                    if (result == null) return;

                    switch (result.MessageType)
                    {
                        case AisMessageType.PositionReportClassA:
                        case AisMessageType.PositionReportClassAAssignedSchedule:
                        case AisMessageType.PositionReportClassAResponseToInterrogation:
                            if (result is PositionReportClassAMessageBase msg1)     // message 1~3
                            {
                                _mmsi = msg1.Mmsi;
                                _messageType = msg1.MessageType;
                                _latitude = msg1.Latitude;          // 91: invalid
                                _longitude = msg1.Longitude;        // 181: invalid
                                _navigationalStatus = msg1.NavigationStatus;
                                _rot = msg1.RateOfTurn ?? -729;     //-729: invalid
                                _positionAccuracy = msg1.PositionAccuracy;
                                _speed = msg1.SpeedOverGround;
                                _cog = msg1.CourseOverGround;       // 3600: invalid
                                _trueHeading = msg1.TrueHeading ?? 511;    // 511: invalid (deg)
                                _timestamp = msg1.Timestamp;
                            }
                            else
                            {
                                Debug.WriteLine("Null for AIS Message 3");
                                return;
                            }
                            // _maneuverIndicator = positionReportClassA.ManeuverIndicator;
                            // _spare = positionReportClassA.Spare;
                            // raim = positionReportClassA.Raim;
                            // _radioStatus = positionReportClassA.RadioStatus;
                            break;

                        #region Message 4~
                        case AisMessageType.BaseStationReport:                      // message 4 
                            if (result is BaseStationReportMessage msg4)
                            {
                                _messageType = msg4.MessageType;
                                _mmsi = msg4.Mmsi;
                                _latitude = msg4.Latitude;
                                _longitude = msg4.Longitude;
                            }
                            else
                            {
                                Debug.WriteLine("Null for AIS Message 4");
                                return;
                            }
                            break;
                        case AisMessageType.StaticAndVoyageRelatedData:             // message 5
                            if (result is StaticAndVoyageRelatedDataMessage msg5)
                            {
                                _messageType = msg5.MessageType;
                                _mmsi = msg5.Mmsi;
                                _imoNo = msg5.ImoNumber;
                                _callSign = msg5.CallSign;
                                _shipName = msg5.ShipName;
                                _aisShipType = msg5.ShipType;
                                _ = msg5.DimensionToBow;
                                _ = msg5.DimensionToStern;
                                _ = msg5.DimensionToPort;
                                _ = msg5.DimensionToStarboard;
                            }
                            else
                            {
                                Debug.WriteLine("AIS Message 5 : NULL");
                            }
                            break;
                        // case AisMessageType.BinaryAddressedMessage: //6
                        //     BinaryAddressedMessage binAdd = result as BinaryAddressedMessage;
                        //     if (binAdd == null)
                        //         return;
                        //     return;
                        //     break;
                        // case AisMessageType.BinaryAcknowledge: //7
                        //     BinaryAcknowledgeMessage binAck = result as BinaryAcknowledgeMessage;
                        //     if (binAck == null)
                        //         return;
                        //     return;
                        // break;
                        // case AisMessageType.BinaryBroadcastMessage:             // message 8
                        //     if (result is BinaryBroadcastMessage msg8)
                        //     {
                        //
                        //     }
                        //     else
                        //     {
                        //         Debug.WriteLine("Null for AIS Message 8");
                        //         return;
                        //     }
                        //     break;
                        // case AisMessageType.StandardSarAircraftPositionReport:  // message 9
                        //     // StandardSarAircraftPositionReportMessage stdAircraftPosition = result as StandardSarAircraftPositionReportMessage;
                        //     // if (stdAircraftPosition == null)
                        //     //     return;
                        //     break;
                        // case AisMessageType.UtcAndDateInquiry:
                        //     // UtcAndDateInquiryMessage utcInquiry = result as UtcAndDateInquiryMessage;
                        //     // if (utcInquiry == null)
                        //     //     return;
                        //     break;
                        // case AisMessageType.UtcAndDateResponse:
                        //
                        //     break;
                        // case AisMessageType.AddressedSafetyRelatedMessage:
                        //     break;
                        // case AisMessageType.SafetyRelatedAcknowledgement:
                        //     break;
                        // //case AisMessageType.SafetyRelatedBroadcastMessage:
                        // case AisMessageType.Interrogation:
                        //     break;
                        // //case AisMessageType.AssignmentModeCommand:
                        // //case AisMessageType.DgnssBinaryBroadcastMessage:
                        case AisMessageType.StandardClassBCsPositionReport:     // message 18
                            if (result is StandardClassBCsPositionReportMessage msg18)
                            {
                                _messageType = msg18.MessageType;
                                _mmsi = msg18.Mmsi;
                                _speed = msg18.SpeedOverGround;
                                _latitude = msg18.Latitude;
                                _longitude = msg18.Longitude;
                                _cog = msg18.CourseOverGround;  //0.1 degrees from true north
                                _trueHeading = msg18.TrueHeading ?? 511;
                            }
                            else
                            {
                                Debug.WriteLine("Null for AIS Message 18");
                                return;
                            }
                            break;
                        // case AisMessageType.ExtendedClassBCsPositionReport:
                        //     break;
                        // case AisMessageType.DataLinkManagement:
                        //     break;
                        case AisMessageType.AidToNavigationReport:              // message 21 : buoys and light houses
                            if (result is AidToNavigationReportMessage msg21)
                            {
                                _messageType = msg21.MessageType;
                                _mmsi = msg21.Mmsi;
                                _navigationAidType = msg21.NavigationalAidType;
                                _shipName = msg21.Name;
                                _latitude = msg21.Latitude;
                                _longitude = msg21.Longitude;
                            }
                            else
                            {
                                Debug.WriteLine("Null for AIS Message 21");
                                return;
                            }
                            break;
                        // //case AisMessageType.ChannelManagement:
                        // //case AisMessageType.GroupAssignmentCommand:
                        // case AisMessageType.StaticDataReport:                   // message 24 : type 5 for classB ships
                        //     StaticDataReportMessage staticDataReport = result as StaticDataReportMessage;
                        //     if (result is StaticDataReportMessage msg24)
                        //     {
                        //         _messageType=msg24.MessageType;
                        //         _mmsi= msg24.Mmsi;
                        //     }
                        //     else
                        //     {
                        //         Debug.WriteLine("Null for AIS Message 24");
                        //         return;
                        //     }
                        //     break;
                        // //case AisMessageType.SingleSlotBinaryMessage:
                        // //case AisMessageType.MultipleSlotBinaryMessageWithCommunicationsState:
                        // case AisMessageType.PositionReportForLongRangeApplications:
                        //     break;
                        default:
                            Debug.Write($"Unrecognized AIS Message ??? - {result.MessageType.ToString():0}");
                            return;
                            #endregion
                    }

                    var sentenceId = splits[0].Contains("VDM") ? SentenceID.VDM : SentenceID.VDO;
                    parsedData = new SensorData(_mmsi, new Location(_latitude, _longitude), _speed, _cog, sentenceId);
                }
                catch (Exception e)
                {
                    if (splits[0].Contains("VDM"))
                        Debug.Write("!!! Failed to parse VDM ...\n");
                    else
                        Debug.Write("!!! Failed to parse VDO ...\n");
                }
            }
        }


        internal static int intTryParse(string str)
        {
            try { return int.Parse(str, numberFormat_EnUS); }
            catch { return 0; }
        }

        internal static double DM2Decimal(string DM, string Dir)
        {
            try
            {
                if (DM == "" || Dir == "")
                {
                    return 0.0;
                }

                string t = DM.Substring(DM.IndexOf("."));
                double FM = double.Parse(DM.Substring(DM.IndexOf(".")), numberFormat_EnUS);

                //Get the minutes.
                t = DM.Substring(DM.IndexOf(".") - 2, 2);
                double Min = double.Parse(DM.Substring(DM.IndexOf(".") - 2, 2), numberFormat_EnUS);

                //Degrees
                t = DM.Substring(0, DM.IndexOf(".") - 2);
                double Deg = double.Parse(DM.Substring(0, DM.IndexOf(".") - 2), numberFormat_EnUS);

                if (Dir == "S" || Dir == "W")
                    Deg = -(Deg + (Min + FM) / 60);
                else
                    Deg = Deg + (Min + FM) / 60;
                return Deg;
            }
            catch
            {
                return 0.0;
            }
        }

    }
}
