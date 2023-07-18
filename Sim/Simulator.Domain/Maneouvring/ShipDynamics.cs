using System;
using System.IO;
using System.Waf.Foundation;
using System.Windows.Media.Media3D;
using MathNet.Numerics.Differentiation;
using MathNet.Numerics.LinearAlgebra.Double;
using SyDLab.Usv.Simulator.Domain.Utils;

namespace SyDLab.Usv.Simulator.Domain.Models.Platforms
{
    public class ShipDynamics : Model
    {
        private static HullParameter _hull;
        private static KijimaModel _kijima;
        private static HydrodynamicDerivatives _deriv;
        private static ResistanceCoefficient _resistCoeff;
        private static ComponentProperty _property;
        public ShipDynamics()
        {
        }

        public static double[] RudderDynamics(MyShip ship, double action, double dt)
        {
            var rudder = ship.Rudder;
            var delSat = ship.LimitOfRudderAngle * Math.PI / 180;
            var detSatDot = ship.LimitOfRudderAngleDot * Math.PI / 180;
            action = clip(action, -delSat, delSat);
            // action = delSat;
            double del_dot = (action - rudder[0]) / dt;
            del_dot = clip(del_dot, -detSatDot, detSatDot);
            rudder[0] += del_dot * dt;
            rudder[1] = del_dot;

            return rudder;
        }

        public static void CalculateDynamics(MyShip ship, double rudderAngle, double dt)
        {
            
            double izz = ship.Inertia;
            double[] velocity = new[] { ship.Velocity.X *0.5144, ship.Velocity.Y * 0.5144, ship.AngularVelocity * Math.PI / 180 };
            double[] position = new[] { ship.CurrentX, ship.CurrentY };
            double heading = (90 - ship.Heading) * Math.PI / 180;


            double[] input = new[] { velocity[0], velocity[1], velocity[2], heading };
            double u = input[0];
            double v = input[1];
            double r = input[2];

            double U = Math.Sqrt(u * u + v * v); //Ship Speed

            double[] coefficients = RealModel(u, v, r, U, rudderAngle, ship);
            double Xhyd = coefficients[0];
            double Yhyd = coefficients[1];
            double Nhyd = coefficients[2];
            double Xrudder = coefficients[3];
            double Yrudder = coefficients[4];
            double Nrudder = coefficients[5];
            double Xudot = coefficients[6];
            double Yvdot = coefficients[7];
            double Yrdot = coefficients[8];
            double Nvdot = coefficients[9];
            double Nrdot = coefficients[10];


            //jacobian을 통한 운동방정식 solver
            NumericalJacobian jacobian = new NumericalJacobian();
            DenseMatrix H = new DenseMatrix(4, 4);
            H[0, 0] = ship.Mass - Xudot;
            H[1, 1] = ship.Mass - Yvdot;
            H[1, 2] = ship.Mass * ship.CogX - Yrdot;
            H[2, 1] = ship.Mass * ship.CogX - Nvdot;
            H[2, 2] = izz - Nrdot;
            H[3, 3] = 1;

            DenseMatrix f = new DenseMatrix(4, 1);
            f[0, 0] = Xhyd + ship.Mass * (v * r + ship.CogX * r * r) + Xrudder + 0.2 * 1024 * 5 * 5 * 5 * 5 * 2;
            f[1, 0] = Yhyd - ship.Mass * u * r + Yrudder;
            f[2, 0] = Nhyd - ship.Mass * ship.CogX * u * r + Nrudder;
            f[3, 0] = r;

            var output = H.LU().Solve(f) as DenseMatrix;
            velocity[0] = velocity[0] + output[0, 0] * dt;
            velocity[1] = velocity[1] + output[1, 0] * dt;
            velocity[2] = velocity[2] + output[2, 0] * dt;
            heading = heading + velocity[2] * dt;

            double[] XYvel = new double[2];
            XYvel[0] = velocity[0] * Math.Cos(heading) - velocity[1] * Math.Sin(heading);
            XYvel[1] = velocity[0] * Math.Sin(heading) + velocity[1] * Math.Cos(heading);

            position[0] += XYvel[0] * dt;
            position[1] += XYvel[1] * dt;

            ship.CurrentX = position[0];
            ship.CurrentY = position[1];
            ship.CurrentHeading = (Math.PI / 2 - heading) * 180 / Math.PI;
            ship.Velocity = new Vector3D(velocity[0] / 0.5144, velocity[1] / 0.5144, 0);
            ship.CurrentSpeed = Math.Sqrt(velocity[0] * velocity[0] + velocity[1] * velocity[1]) / 0.5144;
            ship.AngularVelocity = velocity[2] * 180 / Math.PI;

            // StreamWriter writer = File.AppendText("D://newPath.csv");
            // writer.WriteLine(ship.CurrentX + "," + ship.CurrentY);
            // writer.Close();
        }

