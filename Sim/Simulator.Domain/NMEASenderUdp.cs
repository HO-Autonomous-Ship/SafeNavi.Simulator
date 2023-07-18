using MathNet.Spatial.Euclidean;
using Microsoft.Maps.MapControl.WPF;
using SyDLab.Usv.Simulator.Domain.Models;
using SyDLab.Usv.Simulator.Domain.Models.Platforms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using SyDLab.Usv.Simulator.Domain.Comm;
using SyDLab.Usv.Simulator.Domain.Utils;

namespace SyDLab.Usv.Simulator.Domain
{
    public class NMEASenderUdp
    {
        public NMEASenderUdp()
        {

        }

        // private UdpSocketClient _sendOutClient = new UdpSocketClient();

        public Project Project => Singleton<Project>.UniqueInstance;

        private static bool _isWpSent = false;
        private List<string> _waypointNames = new List<string>();

        public void SendData(PlatformState state, double totalSecond, UdpSocketClient udpSocketClient)
        {
            try
            {
                // SendZDA(totalSecond, udpSocketClient);
                //
                // // send waypoints and route info.
                // if (!_isWpSent)
                //     _isWpSent = SendWPL(udpSocketClient);
                // if (_isWpSent && ((int)totalSecond) % 10 == 1)
                //     SendRTE(udpSocketClient);


                if (state.Platform is MyShip)
                {
                    // SendGGA(state, totalSecond, udpSocketClient);
                    // SendHDT(state, udpSocketClient);
                    
                    // SendVDO(state, udpSocketClient);
                    // SendRPM(state, udpSocketClient);
                    // SendRSA(state, udpSocketClient);
                }
                else
                {
                    int idxOfTS = Project.TargetShips.IndexOf(state.Platform as TargetShip);
                    if (Project.TargetShips[idxOfTS].HistoryAISData.Any(n => n == state))
                    {
                        SendVDM01(state, udpSocketClient);
                    //     SendVDM05(state, udpSocketClient);
                    }
                    // else if (Project.TargetShips[idxOfTS].HistoryRadarData.Any(n => n == state))
                    // {
                    //     SendTTM(state, totalSecond, udpSocketClient);
                    // }
                }
            }
            catch   
            {
                throw new Exception("Failed to send NMEA message");
            }
        }

        public void SendTime(double totalSecond, UdpSocketClient udpSocketClient)
        {
            // SendZDA(totalSecond, udpSocketClient);
        }


        // 시뮬레이터의 시간을 ZDA sentence로 전송
        private void SendZDA(double totalSecond, UdpSocketClient udpSocketClient)
        {
            int hour = (int)(totalSecond / 3600) % 24;
            int minute = (int)(totalSecond / 60) % 60;
            int second = (int)(totalSecond % 60);
            string NMEAString = "$GPZDA," + hour.ToString().PadLeft(2, '0') + minute.ToString().PadLeft(2, '0') +
                                 second.ToString().PadLeft(2, '0') + ".00,1,1,1,00,00*";
            NMEAString = NMEAString + Checksum(NMEAString);
            // 시간 메세지 전송
            byte[] timeSender = Encoding.ASCII.GetBytes(NMEAString);
            
            try
            {
                udpSocketClient?.SendUdpData(timeSender);
            }
            catch (Exception e)
            {
                udpSocketClient?.Stop("Failed to send VDO message!!!");
                throw;
            }
        }

