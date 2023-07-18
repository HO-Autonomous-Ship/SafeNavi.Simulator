using MathNet.Numerics.LinearAlgebra.Double;
using MathNet.Spatial.Euclidean;
using Microsoft.Maps.MapControl.WPF;
using SyDLab.Usv.Simulator.Domain.Models;
using SyDLab.Usv.Simulator.Domain.Models.Platforms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using SyDLab.Usv.Simulator.Domain.Comm;
using SyDLab.Usv.Simulator.Domain.Utils;

namespace SyDLab.Usv.Simulator.Domain
{
    public class NMEASender
    {
        private List<string> _waypointNames = new List<string>();

        public NMEASender()
        {
        }

        public Project Project => Singleton<Project>.UniqueInstance;

        private static bool IsWpSent = false;
        public void SendData(PlatformState state, double totalSecond, NetworkStream ns, TcpClient client)
        {
            SendZDA(totalSecond, ns, client);

            // sent once
            if (!IsWpSent)
                IsWpSent = SendWPL(ns, client);

            if (IsWpSent && ((int)totalSecond)% 10 == 1)
                SendRTE(ns, client);
 
            
            try
            {
                if (state.Platform is MyShip)
                {
                    SendGGA(state, totalSecond, ns, client);
                    SendHDT(state, ns, client);
                }
                else
                {
                    int indexOfTargetShip = Project.TargetShips.IndexOf(state.Platform as TargetShip);
                    if (Project.TargetShips[indexOfTargetShip].HistoryAISData.Any(n => n == state))
                    {
                        SendVDM(state, ns, client);
                        // SendVDM05(state, ns, client);
                    }
                    else if (Project.TargetShips[indexOfTargetShip].HistoryRadarData.Any(n => n == state))
                    {
                        SendTTM(state, totalSecond, ns, client);
                    }
                }
            }
            catch
            {
                throw new Exception("Failed to send NMEA message");
            }
        }

        public void SendTime(double totalSecond, NetworkStream ns, TcpClient client)
        {
            SendZDA(totalSecond, ns, client);
        }

        // 시뮬레이터의 시간을 ZDA sentence로 전송
        private void SendZDA(double totalSecond, NetworkStream ns, TcpClient client)
        {
            int hour = (int)(totalSecond / 3600) % 24;
            int minute = (int)(totalSecond / 60) % 60;
            int second = (int)(totalSecond % 60);
            string NMEAString = "$GPZDA," + hour.ToString().PadLeft(2, '0') + minute.ToString().PadLeft(2, '0') +
                                 second.ToString().PadLeft(2, '0') + ".00,1,1,1,00,00*";
            NMEAString = NMEAString + Checksum(NMEAString);
            // 시간 메세지 전송
            byte[] timeSender = Encoding.ASCII.GetBytes(NMEAString);
            if (ns != null && ns.CanWrite && client.Connected)
            {
                try
                {
                    ns.Write(timeSender, 0, timeSender.Length);
                }
                catch (Exception e)
                {
                }
            }
        }


        private void SendRTE(NetworkStream ns, TcpClient client)
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

            foreach (var item in payloads.Select((value, index)=>(value, index)))      
            {
                string nmeaRte = BuildNmeaRTE("$", talker, formatter, totNum, item.index+ 1, type, routeName, item.value);

                //send sentence
                if (ns != null && ns.CanWrite && client.Connected)
                {
                    byte[] stateSender = Encoding.ASCII.GetBytes(nmeaRte);
                    try
                    {
                        ns.Write(stateSender, 0, stateSender.Length);
                    }
                    catch (SocketException se)
                    {
                    }
                }
            }


        }

