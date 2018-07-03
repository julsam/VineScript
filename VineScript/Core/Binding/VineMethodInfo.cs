using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace VineScript.Binding
{
    internal class VineMethodInfo
    {
        /// <summary>
        /// User defined module/class name. It will be used as a prefix when
        /// calling the method. For example, if a method "Bar" is registered
        /// with a module named "Foo", the complete name for calling it will
        /// be "Foo.Bar".
        /// </summary>
        public string Module { get; private set; }

        /// <summary>Return type of the function.</summary>
        public Type ReturnType { get; private set; }
        
        /// <summary>Name of the method.</summary>
        public string Name { get; private set; }

        /// <summary>List of parameters.</summary>
        public List<VineMethodParameter> Parameters { get; private set; }

        /// <summary>Reference to the MethodInfo used to register it.</summary>
        public MethodInfo MethodRef { get; private set; }

        /// <summary>Indicates if the method is static.</summary>
        public bool IsStatic { get; private set; }

        /// <summary>Indicates if one of the parameters is of type 'params'.</summary>
        public bool HasParams { get; private set; }

        public bool IsBindingOverride { get; private set; }

        /// <summary>
        /// Instance of a class on which to invoke the method. The object
        /// was instanciated by the user before registration. Can be null
        /// if no object was provided by the user (in this case only static methods
        /// were registered).
        /// </summary>
        public object Instance { get; private set; }

        /// <summary>
        /// Indicates if the method should be invoked statically or on the
        /// object provided by Instance.
        /// </summary>
        public bool CallByInstance { get { return Instance != null; } }

        public VineMethodInfo(MethodInfo methodRef, List<VineMethodParameter> parameters,
            string module="", object instance=null)
        {
            MethodRef   = methodRef;
            Name        = methodRef.Name;
            ReturnType  = methodRef.ReturnType;
            Parameters  = parameters;
            Module      = module;
            IsStatic    = methodRef.IsStatic;
            Instance    = instance;
            HasParams   = Parameters.Any(p => p.IsParams);
            
            var attr = methodRef.GetCustomAttribute(typeof(VineBinding), false);
            IsBindingOverride = (attr as VineBinding).Override;
        }

        /// <summary>
        /// Count the number of arguments that are not of type params.
        /// </summary>
        /// <param name="args">Array of arguments to count.</param>
        /// <returns>The number of arguments that are not of type params.</returns>
        public int GetLengthWithoutParams(object[] args)
        {
            return HasParams
                ? Math.Min(args.Length, Parameters.Count - 1)
                : args.Length;
        }

        /// <summary>
        /// Count the number of arguments that are not of type params.
        /// </summary>
        /// <param name="args">Array of arguments to count.</param>
        /// <param name="parameters">List of parameters to compare args to.</param>
        /// <returns>The number of arguments that are not of type params.</returns>
        public static int GetLengthWithoutParams(object[] args, List<VineMethodParameter> parameters)
        {
            return parameters.Count > 0 && parameters.Last().IsParams
                ? Math.Min(args.Length, parameters.Count - 1)
                : args.Length;
        }

        public override string ToString()
        {
            return GetSignature(true, true);
        }
        
        /// <summary>
        /// Signature of the method.
        /// </summary>
        /// <param name="useShortName">
        /// Indicates if the types should be displayed with their
        /// full namespace or not.
        /// </param>
        /// <param name="showReturnType">
        /// Indicates whether the return type should be displayed.
        /// </param>
        /// <returns>The formatted signature.</returns>
        public string GetSignature(bool useShortName=false, bool showReturnType=false)
        {
            string @params = Parameters.Count == 0
                ? "void"
                : String.Join(", ",
                    Parameters.Select(p => p.GetSignature(useShortName))
                );

            string returnType = showReturnType 
                ? useShortName 
                    ? ReturnType.Name + " "
                    : ReturnType.ToString() + " "
                : "";

            return string.Format(
                "{0}{1}{2}({3})",
                returnType,
                string.IsNullOrWhiteSpace(Module) ? "" : Module + "::",
                Name,
                @params
            );
        }
    }
}