        #region [MMG - Existing version]
        public static double[] MotionEquation(PlatformBase ship, double t, double[] state)
        {
            _hull = new HullParameter(ship.ModelType);
            _kijima = new KijimaModel(ship);
            _property = new ComponentProperty(ship.ModelType);

            double[] output = new double[10];

            //double x = state[0];
            //double y = state[1];
            double psi = state[2];      //(rad)
            double u = state[3];
            double v = state[4];
            double r = state[5];        //(rad/s)
            double nP1 = state[6];      //(rev/s)
            double nP2 = state[7];      //(rev/s)
            double I = _property.DriveTrainInertia; //_1e3; //Mass Moment of Inertia of the Drive Train
            double delta1 = state[8];   //(deg)
            double delta2 = state[9];   //(deg)


            double deltaCommand = ship.RudderDesired;  // (rad)
            double rpmCommand = ship.RpmDesired / 60;    // (rev/s)


            // double Eta_S = 0.98;    //shaft efficiency
            // double Q_E1 = (ship as MyShip).ShaftPower[0] * Eta_S / (2 * Math.PI * nP1);
            // double Q_E2 = (ship as MyShip).ShaftPower[1] * Eta_S / (2 * Math.PI * nP2);
            // double Tn = 0.1;
            // double Td = 2; //0.5;
            double TdProp = 1.2; //0.5;
            double TdRudd = 1.5;

            double L = _hull.Lpp;
            double V = _hull.Displacement;
            double xG = _hull.xG;
            double rho = _hull.rho;
            // double U = Math.Sqrt(u * u + v * v);

            double m = rho * V;
            double rZ = L * 0.23;//Test: yaw radius of gyration
            double Iz = m * rZ * rZ;
            double mx = _kijima.mx;
            double my = _kijima.my;
            double Jz = 40000; //_kijima.Jz; //Yaw added mass moment of inertia
            // double mx = _deriv.mx;
            // double my = _deriv.my;
            // double Jz = _deriv.Jz; //Yaw added mass moment of inertia

            double XH = HydrodynamicForceX(ship, u, v, r);
            double YH = HydrodynamicForceY(ship, u, v, r);
            double NH = HydrodynamicForceN(ship, u, v, r);

            double[] Prop1 = PropulsionForce(ship, u, v, r, nP1);
            double[] Prop2 = new double[3] { 0, 0, 0 };
            if (_hull.NumRudder > 1)
                Prop2 = PropulsionForce(ship, u, v, r, nP2);

            double[] PropDesired = PropulsionForce(ship, u, v, r, rpmCommand);

            
            double DP = _hull.DP;
            double PropTorque1, PropTorque2;
            double PropTorqueDesired;
            PropTorque1 = rho * Math.Pow(nP1, 2) * Math.Pow(DP, 5) * Prop1[1];
            PropTorque2 = rho * Math.Pow(nP2, 2) * Math.Pow(DP, 5) * Prop2[1];
            PropTorqueDesired = rho * Math.Pow(rpmCommand, 2) * Math.Pow(DP, 5) * PropDesired[1];
            // double PropPower1 = 2 * Math.PI * nP1 * PropTorque1;
            // double PropPower2 = 2 * Math.PI * nP2 * PropTorque2;
            // double PropPowerDesired = 2 * Math.PI * rpmCommand * PropTorqueDesired;

            var Fr1 = CalculRudderForce(ship, u, v, r, nP1, delta1);
            double[] Fr2 = new double[3] { 0, 0, 0 };
            if (_hull.NumRudder > 1)
                Fr2 = CalculRudderForce(ship, u, v, r, nP2, delta2);
            ////m_cWind.CalculWindForce(u, v, psi * R2D, Fwind);
            ////m_cCurrent.CalculCurrentForce(u, v, psi * R2D, Fc);
            ////m_cWave.DriftForce(psi * R2D, t, F_tmp);

            double X = XH + Fr1[0] + Fr2[0] + Prop1[0] + Prop2[0]; //+ Fwind[0] + Fc[0] + Fdrift[0];
            double Y = YH + Fr1[1] + Fr2[1]; //+ Fwind[1] + Fc[1] + Fdrift[1];
            double N = NH + Fr1[2] + Fr2[2]; //+ Fwind[2] + Fc[2] + Fdrift[2];
            double Xvr = _kijima.Xvr;

            output[0] = u * Math.Cos(psi) - v * Math.Sin(psi);
            output[1] = u * Math.Sin(psi) + v * Math.Cos(psi);
            output[2] = r;
            output[3] = 1 / (m + mx) * (X + (my + Xvr) * v * r);
            output[4] = 1 / (m + my) * (Y - mx * u * r);
            output[5] = 1 / (Iz + Jz) * (N + xG * Y);
            // //output[6] = -1 / I * (PropTorque1 - PropTorqueDesired) / Td;
            // //output[7] = -1 / I * (PropTorque2 - PropTorqueDesired) / Td;
            // ////	out[7] = 1/I*(Q_E2 - m_PropellerTorque);
            // double[] rpmDot = new double[2];
            // rpmDot[0] = -1 / I * (PropTorque1 - PropTorqueDesired) / Td;
            // rpmDot[1] = -1 / I * (PropTorque2 - PropTorqueDesired) / Td;
            // var v1 = Math.Min(Math.Abs(rpmDot[0]), 2);
            // var v2 = Math.Min(Math.Abs(rpmDot[1]), 2);
            // output[6] = v1 * Math.Sign(rpmDot[0]);
            // output[7] = v2 * Math.Sign(rpmDot[1]);
            double dRpm0 = -1 * (nP1 - rpmCommand) / TdProp;
            double dRpm1 = -1 * (nP2 - rpmCommand) / TdProp;
            double dRpm0_ = Math.Min(Math.Abs(dRpm0), 0.5);
            double dRpm1_ = Math.Min(Math.Abs(dRpm1), 0.5);
            output[6] = (dRpm0 > 0 ? 1 : -1) * dRpm0_;
            output[7] = (dRpm1 > 0 ? 1 : -1) * dRpm1_;

            double deltaDot = -1 / TdRudd * (delta1 - deltaCommand);
            double deltaDotLimit = Distance.Degree2Radian((ship as MyShip).LimitOfRudderAngleDot);
            if (Math.Abs(deltaDot) > deltaDotLimit)
                deltaDot = Math.Sign(deltaDot) * deltaDotLimit;

            output[8] = deltaDot;
            output[9] = deltaDot;

            return output;
        }

