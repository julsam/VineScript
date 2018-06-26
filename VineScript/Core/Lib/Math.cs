using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VineScript.Binding;
using VineScript.Core;

namespace VineScript.Lib
{
    public class Math
    {
        [VineBinding]
        public static VineVar Abs(int value)
        {
            return System.Math.Abs(value);
        }
        
        [VineBinding]
        public static VineVar Abs(double value)
        {
            return System.Math.Abs(value);
        }
        
        [VineBinding]
        public static VineVar Ceil(VineVar value)
        {
            return System.Math.Ceiling(value.AsNumber);
        }
        
        [VineBinding]
        public static VineVar Cos(VineVar value)
        {
            return System.Math.Cos(value.AsNumber);
        }
        
        [VineBinding]
        public static VineVar Exp(VineVar value)
        {
            return System.Math.Exp(value.AsNumber);
        }
        
        [VineBinding]
        public static VineVar Floor(VineVar value)
        {
            return System.Math.Floor(value.AsNumber);
        }
        
        [VineBinding]
        public static VineVar Log(VineVar value)
        {
            return System.Math.Log(value.AsNumber);
        }
        
        [VineBinding]
        public static VineVar Log10(VineVar value)
        {
            return System.Math.Log10(value.AsNumber);
        }
        
        [VineBinding]
        public static VineVar Log2(VineVar value)
        {
            return System.Math.Log(value.AsNumber, 2);
        }
        
        [VineBinding]
        public static VineVar Max(params int[] values)
        {
            var max = int.MinValue;
            foreach (var el in values) {
                if (el > max) {
                    max = el;
                }
            }
            return max;
        }
        
        [VineBinding]
        public static VineVar Max(params double[] values)
        {
            var max = double.MinValue;
            foreach (var el in values) {
                if (el > max) {
                    max = el;
                }
            }
            return max;
        }
        
        [VineBinding]
        public static VineVar Min(params int[] values)
        {
            var min = int.MaxValue;
            foreach (var el in values) {
                if (el < min) {
                    min = el;
                }
            }
            return min;
        }
        
        [VineBinding]
        public static VineVar Min(params double[] values)
        {
            var min = double.MaxValue;
            foreach (var el in values) {
                if (el < min) {
                    min = el;
                }
            }
            return min;
        }
        
        [VineBinding]
        public static VineVar PI()
        {
            return System.Math.PI;
        }
        
        [VineBinding]
        public static VineVar Pow(int value, int power)
        {
            return Std.Int(System.Math.Pow(value, power));
        }
        
        [VineBinding]
        public static VineVar Pow(double value, double power)
        {
            return System.Math.Pow(value, power);
        }
        
        [VineBinding]
        public static VineVar Round(double value)
        {
            return System.Math.Round(value);
        }
        
        [VineBinding]
        public static VineVar Round(double value, int digits)
        {
            return System.Math.Round(value, digits);
        }
        
        [VineBinding]
        public static VineVar Sign(int value)
        {
            return System.Math.Sign(value);
        }
        
        [VineBinding]
        public static VineVar Sign(double value)
        {
            return System.Math.Sign(value);
        }
        
        [VineBinding]
        public static VineVar Sin(VineVar value)
        {
            return System.Math.Sin(value.AsNumber);
        }
        
        [VineBinding]
        public static VineVar Sqrt(VineVar value)
        {
            return System.Math.Sqrt(value.AsNumber);
        }
        
        [VineBinding]
        public static VineVar Tan(VineVar value)
        {
            return System.Math.Tan(value.AsNumber);
        }
    }
}
