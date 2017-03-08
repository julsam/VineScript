using System;
using System.Collections.Generic;
using System.Reflection;

namespace VineScriptLib.Core
{
    /// <summary>
    /// TODO: delete. This is an old version of Filters.cs. 
    /// </summary>
    class FiltersOld
    {
        public delegate object FilterCallback(object context, params object[] args);

        private Dictionary<string, FilterCallback> filters = new Dictionary<string, FilterCallback>();

        public void Register(string name, FilterCallback callback)
        {
            filters.Add(name, callback);
        }

        public bool Unregister(string name)
        {
            return filters.Remove(name);
        }

        public bool Exists(string name)
        {
            return filters.ContainsKey(name);
        }

        public bool Call(string name, out object result, object context, params object[] args)
        {
            FilterCallback callback;
            if (filters.TryGetValue(name, out callback)) {
                result = callback(context, args);
                return true;
            } else {
                result = null;
                return false;
            }
        }

    }
}