        private static double HydrodynamicForceX(PlatformBase ship, double u, double v, double r)
        {
            //Reference: Introduction of MMG Standard Method for Ship Manuevering Predictions (Yasukawa, 2014)
            double L = _hull.Lpp;
            double T = _hull.Draft;
            // double V = _hull.Displacement;
            double rho = _hull.rho;
            double U = Math.Sqrt(u * u + v * v);
            // double _v = v / U;
            // double _r = r * L / U;

            // double LWL = 1.02 * L; //Case of Tankers or Bulk Carriers	
            //	double S=0.99*(V/T+1.9*LWL*T);	//Case of Tanker or Bulk Carrier
            //	double _R = HullResistance(0,U)/(0.5*rho*L*T*U*U);
            double Xuu = _kijima.Xuu;
            // double my = _kijima.my;
            // double Xvr = _kijima.Xvr;

            //	X = Xuu*u*u*(0.5*rho*L*T*U*U) + Xvr*v*r;	//To be Checked!!
            double X = Xuu * u * u * (0.5 * rho * L * T * U * U);// + Xvr*v*r;	//To be Checked!!
            //	X = -HullResistance(0,U) + Xvr*v*r;
            //	X *= (0.5*rho*L*T*U*U);

            return X;
        }

        private static double HydrodynamicForceY(PlatformBase ship, double u, double v, double r)
        {
            //Reference: Introduction of MMG Standard Method for Ship Manuevering Predictions (Yasukawa, 2014)
            double Y;
            double L = _hull.Lpp;
            double T = _hull.Draft;
            double rho = _hull.rho;   //1.025;
            double U = Math.Sqrt(u * u + v * v);
            double _v = v / U;
            double _r = r * L / U;

            //Nondimensional derivatives
            double Yv = _kijima.Yv;
            double Yr = _kijima.Yr;
            double Yvv = _kijima.Yvv;
            double Yrr = _kijima.Yrr;
            double Yvrr = _kijima.Yvrr;
            double Yvvr = _kijima.Yvvr;

            Y = Yv * _v + Yr * _r + Yvv * _v * Math.Abs(_v) + Yrr * _r * Math.Abs(_r) + Yvrr * _v * _r * _r + Yvvr * _v * _v * _r;
            Y *= (0.5 * rho * L * T * U * U);

            return Y;
        }

