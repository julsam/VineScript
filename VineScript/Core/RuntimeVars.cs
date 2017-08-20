using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

namespace VineScript.Core
{
    public class RuntimeVars : DynamicObject, IDictionary<string, VineVar>
    {
        private Dictionary<string, VineVar> __VineVars__ = new Dictionary<string, VineVar>();

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (this.ContainsKey(binder.Name)) {
                result = this[binder.Name].AsObject;
            } else {
                result = VineVar.NULL;
            }
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            this[binder.Name] = new VineVar(value);

            // You can always add a value to a dictionary,
            // so this method always returns true.
            return true;
        }

        public VineVar this[string key]
        {
            get {
                return ((IDictionary<string, VineVar>)__VineVars__)[key];
            }

            set {
                ((IDictionary<string, VineVar>)__VineVars__)[key] = value;
                value.name = key;
            }
        }

        public int Count
        {
            get {
                return ((IDictionary<string, VineVar>)__VineVars__).Count;
            }
        }

        public bool IsReadOnly
        {
            get {
                return ((IDictionary<string, VineVar>)__VineVars__).IsReadOnly;
            }
        }

        public ICollection<string> Keys
        {
            get {
                return ((IDictionary<string, VineVar>)__VineVars__).Keys;
            }
        }

        public ICollection<VineVar> Values
        {
            get {
                return ((IDictionary<string, VineVar>)__VineVars__).Values;
            }
        }

        public void Add(KeyValuePair<string, VineVar> item)
        {
            ((IDictionary<string, VineVar>)__VineVars__).Add(item);
        }

        public void Add(string key, VineVar value)
        {
            ((IDictionary<string, VineVar>)__VineVars__).Add(key, value);
        }

        public void Clear()
        {
            ((IDictionary<string, VineVar>)__VineVars__).Clear();
        }

        public bool Contains(KeyValuePair<string, VineVar> item)
        {
            return ((IDictionary<string, VineVar>)__VineVars__).Contains(item);
        }

        public bool ContainsKey(string key)
        {
            return ((IDictionary<string, VineVar>)__VineVars__).ContainsKey(key);
        }

        public void CopyTo(KeyValuePair<string, VineVar>[] array, int arrayIndex)
        {
            ((IDictionary<string, VineVar>)__VineVars__).CopyTo(array, arrayIndex);
        }

        public IEnumerator<KeyValuePair<string, VineVar>> GetEnumerator()
        {
            return ((IDictionary<string, VineVar>)__VineVars__).GetEnumerator();
        }

        public bool Remove(KeyValuePair<string, VineVar> item)
        {
            return ((IDictionary<string, VineVar>)__VineVars__).Remove(item);
        }

        public bool Remove(string key)
        {
            return ((IDictionary<string, VineVar>)__VineVars__).Remove(key);
        }

        public bool TryGetValue(string key, out VineVar value)
        {
            return ((IDictionary<string, VineVar>)__VineVars__).TryGetValue(key, out value);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IDictionary<string, VineVar>)__VineVars__).GetEnumerator();
        }
    }
}
