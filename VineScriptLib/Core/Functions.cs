using System;
using System.Collections.Generic;
using System.Reflection;

namespace VineScriptLib.Core
{
    /// <summary>
    /// TODO: delete when sure it won't be used. See FunctionsCollection instead.
    /// </summary>
    class Functions : IFunctionsCollection
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

        public bool Call(string name, out VineValue result, object context, params object[] args)
        {
            // if needed later, exemple of params args:
            // http://stackoverflow.com/questions/6484651/calling-a-function-using-reflection-that-has-a-params-parameter-methodbase
            
            Type cls;
            if (functions.TryGetValue(name, out cls)) {
                // get static method info by name from it's type
                MethodInfo method = cls.GetMethod(name);
                // get method's parameters info
                ParameterInfo[] parameters = method.GetParameters();
                
                // context and args must be combined in an array: combinedArgs = [context, args];
                object[] combinedArgs;
                if (parameters.Length == 2 && parameters[1].ParameterType == typeof(Object[])) {
                    // if the parameters accepts only 1 argument (not counting context) and it's of type Object[]
                    combinedArgs = new object[2];
                    combinedArgs[0] = context;
                    combinedArgs[1] = args;
                } else {
                    // if the parameters accepts multiple or zero args
                    combinedArgs = new object[1 + args.Length];
                    combinedArgs[0] = context;
                    if (args.Length > 0) {
                        args.CopyTo(combinedArgs, 1);
                    }
                }
                
                // calling the static method by name
                result = new VineValue(method.Invoke(null /*null for static methods*/, combinedArgs));
                return true;
            } else {
                result = null;
                return false;
            }
        }
    }
}
