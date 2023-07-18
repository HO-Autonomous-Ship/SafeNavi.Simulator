using System;

namespace SyDLab.Usv.Simulator.Domain.Comm
{
    [Serializable]
    public class ST_POS : IConvertData
    {
        public ST_POS()
        {
            _lat = 35.5F;
            _lon = 127.7F;
            _bearing = 0F;
        }

        private float _lat;

        public float Lat
        {
            get => _lat;
            set => _lat = value;
        }
        private float _lon;
        public float Lon
        {
            get => _lon;
            set => _lon = value;
        }
        private float _bearing;
        public float Bearing
        {
            get => _bearing;
            set => _bearing = value;
        }
        public void Serialize(ref byte[] bytes)
        {
            EncoderHelper.FloatEncoder(this._lat, ref bytes);
            EncoderHelper.FloatEncoder(this._lon, ref bytes);
            EncoderHelper.FloatEncoder(this._bearing, ref bytes);
        }

        public void Deserialize(byte[] bytes, ref int offset)
        {
            if (bytes == null) return;

            this._lat = DecoderHelper.FloatDecoder(bytes, ref offset);
            this._lon = DecoderHelper.FloatDecoder(bytes, ref offset);
            this._bearing = DecoderHelper.FloatDecoder(bytes, ref offset);
        }
    }

    [Serializable]
    public class ST_DGPS_POS : IConvertData
    {
        private float _latSign;
        public float LatSign
        {
            get => _latSign;
            set => _latSign = value;
        }
        private float _latDeg;
        public float LatDeg
        {
            get => _latDeg;
            set => _latDeg = value;
        }
        private float _latMin;
        public float LatMin
        {
            get => _latMin;
            set => _latMin = value;
        }
        private float _lonSign;
        public float LonSign
        {
            get => _lonSign;
            set => _lonSign = value;
        }
        private float _lonDeg;
        public float LonDeg
        {
            get => _lonDeg;
            set => _lonDeg = value;
        }
        private float _lonMin;
        public float LonMin
        {
            get => _lonMin;
            set => _lonMin = value;
        }

        public void Serialize(ref byte[] bytes)
        {
            EncoderHelper.FloatEncoder(this._latSign, ref bytes);
            EncoderHelper.FloatEncoder(this._latDeg, ref bytes);
            EncoderHelper.FloatEncoder(this._latMin, ref bytes);
            EncoderHelper.FloatEncoder(this._lonSign, ref bytes);
            EncoderHelper.FloatEncoder(this._lonDeg, ref bytes);
            EncoderHelper.FloatEncoder(this._lonMin, ref bytes);
        }
        public void Deserialize(byte[] bytes, ref int offset)
        {
            if (bytes == null) return;

            this._latSign = DecoderHelper.FloatDecoder(bytes, ref offset);
            this._latDeg = DecoderHelper.FloatDecoder(bytes, ref offset);
            this._latMin = DecoderHelper.FloatDecoder(bytes, ref offset);
            this._lonSign = DecoderHelper.FloatDecoder(bytes, ref offset);
            this._lonDeg = DecoderHelper.FloatDecoder(bytes, ref offset);
            this._lonMin = DecoderHelper.FloatDecoder(bytes, ref offset);
        }
    }

    [Serializable]
    public class ST_VESSEL_VELOCITY : IConvertData
    {

        private float _sog;
        public float Sog
        {
            get => _sog;
            set => _sog = value;
        }

        private float _cog;
        public float Cog
        {
            get => _cog;
            set => _cog = value;
        }

        private float _rot;
        public float Rot
        {
            get => _rot;
            set => _rot = value;
        }

        public void Serialize(ref byte[] bytes)
        {
            EncoderHelper.FloatEncoder(this._sog, ref bytes);
            EncoderHelper.FloatEncoder(this._cog, ref bytes);
            EncoderHelper.FloatEncoder(this._rot, ref bytes);
        }
        public void Deserialize(byte[] bytes, ref int offset)
        {
            if (bytes == null) return;

            this._sog = DecoderHelper.FloatDecoder(bytes, ref offset);
            this._cog = DecoderHelper.FloatDecoder(bytes, ref offset);
            this._rot = DecoderHelper.FloatDecoder(bytes, ref offset);
        }

        public ST_VESSEL_VELOCITY()
        {
        }
    }

    [Serializable]
    public class ST_BASE_STATE : IConvertData
    {

        private float _posX;
        public float PosX
        {
            get => _posX;
            set => _posX = value;
        }

        private float _posY;
        public float PosY
        {
            get => _posY;
            set => _posY = value;
        }

        private float _sog;
        public float Sog
        {
            get => _sog;
            set => _sog = value;
        }

        private float _cog;
        public float Cog
        {
            get => _cog;
            set => _cog = value;
        }

        public void Serialize(ref byte[] bytes)
        {
            EncoderHelper.FloatEncoder(this._posX, ref bytes);
            EncoderHelper.FloatEncoder(this._posY, ref bytes);
            EncoderHelper.FloatEncoder(this._sog, ref bytes);
            EncoderHelper.FloatEncoder(this._cog, ref bytes);
        }
        public void Deserialize(byte[] bytes, ref int offset)
        {
            if (bytes == null) return;

            this._posX = DecoderHelper.FloatDecoder(bytes, ref offset);
            this._posY = DecoderHelper.FloatDecoder(bytes, ref offset);
            this._sog = DecoderHelper.FloatDecoder(bytes, ref offset);
            this._cog = DecoderHelper.FloatDecoder(bytes, ref offset);
        }
    }


    [Serializable]
    public class ST_VOYAGE_PLAN : IConvertData
    {

        private int _mainDirection;
        public int MainDirection
        {
            get => _mainDirection;
            set => _mainDirection = value;
        }

