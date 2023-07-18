using System;
using System.Collections.Generic;
using System.Waf.Foundation;

namespace SyDLab.Usv.Simulator.Domain.Models.Platforms
{
    public class VesselItemService
    {
        public static readonly string[] VesselList =
        {
            "KVLCC2",
            "DANV",
            "174K LNGC"
        };
    };

    
    public class ShipCoefficient : Model
    {
        private double _xudot = -0.000954;
    
        public double Xudot
        {
            get => _xudot;
            set => SetProperty(ref _xudot, value);
        }
    
        private double _xuu = -0.00041947275;
    
        public double Xuu
        {
            get => _xuu;
            set => SetProperty(ref _xuu, value);
        }
    
        private double _xvv = 0.001688;
    
        public double Xvv
        {
            get => _xvv;
            set => SetProperty(ref _xvv, value);
        }
    
        private double _xrr = 0.001219;
    
        public double Xrr
        {
            get => _xrr;
            set => SetProperty(ref _xrr, value);
        }
    
        private double _xvr = 0.013821;
    
        public double Xvr
        {
            get => _xvr;
            set => SetProperty(ref _xvr, value);
        }
    
        private double _xdel;
    
        public double Xdel
        {
            get => _xdel;
            set => SetProperty(ref _xdel, value);
        }
    
        private double _xdd;
    
        public double Xdd
        {
            get => _xdd;
            set => SetProperty(ref _xdd, value);
        }
    
        private double _yvdot = -0.015104;
    
        public double Yvdot
        {
            get => _yvdot;
            set => SetProperty(ref _yvdot, value);
        }
    
        private double _yrdot = -0.001428;
    
        public double Yrdot
        {
            get => _yrdot;
            set => SetProperty(ref _yrdot, value);
        }
    
        private double _yv = -0.016190;
    
        public double Yv
        {
            get => _yv;
            set => SetProperty(ref _yv, value);
        }
    
        private double _yvv = -0.030441;
    
        public double Yvv
        {
            get => _yvv;
            set => SetProperty(ref _yvv, value);
        }
    
        private double _yr = 0.004720;
    
        public double Yr
        {
            get => _yr;
            set => SetProperty(ref _yr, value);
        }
    
        private double _yrr = 0.000205;
    
        public double Yrr
        {
            get => _yrr;
            set => SetProperty(ref _yrr, value);
        }
    
        private double _yvrr = -0.022999;
    
        public double Yvrr
        {
            get => _yvrr;
            set => SetProperty(ref _yvrr, value);
        }
    
        private double _yrvv = 0.021775;
    
        public double Yrvv
        {
            get => _yrvv;
            set => SetProperty(ref _yrvv, value);
        }
    
        private double _ydel = 0.003706;
    
        public double Ydel
        {
            get => _ydel;
            set => SetProperty(ref _ydel, value);
        }
    
        private double _nvdot = -0.000785;
    
        public double Nvdot
        {
            get => _nvdot;
            set => SetProperty(ref _nvdot, value);
        }
    
        private double _nrdot = -0.000800;
    
        public double Nrdot
        {
            get => _nrdot;
            set => SetProperty(ref _nrdot, value);
        }
    
        private double _nv = -0.008754;
    
        public double Nv
        {
            get => _nv;
            set => SetProperty(ref _nv, value);
        }
    
        private double _nvv = 0.001250;
    
        public double Nvv
        {
            get => _nvv;
            set => SetProperty(ref _nvv, value);
        }
    
        private double _nr = -0.003115;
    
        public double Nr
        {
            get => _nr;
            set => SetProperty(ref _nr, value);
        }
    
        private double _nrr = 0.000038;
    
        public double Nrr
        {
            get => _nrr;
            set => SetProperty(ref _nrr, value);
        }
    
        private double _nvrr = 0.001063;
    
        public double Nvrr
        {
            get => _nvrr;
            set => SetProperty(ref _nvrr, value);
        }
    
        private double _nrvv = -0.017563;
    
        public double Nrvv
        {
            get => _nrvv;
            set => SetProperty(ref _nrvv, value);
        }
    
        private double _ndel = -0.00166365;
    
        public double Ndel
        {
            get => _ndel;
            set => SetProperty(ref _ndel, value);
        }
    }



    #region Hull parameters
    /// Hull parameter
    public class HullParameter
    {

        #region Public variables
        public double Lpp;
        public double Breadth;
        public double Draft;
        public double Displacement;
        public double xG;
        public double Cb;
        public int NumRudder;
        public double DP;
        public double HR; // rudder span
        public double AR; // rudder area

