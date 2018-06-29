using System;
using System.Collections.Generic;
using System.Linq;

namespace VineScript.Binding
{
    /// <summary>
    /// Method attribute that allows to bind a method to VineScript.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class VineBinding : Attribute
    {
        /// <summary>
        /// Method attribute that allows to bind a method to VineScript.
        /// </summary>
        public VineBinding()
        {
        }
    }
}
