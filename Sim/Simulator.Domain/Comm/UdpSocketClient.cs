using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using SyDLab.Usv.Simulator.Domain.Logs;

namespace SyDLab.Usv.Simulator.Domain.Comm
{
    public class UdpSocketClient : IDisposable
    {
        protected object LOCKOBJECT = new object();
        protected Socket _sockOut = null;
        protected Socket _sockIn1 = null;
        protected Socket _sockIn2 = null;
        private Socket _sockSaOut = null;
        protected bool _start = false;
        private bool _connected = false;


        private IPEndPoint _srcIPEndPoint;// = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 9002);
        private IPEndPoint _remoteIpEndPointSendSaOut;
        private EndPoint _srcEndPoint;
        private IPEndPoint _srcIPEndPoint3;
        private EndPoint _srcEndPoint3;
        private EndPoint _dstEndPoint;

        private byte[] byteData = new byte[1024];

        public UdpSocketClient()
        {
        }

        public void SendUdpData(byte[] bytes)
        {
            try
            {
                if(_connected)
                    this._sockOut?.SendTo(bytes, _dstEndPoint);
            }
            catch (SocketException se)
            {
                throw;
            }
        }

        public void SendDataSaOut(byte[] bytes)
        {
            try
            {
                if (_sockSaOut == null)
                {
                    _sockSaOut = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                    _sockSaOut.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                }
                _sockSaOut?.SendTo(bytes, _remoteIpEndPointSendSaOut);
            }
            catch (SocketException se)
            {
                _sockSaOut?.Close();
                throw;
            }
        }

        public byte[] RecvUdpData2(int buffSize)
        {
            byte[] recvUdpData = new byte[buffSize];

            try
            {
                if (_connected)
                    _sockIn1?.ReceiveFrom(recvUdpData, ref _srcEndPoint);
                // reconnect when socket connection is failled
                // else if (!_connected && _sockIn1==null)
                        
            }
            catch (SocketException se)
            {
                // Console.WriteLine(se);
                // throw;
            }
            
            return recvUdpData;
        }

        public bool RecvSensorData(ref byte[] recvData)
        {
            try
            {
                if (_connected)
                    _sockIn2?.ReceiveFrom(recvData, ref _srcEndPoint3);
            }
            catch (SocketException se)
            {
                Debug.Print(se.ToString());
                return false;
            }

            return true;
        }

        public void Start(int inPort, int inPort3, IPAddress outIpAddress, int outPort)
        {
            try
            {
                this._sockOut?.Close();
                this._sockOut = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                this._sockOut?.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 10000);
                this._sockOut?.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                // this._sockOut.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, false);
                this._dstEndPoint = new IPEndPoint(outIpAddress, outPort);
                
                // recv.
                this._sockIn1?.Close();
                this._sockIn1 = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                this._sockIn1?.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 10000);
                this._sockIn1?.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                // this._sockIn1?.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, false);
                this._srcIPEndPoint = new IPEndPoint(IPAddress.Any, inPort);
                this._srcEndPoint = (EndPoint)this._srcIPEndPoint;
                this._sockIn1.Bind(this._srcEndPoint);

                //recv2
                this._sockIn2?.Close();
                this._sockIn2 = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                this._sockIn2?.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReceiveTimeout, 10000);
                this._sockIn2?.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                // this._sockIn1?.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ExclusiveAddressUse, false);
                this._srcIPEndPoint3 = new IPEndPoint(IPAddress.Any, inPort3);
                this._srcEndPoint3 = (EndPoint)this._srcIPEndPoint3;
                this._sockIn2.Bind(this._srcEndPoint3);

                
                //Send Sa data to Ca via port 9002
                _sockSaOut?.Close();
                this._sockSaOut = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
                this._sockSaOut.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _sockSaOut.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
                _remoteIpEndPointSendSaOut = new IPEndPoint(outIpAddress, 9002);

                this._connected = true;
            }
            catch (SocketException se)
            {
                this._sockOut?.Close();
                this._sockIn1?.Close();
                this._sockIn2?.Close();
                this._sockSaOut?.Close();
                this._connected = false;
                Debug.Print(se.ToString());
                throw;
            }
        }

        public void Stop(string logMessage)
        {
            // LogManager.GetInstance().AddLog("Stop client for " + logMessage + ".");
            // LogManager.GetInstance().StopLogging();

            try
            {
                this._start = false;
                this._sockOut?.Close();
                this._sockIn1?.Close();
                this._sockIn2?.Close();
                this._sockSaOut?.Close();
                _connected = false;

            }
            catch (SocketException se)
            {
                Console.WriteLine(se);
                throw;
            }
        }

        #region IDisposable Member
        public void Dispose()
        {
            _sockOut.Dispose();
            _sockIn1.Dispose();
        }
        #endregion


    }

}
