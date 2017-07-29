using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

namespace VineScriptLib.Core
{
    interface IFunctionsCollection
    {
        void Register(string name, Type cls);
        bool Unregister(string name);
        bool Exists(string name);
        bool Call(string name, out VineVar result, object context, params object[] args);
    }

    public class FunctionsCollection : IFunctionsCollection
    {
        private Dictionary<string, Type> functions = new Dictionary<string, Type>();

        public void Register(string name, Type cls)
        {
            functions.Add(name, cls);
        }

        public bool Unregister(string name)
        {
            return functions.Remove(name);
        }

        public bool Exists(string name)
        {
            return functions.ContainsKey(name);
        }

        public bool Call(string name, out VineVar result, object context, params object[] args)
        {
            // if needed later, exemple of params args:
            // http://stackoverflow.com/questions/6484651/calling-a-function-using-reflection-that-has-a-params-parameter-methodbase
            
            Type cls;
            if (functions.TryGetValue(name, out cls)) {
                // get static method info by name from it's type
                MethodInfo method = cls.GetMethod(name);

                // TODO ... only allow static methods
                bool isStatic = method.IsStatic;

                // get method's parameters info
                ParameterInfo[] parameters = method.GetParameters();
                
                // context and args must be combined in an array: combinedArgs = [context, args];
                List<object> combinedArgs = new List<object>();
                if (parameters.Length == 2 && parameters[1].ParameterType == typeof(Object[])) {
                    // if the parameters accepts only 1 argument (not counting context) and it's of type Object[]
                    combinedArgs.Add(context);
                    combinedArgs.Add(args);
                } else {
                    // if the parameters accepts multiple or zero args (not counting context)
                    combinedArgs.Add(context);
                    if (args.Length > 0) {
                        combinedArgs.AddRange(args);
                    }
                }

                // Arguments are usually of type VineVar, but "normal" types are allowed.
                // This part try to convert arguments to the types expected by the function.
                // If VineVar is expected, do nothing. Else, converts to bool, int, float,
                // double, string.
                // (skip the first argument because it's the context)
                for (int i = 1; i < parameters.Length; i++) {
                    if (parameters[i].ParameterType != typeof(VineVar)) {
                        if (parameters[i].ParameterType == typeof(bool)) {
                            combinedArgs[i] = ((VineVar)combinedArgs[i]).AsBool;
                        }
                        else if (parameters[i].ParameterType == typeof(string)) {
                            combinedArgs[i] = ((VineVar)combinedArgs[i]).AsString;
                        }
                        else if (parameters[i].ParameterType == typeof(int)) {
                            combinedArgs[i] = ((VineVar)combinedArgs[i]).AsInt;
                        }
                        else if (   parameters[i].ParameterType == typeof(double)
                                ||  parameters[i].ParameterType == typeof(float)
                        ) {
                            combinedArgs[i] = ((VineVar)combinedArgs[i]).AsNumber;
                        }
                        else {
                            // TODO: should throw a custom exception
                            // TODO: we should do this check on library loading and inform the 
                            //       user right away.
                            //       Doing this while the script is running is not the best time. 
                            //       Plus, this error has nothing to do with the script, it's the
                            //       function definition that should be fixed.
                            throw new Exception(string.Format("Error calling function \"{0}\""
                                + "\nThe argument type \"{1}\" is not a recognized type by VineScript."
                                + "\nExpected types are: VineVar, bool, int, float, double, string",
                                name, parameters[i].ParameterType.Name));
                        }
                    }
                }

                // calling the static method by name
                result = new VineVar(method.Invoke(null /*null for static methods*/, combinedArgs.ToArray()));
                return true;
            } else {
                result = VineVar.NULL;
                return false;
            }
        }
    }
}