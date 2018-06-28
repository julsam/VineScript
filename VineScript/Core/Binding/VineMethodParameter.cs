using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace VineScript.Binding
{
    internal class VineMethodParameter
    {
        /// <summary>
        /// Type of this parameter.
        /// </summary>
        public Type ParameterType { get; private set; }

        /// <summary>
        /// Type of the object referred by the current pointer or reference type.
        /// </summary>
        public Type ReferredType { get; private set; }

        /// <summary>
        /// Name of this parameter.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Indicates whether this is an output parameter.
        /// Only true for <c>out</c> type
        /// </summary>
        public bool IsOut { get; private set; }
            
        /// <summary>
        /// Indicates whether the parameter is passed by reference.
        /// True for both <c>out</c> and <c>ref</c> types.
        /// </summary>
        public bool IsByRef { get; private set; }
            
        /// <summary>
        /// Indicates whether the parameter has a default value.
        /// </summary>
        public bool HasDefaultValue { get; private set; }

        /// <summary>
        /// Indicates if this parameter has a <c>params</c> attribute.
        /// </summary>
        public bool IsParams { get; private set; }

        public VineMethodParameter(ParameterInfo parameter)
        {
            ParameterType   = parameter.ParameterType;
            ReferredType    = parameter.ParameterType.GetElementType();
            Name            = parameter.Name;
            IsOut           = parameter.IsOut;
            IsByRef         = parameter.ParameterType.IsByRef;
            HasDefaultValue = parameter.HasDefaultValue;
            
            // http://stackoverflow.com/questions/6484651/calling-a-function-using-reflection-that-has-a-params-parameter-methodbase
            IsParams = parameter.IsDefined(typeof(ParamArrayAttribute), false);
        }

        public Type GetRealType()
        {
            if (ReferredType != null) {
                return ReferredType;
            } else {
                return ParameterType;
            }
        }

        public override string ToString()
        {
            return GetSignature();
        }

        /// <summary>
        /// Signature of the parameter.
        /// </summary>
        /// <param name="useShortName">
        /// Indicates if the types should be displayed with their
        /// full namespace or not.
        /// </param>
        /// <returns>The formatted signature.</returns>
        public string GetSignature(bool useShortName=true)
        {
            string refOrOut = IsByRef
                ? IsOut ? "out " : "ref "
                : "" ;

            string type = IsByRef 
                ? useShortName ? ReferredType.Name : ReferredType.ToString()
                : useShortName ? ParameterType.Name : ParameterType.ToString();

            string str = string.Format(
                "{0}{1}{2}",
                IsParams ? "params " : "",  // 'params'
                refOrOut,                   // 'ref' or 'out'
                type                        // type
            );
            return str;
        }
    }
}
