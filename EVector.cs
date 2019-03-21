using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Image_Enhance
{
    class EVector
    {
        private double val;
        private static double STEP = 2.0d / 256.0d;

        public EVector(double val)
        {
            if (val > -1 && val < 1)
                this.val = val;
            else if (val <= -1)
                val = -0.99999;
            else
                val = 0.99999;
        }

        public EVector(int val255)
        {
            val = -0.999d + (val255 * STEP);
            if (val >= 1)
                val = 0.9999d;
        }

        public double Value
        {
            get
            {
                return val;
            }
        }

        public double RealValue
        {
            get
            {
                return 0.5d * Math.Log((1 + val) / (1 - val));
            }
        }

        public int IntValue
        {
            get
            {
                return (int)((val + 1) / STEP);
            }
        }

        public static EVector operator +(EVector v1, EVector v2)
        {
            return new EVector((v1.val + v2.val) / (1 + v1.val * v2.val));
        }
        public static EVector operator -(EVector v1, EVector v2)
        {
            return new EVector((v1.val - v2.val) / (1 - v1.val * v2.val));
        }
        public static EVector operator *(EVector v1, double d)
        {
            return new EVector((Math.Pow((1 + v1.val), d) - Math.Pow((1 - v1.val), d)) / (Math.Pow((1 + v1.val), d) + Math.Pow((1 - v1.val), d)));
        }
        public static EVector operator *(double d, EVector v1)
        {
            return new EVector((Math.Pow((1 + v1.val), d) - Math.Pow((1 - v1.val), d)) / (Math.Pow((1 + v1.val), d) + Math.Pow((1 - v1.val), d)));
        }

        public static EVector Mean(EVector[][] F)
        {
            EVector acc = new EVector(0.0d);
            double stat = 1.0d / (F.Length * F[0].Length);
            for (int x = 0; x < F.Length; x++)
            {
                for (int y = 0; y < F[x].Length; y++)
                {
                    acc = acc + (stat * F[x][y]);
                }
            }
            return acc;
        }

        public static double Variance(EVector[][] F, EVector mean)
        {
            double acc = 0;
            EVector temp; double temp2; double stat=(F.Length * F[0].Length);
            for (int x = 0; x < F.Length; x++)
            {
                for (int y = 0; y < F[x].Length; y++)
                {
                    temp = F[x][y] - mean;
                    temp2 = temp.RealValue;
                    temp2 = Math.Pow(temp2,2);
                    acc += temp2 / stat;
                }
            }
            return acc;
        }
    }
}
