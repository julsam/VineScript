using System;
using System.Collections.Generic;
using System.Linq;
using VineScript.Core;

namespace VineScript.Binding
{
    /// <summary>
    /// Error to throw when there's an error inside a C# function
    /// defined with the attribute VineBinding and called by a Vine script.
    /// </summary>
    public class VineFunctionCallException : Exception
    {
        public VineFunctionCallException(string msg) : base(msg) { }
    }
    
    /// <summary>
    /// Error to throw when there's an error inside a C# function
    /// defined with the attribute VineBinding and called by a Vine script.
    /// This exception should be called when the parameter of a function
    /// is not of the expected type (due to the dynamic type of VineVar).
    /// </summary>
    public class VineFunctionTypeError : Exception
    {
        public VineFunctionTypeError(string msg) : base(msg) { }

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
