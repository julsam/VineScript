using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VineScript.Core.VineValue
{
    public class VineBool : VineValue<bool>
    {
        public override bool Value {
            get {
                return vinevar.AsBool;
            }
        }
        
        public VineBool(VineVar value) : base(value, VineVar.Type.Bool)
        { }

        public VineBool(bool value) : base(value, VineVar.Type.Bool)
        { }

        public static implicit operator VineBool(bool value)
        {
            return new VineBool(value);
        }

        public static implicit operator bool(VineBool value)
        {
            return value.vinevar.AsBool;
        }
    }
}
