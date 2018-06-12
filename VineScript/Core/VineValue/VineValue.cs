using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VineScript.Core.VineValue
{
    public interface IVineValue
    {
        VineVar ToVineVar();
    }

    public interface IVineValueType<T>
    {
        T Value { get; }
    }

    public abstract class VineValue : IVineValue, IEquatable<object>
    {
        protected VineVar vinevar;

        public VineValue(VineVar value, VineVar.Type type)
        {
            if (value?.type == type) {
                vinevar = value;
            } else {
                throw new VineConversionException(string.Format(
                    "Can't create a VineValue of type {0} when"
                    + " the underlying VineVar type is {1}",
                    type, value?.type.ToString() ?? "<null>"
                ));
            }
        }

        public override int GetHashCode()
        {
            return vinevar.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (obj == null) {
                return false;
            }

            if (obj is VineValue == false) {
                obj = Converter.ToVineValue(obj);
            }
            return ToVineVar() == ((VineValue)obj).ToVineVar();
        }

        public VineVar ToVineVar()
        {
            return vinevar;
        }

        public override string ToString()
        {
            return vinevar.ToString();
        }

        public static bool operator == (VineValue a, object b)
        {
            if (!object.ReferenceEquals(a, null)) {
                return a.Equals(b);
            }
            if (!object.ReferenceEquals(b, null)) {
                return b.Equals(a);
            }
            // they're both null
            return true;
        }

        public static bool operator != (VineValue a, object b)
        {
            if (!object.ReferenceEquals(a, null)) {
                return !a.Equals(b);
            }
            if (!object.ReferenceEquals(b, null)) {
                return !b.Equals(a);
            }
            // they're both null: they're not different
            return false;
        }
    }

    /// <summary>
    /// Base class for VineValues, which are wrappers around VineVar.
    /// </summary>
    public abstract class VineValue<T> : VineValue, IVineValueType<T>
    {
        public abstract T Value { get; }

        public VineValue(VineVar value, VineVar.Type type)
            : base(value, type)
        { }
        
        #region implicit conversion

        //public static implicit operator VineVar(VineValue<T> value)
        //{
        //    return value.ToVineVar();
        //}

        //public static implicit operator VineValue<T>(bool value)
        //{
        //    return new VineBool(value) as VineValue<T>;
        //}

        //public static implicit operator VineValue<T>(int value)
        //{
        //    return new VineInt(value) as VineValue<T>;
        //}

        //public static implicit operator VineValue<T>(double value)
        //{
        //    return new VineNumber(value) as VineValue<T>;
        //}

        //public static implicit operator VineValue<T>(float value)
        //{
        //    return new VineNumber(value) as VineValue<T>;
        //}

        //public static implicit operator VineValue<T>(string value)
        //{
        //    return new VineString(value) as VineValue<T>;
        //}

        //public static implicit operator VineValue<T>(List<bool> value)
        //{
        //    return new VineArray(value) as VineValue<T>;
        //}

        //public static implicit operator VineValue<T>(List<int> value)
        //{
        //    return new VineArray(value) as VineValue<T>;
        //}

        //public static implicit operator VineValue<T>(List<double> value)
        //{
        //    return new VineArray(value) as VineValue<T>;
        //}

        //public static implicit operator VineValue<T>(List<string> value)
        //{
        //    return new VineArray(value) as VineValue<T>;
        //}

        //public static implicit operator VineValue<T>(List<IList> value)
        //{
        //    return new VineArray(value) as VineValue<T>;
        //}

        //public static implicit operator VineValue<T>(List<IDictionary> value)
        //{
        //    return new VineArray(value) as VineValue<T>;
        //}

        //public static implicit operator VineValue<T>(Dictionary<string, bool> value)
        //{
        //    return new VineDictionary(value) as VineValue<T>;
        //}

        //public static implicit operator VineValue<T>(Dictionary<string, int> value)
        //{
        //    return new VineDictionary(value) as VineValue<T>;
        //}

        //public static implicit operator VineValue<T>(Dictionary<string, double> value)
        //{
        //    return new VineDictionary(value) as VineValue<T>;
        //}

        //public static implicit operator VineValue<T>(Dictionary<string, string> value)
        //{
        //    return new VineDictionary(value) as VineValue<T>;
        //}

        //public static implicit operator VineValue<T>(Dictionary<string, IList> value)
        //{
        //    return new VineDictionary(value) as VineValue<T>;
        //}

        //public static implicit operator VineValue<T>(Dictionary<string, IDictionary> value)
        //{
        //    return new VineDictionary(value) as VineValue<T>;
        //}

        #endregion implicit conversion
    }
}