        private void SendVDO(PlatformState state, UdpSocketClient udpSocketClient)
        {
            int msg = 1;
            int repeat = 0;
            uint mmsi = 440091740;  // (state.Platform as MyShip)?.MMSI ?? 0;
            int navStatus = 0; // enum 으로 바꿔야함
            int rot = 0; // 4.733*sqrt((deg/min))
            double speed = state.Speed * 10; // 1/10 knot
            int posAcc = 0; // false(0) : DGPS, true(1) : GNSS

            //// double lon = 100 * state.Position.X; // 600000 * longitude 로 변환해야함, 181 degree 의 경우는 default
            //// double lat = 100 * state.Position.Y; // 600000 * latitude 로 변환해야함, 91 degree 의 경우는 default
            var location = Distance.GetLatLng(new Point3D(state.Position.X, state.Position.Y, 0));
            double lon = 600000 * location.Longitude;
            double lat = 600000 * location.Latitude;

            // Location location = Distance.GetLatLng(state.Position);
            // double lat = Math.Truncate(location.Latitude) * 100 + (location.Latitude - Math.Truncate(location.Latitude)) * 60;
            // double lon = Math.Truncate(location.Longitude) * 100 + (location.Longitude - Math.Truncate(location.Longitude)) * 60;

            double cog = state.Heading * 10; // 북쪽이 0도, 5110 : not available
            if (cog < 0)
            {
                cog += 3600;
            }
            int trueHeading = (int)state.Heading;
            int utcSec = (int)state.Time % 60;
            // int regApplication = 0; // 0 : default,
            int maneuverInd = 0; // 0 : Not available, 1 : No special maneuver, 2 : Special maneuver
            int spare = 0;
            int raimFlag = 0; // 0 : default, 1 : RAIM in use
            int commState = 0; //communications state 뭔지몰르겠음

            // 위 내용들을 전부 bit string 으로 변환
            // 함수 ToBinaryString(int, len)
            string binaryPayload = ToBinaryString(msg, 6) + ToBinaryString(repeat, 2) + ToBinaryString(mmsi, 30) +
                                   ToBinaryString(navStatus, 4) + ToBinaryString(rot, 8) +
                                   ToBinaryString((int)speed, 10) + ToBinaryString(posAcc, 1) +
                                   ToBinaryString((int)lon, 28) + ToBinaryString((int)lat, 27) +
                                   ToBinaryString((int)cog, 12) + ToBinaryString(trueHeading, 9) +
                                   ToBinaryString(utcSec, 6) + ToBinaryString(maneuverInd, 2) +
                                   ToBinaryString(spare, 3) + ToBinaryString(raimFlag, 1) +
                                   ToBinaryString(commState, 19);
            // Common Navigation Block
            //string binaryPayload = ToBinaryString(msg, 6) + ToBinaryString(repeat, 2) + ToBinaryString(mmsi, 30) +
            //                       ToBinaryString(navStatus, 4) + ToBinaryString(rot, 8) +
            //                       ToBinaryString((int)speed, 10) + ToBinaryString(posAcc, 1) +
            //                       ToBinaryString((int)lon, 28) + ToBinaryString((int)lat, 27) +
            //                       ToBinaryString((int)cog, 12) + ToBinaryString(trueHeading, 9) +
            //                       ToBinaryString(utcSec, 6) + ToBinaryString(maneuver, 2) +
            //                       ToBinaryString(spare, 3) + ToBinaryString(raimFlag, 1) +
            //                       ToBinaryString(commState, 19);

            // bit string을 6bit ascii로 armoring
            // 그 후 nmea string에 부착
            string payloadData = PayloadArmoring(binaryPayload);

            string NMEAString = "!AIVDO,1,1,0,A,"; // VDO : 자선 ais 데이터
            NMEAString = NMEAString + payloadData + ",0*";
            NMEAString = NMEAString + Checksum(NMEAString);

            byte[] stateSender = Encoding.ASCII.GetBytes(NMEAString);

            try
            {
                udpSocketClient?.SendUdpData(stateSender);
            }
            catch (Exception e)
            {
                udpSocketClient?.Stop("Failed to send VDO message!!!");
                throw;
            }
        }

        
        private void SendVDM01(PlatformState state, UdpSocketClient udpSocketClient)
        {
            int msg = 1;
            int repeat = 0;
            int mmsi = (state.Platform as TargetShip)?.MMSI ?? 0;
            int navStatus = 0; // enum 으로 바꿔야함
            int rot = 0; // 4.733*sqrt((deg/min))
            double speed = state.Speed * 10; // 1/10 knot
            int posAcc = 0; // false(0) : DGPS, true(1) : GNSS
            // double lon = 100 * state.Position.X; // 600000 * longitude 로 변환해야함, 181 degree 의 경우는 default
            // double lat = 100 * state.Position.Y; // 600000 * latitude 로 변환해야함, 91 degree 의 경우는 default
            var location = Distance.GetLatLng(new Point3D(state.Position.X, state.Position.Y, 0));
            double lon = 600000 * location.Longitude;
            double lat = 600000 * location.Latitude;
            double cog = state.Heading * 10; // 북쪽이 0도, 3600 : not available
            if (cog < 0)
            {
                cog += 3600;
            }
            int trueHeading = (int)state.Heading;
            int utcSec = (int)state.Time % 60;
            int regApplication = 0; // 0 : default,
            int spare = 0;
            int raimFlag = 0; // 0 : default, 1 : RAIM in use
            int commState = 0; //communications state 뭔지몰르겠음
            
            // 위 내용들을 전부 bit string 으로 변환
            // 함수 ToBinaryString(int, len)
            string binaryPayload = ToBinaryString(msg, 6) + ToBinaryString(repeat, 2) + ToBinaryString(mmsi, 30) +
                                   ToBinaryString(navStatus, 4) + ToBinaryString(rot, 8) +
                                   ToBinaryString((int)speed, 10) + ToBinaryString(posAcc, 1) +
                                   ToBinaryString((int)lon, 28) + ToBinaryString((int)lat, 27) +
                                   ToBinaryString((int)cog, 12) + ToBinaryString(trueHeading, 9) +
                                   ToBinaryString(utcSec, 6) + ToBinaryString(regApplication, 2) +
                                   ToBinaryString(spare, 3) + ToBinaryString(raimFlag, 1) +
                                   ToBinaryString(commState, 19);
            
            // bit string을 6bit ascii로 armoring
            // 그 후 nmea string에 부착
            string payloadData = PayloadArmoring(binaryPayload);
            
            string NMEAString = "!AIVDM,1,1,0,A,"; // VDM : 타선 ais 데이터
            NMEAString = NMEAString + payloadData + ",0*";
            NMEAString = NMEAString + Checksum(NMEAString);
            
            byte[] stateSender = Encoding.ASCII.GetBytes(NMEAString);
            
            try
            {
                udpSocketClient?.SendUdpData(stateSender);
            }
            catch (Exception e)
            {
                udpSocketClient?.Stop("Failed to send VDM01 message!!!");
                throw;
            }
        }


