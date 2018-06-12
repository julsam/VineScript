using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VineScript.Core.VineValue
{
    public class VineDictionary : VineValue<VineDictionary>, IDictionary<string, object>
    {
        private readonly Dictionary<string, VineVar> mirroredDict;
        private Dictionary<string, dynamic> vineValues
        {
            get {
                return Converter.ToDictOfVineValues(mirroredDict);
            }
        }

        public override VineDictionary Value
        {
            get {
                return this;
            }
        }

        public VineDictionary(VineVar value) : base(value ?? VineVar.newDict, VineVar.Type.Dict)
        {
            mirroredDict = vinevar.AsDict;
        }

        public VineDictionary() : base(VineVar.newDict, VineVar.Type.Dict)
        {
            mirroredDict = vinevar.AsDict;
        }
        
        #region implicit conversion

        public static implicit operator VineDictionary(Dictionary<string, VineVar> value)
        {
            return new VineDictionary(new VineVar(value));
        }

        public static implicit operator VineDictionary(Dictionary<string, bool> value)
        {
            return new VineDictionary(new VineVar(value));
        }

        public static implicit operator VineDictionary(Dictionary<string, int> value)
        {
            return new VineDictionary(new VineVar(value));
        }

        public static implicit operator VineDictionary(Dictionary<string, double> value)
        {
            return new VineDictionary(new VineVar(value));
        }

        public static implicit operator VineDictionary(Dictionary<string, float> value)
        {
            return new VineDictionary(new VineVar(value));
        }

        public static implicit operator VineDictionary(Dictionary<string, string> value)
        {
            return new VineDictionary(new VineVar(value));
        }

        public static implicit operator VineDictionary(Dictionary<string, dynamic> value)
        {
            return new VineDictionary(new VineVar(value));
        }

        //public static implicit operator VineDictionary(Dictionary<string, IList> value)
        //{
        //    return new VineDictionary(value);
        //}

        //public static implicit operator VineDictionary(Dictionary<string, IDictionary> value)
        //{
        //    return new VineDictionary(value);
        //}

        public static implicit operator Dictionary<string, VineVar>(VineDictionary value)
        {
            return value.vinevar.AsDict;
        }

        #endregion implicit conversion

        #region IList implementation

        public ICollection<string> Keys
        {
            get {
                return ((IDictionary<string, dynamic>)vineValues).Keys;
            }
        }

        public ICollection<dynamic> Values
        {
            get {
                return ((IDictionary<string, dynamic>)vineValues).Values;
            }
        }

        public int Count
        {
            get {
                return vineValues.Count;
            }
        }

        public bool IsReadOnly
        {
            get {
                return ((IDictionary<string, dynamic>)vineValues).IsReadOnly;
            }
        }

        public dynamic this[string key]
        {
            get {
                return vineValues[key];
            }

            set {
                mirroredDict[key] = Converter.ToVineValue(value)?.ToVineVar();
            }
        }

        public bool ContainsKey(string key)
        {
            return vineValues.ContainsKey(key);
        }

        public void Add(string key, dynamic value)
        {
            IVineValue converted = Converter.ToVineValue(value);
            mirroredDict.Add(key, converted?.ToVineVar());
        }

        public bool Remove(string key)
        {
            return mirroredDict.Remove(key);
        }

        public bool TryGetValue(string key, out dynamic value)
        {
            VineVar outValue;
            bool result = mirroredDict.TryGetValue(key, out outValue);
            value = Converter.ToVineValue(outValue);
            return result;
        }

        public void Add(KeyValuePair<string, dynamic> item)
        {
            IVineValue converted = Converter.ToVineValue(item.Value);
            var pair = new KeyValuePair<string, VineVar>(item.Key, converted?.ToVineVar());
            ((IDictionary<string, VineVar>)mirroredDict).Add(pair);
        }

        public void Clear()
        {
            mirroredDict.Clear();
        }

        public bool Contains(KeyValuePair<string, dynamic> item)
        {
            IVineValue converted = Converter.ToVineValue(item.Value);
            var pair = new KeyValuePair<string, VineVar>(item.Key, converted?.ToVineVar());
            return ((IDictionary<string, VineVar>)mirroredDict).Contains(pair);
        }

        public void CopyTo(KeyValuePair<string, dynamic>[] array, int arrayIndex)
        {
            ((IDictionary<string, dynamic>)mirroredDict).CopyTo(array, arrayIndex);
        }

        public bool Remove(KeyValuePair<string, dynamic> item)
        {
            IVineValue converted = Converter.ToVineValue(item.Value);
            var pair = new KeyValuePair<string, VineVar>(item.Key, converted?.ToVineVar());
            return ((IDictionary<string, VineVar>)mirroredDict).Remove(pair);
        }

        public IEnumerator<KeyValuePair<string, dynamic>> GetEnumerator()
        {
            return vineValues.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return vineValues.GetEnumerator();
        }

        #endregion IList implementation
    }
}
