using System;
using System.Waf.Foundation;
using System.Windows.Media.Media3D;
using MathNet.Numerics.LinearAlgebra.Double;

namespace SyDLab.Usv.Simulator.Domain.Models.Platforms
{
    public class Calculator : Model
    {
        public static double CalculateRudderAngleWithPd(double error, double errorDot)
        {
            double action = error * 50 + errorDot * 20;

            return action;
        }

        public static double[] CalculateState(Waypoint wayPointPrev, Waypoint wayPointNext, Vector3D position, double heading, double[] tempVar, double dt)
        {
            double[] tempVarNext = new double[3];
            double[] recentPoint = { wayPointNext.Location.X, wayPointNext.Location.Y };
            double[] lastPoint = { wayPointPrev.Location.X, wayPointPrev.Location.Y };

            double psiPath = Math.Atan2(recentPoint[1] - lastPoint[1], recentPoint[0] - lastPoint[0]);
            double psiShip = Math.Atan2(recentPoint[1] - position.Y, recentPoint[0] - position.X);
            double psiRel = psiPath - psiShip;
            double psiTemp = tempVar[0];
            double psiTempHeading = tempVar[1];

            double psiErr = psiRel;
            double psiErrHeading = heading * Math.PI / 180 - psiShip;
            double psiErrDiff = psiErr - psiTemp;

            double psiErrHeadingDiff = psiErrHeading - psiTempHeading;


            double psiErrDot = psiErrDiff / dt;

            double psiErrHeadingDot = psiErrHeadingDiff / dt;


            
            tempVarNext[0] = psiErr;
            tempVarNext[1] = psiErrHeading;
            tempVarNext[2] = psiErrHeadingDot;

            return tempVarNext;
        }


    }
}