        private void SendVDM05(PlatformState state, UdpSocketClient udpSocketClient)
        {
            int msg = 1;
            int repeat = 0;
            int mmsi = (state.Platform as TargetShip)?.MMSI ?? 0;
            int aisVersion = 0; // enum 으로 바꿔야함
            uint imo = 0;
            char[] callSign = new char[16];
            char[] vesselName = new char[30];
            int shipType = 0; //enum
            int toBow = 0;
            int toStern = 0;
            int toPort = 0;
            int toStbd = 0;
            int posFixType = 0; //enum (EPFD Fix Type)
            uint month = 0;
            uint day = 0;
            uint hour = 0;
            uint minute = 0;
            uint draught = 0;   //scale 1/10
            char[] destination = new char[16];  //20 6-bit characters
            bool dte = false;   //0: data terminal ready, 1: not ready (defualt)


            shipType = state.Platform.ShipType;

            //
            //
            // double cog = state.Heading * 10; // 북쪽이 0도, 3600 : not available
            // if (cog < 0)
            // {
            //     cog += 3600;
            // }
            // int trueHeading = (int)state.Heading;
            // int utcSec = (int)state.Time % 60;
            // int regApplication = 0; // 0 : default,
            // int spare = 0;
            // int raimFlag = 0; // 0 : default, 1 : RAIM in use
            // int commState = 0; //communications state 뭔지몰르겠음
            //
            // // 위 내용들을 전부 bit string 으로 변환
            // // 함수 ToBinaryString(int, len)
            // string binaryPayload = ToBinaryString(msg, 6) + ToBinaryString(repeat, 2) + ToBinaryString(mmsi, 30) +
            //                        ToBinaryString(navStatus, 4) + ToBinaryString(rot, 8) +
            //                        ToBinaryString((int)speed, 10) + ToBinaryString(posAcc, 1) +
            //                        ToBinaryString((int)lon, 28) + ToBinaryString((int)lat, 27) +
            //                        ToBinaryString((int)cog, 12) + ToBinaryString(trueHeading, 9) +
            //                        ToBinaryString(utcSec, 6) + ToBinaryString(regApplication, 2) +
            //                        ToBinaryString(spare, 3) + ToBinaryString(raimFlag, 1) +
            //                        ToBinaryString(commState, 19);
            //
            // // bit string을 6bit ascii로 armoring
            // // 그 후 nmea string에 부착
            // string payloadData = PayloadArmoring(binaryPayload);
            //
            // string NMEAString = "!AIVDM,1,1,0,A,"; // VDM : 타선 ais 데이터
            // NMEAString = NMEAString + payloadData + ",0*";
            // NMEAString = NMEAString + Checksum(NMEAString);
            //
            // byte[] stateSender = Encoding.ASCII.GetBytes(NMEAString);
            //
            // if (ns != null && ns.CanWrite && client.Connected)
            // {
            //     try
            //     {
            //         ns.Write(stateSender, 0, stateSender.Length);
            //     }
            //     catch (Exception e)
            //     {
            //     }
            // }
        }


