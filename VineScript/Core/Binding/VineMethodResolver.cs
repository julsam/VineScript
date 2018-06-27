using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using VineScript.Core;
using VineScript.Core.VineValue;
using System.Runtime.ExceptionServices;

namespace VineScript.Binding
{
    public class VineMethodResolver
    {
        private List<VineMethodInfo> bindings = new List<VineMethodInfo>();
            
        static readonly BindingFlags allMethodsFlags =  BindingFlags.NonPublic
                                                    |   BindingFlags.Public
                                                    |   BindingFlags.Static
                                                    |   BindingFlags.Instance;

        public VineMethodResolver()
        {
        }

        public void Unregister(string name, params Type[] parametersType)
        {
            // TODO
            throw new NotImplementedException();
        }

        public bool Register(object instance, string methodName,
            params Type[] parametersType)
        {
            if (instance == null) {
                throw new VineBindingException(
                    methodName, "Trying to bind a method to a null object."
                );
            }

            Type type = instance.GetType();

            // Checks if the class is valid
            CheckClassFlags(type);

            MethodInfo[] methods = type.GetMethods(allMethodsFlags)
                .Where(m => m.IsDefined(typeof(VineBinding), false))
                .ToArray();

            foreach (MethodInfo method in methods)
            {
                // Name
                if (method.Name != methodName) {
                    continue;
                }

                // Checks if the method is valid
                try {
                    CheckMethodFlags(method, type.Name, true);
                } catch (Exception) {
                    continue;
                }

                // Checks there the same number of parameters
                var expectedParameters = method.GetParameters();
                if (parametersType.Length != expectedParameters.Length) {
                    continue;
                }

                // Checks that the parameters type are the same
                bool diffType = false;
                for (int i = 0; i < expectedParameters.Length; i++)
                {
                    if (expectedParameters[i].ParameterType != parametersType[i]) {
                        diffType = true;
                        break;
                    }
                }
                if (!diffType) {
                    // Found it!
                    var binding = AddBinding(method, "", instance);
#if DEBUG
                    Console.WriteLine("Registered: " + binding);
#endif
                    return true;
                }
            }
            throw new VineBindingException(methodName,
                "The method cannot be found in the object provided. Make sure"
                + " that the name is correct, the parameters type are corrects,"
                + "  the method and class are visibles, and the attribute"
                + " '[VineBinding]' is not missing."
            );
        }

        public bool Register(Type cls, string module="")
        {
            // Checks if the class is valid
            CheckClassFlags(cls);
                
            // Everything needs to be included at first, so these 4 flags are necessary
            MethodInfo[] methods = cls.GetMethods(allMethodsFlags)
                .Where(m => m.IsDefined(typeof(VineBinding), false))
                .ToArray();
            
            foreach (MethodInfo method in methods)
            {
                // Checks if the method is valid
                CheckMethodFlags(method, cls.Name, false);

                var binding = AddBinding(method, module);
#if DEBUG
                Console.WriteLine("Registered: " + binding);
#endif
            }
            return methods.Length > 0;
        }

        private VineMethodInfo AddBinding(MethodInfo method, string module,
            object instance=null)
        {
            // Parameters
            List<VineMethodParameter> pInfo = new List<VineMethodParameter>();
            foreach (ParameterInfo parameter in method.GetParameters()) {
                pInfo.Add(new VineMethodParameter(parameter));
            }

            // Method Info
            VineMethodInfo binding = new VineMethodInfo(
                method, pInfo, module, instance
            );

            bindings.Add(binding);
            return binding;
        }

        private Array MakeParams(object[] args, Type paramsType, int paramsPos=0)
        {
            //if (args[args.Length - 1] is Array) {
            //    // Unfold
            //    int extraLength = (args[args.Length - 1] as Array).Length;
            //}
            Array extra = Array.CreateInstance(paramsType, args.Length - paramsPos);
            for (int i = 0; i < extra.Length; i++) {
                var converted = Arg2Parameter(args[i + paramsPos], paramsType);
                extra.SetValue(converted, i);
            }
            return extra;
        }