        private int _numWaypoints;
        public int NumWaypoints
        {
            get => _numWaypoints;
            set => _numWaypoints = value;
        }

        private ST_POS[] _stPos;
        public ST_POS[] StPos
        {
            get => _stPos;
            set => _stPos = value;
        }

        private float[] _heading;
        public float[] Heading
        {
            get => _heading;
            set => _heading = value;
        }

        private float[] _speed;
        public float[] Speed
        {
            get => _speed;
            set => _speed = value;
        }

        private float[] _rpm;
        public float[] Rpm
        {
            get => _rpm;
            set => _rpm = value;
        }

        public void Serialize(ref byte[] bytes)
        {
            EncoderHelper.IntEncoder(this._mainDirection, ref bytes);
            EncoderHelper.IntEncoder(this._numWaypoints, ref bytes);
            EncoderHelper.ST_POS_Encoder(this._stPos, Common.MAX_NUM_WAYPOINT, ref bytes);
            EncoderHelper.FloatEncoder(this._heading, Common.MAX_NUM_WAYPOINT, ref bytes);
            EncoderHelper.FloatEncoder(this._speed, Common.MAX_NUM_WAYPOINT, ref bytes);
            EncoderHelper.FloatEncoder(this._rpm, Common.MAX_NUM_WAYPOINT, ref bytes);
        }
        public void Deserialize(byte[] bytes, ref int offset)
        {
            if (bytes == null) return;

            this._mainDirection = DecoderHelper.IntDecoder(bytes, ref offset);
            this._numWaypoints = DecoderHelper.IntDecoder(bytes, ref offset);
            this._stPos = DecoderHelper.ST_POSDecoder(bytes, Common.MAX_NUM_WAYPOINT, ref offset);
            this._heading = DecoderHelper.FloatDecoder(bytes, Common.MAX_NUM_WAYPOINT, ref offset);
            this._speed = DecoderHelper.FloatDecoder(bytes, Common.MAX_NUM_WAYPOINT, ref offset);
            this._rpm = DecoderHelper.FloatDecoder(bytes, Common.MAX_NUM_WAYPOINT, ref offset);
        }

        public ST_VOYAGE_PLAN()
        {
            this._stPos = new ST_POS[Common.MAX_NUM_WAYPOINT];
            this._heading = new float[Common.MAX_NUM_WAYPOINT];
            this._speed = new float[Common.MAX_NUM_WAYPOINT];
            this._rpm = new float[Common.MAX_NUM_WAYPOINT];
        }
    }

    [Serializable]
    public class ST_ENVIRONMENT : IConvertData
    {
        private float _waterTemp;
        public float WaterTemp
        {
            get => _waterTemp;
            set => _waterTemp = value;
        }
        private float _airTemp;
        public float AirTemp
        {
            get => _airTemp;
            set => _airTemp = value;
        }
        private float _waterDepth;
        public float WaterDepth
        {
            get => _waterDepth;
            set => _waterDepth = value;
        }
        private float _currentDir;
        public float CurrentDir
        {
            get => _currentDir;
            set => _currentDir = value;
        }
        private float _currentSpeed;
        public float CurrentSpeed
        {
            get => _currentSpeed;
            set => _currentSpeed = value;
        }
        private float _windDir;
        public float WindDir
        {
            get => _windDir;
            set => _windDir = value;
        }
        private float _windSpeed;
        public float WindSpeed
        {
            get => _windSpeed;
            set => _windSpeed = value;
        }
        private float _waveDir;
        public float WaveDir
        {
            get => _waveDir;
            set => _waveDir = value;
        }
        private float _waveHeight;
        public float WaveHeight
        {
            get => _waveHeight;
            set => _waveHeight = value;
        }
        private float _wavePeriod;
        public float WavePeriod
        {
            get => _wavePeriod;
            set => _wavePeriod = value;
        }
        private int _weatherHarshness;
        public int WeatherHarshness
        {
            get => _weatherHarshness;
            set => _weatherHarshness = value;
        }
        private int _weatherCase;
        public int WeatherCase
        {
            get => _weatherCase;
            set => _weatherCase = value;
        }

        public void Serialize(ref byte[] bytes)
        {
            EncoderHelper.FloatEncoder(this._waterTemp, ref bytes);
            EncoderHelper.FloatEncoder(this._airTemp, ref bytes);
            EncoderHelper.FloatEncoder(this._waterDepth, ref bytes);
            EncoderHelper.FloatEncoder(this._currentDir, ref bytes);
            EncoderHelper.FloatEncoder(this._currentSpeed, ref bytes);
            EncoderHelper.FloatEncoder(this._windDir, ref bytes);
            EncoderHelper.FloatEncoder(this._windSpeed, ref bytes);
            EncoderHelper.FloatEncoder(this._waveDir, ref bytes);
            EncoderHelper.FloatEncoder(this._waveHeight, ref bytes);
            EncoderHelper.FloatEncoder(this._wavePeriod, ref bytes);
            EncoderHelper.IntEncoder(this._weatherHarshness, ref bytes);
            EncoderHelper.IntEncoder(this._weatherCase, ref bytes);
        }
        public void Deserialize(byte[] bytes, ref int offset)
        {
            if (bytes == null) return;

            this._waterTemp = DecoderHelper.FloatDecoder(bytes, ref offset);
            this._airTemp = DecoderHelper.FloatDecoder(bytes, ref offset);
            this._waterDepth = DecoderHelper.FloatDecoder(bytes, ref offset);
            this._currentDir = DecoderHelper.FloatDecoder(bytes, ref offset);
            this._currentSpeed = DecoderHelper.FloatDecoder(bytes, ref offset);
            this._windDir = DecoderHelper.FloatDecoder(bytes, ref offset);
            this._windSpeed = DecoderHelper.FloatDecoder(bytes, ref offset);
            this._waveDir = DecoderHelper.FloatDecoder(bytes, ref offset);
            this._waveHeight = DecoderHelper.FloatDecoder(bytes, ref offset);
            this._wavePeriod = DecoderHelper.FloatDecoder(bytes, ref offset);
            this._weatherHarshness = DecoderHelper.IntDecoder(bytes, ref offset);
            this._weatherCase = DecoderHelper.IntDecoder(bytes, ref offset);
        }
    }