        private static double HydrodynamicForceN(PlatformBase ship, double u, double v, double r)
        {
            //Reference: Introduction of MMG Standard Method for Ship Manuevering Predictions (Yasukawa, 2014)
            double N;
            double L = _hull.Lpp;
            double T = _hull.Draft;
            double rho = _hull.rho;
            double U = Math.Sqrt(u * u + v * v);
            double _u = u / U;
            double _v = v / U;
            double _r = r * L / U;

            double Nv = _kijima.Nv;
            double Nr = _kijima.Nr;
            double Nvv = _kijima.Nvv;
            double Nrr = _kijima.Nrr;
            double Nvrr = _kijima.Nvrr;
            double Nvvr = _kijima.Nvvr;

            N = Nv * _v + Nr * _r + Nvv * _v * Math.Abs(_v) + Nrr * _r * Math.Abs(_r) + Nvrr * _v * _r * _r + Nvvr * _v * _v * _r;
            N *= (0.5 * rho * L * L * T * U * U);

            return N;
        }

        private static double[] PropulsionForce(PlatformBase ship, double u, double v, double r, double nP)
        {
            PropellerOpenWater _pow = new PropellerOpenWater(ship.ModelType);
            int len = _pow.PowCurve.GetLength(1);

            var vJ = new double[len];
            var vKt = new double[len];
            var vKq = new double[len];
            var vEta = new double[len];

            for (int j = 0; j < _pow.PowCurve.GetLength(1); j++)
            {
                vJ[j] = _pow.PowCurve[0, j];
                vKt[j] = _pow.PowCurve[1, j];
                vKq[j] = _pow.PowCurve[2, j];
                vEta[j] = _pow.PowCurve[3, j];
            }

            // double XP;
            double tP = _hull.tP;
            double wP0 = _hull.wP0;
            // double C1 = _hull.C1;
            // double C2 = _hull.C2p;  
            // int NumPropeller = _hull.NumRudder;
            double xP_ = -0.5;
            //	double u = m_vState[3];
            // double v = m_vState[4];
            // double r = m_vState[5];
            double U = Math.Sqrt(u * u + v * v);
            double L = _hull.Lpp;
            double r_ = r * L / U;

            double beta_P = Math.Atan2(v, u) - xP_ * r_;
            //	if(beta_P<0)	C2 = m_stShip.C2n;
            //	double wP=1-(1-wP0)*( 1 +    (1- exp(-C1*fabs(beta_P)))*(C2-1) );	wP = wP0;
            double k1 = -4.0;
            double wP = wP0 * Math.Exp(k1 * beta_P * beta_P);
            double DP = _hull.DP;
            double rho = 1.025;
            double VA = (1 - wP) * u;
            double J = VA / (Math.Abs(nP) * DP); //Advance ratio
            double KT = DefineFunction.LinInterpol(true, vJ, vKt, J);
            double KQ = DefineFunction.LinInterpol(true, vJ, vKq, J);
            // double KT = LinInterpol(TRUE, vJ, vKt, m_iNumPowCurve, J);
            // double KQ = LinInterpol(TRUE, vJ, vKq, m_iNumPowCurve, J);
            // double eta_H = (1 - tP) / (1 - wP); //Hull efficiency
            //	double eta0=0.5;
            // double eta0 = UDF.LinInterpol(true, vJ, vEta, J);
            // double eta0 = LinInterpol(TRUE, vJ, vEta, m_iNumPowCurve, J);//	eta0 = 0.4;
            // double eta_S = 0.96; //Shaft efficiency
            double Thrust = rho * Math.Pow(nP, 2) * Math.Pow(DP, 4) * KT;
            // m_PropellerTorqueCoefficient = KQ;
            //m_PropellerTorque = rho*pow(nP,2.)*pow(DP,5.)*KQ;
            //m_PropellerPower = 2*pi*nP*m_PropellerTorque;
            //	XP = eta_H*eta0*eta_S*Thrust;
            double XP = (1 - tP) * Thrust;
            var signOfForce = nP > 0 ? 1 : -1;
            XP *= signOfForce;
            KQ *= signOfForce;
            double[] res = new double[2] { XP, KQ };

            return res;
        }

