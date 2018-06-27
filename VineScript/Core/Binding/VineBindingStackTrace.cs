using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace VineScript.Binding
{
    [Serializable]
    public class VineBindingStackTrace
    {
        public MethodInfo InvokedMethod { get; private set; }
        public string MethodSignature { get; private set; }
        public string StackTrace { get; private set; } // could be an array of string

        public VineBindingStackTrace(MethodInfo methodInfo, string stackTrace,
            string methodSignature)
        {
            this.InvokedMethod = methodInfo;
            this.StackTrace = stackTrace;
            this.MethodSignature = methodSignature;
        }

        public string GetErrorMessage()
        {
            return "Exception thrown when calling the method '"
                + MethodSignature + "'"
                + Environment.NewLine + StackTrace;
        }
    }
}
