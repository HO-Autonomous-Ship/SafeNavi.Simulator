using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using SyDLab.Usv.Simulator.Domain.Models.Platforms;

namespace SyDLab.Usv.Simulator.Domain.Utils
{
    public class DefineFunction
    {

        public static double LinInterpol(bool isIncreasing, double[] X, double[] Y, double x)
        {
            double y;
            int n = X.GetLength(0);
            int i = 0, k = 0;

            if (isIncreasing)
            {
                while (i < n - 1)
                {
                    if (x >= X[i])
                    {
                        if (x < X[i + 1]) { k = i; break; }
                    }
                    i++;
                }
                k = i;

                if (k == n - 1) y = Y[k];
                else y = Y[k] + (Y[k + 1] - Y[k]) * (x - X[k]) / (X[k + 1] - X[k]);
                if (x < X[0]) y = Y[0];
            }

            else
            {
                while (i < n - 1)
                {
                    if (x <= X[i])
                    {
                        if (x > X[i + 1]) { k = i; break; }
                    }
                    i++;
                }
                k = i;

                if (k == n - 1) y = Y[k];
                else y = Y[k] + (Y[k + 1] - Y[k]) * (x - X[k]) / (X[k + 1] - X[k]);
                if (x > X[0]) y = Y[0];
            }

            return y;
        }

        public static void ConvertCoordiPiPi(ref double alpha)
        {
            while (alpha > Math.PI)
                alpha -= 2*Math.PI;

            while (alpha < -Math.PI)
                alpha += 2*Math.PI;
        }

        // public static bool EvaluateInsideCone(ConeInfo cone, Vector vec)
        // {
        //     Vector v1 = new Vector(cone.points[1].x - cone.points[0].x, cone.points[1].y - cone.points[0].y);
        //     Vector v2 = new Vector(cone.points[2].x - cone.points[0].x, cone.points[2].y - cone.points[0].y);
        //
        //     double innerProduct = Vector.Multiply(v1, v2);
        //     double phi = Math.Acos(innerProduct/(v1.Length*v2.Length));
        //
        //
        //     innerProduct = Vector.Multiply(v1, vec);
        //     double phi1 = Math.Acos(innerProduct / (v1.Length * vec.Length));
        //     innerProduct = Vector.Multiply(v2, vec);
        //     double phi2 = Math.Acos(innerProduct / (v2.Length * vec.Length));
        //
        //     if (Math.Abs(phi - (phi1 + phi2)) < 0.001)
        //         return true;
        //
        //     return false;
        // }

        public static bool CheckSum(string sentence)
        {
            try
            {
                // byte sum = Convert.ToByte(sentence[sentence.IndexOf("!") + 1]);
                byte sum = Convert.ToByte(sentence[1]);

                // for (int i = sentence.IndexOf("!") + 2; i < sentence.IndexOf("*"); ++i)
                for (int i = 2; i < sentence.IndexOf("*"); ++i)
                {
                    sum ^= Convert.ToByte(sentence[i]);
                }

                int idx = sentence.IndexOf("*");
                if (idx < 0)
                    return false;

                return sum == Convert.ToByte(sentence.Substring(idx + 1, 2), 16);
            }
            catch (Exception e)
            {
                // Debug.WriteLine(e.Message);
                Debug.WriteLine("Failed to check checksum !!! ");
                return false;
            }
        }
    }

    public class MAF
    {
        private Queue<double> _rawArray=new Queue<double>();

        public MAF()
        {
            Initialize();
        }

        public void Initialize()
        {
            _rawArray.Clear();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="num">the number of data to be filtered</param>
        /// <param name="value"></param>
        /// <returns></returns>
        public double GetMovAveFilteredValue(int num, double value)
        {
            double sum = 0;
            
            _rawArray.Enqueue(value);

            if (_rawArray.Count > num)
                _rawArray.Dequeue();

            foreach (var item in _rawArray)
                sum += item;

            return sum / _rawArray.Count;
        }
    }

    public class RandomGeneration
    {
        public RandomGeneration()
        {

        }

        const string characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private static Random random = new Random();

        public string GenRandomName(int length)
        {
            return new string(Enumerable.Repeat(characters, length).Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public int GenRandomNumber(int lower, int upper)
        {
            return random.Next(lower, upper);
        }
    }

}