        public double Xuu;
        public double tP;
        public double tR;
        public double aH;
        public double xH;

        public double C1;
        public double C2p;
        public double C2n;
        public double gammaRp;
        public double gammaRn;
        public double lR;
        public double epsilon;
        public double kappa;
        public double f_alpha;
        public double k0;
        public double k1;
        public double k2;
        public double wP0;

        public double rho;
        #endregion

        private List<string> _namesList = new List<string>(VesselItemService.VesselList);

        public HullParameter(string modelType)
        {
            switch (_namesList.IndexOf(modelType))
            {
                case 0:     // KVLCC2
                    {
                        Lpp = 320;
                        Breadth = 58;
                        Draft = 20.8;
                        Displacement = 312600;
                        xG = 11.2;
                        Cb = 0.810;
                        NumRudder = 1;
                        DP = 9.86;
                        HR = 15.80;
                        AR = 112.5;

                        Xuu = -0.00014;        // to be checked
                        tP = 0.220;
                        tR = 0.387;
                        aH = 0.312;
                        xH = -0.464;

                        C1 = 2.0;
                        C2p = 1.6;
                        C2n = 1.1;
                        gammaRp = 0.640;
                        gammaRn = 0.395;
                        lR = -0.710;
                        epsilon = 1.09;
                        kappa = 0.50;
                        f_alpha = 2.747;
                        k0 = 0.482;     // to be checked
                        k1 = -4.0;      // to be checked
                        k2 = 0;         // to be checked
                        wP0 = 0.35;     // to be checked

                        rho = 1.025;
                    }
                    break;
                case 1:     // DANV
                    {
                        Lpp = 9.947;
                        Breadth = 3.32;
                        Draft = 0.55;
                        Displacement = 8.456;
                        xG = 0;
                        Cb = 0.465556;
                        NumRudder = 1;
                        DP = 0.56;
                        HR = 0.68; // rudder span
                        AR = 0.37; // rudder area

                        Xuu = -0.00014;
                        tP = 0.18;
                        tR = 0.3;
                        aH = 0.312;
                        xH = -0.464;

                        C1 = 2;
                        C2p = 1.6;
                        C2n = 1.1;
                        gammaRp = 0.506;
                        gammaRn = 0.506;
                        lR = -0.71;
                        epsilon = 1.09;
                        kappa = 0.425;
                        f_alpha = 2.425;
                        k0 = 0.482;
                        k1 = -0.419;
                        k2 = 0;
                        wP0 = 0.35;

                        rho = 1.025;
                    }
                    break;
                case 2:     // 173K_LNGC
                    {
                        Lpp = 283.5;
                        Breadth = 46.4;
                        Draft = 11.7;
                        Displacement = 119034;
                        xG = -2.75;
                        Cb = 0.7734;
                        NumRudder = 2;
                        DP = 8.3;
                        HR = 7.7; // rudder span
                        AR = 40.15; // rudder area

                        Xuu = -0.00014;
                        tP = 0.18;
                        tR = 0.3;
                        aH = 0.312;
                        xH = -0.464;

                        C1 = 2;
                        C2p = 1.6;
                        C2n = 1.1;
                        gammaRp = 0.506;
                        gammaRn = 0.506;
                        lR = -0.71;
                        epsilon = 1.09;
                        kappa = 0.425;
                        f_alpha = 2.425;
                        k0 = 0.482;
                        k1 = -4.0;
                        k2 = 0;
                        wP0 = 0.35;

                        rho = 1.025;
                    }
                    break;
            }
        }
    }
    #endregion


    #region Kijima model
    public class KijimaModel : Model
    {
        private HullParameter hullParam;

        #region variables
        private double _L;
        private double _B;
        private double _d;
        private double _Cb;
        private double _k;
        private double _m;

        private double _mx;
        private double _my;
        private double _Jz;
        private double _Xuu;
        private double _Xvr;
        private double _Yv;
        private double _Yr;
        private double _Yvv;
        private double _Yrr;
        private double _Yvrr;
        private double _Yvvr;
        private double _Nv;
        private double _Nr;
        private double _Nvv;
        private double _Nrr;
        private double _Nvrr;
        private double _Nvvr;

        public double mx => _mx;
        public double my => _my;
        public double Jz => _Jz;
        public double Xuu => _Xuu;
        public double Xvr => _Xvr;
        public double Yv => _Yv;
        public double Yr => _Yr;
        public double Yvv => _Yvv;
        public double Yrr => _Yrr;
        public double Yvrr => _Yvrr;
        public double Yvvr => _Yvvr;
        public double Nv => _Nv;
        public double Nr => _Nr;
        public double Nvv => _Nvv;
        public double Nrr => _Nrr;
        public double Nvrr => _Nvrr;
        public double Nvvr => _Nvvr;
        #endregion