        private List<object> ConvertArgs(object[] args, List<VineMethodParameter> parameters,
            bool hasParams=false)
        {
            List<object> convertedArgs = new List<object>();
            int normalArgsCount = VineMethodInfo.GetLengthWithoutParams(args, parameters);
            // loop throught args and stops before params
            for (int i = 0; i < normalArgsCount; i++)
            {
                convertedArgs.Add(Arg2Parameter(
                    args[i], parameters[i].ParameterType
                ));
            }

            // Default parameters
            if (args.Length < parameters.Count) {
                for (int i = args.Length; i < parameters.Count; i++) {
                    if (parameters[i].HasDefaultValue) {
                        convertedArgs.Add(Type.Missing);
                    } else {
                        break;
                    }
                }
            }

            if (parameters.Count > 0 && parameters.Last().IsParams) {
                convertedArgs.Add(
                    MakeParams(args, parameters.Last().GetRealType(), normalArgsCount)
                );
            }
            return convertedArgs;
        }

        private Array FlattenLastParameter(object[] args)
        {
            List<object>  list = new List<object>();
            list.AddRange(args.Where(a => a != args.Last()));
            list.AddRange((args.Last() as VineVar).AsArray);
            return list.ToArray();
        }

        public bool Exists(string name, params object[] args)
        {
            return Find(name, args) != null;
        }

        private bool FindUndefinedArg(object[] args, List<VineMethodParameter> parameters,
            out VineVar foundArg)
        {
            for (int i = 0; i < args.Length; i++)
            {
                // only out parameters can be undefined
                VineVar vinevar = (args[i] as VineVar);
                if (vinevar.IsUndefined && !parameters[i].IsOut) {
                    foundArg = vinevar;
                    return true;
                }
            }
            foundArg = null;
            return false;
        }

        public bool Call(string name, out VineVar result, object ctx, params object[] args)
        {
            var method = Find(name, args);
            if (method != null)
            {
                VineVar undefArg;
                if (FindUndefinedArg(args, method.Parameters, out undefArg)) {
                    // only out parameters can be undefined
                    throw new VineUndefinedVarException(
                        undefArg.name,
                        "Please declare your variables with the command"
                        + " '<< set >>' before passing them through a"
                        + " function. Only functions with parameters"
                        + " of type 'out' are"
                        + " allowed to be called with undefined variables."
                    );
                }

                object[] unfoldedArgs = args;
                if (args.Length > 0 && method.HasParams) {
                    if ((args.Last() as VineVar).IsArray) {
                        unfoldedArgs = FlattenLastParameter(args) as object[];
                    }
                }
                List<object> combinedArgs = ConvertArgs(unfoldedArgs, method.Parameters);
                
                // init return value
                object returnValue = null;
                // init final parameters
                object[] refCombinedArgs = combinedArgs.ToArray();

                //--------
                // CALLING
                //--------
#if DEBUG
                Console.WriteLine("Calling: " + method);
#endif
                try {
                    if (method.CallByInstance) {
                        returnValue = method.MethodRef.Invoke(method.Instance, refCombinedArgs);
                    } else {
                        returnValue = method.MethodRef.Invoke(null, refCombinedArgs);
                    }
                } catch (Exception e) {
                    Exception actualException = e.InnerException ?? e;
                    var vineStack = new VineBindingStackTrace(
                        method.MethodRef, actualException.StackTrace, method.ToString()
                    );
                    actualException.Data.Add("VineStackTrace", vineStack);
                    ExceptionDispatchInfo.Capture(actualException).Throw();
                    throw;
                }

                // Sets result
                if (returnValue == null) {
                    result = null;
                } 
                else
                {
                    if (typeof(IVineValue).IsAssignableFrom(returnValue.GetType())) {
                        result = (returnValue as VineValue).ToVineVar();
                    } else if (returnValue.GetType() == typeof(VineVar)) {
                        result = new VineVar(returnValue);
                    } else {
                        result = new VineVar(returnValue);
                        //throw new Exception("Vine Function returning an unauthorized type");
                    }
                }
                
                // Sets back values of ref/out parameters
                for (int i = 0; i < method.GetLengthWithoutParams(args); i++)
                {
                    if (method.Parameters[i].IsByRef) {
                        string varName = (args[i] as VineVar).name;
                        if (refCombinedArgs[i] is VineValue) {
                            args[i] = (refCombinedArgs[i] as VineValue).ToVineVar();
                        } else {
                            args[i] = new VineVar(refCombinedArgs[i]);
                        }
                        //VineVar a = new VineVar(args[i]);
                        (args[i] as VineVar).name = varName;
                        ((RuntimeVars)ctx)[varName] = (args[i] as VineVar);
                    }
                }
                return true;
            }
            else
            {
                throw new VineBindingException(name, 
                    "The method cannot be called because it wasn't found."
                    + " Make sure that the name is correct,"
                    + " the method and class are visibles, the attribute"
                    + " '[VineBinding]' is not missing, and that the"
                    + " method is correctly bound to VineScript."
                );
            }
        }

