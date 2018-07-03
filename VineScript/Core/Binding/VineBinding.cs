using System;

namespace VineScript.Binding
{
    /// <summary>
    /// Method attribute that allows to bind a method to VineScript.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class VineBinding : Attribute
    {
        /// <summary>
        /// Indicates that this method will override a registered method
        /// with the same signature.
        /// </summary>
        public bool Override { get; set; } = false;

        /// <summary>
        /// Method attribute that allows to bind a method to VineScript.
        /// </summary>
        public VineBinding()
        {
        }
    }
}
