using System;
using Microsoft.Maps.MapControl.WPF;

namespace NmeaParser
{
    public enum SentenceID
    {
        VDO,
        VDM,
        RMC,
        VTG,
        GGA,
        GLL,
        HDT,
        RPM,
        RSA,
        TTM
    }

    public class SensorData
    {
        public SensorData(double lat, double lon, SentenceID sentenceId)
        {
            _lat = lat;
            _lon = lon;
            _sentenceId = sentenceId;
        }
        public SensorData(Location position,SentenceID sentenceId)
        {
            _lat = position.Latitude;
            _lon = position.Longitude;
            _sentenceId = sentenceId;
        }

        public SensorData(double lat, double lon, double sog, double cog, SentenceID sentenceId)
        {
            _lat = lat;
            _lon = lon;
            _sog = sog;
            _cog = cog;
            _sentenceId = sentenceId;
        }

        public SensorData(Location position, double sog, double cog, SentenceID sentenceId)
        {
            _lat = position.Latitude;
            _lon = position.Longitude;
            _sog = sog;
            _cog = cog;
            _sentenceId = sentenceId;
        }

        // engine rpm
        public SensorData(int side, double rpm, SentenceID sentenceId)
        {
            _side = side;
            _rpm = rpm;
            _sentenceId = sentenceId;
        }

        public SensorData(uint mmsi, Location position, double sog, double cog, SentenceID sentenceId)
        {
            _mmsi = mmsi;
            _lat = position.Latitude;
            _lon = position.Longitude;
            _sog = sog;
            _cog = cog;
            _sentenceId = sentenceId;
        }

        // rudder angle
        public SensorData(bool isStbdValid, double stbd, bool isPortValid, double port, SentenceID sentenceId)
        {
            _sentenceId = sentenceId;
        }

        private double _lat;
        private double _lon;
        private SentenceID _sentenceId;
        private double _sog;
        private double _cog;
        private int _side;
        private double _rpm;
        private uint _mmsi;
        public double Lat { get=>_lat; set=>_lat=value; }
        public double Lon { get=>_lon; set=>_lon=value; }
        public SentenceID SentenceId { get=>_sentenceId; set=>_sentenceId=value; }
        public double Sog { get=>_sog; set=>_sog=value; }
        public double Cog { get=>_cog; set=>_cog=value; }
        public int Side{ get=>_side; set=>_side=value; }
        public double Rpm { get=>_rpm; set=>_rpm=value; }
        public uint Mmsi { get=>_mmsi; set=>_mmsi=value; }
    }
}