    [Serializable]
    public class ST_VESSEL_INFO : IConvertData
    {

        private uint _shipId;
        public uint ShipId
        {
            get => _shipId;
            set => _shipId = value;
        }
        private uint _imoNum;
        public uint ImoNum
        {
            get => _imoNum;
            set => _imoNum = value;
        }
        private uint _mmsi;
        public uint Mmsi
        {
            get => _mmsi;
            set => _mmsi = value;
        }
        private char[] _callsign;
        public char[] CallSign
        {
            get => _callsign;
            set => _callsign = value;
        }
        private char[] _vesselName;
        public char[] VesselName
        {
            get => _vesselName;
            set => _vesselName = value;
        }
        private uint _vesselType;
        public uint VesselType
        {
            get => _vesselType;
            set => _vesselType = value;
        }
        private float _lpp;
        public float Lpp
        {
            get => _lpp;
            set => _lpp = value;
        }
        private float _breadth;
        public float Breadth
        {
            get => _breadth;
            set => _breadth = value;
        }
        // private int _numPorpeller;
        // public int NumPropeller
        // {
        //     get => _numPorpeller;
        //     set => _numPorpeller=value;
        // }
        // private float _minWaterDepth;
        // public float MinWaterDepth
        // {
        //     get { return _minWaterDepth; }
        //     set { _minWaterDepth=value; }
        // }
        // private float _maxVesselSpeed;
        // public float MaxVesselSpeed
        // {
        //     get { return _maxVesselSpeed; }
        //     set { _maxVesselSpeed=value; }
        // }

        public void Serialize(ref byte[] bytes)
        {
            EncoderHelper.UintEncoder(this._shipId, ref bytes);
            EncoderHelper.UintEncoder(this._imoNum, ref bytes);
            EncoderHelper.UintEncoder(this._mmsi, ref bytes);
            EncoderHelper.CharEncoder(this._callsign, Common.MAX_CALL_SIGN_LENGTH, ref bytes);
            EncoderHelper.CharEncoder(this._vesselName, Common.MAX_VESSEL_NAME_LENGTH, ref bytes);
            EncoderHelper.UintEncoder(this._vesselType, ref bytes);
            EncoderHelper.FloatEncoder(this._lpp, ref bytes);
            EncoderHelper.FloatEncoder(this._breadth, ref bytes);
            // EncoderHelper.IntEncoder(this._numPorpeller, ref bytes);
            // EncoderHelper.FloatEncoder(this._minWaterDepth, ref bytes);
            // EncoderHelper.FloatEncoder(this._maxVesselSpeed, ref bytes);
        }
        public void Deserialize(byte[] bytes, ref int offset)
        {
            if (bytes == null) return;

            this._shipId = DecoderHelper.UintDecoder(bytes, ref offset);
            this._imoNum = DecoderHelper.UintDecoder(bytes, ref offset);
            this._mmsi = DecoderHelper.UintDecoder(bytes, ref offset);
            this._callsign = DecoderHelper.CharDecoder(bytes, Common.MAX_CALL_SIGN_LENGTH, ref offset);
            this._vesselName = DecoderHelper.CharDecoder(bytes, Common.MAX_VESSEL_NAME_LENGTH, ref offset);
            this._vesselType = DecoderHelper.UintDecoder(bytes, ref offset);
            this._lpp = DecoderHelper.FloatDecoder(bytes, ref offset);
            this._breadth = DecoderHelper.FloatDecoder(bytes, ref offset);
            // this._numPorpeller = DecoderHelper.IntDecoder(bytes, ref offset);
            // this._minWaterDepth = DecoderHelper.FloatDecoder(bytes, ref offset);
            // this._maxVesselSpeed = DecoderHelper.FloatDecoder(bytes, ref offset);
        }


        public ST_VESSEL_INFO()
        {
            this._callsign = new char[Common.MAX_CALL_SIGN_LENGTH];
            this._vesselName = new char[Common.MAX_VESSEL_NAME_LENGTH];
        }
    }

    [Serializable]
    public class ST_VOYAGE_INFO : IConvertData
    {
        private ST_DGPS_POS _stPosDeparture;
        public ST_DGPS_POS StPosDeparture
        {
            get => _stPosDeparture;
            set => _stPosDeparture = value;
        }
        private ST_DGPS_POS _stPosDestination;
        public ST_DGPS_POS StPosDestination
        {
            get => _stPosDestination;
            set => _stPosDestination = value;
        }
        private bool _collisionSituation;
        public bool CollisionSituation
        {
            get => _collisionSituation;
            set => _collisionSituation = value;
        }
        private ST_VOYAGE_PLAN _stVoyagePlan;
        public ST_VOYAGE_PLAN StVoyagePlan
        {
            get => _stVoyagePlan;
            set => _stVoyagePlan = value;
        }
        private ST_VOYAGE_PLAN _stAvoidPlan;
        public ST_VOYAGE_PLAN StAvoidPlan
        {
            get => _stAvoidPlan;
            set => _stAvoidPlan = value;
        }

        private uint _rta;
        public uint Rta
        {
            get => _rta;
            set => _rta = value;
        }

        private uint _eta;
        public uint Eta
        {
            get => _eta;
            set => _eta = value;
        }

