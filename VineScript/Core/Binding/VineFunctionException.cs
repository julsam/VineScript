using System;
using System.Collections.Generic;
using System.Linq;
using VineScript.Core;

namespace VineScript.Binding
{
    public class VineFunctionCallException : Exception
    {
        public VineFunctionCallException(string msg) : base(msg) { }
    }

    public class VineFunctionTypeError : Exception
    {
        public VineFunctionTypeError(string msg) : base("TypeError: " + msg) { }

        public VineFunctionTypeError(string funcName, VineVar.Type type)
            : base(string.Format(
                "TypeError: can't use {0} on a variable of type {1}.",
                funcName, type
            )) { }

        public VineFunctionTypeError(string funcName, VineVar.Type type, string msg)
            : base(string.Format(
                "TypeError: can't use {0} on a variable of type {1}. " + msg,
                funcName, type
            )) { }
    }
}