        // GPS 데이터를 GGA sentence로 전송
        private void SendGGA(PlatformState state, double totalSecond, UdpSocketClient udpSocketClient)
        {
            string NMEAString = "$GPGGA,";
            int hour = (int)(totalSecond / 3600) % 24;
            int minute = (int)(totalSecond / 60) % 60;
            int second = (int)(totalSecond % 60);
            string time = hour.ToString().PadLeft(2, '0') + minute.ToString().PadLeft(2, '0') +
                                 second.ToString().PadLeft(2, '0') + ".00,";
            Location location = Distance.GetLatLng(state.Position);
            double lat = Math.Truncate(location.Latitude) * 100 + (location.Latitude - Math.Truncate(location.Latitude)) * 60;
            double lon = Math.Truncate(location.Longitude) * 100 + (location.Longitude - Math.Truncate(location.Longitude)) * 60;
            NMEAString += time + lat.ToString("F6") + ",N," + lon.ToString("F6") + ",E,1,00,01,0,M,0,M,,*";
            NMEAString += Checksum(NMEAString);
        
            byte[] stateSender = Encoding.ASCII.GetBytes(NMEAString);
            try
            {
                udpSocketClient?.SendUdpData(stateSender);
            }
            catch (Exception e)
            {
                udpSocketClient?.Stop("Failed to send GGA message!!!");
                throw;
            }
        }

        // GPS의 방향 데이터를 HDT sentence로 전송
        private void SendHDT(PlatformState state, UdpSocketClient udpSocketClient)
        {
            string NMEAString = "$GPHDT,";
            double heading = state.Heading;
            NMEAString += heading + ",T*";
            NMEAString += Checksum(NMEAString);
        
            byte[] stateSender = Encoding.ASCII.GetBytes(NMEAString);
            try
            {
                udpSocketClient?.SendUdpData(stateSender);
            }
            catch (Exception e)
            {
                udpSocketClient?.Stop("Failed to send HDT message!!!");
                throw;
            }
        }


        private void SendRPM(PlatformState state, UdpSocketClient udpSocketClient)
        {
            double rpm = state.Rpm;
            string NMEAString = $"$P3RPM,E,1,{rpm:0.0},,A*";
            NMEAString += Checksum(NMEAString);

            byte[] stateSender = Encoding.ASCII.GetBytes(NMEAString);
            try
            {
                udpSocketClient?.SendUdpData(stateSender);
            }
            catch (Exception e)
            {
                udpSocketClient?.Stop("Failed to send RPM message!!!");
                throw;
            }
        }

        private void SendRSA(PlatformState state, UdpSocketClient udpSocketClient)
        {
            double rudder = state.RudderAngle;
            string NMEAString = $"$AGRSA,,V,{rudder:0.0},A*";
            NMEAString += Checksum(NMEAString);

            byte[] stateSender = Encoding.ASCII.GetBytes(NMEAString);
            try
            {
                udpSocketClient?.SendUdpData(stateSender);
            }
            catch (Exception e)
            {
                udpSocketClient?.Stop("Failed to send RPM message!!!");
                throw;
            }
        }

