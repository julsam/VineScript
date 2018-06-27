using System;
using System.Collections.Generic;
using System.Linq;
using VineScript.Core;

namespace VineScript.Binding
{
    public class VineBindingException : VineException
    {
        public VineBindingException(string message) 
            : base(message) { }

        public VineBindingException(string methodName, string message) 
            : base(string.Format(
                "VineBinding: error with the function '{0}':{1}   {2}",
                methodName, Environment.NewLine, message
            ))
        {
            this.Title = string.Format(
                "error with the function '{0}':",
                methodName
            );
            this.Details = message;
        }

        public VineBindingException(string module, string methodName, string message) 
            : base(string.Format(
                "VineBinding: error with the function '{0}' in '{1}':{2}  {3}",
                methodName, module, Environment.NewLine, message
            ))
        {
            this.Title = string.Format(
                "error with the function '{0}' in '{1}'",
                methodName, module
            );
            this.Details = message;
        }
    }
}
