using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using VineScript.Compiler;

namespace VineScript.Core
{

    public class VineStory
    {
        private Loader loader;
        private Interpreter interpreter;

        // Std Lib
        private StdLibrary std;

        // User Lib
        private IVineLibrary userlib;

        // Vars
        // Change the type to dynamic to have the DynamicObject feature to work
        public RuntimeVars vars { get; private set; }

        public PassageResult currentPassage;
        public List<PassageResult> history { get; private set; }

        public VineStory(string loadDir, IVineLibrary userlib=null)
        {
            var loader = new Loader(loadDir);
            loader.LoadFromDir();
            Init(loader, userlib);
        }

        public VineStory(Loader loader=null, IVineLibrary userlib=null)
        {
            Init(loader, userlib);
        }

        private void Init(Loader loader, IVineLibrary userlib)
        {
            // std
            std = new StdLibrary();
            std.RegisterFunctions();
            std.RegisterFilters();
            
            // vars
            vars = new RuntimeVars();
            
            history = new List<PassageResult>();

            interpreter = new Interpreter(this);

            if (loader != null) {
                this.loader = loader;
            } else {
                this.loader = new Loader();
                this.loader.LoadFromDir();
            }

            if (userlib != null) {
                Register(userlib);
            }
        }

        public PassageResult RunPassage(string scriptname)
        {
            if (loader.LoadedScriptsCount == 0) {
                throw new Exception("The Loader doesn't contain any scripts!");
            }

            PassageScript script = loader.Get(scriptname);

            if (script != null) {
                currentPassage = new PassageResult();
                currentPassage = interpreter.Execute(script);
                history.Add(currentPassage);
                return currentPassage;
            } else {
                throw new Exception("Couldn't find " + scriptname);
            }
        }

        public string Eval(StreamReader istream, string sourceName="<stdin>")
        {
            return interpreter.Eval(istream, sourceName);
        }

        public string Eval(string expr, string sourceName="<stdin>")
        {
            return interpreter.Eval(expr, sourceName);
        }

        public void Register(IVineLibrary lib)
        {
            userlib = lib;
            userlib.RegisterFunctions();
            userlib.RegisterFilters();
        }

        public VineVar GetVar(string name)
        {
            VineVar output;
            if (vars.TryGetValue(name, out output)) {
                return output;
            }
            return VineVar.NULL;
        }

        public void SetVar(string name, VineVar value)
        {
            vars.Add(name, value);
        }

        //public void RegisterFunction(string name, Type cls)
        //{
        //    userlib.functions.Register(name, cls);
        //}

        public VineVar CallFunction(string name, params object[] args)
        {
            VineVar output = VineVar.NULL;

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

        public VineVar CallFilter(string name, params object[] args)
        {
            VineVar output = VineVar.NULL;
            
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