        public KijimaModel(PlatformBase ship)
        {
            hullParam = new HullParameter(ship.ModelType);

            _L = hullParam.Lpp;
            _B = hullParam.Breadth;
            _d = hullParam.Draft;
            _Cb = hullParam.Cb;
            _k = 2 * _d / _L;
            _m = hullParam.rho * _L * _B * _d * _Cb;

            _mx = 2.7 * hullParam.rho * Math.Pow(_Cb * _L * _B * _d, 5 / 3) / (_L * _L);
            _my = Math.PI / 2 * hullParam.rho * _L * _d * _d * (1 + 0.16 * _Cb * _B / _d - 5.1 / Math.Pow(_L / _B, 2));
            _Jz = Math.PI / 24 * hullParam.rho * Math.Pow(_L, 3) * _d * _d * (1 + 0.2 * _Cb * _B / _d - 4 / (_L / _B));
            _Xuu = hullParam.Xuu;
            _Xvr = -0.4 * _my; //dimensional value
            _Yv = -(0.5 * _k * Math.PI + 1.4 * _Cb * _B / _L);
            _Yr = -1.5 * _Cb * _B / _L + (_m + _mx) / (0.5 * hullParam.rho * _L * _L * _L);
            _Yvv = -2.5 * _d * (1 - _Cb) / _B - 0.5;
            _Yrr = 0.343 * _d * _Cb / _B - 0.07;
            _Yvrr = -5.95 * _d * (1 - _Cb) / _B;
            _Yvvr = 1.5 * _d * _Cb / _B - 0.65;
            _Nv = -_k;
            _Nr = -0.54 * _k + _k * _k;
            _Nvv = 0.96 * _d * (1 - _Cb) / _B - 0.066;
            _Nrr = 0.5 * _Cb * _B / _L - 0.09;
            _Nvrr = 0.5 * _d * _Cb / _B - 0.05;
            _Nvvr = -57.5 * Math.Pow(_Cb * _B / _L, 2) + 18.4 * _Cb * _B / _L - 1.6;
        }
    }

    #endregion


    #region POW
    public class PropellerOpenWater
    {
        public double[,] PowCurve;

        private List<string> _namesList = new List<string>(VesselItemService.VesselList);

