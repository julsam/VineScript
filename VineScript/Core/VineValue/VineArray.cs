using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VineScript.Core.VineValue
{
    public class VineArray : VineValue<VineArray>, IList<object>
    {
        private readonly List<VineVar> mirroredList;
        private List<dynamic> vineValues {
            get {
                return Converter.ToListOfVineValues(mirroredList);
            }
        }

        public override VineArray Value
        {
            get {
                return this;
            }
        }

        public VineArray(VineVar value) : base(value, VineVar.Type.Array)
        {
            mirroredList = value.AsArray;
        }

        #region implicit conversion

        public static implicit operator VineArray(VineVar value)
        {
            return new VineArray(value);
        }

        public static implicit operator VineArray(List<VineVar> value)
        {
            return new VineArray(new VineVar(value));
        }

        public static implicit operator VineArray(List<bool> value)
        {
            return new VineArray(new VineVar(value));
        }

        public static implicit operator VineArray(List<int> value)
        {
            return new VineArray(new VineVar(value));
        }

        public static implicit operator VineArray(List<double> value)
        {
            return new VineArray(new VineVar(value));
        }

        public static implicit operator VineArray(List<float> value)
        {
            return new VineArray(new VineVar(value));
        }

        public static implicit operator VineArray(List<string> value)
        {
            return new VineArray(new VineVar(value));
        }

        public static implicit operator VineArray(List<dynamic> value)
        {
            return new VineArray(new VineVar(value));
        }

        //public static implicit operator VineArray(List<IList> value)
        //{
        //    return new VineArray(value);
        //}

        //public static implicit operator VineArray(List<IDictionary> value)
        //{
        //    return new VineArray(value);
        //}

        public static implicit operator List<VineVar>(VineArray value)
        {
            return value.vinevar.AsArray;
        }

        #endregion implicit conversion
        
        #region IList implementation

        public dynamic this[int index]
        {
            get {
                return vineValues[index];
            }

            set {
                mirroredList[index] = Converter.ToVineValue(value).ToVineVar();
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
                return ((IList<dynamic>)vineValues).IsReadOnly;
            }
        }

        public void Add(dynamic item)
        {
            var converted = Converter.ToVineValue(item);
            mirroredList.Add(converted.ToVineVar());
        }

        public void Clear()
        {
            mirroredList.Clear();
        }

        public bool Contains(dynamic item)
        {
            var v = Converter.ToVineValue(item);
            return vineValues.Contains(v?.ToVineVar());
        }

        public void CopyTo(dynamic[] array, int arrayIndex)
        {
            vineValues.CopyTo(array, arrayIndex);
        }

        public IEnumerator<dynamic> GetEnumerator()
        {
            return vineValues.GetEnumerator();
        }

        public int IndexOf(dynamic item)
        {
            return vineValues.IndexOf(Converter.ToVineValue(item));
        }

        public void Insert(int index, dynamic item)
        {
            var converted = Converter.ToVineValue(item);
            mirroredList.Insert(index, converted.ToVineVar());
        }

        public bool Remove(dynamic item)
        {
            var converted = Converter.ToVineValue(item);
            return mirroredList.Remove(converted?.ToVineVar());
        }

        public void RemoveAt(int index)
        {
            mirroredList.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return vineValues.GetEnumerator();
        }

        #endregion IList implementation
    }
}
