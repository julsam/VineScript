using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VineScript.Binding;
using VineScript.Core.VineValue;

namespace VineScript.Core
{
    public interface IVineLibrary
    {
        void BindLibrary();
        void Register(object instance, string methodName, params Type[] paramsType);
        //bool Register(Type cls, object instance, string module);
        void Register(Type cls, string module);
    }

    public abstract class VineLibrary
    {
        protected List<Tuple<Type, string>> staticsList
            = new List<Tuple<Type, string>>();

        protected List<Tuple<object, string, Type[]>> instancesList 
            = new List<Tuple<object, string, Type[]>>();

        public VineLibrary()
        {
        }

        public void Register(Type cls, string module="")
        {
            staticsList.Add(new Tuple<Type, string>(cls, module));
        }

        public void Register(object instance, string methodName, params Type[] paramsType)
        {
            instancesList.Add(new Tuple<object, string, Type[]>(
                instance, methodName, paramsType
            ));
        }

        /// <summary>
        /// Starts the registration process.
        /// </summary>
        internal void BindLibrary(VineMethodResolver resolver)
        {
            foreach (var def in staticsList) {
                //                class,     module
                resolver.Register(def.Item1, def.Item2);
            }
            foreach (var def in instancesList) {
                //                instance,  methodname, paramsType
                resolver.Register(def.Item1, def.Item2, def.Item3);
            }
            // Remove them so the methods won't be registered again
            // if BindLibrary() is called again.
            staticsList.Clear();
            instancesList.Clear();
        }
    }

    public sealed class StdLibrary : VineLibrary
    {
        Lib.StoryState storyStateLib;

        public StdLibrary(VineStory story)
        {
            storyStateLib = new Lib.StoryState(story);
            
            Register(typeof(Lib.Rand));
            Register(typeof(Lib.Std));
            Register(typeof(Lib.Date));
            Register(typeof(Lib.Math));
            Register(typeof(Lib.Sequence));
            Register(typeof(Lib.Array));
            Register(typeof(Lib.Dictionary));
            Register(typeof(Lib.String));
            Register(storyStateLib, "History");
            Register(storyStateLib, "CurrentPassage");
        }
    }

    public class UserLib : VineLibrary
    {
        public UserLib()
        {
        }
    }
}