        public PropellerOpenWater(string modelType)
        {
            switch (_namesList.IndexOf(modelType))
            {
                case 0:     // KVLCC2 - MOERI
                    {
                        PowCurve = new double[4, 17]
                        {
                        {
                            0, 0.05, 0.1, 0.15, 0.2, 0.25, 0.3, 0.35, 0.4, 0.45, 0.5, 0.55, 0.6, 0.65, 0.7, 0.75, 0.8
                        },
                        {
                            0.3183, 0.3014, 0.2843, 0.267, 0.2493, 0.2315, 0.2132, 0.1947, 0.1757, 0.1563, 0.1365, 0.1161, 0.0951, 0.0735, 0.0511, 0.028, 0.004
                        },
                        {
                            0.0311, 0.03032, 0.02932, 0.02814, 0.02682, 0.02539, 0.02388, 0.0223, 0.02067, 0.01897, 0.01721, 0.01538, 0.01344, 0.01138, 0.00915, 0.00671, 0.00402
                        },
                        {
                            0, 0.0791, 0.1543, 0.2265, 0.2959, 0.3628, 0.4263, 0.4864, 0.5411, 0.5901, 0.6312, 0.6608, 0.6757, 0.6682, 0.6222, 0.4981, 0.1267
                        }
                        };
                    }
                    break;
                case 1:     // DANV
                    {
                        // ToDo: 임시로 174K LNGC데이터 사용. 추후 업데이트
                        PowCurve = new double[4, 22]
                        {
                            {
                                0, 0.05, 0.1, 0.15, 0.2, 0.25, 0.3, 0.35, 0.4, 0.45, 0.5, 0.55, 0.6, 0.65, 0.7, 0.75, 0.8, 0.85, 0.9,
                                0.95, 1.0, 1.05
                            },
                            {
                                0.4093, 0.3994, 0.3876, 0.3774, 0.3601, 0.3448, 0.3291, 0.313, 0.2966, 0.28, 0.2633, 0.2465, 0.2295,
                                0.2122, 0.1944, 0.1759, 0.1563, 0.1354, 0.1128, 0.0881, 0.0608, 0.0303
                            },
                            {
                                0.0533, 0.0519, 0.0505, 0.0491, 0.0476, 0.0461, 0.0446, 0.0431, 0.0415, 0.04, 0.0384, 0.0368, 0.0351,
                                0.0332, 0.0312, 0.0290, 0.0265, 0.0237, 0.0206, 0.0170, 0.0129, 0.0083
                            },
                            {
                                0.4, 0.4, 0.4, 0.4, 0.4, 0.4, 0.4, 0.4, 0.4, 0.45, 0.5, 0.55, 0.6, 0.65, 0.7, 0.75, 0.768, 0.772, 0.77,
                                0.76, 0.75, 0.74
                            }
                        };
                    }
                    break;
                case 2: // 173K_LNGC
                    {
                        PowCurve = new double[4, 22]
                        {
                        {
                            0, 0.05, 0.1, 0.15, 0.2, 0.25, 0.3, 0.35, 0.4, 0.45, 0.5, 0.55, 0.6, 0.65, 0.7, 0.75, 0.8, 0.85, 0.9,
                            0.95, 1.0, 1.05
                        },
                        {
                            0.4093, 0.3994, 0.3876, 0.3774, 0.3601, 0.3448, 0.3291, 0.313, 0.2966, 0.28, 0.2633, 0.2465, 0.2295,
                            0.2122, 0.1944, 0.1759, 0.1563, 0.1354, 0.1128, 0.0881, 0.0608, 0.0303
                        },
                        {
                            0.0533, 0.0519, 0.0505, 0.0491, 0.0476, 0.0461, 0.0446, 0.0431, 0.0415, 0.04, 0.0384, 0.0368, 0.0351,
                            0.0332, 0.0312, 0.0290, 0.0265, 0.0237, 0.0206, 0.0170, 0.0129, 0.0083
                        },
                        {
                            0.4, 0.4, 0.4, 0.4, 0.4, 0.4, 0.4, 0.4, 0.4, 0.45, 0.5, 0.55, 0.6, 0.65, 0.7, 0.75, 0.768, 0.772, 0.77,
                            0.76, 0.75, 0.74
                        }
                        };
                    }
                    break;
            }
        }
    }

    #endregion


    #region Hydrodynamic derivatives

    public class HydrodynamicDerivatives
    {
        public double Xvv; //Xw
        public double Xvr; //Xvr
        public double Xrr; //Xrr
        public double Xvvvv; //Xvvvv
        public double Yv; //Yv
        public double Yr; //Yr
        public double Yvvv; //Yvvv
        public double Yvvr; //Yvvr
        public double Yvrr; //Yvrr
        public double Yrrr; //Yrrr

        public double Nv; //Nv
        public double Nr; //Nr
        public double Nvvv; //Nvvv
        public double Nvvr; //Nvvr
        public double Nvrr; //Nvrr
        public double Nrrr; //Nrrr
        public double mx; //mx (added mass)
        public double my; //my (added mass)
        public double Jz; //Jz (added mass moment of inertia)
        public double tp; //tp (thrust deduction factor)

        public double tR; //tR (steering resistance deduction factor)
        public double aH; //aH (rudder force increase factor)
        public double xH; //xH (longitudinal position of acting point of the additional lateral force)
        public double C1; //C1 (experimental constants representing wake characteristics in manoeuvring)

        public double C2p; //C2 (beta_p >0) (experimental constants representing wake characteristics in manoeuvring)

        public double C2n; //C2 (beta_p <0) (experimental constants representing wake characteristics in manoeuvring)

        public double gammaRp; //gammaR (beta_R >0) (flow straightening coefficient)
        public double gammaRn; //gammaR (beta_R <0)
        public double lR; //lR (effective longitudinal position of rudder position in formula of beta_R)

        public double epsilon; //epsilon (ratio of wake fraction at propeller and rudder position = (1-wR)/(1-wP))

        public double kappa; //kappa (experimental constant for expressing uR)
        public double f_alpha; //f_alpha (rudder lift gradient coefficient)
        public double k0; //k0 (Coefficient representing KT as a 2nd order polynomial)
        public double k1; //k1 (Coefficient representing KT as a 2nd order polynomial)
        public double k2; //k2 (Coefficient representing KT as a 2nd order polynomial)
        public double wP0; //wP0 (wake coefficient at propeller position in manoeuvring motions)


        private List<string> _namesList = new List<string>(VesselItemService.VesselList);

