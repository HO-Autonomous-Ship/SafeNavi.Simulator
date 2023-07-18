using System;
using System.Collections.Generic;
using System.Linq;
using System.Waf.Foundation;
using System.Windows.Media.Media3D;
using MathNet.Numerics.Differentiation;
using MathNet.Numerics.LinearAlgebra.Double;
using SyDLab.Usv.Simulator.Domain.Utils;

namespace SyDLab.Usv.Simulator.Domain.Models.Platforms
{
    public class ManeouvringEquation : Model
    {
        private static double _previousShipSpeed=10; //(m/s))
        private static double _speedErrorAccum = 0;
        private static double _maxSpeedErrorAccum = 100;
        public static void UpdateMyShip(MyShip ship, Waypoint wayPointPrev, Waypoint wayPointNext, double dt)
        {
            Vector3D position = new Vector3D(ship.CurrentX, ship.CurrentY, 0);
            double heading = 90 - ship.Heading;
            var tempVar = ship.TempVar;
            var tempVarNext = Calculator.CalculateState(wayPointPrev, wayPointNext, position, heading, tempVar, dt);
            ship.TempVar = tempVarNext;
            double action = Calculator.CalculateRudderAngleWithPd(tempVarNext[1], tempVarNext[2]);
            double[] rudderAngle = ShipDynamics.RudderDynamics(ship, action, dt);
            
            ship.Rudder = rudderAngle;
            ShipDynamics.CalculateDynamics(ship, rudderAngle[0], dt);
        }

        #region [MMG]
        // public static void UpdateMyShipMMG(PlatformBase ship, double[] setPoints, double dt, bool isExtCtrl)
        public static void UpdateMyShipMMG(PlatformBase ship, double dt, bool isExtCtrl)
        {
            // if (!isExtCtrl)
            // {
                AutoPilotRudder(ship);
                EngineRpmController(ship, dt);
            // }

            IntegrateState(ship, dt);
        }


        internal static void IntegrateState(PlatformBase ship, double dt)
        {
            double[] dx1 = new double[10], dx2 = new double[10];
            double[] dx3 = new double[10], dx4 = new double[10];
            double[] dx1Tmp = new double[10], dx2Tmp = new double[10];
            double[] dx3Tmp = new double[10];
            double[] state = new double[10];

            state[0] = ship.CurrentY;
            state[1] = ship.CurrentX;
            state[2] = ship.CurrentHeading * Distance.D2R; //(deg) --> (rad)
            state[3] = ship.Velocity.X * Distance.KN2MS;
            state[4] = ship.Velocity.Y * Distance.KN2MS;
            state[5] = ship.AngularVelocity * Distance.D2R;   //(deg/s) --> (rad/s)

            state[6] = ship.Rpm / 60;     //engine(stbd) (rev/s)
            state[7] = ship.Rpm / 60;     //engine(port) (rev/s)
            state[8] = ship.Rudder[0];  //rudder(stbd) (rad)
            state[9] = ship.Rudder[0];  //rudder(port) (rad)

            double t = 0; //추후 환경 외력 계산할때, 시간 동기화에 사용됨 ==> 입력 변수

            var res = ShipDynamics.MotionEquation(ship, t, state);
            foreach (var output in res.Select((value, index) => (value, index)))
            {
                var value = output.value;
                var index = output.index;
                dx1[index] = dt * value;
                dx1Tmp[index] = state[index] + dx1[index] / 2;
            }

            res = ShipDynamics.MotionEquation(ship, t + dt / 2, dx1Tmp);
            foreach (var output in res.Select((value, index) => (value, index)))
            {
                var value = output.value;
                var index = output.index;
                dx2[index] = dt * value;
                dx2Tmp[index] = state[index] + dx2[index] / 2;
            }

            res = ShipDynamics.MotionEquation(ship, t + dt / 2, dx2Tmp);
            foreach (var output in res.Select((value, index) => (value, index)))
            {
                var value = output.value;
                var index = output.index;
                dx3[index] = dt * value;
                dx3Tmp[index] = state[index] + dx3[index];
            }

            res = ShipDynamics.MotionEquation(ship, t + dt, dx3Tmp);
            foreach (var output in res.Select((value, index) => (value, index)))
            {
                var value = output.value;
                var index = output.index;
                dx4[index] = dt * value;
            }

            foreach (var output in res.Select((value, index) => (value, index)))
            {
                var value = output.value;
                var index = output.index;
                state[index] += (dx1[index] + 2 * dx2[index] + 2 * dx3[index] + dx4[index]) / 6;
            }

            // psi 범위 확인 필요
            if (state[2] > 2 * Math.PI)
                state[2] -= 2 * Math.PI;
            if (state[2] < -2 * Math.PI)
                state[2] += 2 * Math.PI;

            // Rudder angle limit 확인
            if (Math.Abs(state[8]) > ((MyShip)ship).LimitOfRudderAngle * Distance.D2R)
                state[8] = Math.Sign(state[8]) * ((MyShip)ship).LimitOfRudderAngle * Distance.D2R;

            ship.CurrentY = state[0];
            ship.CurrentX = state[1];
            ship.CurrentHeading = state[2] * Distance.R2D;
            // ship.Velocity = new Vector3D(state[3] * Distance.MS2KN, state[4] * Distance.MS2KN, 0);
            ship.Velocity = new Vector3D(state[3], state[4], 0);
            ship.AngularVelocity = state[5] * Distance.R2D;
            ship.Rpm = state[6] * 60;
            // ship.Rpm = state[7];
            ship.Rudder[0] = state[8];
            ship.Rudder[1] = (dx1[8] + 2 * dx2[8] + 2 * dx3[8] + dx4[8]) / 6;
            // ship.Rudder[0] = state[9];

            ship.Speed = Math.Sqrt(Math.Pow(ship.Velocity.X, 2) + Math.Pow(ship.Velocity.Y, 2));  //(knots)
        }
        #endregion