        private object Arg2Parameter(object arg, Type expectedType)
        {
            if (expectedType.IsByRef) {
                expectedType = expectedType.GetElementType();
            }

            if (arg.GetType().IsAssignableFrom(expectedType)) {
                return arg;
            }
            else if (arg is VineVar)
            {
                Debug.Assert(arg is VineVar);
                VineVar vinevar = arg as VineVar;
                if (vinevar.IsBool)
                {
                    if (expectedType == typeof(VineBool)) {
                        return new VineBool(vinevar);
                    }
                    return vinevar.AsBool;
                }
                else if (vinevar.IsInt)
                {
                    if (expectedType == typeof(VineInt)) {
                        return new VineInt(vinevar);
                    } 
                    return vinevar.AsInt;
                }
                else if (vinevar.IsNumber)
                {
                    if (expectedType == typeof(VineNumber)) {
                        return new VineNumber(vinevar);
                    }
                    return vinevar.AsNumber;
                }
                else if (vinevar.IsString)
                {
                    if (expectedType == typeof(VineString)) {
                        return new VineString(vinevar);
                    }
                    return vinevar.AsString;
                }
                else if (vinevar.IsArray)
                {
                    if (expectedType == typeof(VineArray)) {
                        return new VineArray(vinevar);
                    }
                    return vinevar.AsArray;
                }
                else if (vinevar.IsDict)
                {
                    if (expectedType == typeof(VineDictionary)) {
                        return new VineDictionary(vinevar);
                    }
                    return vinevar.AsDict;
                }
                else if (vinevar.IsNull)
                {
                    return vinevar;
                }
                Debug.Assert(true, "Should never reaches this.");
            }
            //else if (arg is Array) // array like VineVar[] or int[], used for params
            //{
            //    return new VineVar(arg);
            //}
            return null;
        }

        private VineMethodInfo Find(string name, params object[] args)
        {
            // Checks for any excessive amount of '.' in the name
            if (name.Count(c => c == '.') > 1) {
                throw new VineBindingException(name, string.Format(
                    "Trying to call a function with an excessive amount of"
                    + "  '.' in the name. The pattern should be"
                    + " 'ModuleName.FunctionName' or just 'FunctionName'"
                    + " if the function is not part of any module.",
                    name
                ));
            }

            string[] fullname = { "", "" };
            if (name.Contains(".")) {
                    fullname = name.Split(new[] { '.' });
            } else {
                fullname[1] = name;
            }

            VineMethodInfo match = null;
            foreach (VineMethodInfo bind in bindings)
            {
                if (bind.Module == fullname[0] && bind.Name == fullname[1])
                {
                    if (ArgumentListMatches(bind, args))
                    {
                        // Found it!
                        match = bind;
                        break;
                    }
                }
            }
            return match;
        }

