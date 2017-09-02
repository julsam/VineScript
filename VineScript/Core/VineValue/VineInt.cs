using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VineScript.Core.VineValue
{
    public class VineInt : VineValue<int>
    {
        public override int Value {
            get {
                return vinevar.AsInt;
            }
        }
        
        public VineInt(VineVar value) : base(value, VineVar.Type.Int)
        { }

        public VineInt(int value) : base(value, VineVar.Type.Int)
        { }
        
        public static implicit operator VineInt(int value)
        {
            return new VineInt(value);
        }

        public static implicit operator int(VineInt value)
        {
            return value.vinevar.AsInt;
        }

        public static VineInt operator ++ (VineInt value)
        {
            int i = value.vinevar.AsInt;
            return ++i;
        }

        public static VineInt operator -- (VineInt value)
        {
            int i = value.vinevar.AsInt;
            return --i;
        }
    }
}