        private int _posIndex;
        public int PosIndex
        {
            get => _posIndex;
            set => _posIndex = value;
        }

        // private ST_DGPS_POS _stVesselPos;
        // public ST_DGPS_POS StVesselPos
        // {
        //     get { return _stVesselPos; }
        //     set { _stVesselPos=value; }
        // }
        //
        // private ST_VESSEL_VELOCITY _stVelocity;
        // public ST_VESSEL_VELOCITY StVelocity
        // {
        //     get { return _stVelocity; }
        //     set { _stVelocity=value; }
        // }
        //
        // private float _draftFwd;
        // public float DraftFwd
        // {
        //     get { return _draftFwd; }
        //     set { _draftFwd=value; }
        // }
        //
        // private float _draftAft;
        // public float DraftAft
        // {
        //     get { return _draftAft; }
        //     set { _draftAft=value; }
        // }
        //
        // private float _heading;
        // public float  Heading
        // {
        //     get { return _heading; }
        //     set { _heading=value; }
        // }
        //
        // private float[] _propellerRpm;
        // public float[] PropellerRpm
        // {
        //     get { return _propellerRpm; }
        //     set { _propellerRpm=value; }
        // }
        //
        // private float[] _rudderAngle;
        // public float[] RudderAngle
        // {
        //     get { return _rudderAngle; }
        //     set { _rudderAngle=value; }
        // }

        public void Serialize(ref byte[] bytes)
        {
            EncoderHelper.ST_DGPS_POS_Encoder(this._stPosDeparture, ref bytes);
            EncoderHelper.ST_DGPS_POS_Encoder(this._stPosDestination, ref bytes);
            EncoderHelper.BOOLEncoder(this._collisionSituation, ref bytes);
            EncoderHelper.ST_VOYAGE_PLAN_Encoder(this._stVoyagePlan, ref bytes);
            EncoderHelper.ST_VOYAGE_PLAN_Encoder(this._stAvoidPlan, ref bytes);
            EncoderHelper.UintEncoder(this._rta, ref bytes);
            EncoderHelper.UintEncoder(this._eta, ref bytes);
            EncoderHelper.IntEncoder(this._posIndex, ref bytes);
            // EncoderHelper.ST_DGPS_POS_Encoder(this._stVesselPos, ref bytes);
            // EncoderHelper.ST_VESSEL_VELOCITY_Encoder(this._stVelocity, ref bytes);
            // EncoderHelper.FloatEncoder(this._draftFwd, ref bytes);
            // EncoderHelper.FloatEncoder(this._draftAft, ref bytes);
            // EncoderHelper.FloatEncoder(this._heading, ref bytes);
            // EncoderHelper.FloatEncoder(this._propellerRpm, Common.MAX_NUM_PROPELLER, ref bytes);
            // EncoderHelper.FloatEncoder(this._rudderAngle, Common.MAX_NUM_PROPELLER, ref bytes);
        }
        public void Deserialize(byte[] bytes, ref int offset)
        {
            if (bytes == null) return;

            this._stPosDeparture = DecoderHelper.ST_DGPS_POSDecoder(bytes, ref offset);
            this._stPosDestination = DecoderHelper.ST_DGPS_POSDecoder(bytes, ref offset);
            this._collisionSituation = DecoderHelper.BOOLDecoder(bytes, ref offset);
            this._stVoyagePlan = DecoderHelper.ST_VOYAGE_PLANDecoder(bytes, ref offset);
            this._stAvoidPlan = DecoderHelper.ST_VOYAGE_PLANDecoder(bytes, ref offset);
            this._rta = DecoderHelper.UintDecoder(bytes, ref offset);
            this._eta = DecoderHelper.UintDecoder(bytes, ref offset);
            this._posIndex = DecoderHelper.IntDecoder(bytes, ref offset);
            // this._stVesselPos = DecoderHelper.ST_DGPS_POSDecoder(bytes, ref offset);
            // this._stVelocity = DecoderHelper.ST_VESSEL_VELOCITYDecoder(bytes, ref offset);
            // this._draftFwd = DecoderHelper.FloatDecoder(bytes, ref offset);
            // this._draftAft = DecoderHelper.FloatDecoder(bytes, ref offset);
            // this._heading = DecoderHelper.FloatDecoder(bytes, ref offset);
            // this._stVelocity = DecoderHelper.ST_VESSEL_VELOCITYDecoder(bytes, ref offset);
            // this._propellerRpm = DecoderHelper.FloatDecoder(bytes, Common.MAX_NUM_PROPELLER, ref offset);
            // this._rudderAngle = DecoderHelper.FloatDecoder(bytes, Common.MAX_NUM_PROPELLER, ref offset);
        }

        public ST_VOYAGE_INFO()
        {
            _stPosDeparture = new ST_DGPS_POS();
            _stPosDestination = new ST_DGPS_POS();
            _stVoyagePlan = new ST_VOYAGE_PLAN();
            _stAvoidPlan = new ST_VOYAGE_PLAN();
            // _propellerRpm = new float[Common.MAX_NUM_PROPELLER];
            // _rudderAngle = new float[Common.MAX_NUM_PROPELLER];
        }
    }

    [Serializable]
    public class ST_VESSEL_MOTION : IConvertData
    {
        private float _roll;
        public float Roll
        {
            get => _roll;
            set => _roll = value;
        }

        private float _pitch;
        public float Pitch
        {
            get => _pitch;
            set => _pitch = value;
        }

        public void Serialize(ref byte[] bytes)
        {
            EncoderHelper.FloatEncoder(this._roll, ref bytes);
            EncoderHelper.FloatEncoder(this._pitch, ref bytes);
        }
        public void Deserialize(byte[] bytes, ref int offset)
        {
            if (bytes == null) return;

            this._roll = DecoderHelper.FloatDecoder(bytes, ref offset);
            this._pitch = DecoderHelper.FloatDecoder(bytes, ref offset);
        }
    }