        private bool ArgumentListMatches(VineMethodInfo bind, object[] args)
        {
            // params position
            int paramsPosition = -1;
            // Type of the params parameter
            Type paramsType = null;
            if (bind.HasParams) {
                // params position
                paramsPosition = bind.Parameters.Count - 1;
                // Type of the params parameter
                paramsType = bind.Parameters.Last().ParameterType.GetElementType();
            }

            if (bind.HasParams)
            {
                // the params parameter can be empty
                if (args.Length < bind.Parameters.Count - 1) {
                    //return false;
                }
            }
            else
            {
                // if there are less parameters, skip it
                // (last parameter(s) could be optional so if fine to have more args
                // than parameters)
                if (bind.Parameters.Count < args.Length) {
                    return false;
                }
            }

            // checking arguments against parameters
            for (int i = 0; i < args.Length; i++)
            {
                int j = (bind.HasParams && i > paramsPosition) ? paramsPosition : i;
                
                Type expectedType = bind.Parameters[j].GetRealType();
                if (!IsAssignable((VineVar)args[i], expectedType, bind.HasParams)) {
                    return false;
                }
            }

            // make sure the last set of parameters are actually optionals
            return bind.Parameters.Skip(args.Length)
                .All(p => p.HasDefaultValue || p.IsParams);
        }

        #region acceptedTypes
        static readonly List<Type> acceptedBool = new List<Type>() {
            typeof(bool), typeof(VineBool)
        };

        static readonly List<Type> acceptedInt = new List<Type>() {
            typeof(int), typeof(VineInt)
        };

        static readonly List<Type> acceptedNumber = new List<Type>() {
            typeof(double), typeof(float), typeof(VineNumber)
        };

        static readonly List<Type> allNumerical = new List<Type>() {
            typeof(int), typeof(VineInt), typeof(double), typeof(float), typeof(VineNumber)
        };

        static readonly List<Type> acceptedString = new List<Type>() {
            typeof(string), typeof(VineString)
        };
        #endregion acceptedTypes

        private bool IsAssignable(VineVar from, Type to, bool hasParams=false)
        {
            // both are VineVar
            if (to.IsAssignableFrom(from.GetType())) {
                return true;
            }

            if (from.IsBool && acceptedBool.Any(t => t == to)) {
                return true;
            } else if (from.IsInt && allNumerical.Any(t => t == to)) {
                return true;
            } else if (from.IsNumber && acceptedNumber.Any(t => t == to)) {
                return true;
            } else if (from.IsString && acceptedString.Any(t => t == to)) {
                return true;
            } else if (from.IsArray && to == typeof(VineArray)) {
                return true;
            } else if (from.IsDict && to == typeof(VineDictionary)) {
                return true;
            } else if (from.IsNull) {
                return true;
            }
            else if (from.IsArray && hasParams)
            {
                return from.AsArray.All(el => IsAssignable(el, to));
            }
            return false;
        }

        private void CheckClassFlags(Type cls)
        {
            if (!cls.IsClass) {
                throw new VineBindingException(cls.Name,
                    "The given type is not a class."
                );
            }
            else if (cls.IsAbstract) {
                throw new VineBindingException(cls.Name,
                    "Can't register an abstract class."
                );
            }
            else if (!cls.IsPublic && !cls.IsNestedPublic) {
                throw new VineBindingException(cls.Name,
                    "Can't register a non-public class."
                );
            }
        }

        private void CheckMethodFlags(MethodInfo method, string module, bool isInstance)
        {
            if (method.IsAbstract) {
                throw new VineBindingException(module, method.Name,
                    "Can't register an abstract method."
                );
            }
            else if (!method.IsPublic) {
                throw new VineBindingException(module, method.Name,
                    "Can't register a non-public method."
                );
            }
            else if (!method.IsStatic && !isInstance) {
                // does not use any instance, so static only
                throw new VineBindingException(module, method.Name,
                    "Can't register a non-static method."
                );
            }
            else if (method.IsStatic && isInstance) {
                // use an instance of the object, so non-static only
                throw new VineBindingException(module, method.Name,
                    "Can't register a static method."
                );
            }
            else if (method.IsGenericMethod) {
                throw new VineBindingException(module, method.Name,
                    "Can't register a generic method."
                );
            }
            else if (method.IsConstructor) {
                throw new VineBindingException(module, method.Name,
                    "Can't register a class constructor."
                );
            }
        }
    }
}
