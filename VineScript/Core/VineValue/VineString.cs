using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VineScript.Core.VineValue
{
    public class VineString : VineValue<string>
    {
        public override string Value {
            get {
                return vinevar.AsString;
            }
        }

        public VineString(VineVar value) : base(value, VineVar.Type.String)
        { }

        public VineString(string value) : base(value, VineVar.Type.String)
        { }

        public static implicit operator VineString(string value)
        {
            return new VineString(value);
        }

        public static implicit operator string(VineString value)
        {
            return value.vinevar.AsString;
        }
    }
}
