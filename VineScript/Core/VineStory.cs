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
        private VineLibrary userlib;

        // Vars
        // Change the type to dynamic to have the DynamicObject feature to work
        public RuntimeVars vars { get; private set; }

        public PassageScript currentScript { get; private set; }

        public PassageResult currentPassage { get; private set; }
        public List<PassageResult> history { get; private set; }

        public VineStory(string loadDir, VineLibrary userlib=null)
        {
            var loader = new Loader(loadDir);
            loader.LoadFromDir();
            Init(loader, userlib);
        }

        public VineStory(Loader loader=null, VineLibrary userlib=null)
        {
            Init(loader, userlib);
        }

        private void Init(Loader loader, VineLibrary userlib)
        {
            // std
            std = new StdLibrary(this);
            std.RegisterLib();
            
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
                RegisterUserLib(userlib);
            }
        }

        public PassageResult RunPassage(string scriptname)
        {
            if (loader.LoadedScriptsCount == 0) {
                throw new Exception("The Loader doesn't contain any scripts!");
            }

            currentPassage = null;

            currentScript = loader.Get(scriptname);
            if (currentScript != null) {
                currentPassage = interpreter.Execute(currentScript);
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

        /// <summary>
        /// Register User Library
        /// </summary>
        /// <param name="lib"></param>
        public void RegisterUserLib(VineLibrary lib)
        {
            userlib = lib;
            userlib.RegisterLib();
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

        public VineVar CallFunction(string name, params object[] args)
        {
            VineVar output = VineVar.NULL;

            if (userlib != null && userlib.resolver != null && userlib.resolver.Exists(name, args)) {
                if (userlib.resolver.Call(name, out output, vars, args)) {
                    return output;
                } else {
                    throw new Exception(string.Format("Error calling function '{0}' in user lib", name));
                }
            } else if (std.resolver.Exists(name, args)) {
                if (std.resolver.Call(name, out output, vars, args)) {
                    return output;
                } else { 
                    throw new Exception(string.Format("Error calling function '{0}' in std lib", name));
                }
            } else {
                throw new Exception(string.Format("Calling unknown function '{0}'", name));
            }
        }
    }
}