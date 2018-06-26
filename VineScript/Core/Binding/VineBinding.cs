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
        public Type[] Values { get; private set; }

        /// <summary>
        /// Method attribute that allows to bind a method to VineScript.
        /// </summary>
        public VineBinding()
        {
        }

        /// <summary>
        /// Method attribute that allows to bind a method to VineScript.
        /// </summary>
        /// <param name="types">
        /// [Not Implemented] Specify the expected type for each argument (optional)
        /// </param>
        public VineBinding(params Type[] types)
        {
            Values = types;
        }
    }
}