        //private static string BuildNmeaRTE(string strStarter, string talker, string formatter,
        //    IEnumerable<KeyValuePair<string, object>> data)
        private string BuildNmeaRTE(string strStarter, string talker, string formatter, int sentencesNo, int seqNo, string type, string routeName,
            string payload)
        {
            StringBuilder builder = new StringBuilder(256);
            // $GPRTE,2,1,c,0,W3IWI,DRIVWY,32CEDR,32-29,32BKLD,32-I95,32-US1,BW-32,BW-198*69

            builder.Append(strStarter[0]);
            builder.Append(talker);
            builder.Append(formatter);
            builder.Append(",");
            builder.Append(sentencesNo);
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


        // sent once for each waypoint in the route
        private bool SendWPL(NetworkStream ns, TcpClient client)
        {
            List<string> wplSentence = new List<string>();

            string wplHeader = "$GPWPL,";  //WPL
            string payload;
            foreach (var item in Project.Scenarios[0].Tasks[0].Waypoints.ToList().Select((value, index)=>(value, index)))
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

            // send sentence
            if (ns != null && ns.CanWrite && client.Connected)
            {
                foreach (var item in wplSentence)
                {
                    byte[] stateSender = Encoding.ASCII.GetBytes(item);
                    try
                    {
                        ns.Write(stateSender, 0, stateSender.Length);
                    }
                    catch (Exception e)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
            

        // AIS 데이터를 VDM sentence로 전송
        private void SendVDM(PlatformState state, NetworkStream ns, TcpClient client)
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
                                   ToBinaryString(utcSec, 6) + ToBinaryString(regApplication, 4) +
                                   ToBinaryString(spare, 1) + ToBinaryString(raimFlag, 1) +
                                   ToBinaryString(commState, 19);

            // bit string을 6bit ascii로 armoring
            // 그 후 nmea string에 부착
            string payloadData = PayloadArmoring(binaryPayload);

            string NMEAString = "!AIVDM,1,1,0,A,"; // VDM : 타선 ais 데이터
            NMEAString = NMEAString + payloadData + ",0*";
            NMEAString = NMEAString + Checksum(NMEAString);

            byte[] stateSender = Encoding.ASCII.GetBytes(NMEAString);

            if (ns != null && ns.CanWrite && client.Connected)
            {
                try
                {
                    ns.Write(stateSender, 0, stateSender.Length);
                }
                catch (Exception e)
                {
                }
            }
        }

        // GPS 데이터를 GGA sentence로 전송
        private void SendGGA(PlatformState state, double totalSecond, NetworkStream ns, TcpClient client)
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
            if (ns != null && ns.CanWrite && client.Connected)
            {
                try
                {
                    ns.Write(stateSender, 0, stateSender.Length);
                }
                catch (Exception e)
                {
                }
            }
        }

        // GPS의 방향 데이터를 HDT sentence로 전송
        private void SendHDT(PlatformState state, NetworkStream ns, TcpClient client)
        {
            string NMEAString = "$GPHDT,";
            double heading = state.Heading;
            NMEAString += heading + ",T*";
            NMEAString += Checksum(NMEAString);

            byte[] stateSender = Encoding.ASCII.GetBytes(NMEAString);
            if (ns != null && ns.CanWrite && client.Connected)
            {
                try
                {
                    ns.Write(stateSender, 0, stateSender.Length);
                }
                catch (Exception e)
                {
                }
            }
        }

        // Radar 데이터를 TTM sentence로 전송
        private void SendTTM(PlatformState state, double totalSecond, NetworkStream ns, TcpClient client)
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
            if (ns != null && ns.CanWrite && client.Connected)
            {
                ns.Write(stateSender, 0, stateSender.Length);
            }
        }

        // AIS 데이터를 VDM sentence로 전송
        public void SendVDM5(TargetShip target, NetworkStream ns, TcpClient client)
        {
            int msg = 5;
            int repeat = 0;
            int mmsi = target?.MMSI ?? 0;
            int ais_version = 0; // AIS Version, 0=[ITU1371](default)
            int imo = 1234567; // IMO Number, 7자리 숫자
            string callsign = "callsign"; // Call Sign, 7 six-bit characters, 더미 데이터 사용
            string shipname = "shipname"; // Vessel Name, 20 six-bit characters, 더미 데이터 사용
            int shiptype = 0; // Ship Type, 0 = Not available (default)
            int to_bow = (int)(0.5*target.Length); // Dimension to Bow, Meters
            int to_stern = (int)(0.5 * target.Length); // Dimension to Stern, Meters
            int to_port = (int)(10); // Dimension to Port, Meters, 더미 데이터 사용
            int to_starboard = (int)(10); // Dimension to Starboard, Meters, 더미 데이터 사용
            int epfd = 0; // Type of EPFD(Electronic Position Fixing Device), 0 = Undefined (default)
            int month = 0; // ETA month (UTC), 0 = N/A (default)
            int day = 0; // ETA day (UTC), 0 = N/A (default)
            int hour = 24; // ETA hour (UTC), 24 = N/A (default)
            int minute = 60; // ETA minute (UTC), 60 = N/A (default)
            int draught = 100; // Draught, Meters/10 단위, 더미 데이터 사용
            string destination = "desntination"; // Destination, 20 six-bit characters, 더미 데이터 사용
            int dte = 1; // DTE, 0=Data terminal ready, 1=Not ready (default)
            int spare = 0; // spare

            // 위 내용들을 전부 bit string 으로 변환
            // 함수 ToBinaryString(int, len)
            string binaryPayload = ToBinaryString(msg, 6).PadLeft(6, '0') + ToBinaryString(repeat, 2).PadLeft(2, '0') + ToBinaryString(mmsi, 30).PadLeft(30, '0') +
                                   ToBinaryString(ais_version, 2).PadLeft(2, '0') + ToBinaryString(imo, 30).PadLeft(30, '0') +
                                   ToBinaryString(callsign, 42).PadLeft(42, '0') + ToBinaryString(shipname, 120).PadLeft(120, '0') +
                                   ToBinaryString(shiptype, 8).PadLeft(8, '0') + ToBinaryString(to_bow, 9).PadLeft(9, '0') +
                                   ToBinaryString(to_stern, 9).PadLeft(9, '0') + ToBinaryString(to_port, 6).PadLeft(6, '0') +
                                   ToBinaryString(to_starboard, 6).PadLeft(6, '0') + ToBinaryString(epfd, 4).PadLeft(4, '0') +
                                   ToBinaryString(month, 4).PadLeft(4, '0') + ToBinaryString(day, 5).PadLeft(5, '0') +
                                   ToBinaryString(hour, 5).PadLeft(5, '0') + ToBinaryString(minute, 6).PadLeft(6, '0') + 
                                   ToBinaryString(draught, 8).PadLeft(8, '0') + ToBinaryString(destination, 120).PadLeft(120, '0') + 
                                   ToBinaryString(dte, 1) + ToBinaryString(spare, 1) + "00"; 

            // bit string을 6bit ascii로 armoring
            // 그 후 nmea string에 부착
            string payloadData = PayloadArmoring(binaryPayload);

            string NMEAString = "!AIVDM,1,1,0,A,"; // VDM : 타선 ais 데이터
            NMEAString = NMEAString + payloadData + ",0*";
            NMEAString = NMEAString + Checksum(NMEAString);

            byte[] stateSender = Encoding.ASCII.GetBytes(NMEAString);

            if (ns != null && ns.CanWrite && client.Connected)
            {
                try
                {
                    ns.Write(stateSender, 0, stateSender.Length);
                }
                catch (Exception e)
                {
                }
            }
        }

        public void SendCat240(DenseMatrix mat, int az, NetworkStream ns, TcpClient client)
        {
            int cat = 240;
            int len;// int를 다른걸로 수정해야함
            //int fspec; // 11100111 10100000 = 231 160
            byte[] fspec = new byte[2];
            fspec[0] = 231;
            fspec[1] = 160;
            int sac = 0; //dataSourceIdentifier;
            int sic = 0; //dataSourceIdentifier;
            int messageType =2; //Message type = 002,  video message
            int msgIndex = az; //video record header, 매번 증가해야 되지만 일단 임시로 0으로 고정
            int startAz = (int)Math.Round(65536 * az / 3600.0); // video header
            int endAz = startAz + 20;
            int startRg=0;
            int cellDur = 8151615; //femtoSecond, D=1250km
            int isCompressed = 0; // video cell resolution & data compression indicator
            int resoltuion = 4;
            int nbVb=1024; // video cells counters
            int nbCells=1024;
            int rep = 16; // video block
            byte[] videoBlock = new byte[1024]; // video block , int를 다른걸로 수정해야됨
            len = 1 + 2 + 2 + 2 + 1 + 4 + 12 + 2 + 5 + 1 + 64 * rep;// int를 다른걸로 수정해야함

            var a =mat.Row(az);
            for (int i=0; i<1024; i++)
            {
                try
                {
                    int aa = (int) a[i];
                    if (aa < 0) { aa = 0; }
                    if (aa > 255) { aa = 255; }
                    videoBlock[i] = Convert.ToByte(aa);
                }
                catch (Exception e)
                {
                    var aaaaa = e;
                }
            }

            byte[] signal = new byte[len];
            Array.Copy(BitConverter.GetBytes(cat), 0, signal, 0, 1);
            Array.Copy(BitConverter.GetBytes(len), 0, signal, 1, 2);
            Array.Reverse(signal, 1, 2);
            Array.Copy(fspec, 0, signal, 3, 2);
            Array.Copy(BitConverter.GetBytes(sac), 0, signal, 5, 1);
            Array.Copy(BitConverter.GetBytes(sic), 0, signal, 6, 1);
            Array.Copy(BitConverter.GetBytes(messageType), 0, signal, 7, 1);
            Array.Copy(BitConverter.GetBytes(msgIndex), 0, signal, 8, 4);
            Array.Reverse(signal, 8, 4);
            Array.Copy(BitConverter.GetBytes(startAz), 0, signal, 12, 2);
            Array.Reverse(signal, 12, 2);
            Array.Copy(BitConverter.GetBytes(endAz), 0, signal, 14, 2);
            Array.Reverse(signal, 14, 2);
            Array.Copy(BitConverter.GetBytes(startRg), 0, signal, 16, 4);
            Array.Reverse(signal, 16, 4);
            Array.Copy(BitConverter.GetBytes(cellDur), 0, signal, 20, 4);
            Array.Reverse(signal, 20, 4);
            Array.Copy(BitConverter.GetBytes(isCompressed), 0, signal, 24, 1);
            Array.Copy(BitConverter.GetBytes(resoltuion), 0, signal, 25, 1);
            Array.Copy(BitConverter.GetBytes(nbVb), 0, signal, 26, 2);
            Array.Reverse(signal, 26, 2);
            Array.Copy(BitConverter.GetBytes(nbCells), 0, signal, 28, 3);
            Array.Reverse(signal, 28, 3);
            Array.Copy(BitConverter.GetBytes(rep), 0, signal, 31, 1);
            Array.Copy(videoBlock, 0, signal, 32, 1024);

            if (ns != null && ns.CanWrite && client.Connected)
            {
                try
                {
                    ns.Write(signal, 0, signal.Length);
                }
                catch (Exception e)
                {
                }
            }
        }

        // 기타 기능들
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

        private string ToBinaryString(string text, int len)
        {
            // six-bit ascii 적용
            string binaryData = "";
            for (int i = 0; i < text.Length; i++)
            {
                int charNum = Convert.ToInt32(Char.ToUpper(text[i]));
                if (charNum > 127) { charNum = 0; }
                else if (charNum > 63) { charNum -= 64; }
                string binaryChar = Convert.ToString(charNum, 2).PadLeft(6, '0');
                binaryData += binaryChar;
            }
            if (binaryData.Length < len) { binaryData.PadLeft(len, '0'); }
            else if (binaryData.Length > len) { binaryData = binaryData.Substring(0, len); }
            return binaryData;
        }
    }
}