    [Serializable]
    public class ST_OWNSHIP_INFO : IConvertData
    {
        private uint _time;
        public uint Time
        {
            get => _time;
            set => _time = value;
        }
        private ST_VESSEL_INFO _stVesselInfo;
        public ST_VESSEL_INFO StVesselInfo
        {
            get => _stVesselInfo;
            set => _stVesselInfo = value;
        }
        private int _numPropeller;
        public int NumPropeller
        {
            get => _numPropeller;
            set => _numPropeller = value;
        }

        private float _minWaterDepth;
        public float MinWaterDepth
        {
            get => _minWaterDepth;
            set => _minWaterDepth = value;
        }

        private float _maxVesselSpeed;
        public float MaxVesselSpeed
        {
            get => _maxVesselSpeed;
            set => _maxVesselSpeed = value;
        }
        private ST_BASE_STATE _stBaseState;
        public ST_BASE_STATE StBaseState
        {
            get => _stBaseState;
            set => _stBaseState = value;
        }

        private float _heading;
        public float Heading
        {
            get => _heading;
            set => _heading = value;
        }

        private float _rot;
        public float Rot
        {
            get => _rot;
            set => _rot = value;
        }
        private ST_POS _stPos;
        public ST_POS StPos
        {
            get => _stPos;
            set => _stPos = value;
        }

        private float _draftFwd;
        public float DraftFwd
        {
            get => _draftFwd;
            set => _draftFwd = value;
        }

        private float _draftAft;
        public float DraftAft
        {
            get => _draftAft;
            set => _draftAft = value;
        }

        private float[] _propellerRpm;
        public float[] PropellerRpm
        {
            get => _propellerRpm;
            set => _propellerRpm = value;
        }

        private float[] _rudderAngle;
        public float[] RudderAngle
        {
            get => _rudderAngle;
            set => _rudderAngle = value;
        }

        private ST_VESSEL_MOTION _stVesselMotion;
        public ST_VESSEL_MOTION StVesselMotion
        {
            get => _stVesselMotion;
            set => _stVesselMotion = value;
        }

        private ST_VOYAGE_INFO _stVoyageInfo;
        public ST_VOYAGE_INFO StVoyageInfo
        {
            get => _stVoyageInfo;
            set => _stVoyageInfo = value;
        }

        public void Serialize(ref byte[] bytes)
        {
            EncoderHelper.UintEncoder(this._time, ref bytes);
            EncoderHelper.ST_VESSEL_INFO_Encoder(this._stVesselInfo, ref bytes);
            EncoderHelper.IntEncoder(this._numPropeller, ref bytes);
            EncoderHelper.FloatEncoder(this._minWaterDepth, ref bytes);
            EncoderHelper.FloatEncoder(this._maxVesselSpeed, ref bytes);
            EncoderHelper.ST_BASE_STATE_Encoder(this._stBaseState, ref bytes);
            EncoderHelper.FloatEncoder(this._heading, ref bytes);
            EncoderHelper.FloatEncoder(this._rot, ref bytes);
            EncoderHelper.ST_POS_Encoder(this._stPos, ref bytes);
            EncoderHelper.FloatEncoder(this._draftFwd, ref bytes);
            EncoderHelper.FloatEncoder(this._draftAft, ref bytes);
            EncoderHelper.FloatEncoder(this._propellerRpm, Common.MAX_NUM_PROPELLER, ref bytes);
            EncoderHelper.FloatEncoder(this._rudderAngle, Common.MAX_NUM_PROPELLER, ref bytes);
            EncoderHelper.ST_VESSEL_MOTION_Encoder(this._stVesselMotion, ref bytes);
            EncoderHelper.ST_VOYAGE_INFO_Encoder(this._stVoyageInfo, ref bytes);
        }
        public void Deserialize(byte[] bytes, ref int offset)
        {
            if (bytes == null) return;

            this._time = DecoderHelper.UintDecoder(bytes, ref offset);
            this._stVesselInfo = DecoderHelper.ST_VESSEL_INFODecoder(bytes, ref offset);
            this._numPropeller = DecoderHelper.IntDecoder(bytes, ref offset);
            this._minWaterDepth = DecoderHelper.FloatDecoder(bytes, ref offset);
            this._maxVesselSpeed = DecoderHelper.FloatDecoder(bytes, ref offset);
            this._stBaseState = DecoderHelper.ST_BASE_STATEDecoder(bytes, ref offset);
            this._heading = DecoderHelper.FloatDecoder(bytes, ref offset);
            this._rot = DecoderHelper.FloatDecoder(bytes, ref offset);
            this._stPos = DecoderHelper.ST_POSDecoder(bytes, ref offset);
            this._draftFwd = DecoderHelper.FloatDecoder(bytes, ref offset);
            this._draftAft = DecoderHelper.FloatDecoder(bytes, ref offset);
            this._propellerRpm = DecoderHelper.FloatDecoder(bytes, Common.MAX_NUM_PROPELLER, ref offset);
            this._rudderAngle = DecoderHelper.FloatDecoder(bytes, Common.MAX_NUM_PROPELLER, ref offset);
            this._stVesselMotion = DecoderHelper.ST_VESSEL_MOTIONDecoder(bytes, ref offset);
            this._stVoyageInfo = DecoderHelper.ST_VOYAGE_INFODecoder(bytes, ref offset);
        }

        public ST_OWNSHIP_INFO()
        {
            _stVesselInfo = new ST_VESSEL_INFO();
            _stBaseState = new ST_BASE_STATE();
            _stPos = new ST_POS();
            _stVesselMotion = new ST_VESSEL_MOTION();
            _stVoyageInfo = new ST_VOYAGE_INFO();
        }
    }