        public static double[] CalculRudderForce(PlatformBase ship, double u, double v, double r, double nP, double delta)
        {
            double[] res = new double[3];

            PropellerOpenWater _pow = new PropellerOpenWater(ship.ModelType);
            int len = _pow.PowCurve.GetLength(1);

            var vJ = new double[len];
            var vKt = new double[len];
            var vKq = new double[len];
            var vEta = new double[len];

            for (int j = 0; j < _pow.PowCurve.GetLength(1); j++)
            {
                vJ[j] = _pow.PowCurve[0, j];
                vKt[j] = _pow.PowCurve[1, j];
                vKq[j] = _pow.PowCurve[2, j];
                vEta[j] = _pow.PowCurve[3, j];
            }

            //Reference:(DTU)WP2-Report 4-Resistance and Propulsion Power-Final-October 2012
            double tP = _hull.tP;

            double L = _hull.Lpp;
            //int iNumRudder = _hull.NumRudder;
            double DP = _hull.DP;
            double HR = _hull.HR;
            double AR = _hull.AR;
            double rho = _hull.rho;
            double wP0 = _hull.wP0;
            double C1 = _hull.C1;
            double C2 = _hull.C2p;
            double xP_ = -0.5;
            double U = Math.Sqrt(u * u + v * v);
            double r_ = r * L / U;

            double beta_P = Math.Atan2(v, u) - xP_ * r_;
            //	if(beta_P<0)	C2 = m_stShip.C2n;
            //	double wP=1-(1-wP0)*( 1 +    (1- exp(-C1*fabs(beta_P)))*(C2-1) );

            double k1 = -4.0;
            double wP = wP0 * Math.Exp(k1 * beta_P * beta_P);

            double VA = (1 - wP) * u;
            double J = VA / (nP * DP); //Advance ratio
            double KT = DefineFunction.LinInterpol(true, vJ, vKt, J);

            double beta = Math.Atan2(v, u);
            double lR = _hull.lR;
            double beta_R = beta - lR * r_;
            double gamma_R = (beta_R > 0 ? _hull.gammaRp : _hull.gammaRn);
            double vR = U * gamma_R * beta_R;

            double epsilon = _hull.epsilon;
            double eta = DP / HR;
            double kappa = _hull.kappa;

            double uR = epsilon * u * (1 - wP) * Math.Sqrt(eta * Math.Pow(1 + kappa * (Math.Sqrt(1 + 8 * KT / Math.PI / J / J) - 1), 2) + (1 - eta));
            double U_R = Math.Sqrt(uR * uR + vR * vR);
            double alpha_R = delta - Math.Atan2(vR, uR);
            double f_alpha = _hull.f_alpha;
            double FN = 0.5 * rho * AR * U_R * U_R * f_alpha * Math.Sin(alpha_R);

            double tR = _hull.tR;
            double aH = _hull.aH;
            double xH = _hull.xH * L;
            double xR = -L / 2;

            res[0] = -(1 - tR) * FN * Math.Sin(delta); // * (double)iNumRudder;
            res[1] = -(1 + aH) * FN * Math.Cos(delta); // * (double)iNumRudder;
            res[2] = -(xR + aH * xH) * FN * Math.Cos(delta); // * (double)iNumRudder;

            return res;
        }


