using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Runtime.ExceptionServices;
using VineScript.Core.VineValue;

namespace VineScript.Core
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
                while (combinedArgs.Count < parameters.Length) {
                    combinedArgs.Add(Type.Missing);
                }
                // TODO if (combinedArgs.Count > parameters.Length) {
                //  throw new Exception();
                //}


                // Arguments are usually of type VineVar, but "normal" types are allowed.
                // This part try to convert arguments to the types expected by the function.
                // If VineVar is expected, do nothing. Else, converts to bool, int, float,
                // double, string.
                // (skip the first argument because it's the context)
                for (int i = 1; i < parameters.Length; i++)
                {
                    if (    parameters[i].ParameterType != typeof(VineVar)
                        &&  combinedArgs[i].GetType() != typeof(Missing))
                    {
                        var vinevar = ((VineVar)combinedArgs[i]);
                        if (parameters[i].ParameterType == typeof(VineBool)) {
                            combinedArgs[i] = new VineBool(vinevar);
                        } else if (parameters[i].ParameterType == typeof(VineInt)) {
                            combinedArgs[i] = new VineInt(vinevar);
                        } else if (parameters[i].ParameterType == typeof(VineNumber)) {
                            combinedArgs[i] = new VineNumber(vinevar);
                        } else if (parameters[i].ParameterType == typeof(VineString)) {
                            combinedArgs[i] = new VineString(vinevar);
                        } else if (parameters[i].ParameterType == typeof(VineArray)) {
                            combinedArgs[i] = new VineArray(vinevar);
                        } else if (parameters[i].ParameterType == typeof(VineDictionary)) {
                            combinedArgs[i] = new VineDictionary(vinevar);
                        } else {
                            // TODO: should throw a custom exception
                            // TODO: we should do this check on library loading and inform the 
                            //       user right away.
                            //       Doing this while the script is running is not the best time. 
                            //       Plus, this error has nothing to do with the script, it's the
                            //       function definition that should be fixed.
                            throw new Exception(string.Format("Error calling function \"{0}\""
                                + "\nThe argument type \"{1}\" is not a recognized type by VineScript."
                                + "\nExpected types are: VineVar, VineBool, VineInt, VineNumber,"
                                + "VineString, VineArray or VineDictionary",
                                name, parameters[i].ParameterType.Name
                            ));
                        }
                    }
                }

                // calling the static method by name
                try {
                    result = new VineVar(method.Invoke(null /*null for static methods*/, combinedArgs.ToArray()));
                    return true;
                } catch (TargetInvocationException e) {
                    // Can we give the file, line, char and function name(with args) ?
                    // Should add this message + stack trace to the InnerException.message
                    Console.WriteLine("Exception when calling function '" + name + "'");
                    Console.WriteLine(e?.InnerException.StackTrace ?? e.StackTrace);
                    // This lets you capture an exception and re-throw it without losing the stack trace:
                    ExceptionDispatchInfo.Capture(e?.InnerException ?? e).Throw();
                    throw;
                } catch (TargetParameterCountException e) {
                    // Can we give the file, line, char and function name(with args) ?
                    // Should add this message + stack trace to the InnerException.message
                    Console.WriteLine("Exception when calling function '" + name + "'");
                    Console.WriteLine(e.StackTrace);
                    // This lets you capture an exception and re-throw it without losing the stack trace:
                    ExceptionDispatchInfo.Capture(e).Throw();
                    throw;
                }
            } else {
                result = VineVar.NULL;
                return false;
            }
        }
    }
}