        // Radar 데이터를 TTM sentence로 전송
        private void SendTTM(PlatformState state, double totalSecond, UdpSocketClient udpSocketClient)
        {
            string NMEAString = "$RATTM,";
            // TODO: myship 하나 선택하는 방법 찾아야
            int shipID = Project.TargetShips.IndexOf(state.Platform as TargetShip);
            // 자선 위치 기준
            // double x = Math.Round(-Project.MyShips[0].CurrentX + state.Position.X, 2);
            // double y = Math.Round(-Project.MyShips[0].CurrentY + state.Position.Y, 2);
            // 원점 기준
            double x = Math.Round(state.Position.X, 2);
            double y = Math.Round(state.Position.Y, 2);
        
            double dist = Math.Round(Math.Sqrt(x * x + y * y), 2);
            double bearing = Math.Round(Math.Atan2(y, x), 2);
            double speed = Math.Round(state.Speed, 2);
            double heading = Math.Round(state.Heading, 2);
            int hour = (int)(totalSecond / 3600) % 24;
            int minute = (int)(totalSecond / 60) % 60;
            int second = (int)(totalSecond % 60);
            string time = hour.ToString().PadLeft(2, '0') + minute.ToString().PadLeft(2, '0') +
                          second.ToString().PadLeft(2, '0') + ".00,";
            NMEAString += shipID.ToString().PadLeft(2, '0') + "," + dist + "," + bearing + ",T," + speed + "," + heading +
                          ",T,0,0,K,0,T,," + time + ",A*";
            NMEAString += Checksum(NMEAString);
        
            byte[] stateSender = Encoding.ASCII.GetBytes(NMEAString);
            try
            {
                udpSocketClient?.SendUdpData(stateSender);
            }
            catch (Exception e)
            {
                udpSocketClient?.Stop("Failed to send HDT message!!!");
                throw;
            }
        }


        #region Send NMEA : waypoints and route
        private void SendRTE(UdpSocketClient udpSocketClient)
        {
            List<string> payloads = new List<string>();

            string talker = "GP";       //추후 enum으로 변경
            string formatter = "RTE";   //추후 enum으로 변경
            string type = "c";
            string routeName = "0";

            int charCount = 0;
            int totNum = 1;
            int seqId = 1;
            StringBuilder builder = new StringBuilder(256);

            foreach (var waypointName in _waypointNames)
            {
                charCount += waypointName.Length + 1;
                if (charCount > Common.MAX_NMEA_LENGTH)
                {
                    payloads.Add(builder.ToString());

                    totNum++;
                    seqId++;
                    charCount = 0;
                    builder.Clear();
                    builder.Append(",");
                    builder.Append(waypointName);
                }
                else
                {
                    try
                    {
                        builder.Append(",");
                        builder.Append(waypointName);
                    }
                    catch (Exception e)
                    {
                        throw;
                    }
                }
            }
            payloads.Add(builder.ToString());

            foreach (var item in payloads.Select((value, index) => (value, index)))
            {
                string nmeaRte = BuildNmeaRTE("$", talker, formatter, totNum, item.index + 1, type, routeName, item.value);
                byte[] stateSender = Encoding.ASCII.GetBytes(nmeaRte);

                //send sentence
                try
                {
                    udpSocketClient?.SendUdpData(stateSender);
                }
                catch (Exception e)
                {
                    udpSocketClient?.Stop("Failed to send VDO message!!!");
                    throw;
                }
            }
        }

        private string BuildNmeaRTE(string str0, string talker, string formatter, int sentenceNo, int seqNo, string type, string routeName,
            string payload)
        {
            StringBuilder builder = new StringBuilder(256);
            // $GPRTE,2,1,c,0,W3IWI,DRIVWY,32CEDR,32-29,32BKLD,32-I95,32-US1,BW-32,BW-198*69

            builder.Append(str0[0]);
            builder.Append(talker);
            builder.Append(formatter);
            builder.Append(",");
            builder.Append(sentenceNo);
            builder.Append(",");
            builder.Append(seqNo);
            builder.Append(",");
            builder.Append(type);
            builder.Append(",");
            builder.Append(routeName);
            builder.Append(payload);
            builder.Append("*");
            builder.Append(Checksum(builder.ToString()));
            builder.Append("\r\n");

            string nmea = builder.ToString();

            return nmea;
        }