    [Serializable]
    public class ST_OBJECT_INFO : IConvertData
    {
        private ST_VESSEL_INFO _stVesselInfo;
        public ST_VESSEL_INFO StVesselInfo
        {
            get => _stVesselInfo;
            set => _stVesselInfo = value;
        }
        private ST_BASE_STATE _stBaseState;
        public ST_BASE_STATE StBaseState
        {
            get => _stBaseState;
            set => _stBaseState = value;
        }
        private ST_POS _stPos;
        public ST_POS StPos
        {
            get => _stPos;
            set => _stPos = value;
        }
        private float[,] _variance;
        public float[,] Variance
        {
            get => _variance;
            set => _variance = value;
        }
        private uint _cameraTag;
        public uint CameraTag
        {
            get => _cameraTag;
            set => _cameraTag = value;
        }
        public void Serialize(ref byte[] bytes)
        {
            EncoderHelper.ST_VESSEL_INFO_Encoder(this._stVesselInfo, ref bytes);
            EncoderHelper.ST_BASE_STATE_Encoder(this._stBaseState, ref bytes);
            EncoderHelper.ST_POS_Encoder(this._stPos, ref bytes);
            EncoderHelper.FloatEncoder(this._variance, Common.SIZE_VARIANCE_ROW, Common.SIZE_VARIANCE_COL, ref bytes);
            EncoderHelper.UintEncoder(this._cameraTag, ref bytes);
        }
        public void Deserialize(byte[] bytes, ref int offset)
        {
            if (bytes == null) return;

            this._stVesselInfo = DecoderHelper.ST_VESSEL_INFODecoder(bytes, ref offset);
            this._stBaseState = DecoderHelper.ST_BASE_STATEDecoder(bytes, ref offset);
            this._stPos = DecoderHelper.ST_POSDecoder(bytes, ref offset);
            this._variance = DecoderHelper.FloatDecoder(bytes, Common.SIZE_VARIANCE_ROW, Common.SIZE_VARIANCE_COL, ref offset);
            this._cameraTag = DecoderHelper.UintDecoder(bytes, ref offset);
        }

        public ST_OBJECT_INFO()
        {
            _stVesselInfo = new ST_VESSEL_INFO();
            _stBaseState = new ST_BASE_STATE();
            _stPos = new ST_POS();
            _variance = new float[Common.SIZE_VARIANCE_ROW, Common.SIZE_VARIANCE_COL];
        }
    }


    #region [Collision Risk]
    [Serializable]
    public class ST_DATA_SA_OUT : IConvertData
    {
        private ST_ENVIRONMENT _stEnvironment;
        public ST_ENVIRONMENT StEnvironment
        {
            get => _stEnvironment;
            set => _stEnvironment = value;
        }

        private int _numObjectFused;
        public int NumObjectFused
        {
            get => _numObjectFused;
            set => _numObjectFused = value;
        }

        private ST_OWNSHIP_INFO _stOwnshipInfo;
        public ST_OWNSHIP_INFO StOwnshipInfo
        {
            get => _stOwnshipInfo;
            set => _stOwnshipInfo = value;
        }

        private ST_OBJECT_INFO[] _stObjectInfo;
        public ST_OBJECT_INFO[] StObjectInfo
        {
            get => _stObjectInfo;
            set => _stObjectInfo = value;
        }


        public void Serialize(ref byte[] bytes)
        {
            EncoderHelper.ST_ENVIRONMENT_Encoder(this._stEnvironment, ref bytes);
            EncoderHelper.IntEncoder(this._numObjectFused, ref bytes);
            EncoderHelper.ST_OWNSHIP_INFO_Encoder(this._stOwnshipInfo, ref bytes);
            EncoderHelper.ST_OBJECT_INFO_Encoder(this._stObjectInfo, Common.MAX_NUM_TARGET_SHIP, ref bytes);
        }
        public void Deserialize(byte[] bytes, ref int offset)
        {
            if (bytes == null) return;

            this._stEnvironment = DecoderHelper.ST_ENVIRONMENTDecoder(bytes, ref offset);
            this._numObjectFused = DecoderHelper.IntDecoder(bytes, ref offset);
            this._stOwnshipInfo = DecoderHelper.ST_OWNSHIP_STATEDecoder(bytes, ref offset);
            this._stObjectInfo = DecoderHelper.ST_OBJECT_INFODecoder(bytes, Common.MAX_NUM_TARGET_SHIP, ref offset);
        }

        public ST_DATA_SA_OUT()
        {
            this._stEnvironment = new ST_ENVIRONMENT();
            this._stOwnshipInfo = new ST_OWNSHIP_INFO();
            this._stObjectInfo = new ST_OBJECT_INFO[Common.MAX_NUM_TARGET_SHIP];
        }
    }

    // [Serializable]
    // public class ST_RISK_SIGN : IConvertData
    // {
    //
    //     private int _numAlarmLevel;
    //     public int NumAlarmLevel
    //     {
    //         get { return _numAlarmLevel; }
    //         set { _numAlarmLevel=value; }
    //     }
    //
    // }

    [Serializable]
    public class ST_CR_STATE : IConvertData
    {
        private int _encounterType;
        public int EncounterType
        {
            get => _encounterType;
            set => _encounterType = value;
        }
        private float _dcpa;
        public float Dcpa
        {
            get => _dcpa;
            set => _dcpa = value;
        }
        private float _tcpa;
        public float Tcpa
        {
            get => _tcpa;
            set => _tcpa = value;
        }
        private float _distance;
        public float Distance
        {
            get => _distance;
            set => _distance = value;
        }
        private float _bearing;
        public float Bearing
        {
            get => _bearing;
            set => _bearing = value;
        }
        private float[] _riskIndex;
        public float[] RiskIndex
        {
            get => _riskIndex;
            set => _riskIndex = value;
        }