        public HydrodynamicDerivatives(string modelType)
        {
            switch (_namesList.IndexOf(modelType))
            {
                case 0:
                    Xvv = -0.04;
                    Xvr = 0.002;
                    Xrr = 0.011;
                    Xvvvv = 0.771;
                    Yv = -0.315;
                    Yr = 0.083;
                    Yvvv = -1.607;
                    Yvvr = 0.379;
                    Yvrr = -.391;
                    Yrrr = 0.008;

                    Nv = -0.137;
                    Nr = -0.049;
                    Nvvv = -0.03;
                    Nvvr = -0.294;
                    Nvrr = 0.055;
                    Nrrr = -0.013;
                    mx = 0.022;
                    my = 0.223;
                    Jz = 0.011;
                    tp = 0.22;

                    tR = 0.387;
                    aH = 0.312;
                    xH = -0.464;
                    C1 = 2;
                    C2p = 1.6;
                    C2n = 1.1;

                    gammaRp = 0.395;
                    gammaRn = 0.64;
                    lR = -0.71;

                    epsilon = 1.09;

                    kappa = 0.5;
                    f_alpha = 2.747;
                    k0 = 0.2931;
                    k1 = -0.2753;
                    k2 = -0.1385;
                    wP0 = 0.35;
                    break;
                case 1:

                    break;
                case 2:

                    break;
            }
        }
    }
    #endregion


    #region Resistance Coefficient
    public class ResistanceCoefficient
    {
        public double R0;

        private List<string> _namesList = new List<string>(VesselItemService.VesselList);

        public ResistanceCoefficient(string modelType)
        {
            switch (_namesList.IndexOf(modelType))
            {
                case 0:
                    R0 = 0.022;
                    break;
                case 1:

                    break;
                case 2:

                    break;
            }
        }
    }

    #endregion


    #region Speed vs. Rpm curve
    public class SpeedRpm
    {
        public double[,] PowSpeedCurve;
        private List<string> _nameList = new List<string>(VesselItemService.VesselList);

        public SpeedRpm(string modelType)
        {
            switch (_nameList.IndexOf(modelType))
            {
                case 0:
                {
                    PowSpeedCurve = new double[2, 17]       // knots vs rpm
                    {
                        {
                            10, 10.5, 11, 11.5, 12, 12.5, 13, 13.5, 14, 14.5, 15, 15.5, 16, 16.5, 17, 17.5, 18
                        },
                        {
                            47.283, 49.515, 51.741, 53.964, 56.185, 58.408, 60.639, 62.882, 65.145, 67.437, 69.769, 72.172, 74.669, 77.248, 79.923, 82.711, 85.627
                        }
                    };
                }
                    break;
                case 1:
                {
                    PowSpeedCurve = new double[2, 7]        // knots vs rpm
                    {
                        {
                            0.33, 4.95, 6.60, 8.25, 9.90, 11.55, 12.7
                        },
                        { 
                            1000, 1500, 2000, 2500, 3000, 3500, 3850
                        }
                    };
                }
                    break;
                case 2:
                {

                }
                    break;
            }
        }
    }
    #endregion


    #region Propeller Performance Curve
    public class PropellerPerformance
    {
        public double A = 499.42;
        public double B = -4462.9;
        public double C = 12525;

        private List<string> _namesList = new List<string>(VesselItemService.VesselList);

        public PropellerPerformance(string modelType)
        {
            switch (_namesList.IndexOf(modelType))
            {
                case 0:
                    //ToDo: 174K LNGC 데이터임. KVLCC2에 맞게 업데이트 필요
                    A = 499.42;
                    B = -4462.9;
                    C = 12525;
                    break;
                case 1:

                    break;
                case 2:
                    A = 499.42;
                    B = -4462.9;
                    C = 12525;
                    break;
            }
        }
    }
    #endregion


    #region Components properties

    public class ComponentProperty
    {
        public double DriveTrainInertia;
        public double MaxRpm;
        public double MinRpm;

        private List<string> namesList = new List<string>(VesselItemService.VesselList);

        public ComponentProperty(string model)
        {
            switch (namesList.IndexOf(model))
            {
                case 0:
                    DriveTrainInertia = 1.5e3;
                    MaxRpm = 85;
                    MinRpm = 45;
                    break;
                case 1:
                    DriveTrainInertia = 0.1;    // ToDo: to be updated
                    MaxRpm = 3850;
                    MinRpm = 100;
                    break;
                case 2:
                    DriveTrainInertia = 1e3;
                    MaxRpm = 80;
                    MinRpm = 30;
                    break;
            }
        }
    }

    #endregion
}