        private bool SendWPL(UdpSocketClient udpSocketClient)
        {
            List<string> wplSentence = new List<string>();

            string wplHeader = "$GPWPL,";  //WPL
            string payload;
            foreach (var item in Project.Scenarios[0].Tasks[0].Waypoints.ToList().Select((value, index) => (value, index)))
            {
                payload = null;
                var value = item.value;
                var index = item.index;

                Location location = Distance.GetLatLng(new Point3D(value.Y, value.X, 0));

                double lat = location.Latitude * 100;    //lat.
                string lat_dir = Math.Sign(lat) >= 0 ? "N" : "S";
                string latStr = string.Format("{0:0.0000,0}", lat);
                double lon = location.Longitude * 100;  //lon.
                string lon_dir = Math.Sign(lon) >= 0 ? "E" : "W";
                string lonStr = string.Format("{0:0.0000,0}", lon);

                //generation random name
                RandomGeneration random = new RandomGeneration();
                //int RanNum = random.GenRandomNumber(4, 6);
                //string id = random.GenRandomName(RanNum);
                string id = index.ToString();

                _waypointNames.Add(id);
                payload = latStr + "," + lat_dir + "," + lonStr + "," + lon_dir + "," + id;

                string sentence = wplHeader + payload + ",0*";
                sentence = sentence + Checksum(sentence);
                wplSentence.Add(sentence);
            }

            foreach (var item in wplSentence)
            {
                byte[] stateSender = Encoding.ASCII.GetBytes(item);

                //send sentence
                try
                {
                    udpSocketClient?.SendUdpData(stateSender);
                }
                catch (Exception e)
                {
                    udpSocketClient?.Stop("Failed to send VDO message!!!");
                    throw;
                }
            }
            return true;
        }
        #endregion


        #region Sub-function
        private string Checksum(string sentence)
        {
            byte sum = Convert.ToByte(sentence[1]);

            for (int i = 2; i < sentence.IndexOf("*"); ++i)
            {
                sum ^= Convert.ToByte(sentence[i]);
            }

            return Convert.ToString(sum, 16).PadLeft(2, '0');
        }

        private string PayloadArmoring(string binaryPayload)
        {
            string payloadData = "";
            for (int i = 0; i < binaryPayload.Length / 6; i++)
            {
                int tempInt = Convert.ToInt32(binaryPayload.Substring(6 * i, 6), 2);
                if (tempInt < 40)
                    tempInt += 48; // 40 chars 0-W
                else
                    tempInt += 56; // 24 chars `,a-w
                payloadData += Convert.ToChar(tempInt);
            }

            return payloadData;
        }

        private string ToBinaryString(int data, int len)
        {
            // if (Math.Abs(data) > (int) Math.Pow(2, len))
            // 지정된 자리수보다 큰 값이 들어온 경우 처리 방안 필요

            if (data >= 0)
            {
                string binaryData = Convert.ToString(data, 2).PadLeft(len, '0');
                return binaryData;
            }
            else
            {
                string binaryData = Convert.ToString((int)Math.Pow(2, len) + data, 2).PadLeft(len, '0');
                return binaryData;
            }
        }
        private string ToBinaryString(uint data, int len)
        {
            // if (Math.Abs(data) > (int) Math.Pow(2, len))
            // 지정된 자리수보다 큰 값이 들어온 경우 처리 방안 필요

            if (data >= 0)
            {
                string binaryData = Convert.ToString(data, 2).PadLeft(len, '0');
                return binaryData;
            }
            else
            {
                string binaryData = Convert.ToString((int)Math.Pow(2, len) + data, 2).PadLeft(len, '0');
                return binaryData;
            }
        }

        #endregion


    }
}

   