        private static void AutoPilotRudder(PlatformBase ship)
        {
            double psi = ship.CurrentHeading;   //(deg)
            double rot = ship.AngularVelocity;  //(deg/s)
            double dPsiMax = 30;    //(deg)
            double limitOfRudderAngle = ((MyShip)ship).LimitOfRudderAngle;    //(deg)
            double cog = ship.HeadingDesired;

            double dPsi = psi - cog;
            if (Math.Abs(dPsi) > 1)
                dPsi = psi - cog;

            if (dPsi > 180) dPsi -= 360;
            else if (dPsi < -180) dPsi += 360;

            if (dPsi > dPsiMax) dPsi = dPsiMax;
            else if (dPsi < -dPsiMax) dPsi = -dPsiMax;
            
            double delta = -(ship.HeadingControlGainP * dPsi + ship.HeadingControlGainD * rot);
            if (Math.Abs(delta) > limitOfRudderAngle) delta = Math.Sign(delta) * limitOfRudderAngle;

            ship.RudderDesired = delta * Distance.D2R;
        }

        private static void EngineRpmController(PlatformBase ship, double dt)
        {
            double spdDesired = ship.SpeedDesired * Distance.KN2MS;
            double u = ship.Speed * Distance.KN2MS;
            double du = u - spdDesired;
            double a = (u - _previousShipSpeed) / dt;
            if (Math.Abs(a) > 5) a = 0;
            double rpm;
            double rpmFf = ship.Rpm, rpmFb;
            double minRpm = (ship as MyShip).MinRpm, maxRpm = (ship as MyShip).MaxRpm;

            List<string> nameList = new List<string>(VesselItemService.VesselList);
            switch (nameList.IndexOf(ship.ModelType))
            {
                case 0:
                case 1:
                    SpeedRpm spdRpm = new SpeedRpm(ship.ModelType);
                    int len = spdRpm.PowSpeedCurve.GetLength(1);
                    double[] spdArray = new double[len];
                    double[] rpmArray = new double[len];
                    Buffer.BlockCopy(spdRpm.PowSpeedCurve, 0, spdArray, 0, sizeof(double) * len);
                    Buffer.BlockCopy(spdRpm.PowSpeedCurve, sizeof(double) * len, rpmArray, 0, sizeof(double) * len);
                    rpmFf = DefineFunction.LinInterpol(true, spdArray, rpmArray, spdDesired);
                    break;
                case 2:
                    rpmFf = 6.5 * spdDesired * Distance.KN2MS + 1.67;
                    break;
            }
            rpmFb = -(ship.SpeedControlGainP * du + ship.SpeedControlGainD * a +
                      ship.SpeedControlGainI * _speedErrorAccum);
            rpm = rpmFf + rpmFb;
            rpm = Math.Max(minRpm, Math.Min(rpm, maxRpm));

            _previousShipSpeed = u;
            _speedErrorAccum += du * dt;
            if (Math.Abs(_speedErrorAccum) > _maxSpeedErrorAccum) _speedErrorAccum = Math.Sign(_speedErrorAccum) * _maxSpeedErrorAccum;

            ship.RpmDesired = rpm;
        }
    }
}