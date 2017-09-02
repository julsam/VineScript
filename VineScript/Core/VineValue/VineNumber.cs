using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VineScript.Core.VineValue
{
    public class VineNumber : VineValue<double>
    {
        public override double Value {
            get {
                return vinevar.AsNumber;
            }
        }

        public VineNumber(VineVar value) : base(value, VineVar.Type.Number)
        { }

        public VineNumber(double value) : base(value, VineVar.Type.Number)
        { }

        public VineNumber(float value) : base(value, VineVar.Type.Number)
        { }

        public static implicit operator VineNumber(double value)
        {
            return new VineNumber(value);
        }

        public static implicit operator VineNumber(float value)
        {
            return new VineNumber(value);
        }

        public static implicit operator double(VineNumber value)
        {
            return value.vinevar.AsNumber;
        }

        public static VineNumber operator ++ (VineNumber value)
        {
            double d = value.vinevar.AsNumber;
            return ++d;
        }

        public static VineNumber operator -- (VineNumber value)
        {
            double d = value.vinevar.AsNumber;
            return --d;
        }
    }
}
