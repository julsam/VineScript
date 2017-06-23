using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;

namespace VineScriptLib.Core
{

    public class VineStory
    {
        private Interpreter interpreter;

        // Std Lib
        private StdLibrary std;

        // User Lib
        private IVineLibrary userlib;

        // Vars
        public Dictionary<string, VineValue> vars { get; private set; }

        public VineStory(IVineLibrary userlib=null)
        {
            // std
            std = new StdLibrary();
            std.RegisterFunctions();
            std.RegisterFilters();
            
            // vars
            vars = new Dictionary<string, VineValue>();

            //test2();

            interpreter = new Interpreter(this);

            if (userlib != null) {
                Register(userlib);
            }
        }

        public string RunPassage(StreamReader istream)
        {
            return interpreter.Execute(istream);
        }

        public string RunPassage(string text)
        {
            return interpreter.Execute(text);
        }

        public string Eval(StreamReader istream)
        {
            return interpreter.Eval(istream);
        }

        public string Eval(string expr)
        {
            return interpreter.Eval(expr);
        }

        public void Register(IVineLibrary lib)
        {
            userlib = lib;
            userlib.RegisterFunctions();
            userlib.RegisterFilters();
        }

        public VineValue GetVar(string name)
        {
            VineValue output;
            if (vars.TryGetValue(name, out output)) {
                return output;
            }
            return VineValue.NULL;
        }

        public void SetVar(string name, VineValue value)
        {
            vars.Add(name, value);
        }

        public void RegisterFunction(string name, Type cls)
        {
            userlib.functions.Register(name, cls);
        }

        public VineValue CallFunction(string name, params object[] args)
        {
            VineValue output = VineValue.NULL;

            if (userlib != null && userlib.functions != null && userlib.functions.Exists(name)) {
                if (userlib.functions.Call(name, out output, vars, args)) {
                    return output;
                } else {
                    throw new Exception(string.Format("Error calling function '{0}' in user lib", name));
                }
            } else if (std.functions.Exists(name)) {
                if (std.functions.Call(name, out output, vars, args)) {
                    return output;
                } else { 
                    throw new Exception(string.Format("Error calling function '{0}' in std lib", name));
                }
            } else {
                throw new Exception(string.Format("Calling unknown function '{0}'", name));
            }
        }

        public VineValue CallFilter(string name, params object[] args)
        {
            VineValue output = VineValue.NULL;
            
            if (userlib != null && userlib.filters != null && userlib.filters.Exists(name)) {
                if (userlib.filters.Call(name, out output, vars, args)) {
                    return output;
                } else {
                    throw new Exception(string.Format("Error calling filter '{0}' in user lib", name));
                }
            } else if (std.filters.Exists(name)) {
                if (std.filters.Call(name, out output, vars, args)) {
                    return output;
                } else { 
                    throw new Exception(string.Format("Error calling filter '{0}' in std lib", name));
                }
            } else {
                throw new Exception(string.Format("Calling unknown filter '{0}'", name));
            }
        }
    }
}
