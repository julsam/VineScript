using System;
using System.Collections.Generic;
using System.Linq;
using VineScript.Core;

namespace VineScript.Binding
{
    public enum VineBindingErrorType {
        Method,
        Class
    }

    public class VineBindingException : VineException
    {
        private static string StringifyError(VineBindingErrorType errorType)
        {
            return errorType == VineBindingErrorType.Method ? "function" : "class";
        }

        public VineBindingException(string message) 
            : base(message) { }

        public VineBindingException(string methodName, string message,
            VineBindingErrorType errorType=VineBindingErrorType.Method)
            : base(string.Format(
                "VineBinding: error with the {0} '{1}':{2}{3}",
                StringifyError(errorType), methodName, Environment.NewLine, message
            ))
        {
            this.Title = string.Format(
                "error with the {0} '{1}':",
                StringifyError(errorType), methodName
            );
            this.Details = message;
        }

        public VineBindingException(string module, string methodName, string message,
            VineBindingErrorType errorType=VineBindingErrorType.Method)
            : base(string.Format(
                "VineBinding: error with the {0} '{1}' in '{2}':{3}{4}",
                StringifyError(errorType), methodName, module, Environment.NewLine, message
            ))
        {
            this.Title = string.Format(
                "error with the {0} '{1}' in '{2}':",
                StringifyError(errorType), methodName, module
            );
            this.Details = message;
        }
    }
}
