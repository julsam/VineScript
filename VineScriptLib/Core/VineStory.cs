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
            return interpreter.Evaluate(istream);
        }

        public string RunPassage(string text)
        {
            return interpreter.Evaluate(text);
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

        //public void reflectionTest()
        //{
        //    // Name of the method we want to call.
	       // string name = "Inform";

	       // // Call it with each of these parameters.

	       // // Get MethodInfo.
	       // Type type = typeof(Methods);
	       // MethodInfo info = type.GetMethod(name);
            
	       // object[] parameters = { "Sam", 22, 42.42 };
	       // info.Invoke(null, parameters);
        //}

        public void test2()
        {
            //object result = null;
            //object[] args = new object[]{ 11, 58, 666};
            //filters.Register2("foo2", typeof(VineSession));
            //filters.Call2("foo2", out result, null, args);
            //Console.WriteLine(result);
            //filters.Call2("foo2", out result, null, "foobar", "bar", 42);
            //Console.WriteLine(result);

            // foo3
            VineValue result = null;
            std.filters.Register("func1", typeof(VineStory));
            std.filters.Call("func1", out result, null, new object[] { "a", "b", "c" });
            Console.WriteLine(result);

            std.filters.Register("func2", typeof(VineStory));
            std.filters.Call("func2", out result, null, "a", "b", 42);
            Console.WriteLine(result);
            std.filters.Call("func2", out result, null, new object[] { "a", "b", "c"});
            Console.WriteLine(result);

            std.filters.Register("func3", typeof(VineStory));
            std.filters.Call("func3", out result, null, 1, new object[] { "a", "b", "c"});
            Console.WriteLine(result);

            std.filters.Register("func4", typeof(VineStory));
            std.filters.Call("func4", out result, null, "a");
            Console.WriteLine(result);

            std.filters.Register("func5", typeof(VineStory));
            std.filters.Call("func5", out result, null, "a", "b", 42);
            Console.WriteLine(result);
            std.filters.Call("func5", out result, null, new object[] { "a", "b", "c"});
            Console.WriteLine(result);
            
            //filters.Register("foo2",  (x, y) => { return 42; });
            //filters.Call("foo2", out result, null, 11, 58, 666);
            //Console.WriteLine(result);

        }
        
        public static object func1(object context, object a, object b, object c)
        {
            return a.ToString() + " " + b.ToString() + " " + c.ToString();
        }

        public static object func2(object context, object[] args)
        {
            string str = "";
            foreach (object el in args) {
                str += el.ToString() + " ";
            }
            return str;
        }

        public static object func3(object context, object a, object[] args)
        {
            string str = a.ToString();
            foreach (object el in args) {
                str += " " + el.ToString();
            }
            return str;
        }

        public static object func4(object context, object a)
        {
            return a.ToString();
        }

        public static object func5(object context, params object[] args)
        {
            string str = "func5: ";
            foreach (object el in args) {
                str += el.ToString() + " ";
            }
            return str;
        }


        public void test()
        {
            //object result = null;
            //object[] args = new object[]{ 11, 58, 666};
            //filters.Register("foo", foo);
            //filters.Call("foo", out result, null, args);
            //Console.WriteLine(result);
            
            //filters.Register("foo2",  (x, y) => { return 42; });
            //filters.Call("foo2", out result, null, 11, 58, 666);
            //Console.WriteLine(result);
        }


        private object foo(object context, params object[] args)
        {
            object r = args[0].ToString() + " " + args[1].ToString() + " " + args[2].ToString();
            return r;
        }

    }
}