        #endregion


        private static double[] RealModel(double u, double v, double r, double U, double rudderAngle, MyShip ship)
        {
            //유체력 미계수 호출
            ShipCoefficient coefficient = new ShipCoefficient();

            double Xudot = coefficient.Xudot;
            double Xuu = coefficient.Xuu;
            double Xvv = coefficient.Xvv;
            double Xrr = coefficient.Xrr;
            double Xvr = coefficient.Xvr;
            double Xdel = coefficient.Xdel;
            double Xdd = coefficient.Xdd;

            double Yvdot = coefficient.Yvdot;
            double Yrdot = coefficient.Yrdot;
            double Yv = coefficient.Yv;
            double Yvv = coefficient.Yvv;
            double Yr = coefficient.Yr;
            double Yrr = coefficient.Yrr;
            double Yvrr = coefficient.Yvrr;
            double Yrvv = coefficient.Yrvv;
            double Ydel = coefficient.Ydel;

            double Nvdot = coefficient.Nvdot;
            double Nrdot = coefficient.Nrdot;
            double Nv = coefficient.Nv;
            double Nvv = coefficient.Nvv;
            double Nr = coefficient.Nr;
            double Nrr = coefficient.Nrr;
            double Nvrr = coefficient.Nvrr;
            double Nrvv = coefficient.Nrvv;
            double Ndel = coefficient.Ndel;


            Xudot = Xudot * (0.5 * _rho_sea * Math.Pow(ship.Length, 3));
            Xuu = Xuu * (0.5 * _rho_sea * Math.Pow(ship.Length, 2));
            Xvv = Xvv * (0.5 * _rho_sea * Math.Pow(ship.Length, 2));
            Xrr = Xrr * (0.5 * _rho_sea * Math.Pow(ship.Length, 4));
            Xvr = Xvr * (0.5 * _rho_sea * Math.Pow(ship.Length, 3));


            Yvdot = Yvdot * (0.5 * _rho_sea * Math.Pow(ship.Length, 3));
            Yrdot = Yrdot * (0.5 * _rho_sea * Math.Pow(ship.Length, 4));

            Yv = Yv * (0.5 * _rho_sea * Math.Pow(ship.Length, 2) * U);
            Yvv = Yvv * (0.5 * _rho_sea * Math.Pow(ship.Length, 2));
            Yr = Yr * (0.5 * _rho_sea * Math.Pow(ship.Length, 3) * U);
            Yrr = Yrr * (0.5 * _rho_sea * Math.Pow(ship.Length, 4));
            Yvrr = Yvrr * (0.5 * _rho_sea * Math.Pow(ship.Length, 4) / U);
            Yrvv = Yrvv * (0.5 * _rho_sea * Math.Pow(ship.Length, 3) / U);

            Ydel = Ydel * (0.5 * _rho_sea * Math.Pow(ship.Length, 2) * Math.Pow(U, 2));

            Nvdot = Nvdot * (0.5 * _rho_sea * Math.Pow(ship.Length, 4));
            Nrdot = Nrdot * (0.5 * _rho_sea * Math.Pow(ship.Length, 5));

            Nv = Nv * (0.5 * _rho_sea * Math.Pow(ship.Length, 3) * U);
            Nvv = Nvv * (0.5 * _rho_sea * Math.Pow(ship.Length, 3));
            Nr = Nr * (0.5 * _rho_sea * Math.Pow(ship.Length, 4) * U);
            Nrr = Nrr * (0.5 * _rho_sea * Math.Pow(ship.Length, 5));
            Nvrr = Nvrr * (0.5 * _rho_sea * Math.Pow(ship.Length, 5) / U);
            Nrvv = Nrvv * (0.5 * _rho_sea * Math.Pow(ship.Length, 4) / U);

            Ndel = Ndel * (0.5 * _rho_sea * Math.Pow(ship.Length, 3) * Math.Pow(U, 2));

            double Resitance = -Xuu * u;
            double Xhyd = Xvv * v * v + Xvr * v * r + Xrr * r * r + Xuu * u * Math.Abs(u) + Resitance;
            double Yhyd = Yv * v + Yvv * Math.Abs(v) * v + Yr * r + Yrr * Math.Abs(r) * r + Yvrr * v * r * r +
                          Yrvv * r * v * v;
            double Nhyd = Nv * v + Nvv * Math.Abs(v) * v + Nr * r + Nrr * Math.Abs(r) * r + Nvrr * v * r * r +
                          Nrvv * r * v * v;

            double Xrudder = 0;
            double Yrudder = Ydel * rudderAngle;
            double Nrudder = Ndel * rudderAngle;


            double Fn = 0.5 * 1024 * 136.7 * 1 * u * u * Math.Sign(rudderAngle);
             Xrudder = -0.5677 * Fn * Math.Sign(rudderAngle);
             Yrudder = -1.2723 * Fn * Math.Cos(rudderAngle);
             Nrudder = -320 * Fn * Math.Cos(rudderAngle);
             

            double[] result = new[] { Xhyd, Yhyd, Nhyd, Xrudder, Yrudder, Nrudder, Xudot, Yvdot, Yrdot, Nvdot, Nrdot };
            return result;
        }

        private static double clip(double value, double min, double max)
        {
            if (value < min)
            {
                value = min;
            }

            if (value > max)
            {
                value = max;
            }

            return value;
        }
        //
        // private static double _length = 320; //선박의 길이
         private static double _rho_sea = 1014; //바닷물 밀도
        //
        // private static double _uNorm = 8; //기준 속력 [m/s]
        //
        // private static double _g = 9.80665; //중력 가속도
        //
        // private static double _xg = 11.2;
        //
        // private static double _m = 312622000; //질량 kg단위
        //
        // private static double _izz;
        //
        // private static double _delSat = 35 * Math.PI / 180;
        //
        // private static double _delSatDot = 5 * Math.PI / 180;
    }
}