        // private ST_RISK_SIGN _stRiskSign;
        // public ST_RISK_SIGN StRiskSign
        // {
        //     get { return _stRiskSign; }
        //     set { _stRiskSign=value; }
        // }

        public void Serialize(ref byte[] bytes)
        {
            EncoderHelper.IntEncoder(this._encounterType, ref bytes);
            EncoderHelper.FloatEncoder(this._dcpa, ref bytes);
            EncoderHelper.FloatEncoder(this._tcpa, ref bytes);
            EncoderHelper.FloatEncoder(this._distance, ref bytes);
            EncoderHelper.FloatEncoder(this._bearing, ref bytes);
            EncoderHelper.FloatEncoder(this._riskIndex, 3, ref bytes);
            // EncoderHelper.ST_RISK_SIGNEncoder(this._stRiskSign, Common.MAX_NUM_TARGET_SHIP, ref bytes);
        }
        public void Deserialize(byte[] bytes, ref int offset)
        {
            if (bytes == null) return;

            this._encounterType = DecoderHelper.IntDecoder(bytes, ref offset);
            this._dcpa = DecoderHelper.FloatDecoder(bytes, ref offset);
            this._tcpa = DecoderHelper.FloatDecoder(bytes, ref offset);
            this._distance = DecoderHelper.FloatDecoder(bytes, ref offset);
            this._bearing = DecoderHelper.FloatDecoder(bytes, ref offset);
            this._riskIndex = DecoderHelper.FloatDecoder(bytes, 3, ref offset);
            // this._stRiskSign = DecoderHelper.ST_RISK_SIGNDecoder(bytes, Common.MAX_NUM_TARGET_SHIP, ref offset);
        }

        public ST_CR_STATE()
        {
            this._riskIndex = new float[3];
            // this._stRiskSign = new ST_RISK_SIGN[Common.MAX_NUM_TARGET_SHIP];
        }
    }


    [Serializable]
    public class ST_DATA_CR_OUT : IConvertData
    {
        private ST_DATA_SA_OUT _stSaOut;
        public ST_DATA_SA_OUT StSaOut
        {
            get => _stSaOut;
            set => _stSaOut = value;
        }

        private ST_CR_STATE[] _stCrState;

        public ST_CR_STATE[] StCrState
        {
            get => _stCrState;
            set => _stCrState = value;
        }

        public void Serialize(ref byte[] bytes)
        {
            EncoderHelper.ST_DATA_SA_OUT_Encoder(this._stSaOut, ref bytes);
            EncoderHelper.ST_CR_STATE_Encoder(this._stCrState, Common.MAX_NUM_TARGET_SHIP, ref bytes);
        }
        public void Deserialize(byte[] bytes, ref int offset)
        {
            if (bytes == null) return;

            this._stSaOut = DecoderHelper.ST_SA_OUTDecoder(bytes, ref offset);
            this._stCrState = DecoderHelper.ST_CR_STATEDecoder(bytes, Common.MAX_NUM_TARGET_SHIP, ref offset);
        }

        public ST_DATA_CR_OUT()
        {
            this._stSaOut = new ST_DATA_SA_OUT();
            this._stCrState = new ST_CR_STATE[Common.MAX_NUM_TARGET_SHIP];
        }
    }
    #endregion


    [Serializable]
    public class ST_DATA_CA_OUT
    {
        private ST_DATA_CR_OUT _stDataCrOut;

        public ST_DATA_CR_OUT StDataCrOut
        {
            get => _stDataCrOut;
            set => _stDataCrOut = value;
        }
        private ST_CA_COMMAND _stCaCommand;

        public ST_CA_COMMAND StCaCommand
        {
            get => _stCaCommand;
            set => _stCaCommand = value;
        }
        public void Serialize(ref byte[] bytes)
        {
            EncoderHelper.ST_DATA_CR_OUT_Encoder(this._stDataCrOut, ref bytes);
            EncoderHelper.ST_CA_COMMAND_Encoder(this._stCaCommand, ref bytes);
        }
        public void Deserialize(byte[] bytes, ref int offset)
        {
            if (bytes == null) return;

            this._stDataCrOut = DecoderHelper.ST_DATA_CR_OUT_Decoder(bytes, ref offset);
            this._stCaCommand = DecoderHelper.ST_CA_COMMAND_Decoder(bytes, ref offset);
        }

        public ST_DATA_CA_OUT() { }
    }

    [Serializable]
    public class ST_CA_COMMAND
    {
        private float _speedDesired;
        public float SpeedDesired
        {
            get => _speedDesired;
            set => _speedDesired = value;
        }
        private float _headingDesired;
        public float HeadingDesired
        {
            get => _headingDesired;
            set => _headingDesired = value;
        }

        public void Serialize(ref byte[] bytes)
        {
            EncoderHelper.FloatEncoder(this._speedDesired, ref bytes);
            EncoderHelper.FloatEncoder(this._headingDesired, ref bytes);
        }
        public void Deserialize(byte[] bytes, ref int offset)
        {
            if (bytes == null) return;

            this._speedDesired = DecoderHelper.FloatDecoder(bytes, ref offset);
            this._headingDesired = DecoderHelper.FloatDecoder(bytes, ref offset);
        }

        public ST_CA_COMMAND()
        {

        }
    }


