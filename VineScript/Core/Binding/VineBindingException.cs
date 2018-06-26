using System;
using System.Collections.Generic;
using System.Linq;

namespace VineScript.Binding
{
    public class VineBindingException : Exception
    {
        public VineBindingException(string message) 
            : base("VineBinding: " + message) { }

        public VineBindingException(string methodName, string message) 
            : base(string.Format("VineBinding: error with '{0}':{1}  {2}",
                methodName, Environment.NewLine, message)) { }

        public VineBindingException(string module, string methodName, string message) 
            : base(string.Format("VineBinding: error with '{0}' in '{1}':{2}  {3}",
                methodName, module, Environment.NewLine, message)) { }
    }
}