    [Serializable]
    public class ST_CA_COMMAND_COURSE
    {
        //private bool _followCaCom;
        //public bool FollowCaCom
        //{
        //    get => _followCaCom;
        //    set => _followCaCom = value;
        //}
        private float _headingCom;
        public float HeadingCom
        {
            get => _headingCom;
            set => _headingCom = value;
        }
        private float _speedCom;
        public float SpeedCom
        {
            get => _speedCom;
            set => _speedCom = value;
        }
        public void Serialize(ref byte[] bytes)
        {
            //EncoderHelper.BoolEncoder(this._followCaCom, ref bytes);
            EncoderHelper.FloatEncoder(this._headingCom, ref bytes);
            EncoderHelper.FloatEncoder(this._speedCom, ref bytes);
        }
        public void Deserialize(byte[] bytes, ref int offset)
        {
            if (bytes == null) return;

            //this._followCaCom = DecoderHelper.BoolDecoder(bytes, ref offset);
            this._headingCom = DecoderHelper.FloatDecoder(bytes, ref offset);
            this._speedCom = DecoderHelper.FloatDecoder(bytes, ref offset);
        }
        public ST_CA_COMMAND_COURSE()
        {

        }
    }

    [Serializable]
    public class ST_DATA_REMOTE_COMMAND
    {
        private bool _courseCommand;
        public bool CourseCommand
        {
            get => _courseCommand;
            set => _courseCommand = value;
        }
        private float _hdgCmd;
        public float HdgCmd
        {
            get => _hdgCmd;
            set => _hdgCmd = value;
        }
        private float _spdCmd;
        public float SpdCmd
        {
            get => _spdCmd;
            set => _spdCmd = value;
        }
        private float _ruddCmd;
        public float RuddCmd
        {
            get => _ruddCmd;
            set => _ruddCmd = value;
        }
        private float _rpmCmd;
        public float RpmCmd
        {
            get => _rpmCmd;
            set => _rpmCmd = value;
        }
        public void Serialize(ref byte[] bytes)
        {
            EncoderHelper.BOOLEncoder(this._courseCommand, ref bytes);
            EncoderHelper.FloatEncoder(this._hdgCmd, ref bytes);
            EncoderHelper.FloatEncoder(this._spdCmd, ref bytes);
            EncoderHelper.FloatEncoder(this._ruddCmd, ref bytes);
            EncoderHelper.FloatEncoder(this._rpmCmd, ref bytes);
        }
        public void Deserialize(byte[] bytes, ref int offset)
        {
            if (bytes == null) return;

            this._courseCommand = DecoderHelper.BOOLDecoder(bytes, ref offset);
            this._hdgCmd = DecoderHelper.FloatDecoder(bytes, ref offset);
            this._spdCmd = DecoderHelper.FloatDecoder(bytes, ref offset);
            this._ruddCmd = DecoderHelper.FloatDecoder(bytes, ref offset);
            this._rpmCmd = DecoderHelper.FloatDecoder(bytes, ref offset);
        }
    }

    [Serializable]
    public class ST_DATA_OP_TO_NM
    {
        private int _controlMode;

        public int ControlMode
        {
            get => _controlMode;
            set => _controlMode = value;
        }
        public void Serialize(ref byte[] bytes)
        {
            EncoderHelper.IntEncoder(this._controlMode, ref bytes);
        }
        public void Deserialize(byte[] bytes, ref int offset)
        {
            if (bytes == null) return;

            this._controlMode = DecoderHelper.IntDecoder(bytes, ref offset);
        }
        public ST_DATA_OP_TO_NM()
        {

        }
    }

    [Serializable]
    public class ST_DATA_AUTO_PILOT
    {
        private float _propellerRpm;

        public float PropellerRpm
        {
            get => _propellerRpm;
            set => _propellerRpm = value;
        }

        private float _rudderAngle;

        public float RudderAngle
        {
            get => _rudderAngle;
            set => _rudderAngle = value;
        }
        public void Serialize(ref byte[] bytes)
        {
            EncoderHelper.FloatEncoder(this._propellerRpm, ref bytes);
            EncoderHelper.FloatEncoder(this._rudderAngle, ref bytes);
        }
        public void Deserialize(byte[] bytes, ref int offset)
        {
            if (bytes == null) return;

            this._propellerRpm = DecoderHelper.FloatDecoder(bytes, ref offset);
            this._rudderAngle = DecoderHelper.FloatDecoder(bytes, ref offset);
        }
        public ST_DATA_AUTO_PILOT()
        {

        }
    }

    [Serializable]
    public class ST_DATA_NM_OUT
    {
        private ST_DATA_CA_OUT _stCaData;

        public ST_DATA_CA_OUT StCaData
        {
            get => _stCaData;
            set => _stCaData = value;
        }
        private ST_DATA_OP_TO_NM _stOpData;

        public ST_DATA_OP_TO_NM StOpData
        {
            get => _stOpData;
            set => _stOpData = value;
        }
        private ST_DATA_AUTO_PILOT _stDataAutoPilot;

        public ST_DATA_AUTO_PILOT StDataAutoPilot
        {
            get => _stDataAutoPilot;
            set => _stDataAutoPilot = value;
        }
        public void Serialize(ref byte[] bytes)
        {
            EncoderHelper.ST_DATA_CA_OUT_Encoder(this._stCaData, ref bytes);
            EncoderHelper.ST_DATA_OP_TO_NM_Encoder(this._stOpData, ref bytes);
            EncoderHelper.ST_DATA_AUTO_PILOT_Encoder(this._stDataAutoPilot, ref bytes);
        }
        public void Deserialize(byte[] bytes, ref int offset)
        {
            if (bytes == null) return;

            this._stCaData = DecoderHelper.ST_DATA_CA_OUT_Decoder(bytes, ref offset);
            this._stOpData = DecoderHelper.ST_DATA_OP_TO_NM_Decoder(bytes, ref offset);
            this._stDataAutoPilot = DecoderHelper.ST_DATA_AUTO_PILOT_Decoder(bytes, ref offset);
        }

        public ST_DATA_NM_OUT()
        {

        }
    }





}
