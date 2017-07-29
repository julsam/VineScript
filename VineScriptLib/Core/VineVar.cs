using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace VineScriptLib.Core
{
    public class VineArithmeticException : Exception
    {
        public VineArithmeticException() : base("Not supported operation") { }

        public VineArithmeticException(string msg) : base(msg) { }

        public VineArithmeticException(string msg, Exception innerException)
            : base(msg, innerException) { }

        public VineArithmeticException(string op, VineVar a, VineVar b) 
            : base(string.Format("Operator '{0}' cannot be applied to operands of type '{1}' and '{2}'",
                op, a.type, b.type)) { }

        public VineArithmeticException(string op, string a, string b) 
            : base(string.Format("Operator '{0}' cannot be applied to operands of type '{1}' and '{2}'",
                op, a, b)) { }

        public VineArithmeticException(string op, VineVar a) 
            : base(string.Format("Unary operator '{0}' cannot be applied to operand of type '{1}' ",
                op, a.type)) { }

        public VineArithmeticException(string op, string a) 
            : base(string.Format("Unary operator '{0}' cannot be applied to operand of type '{1}' ",
                op, a)) { }
    }

    public class VineComparisonException : Exception
    {
        public VineComparisonException() : base("Not supported comparison") { }

        public VineComparisonException(string msg) : base(msg) { }

        public VineComparisonException(string msg, Exception innerException)
            : base(msg, innerException) { }

        public VineComparisonException(VineVar a, VineVar b) 
            : base(string.Format("Cannot compare equality between type '{0}' and '{1}'", a.type, b.type)) { }

        public VineComparisonException(string op, VineVar a, VineVar b) 
            : base(string.Format("Comparison '{0}' cannot be applied to operands of type '{1}' and '{2}'",
                op, a.type, b.type)) { }
    }

    public class VineConversionException : Exception
    {
        public VineConversionException() : base("Not supported cast") { }

        public VineConversionException(string msg) : base(msg) { }

        public VineConversionException(string msg, Exception innerException)
            : base(msg, innerException) { }

        public VineConversionException(VineVar.Type from, VineVar.Type to) 
            : base(string.Format("Cannot convert type '{0}' to '{1}'", from, to)) { }

        public VineConversionException(VineVar from, VineVar to) 
            : base(string.Format("Cannot convert type '{0}' to '{1}'", from.type, to.type)) { }
    }


    /// <summary>
    /// VineScript's main variable. Can be an int, a double, a bool, a string,
    /// an array or a dictionnary.
    /// </summary>
    public class VineVar : IComparable, IComparable<VineVar>
    {
        public static bool strictMode = false;

        public static readonly VineVar NULL = new VineVar(null);
        //public static VineVar newInt {
        //    get {
        //        return new VineVar(0);
        //    }
        //}
        //public static VineVar newNumber {
        //    get {
        //        return new VineVar(0.0);
        //    }
        //}
        //public static VineVar newString {
        //    get {
        //        return new VineVar("");
        //    }
        //}
        //public static VineVar newBool {
        //    get {
        //        return new VineVar(false);
        //    }
        //}
        public static VineVar newArray {
            get {
                return new VineVar(new List<VineVar>());
            }
        }
        public static VineVar newDict {
            get {
                return new VineVar(new Dictionary<string, VineVar>());
            }
        }
        
        // used to compare floating point numbers
        public static readonly double SMALL_VALUE = 0.00000000001;

        public enum Type {
            Null,       // null
            Bool,       // bool
            Int,        // int
            Number,     // double
            String,     // string
            Array,      // List<VineVar>
            Dict,       // Dictionnary<string, VineVar>
            //Dataset     // should be an Hashset<VineVar>
        }
        
        public Type type { get; internal set; }

        public string name { get; internal set; }
        
        // Immutables inner values
        private readonly bool boolValue;
        private readonly int intValue;
        private readonly double numberValue;
        private readonly string stringValue;
        private readonly List<VineVar> arrayValue;
        private readonly Dictionary<string, VineVar> dictValue;
        
        public bool IsBool      { get { return type == Type.Bool;   } }
        public bool IsInt       { get { return type == Type.Int;    } }
        public bool IsNumber    { get { return type == Type.Number; } }
        public bool IsString    { get { return type == Type.String; } }
        public bool IsNull      { get { return type == Type.Null;   } }
        public bool IsArray     { get { return type == Type.Array;  } }
        public bool IsDict      { get { return type == Type.Dict;   } }

        public int AsInt {
            get {
                switch (type) {
                    case Type.Int:
                        return intValue;
                    case Type.Number:
                        return (int)numberValue;
                    case Type.String:
                        if (!strictMode) {
                            try {
                                return int.Parse(stringValue);
                            } catch (FormatException) {
                                return 0;
                            }
                        } else {
                            throw new VineConversionException(type, Type.Int);
                        }
                    case Type.Null:
                        return 0;
                    case Type.Bool:
                        if (!strictMode) {
                            return boolValue ? 1 : 0;
                        } else {
                            throw new VineConversionException(type, Type.Int);
                        }
                    default:
                        throw new VineConversionException(type, Type.Int);
                }
            }
        }

        public double AsNumber {
            get {
                switch (type) {
                    case Type.Int:
                        return intValue;
                    case Type.Number:
                        return numberValue;
                    case Type.String:
                        if (!strictMode) {
                            try {
                                return double.Parse(stringValue, CultureInfo.InvariantCulture);
                            } catch (FormatException) {
                                return 0.0;
                            }
                        } else {
                            throw new VineConversionException(type, Type.Number);
                        }
                    case Type.Null:
                        return 0.0;
                    case Type.Bool:
                        if (!strictMode) {
                            return boolValue ? 1.0 : 0.0;
                        } else {
                            throw new VineConversionException(type, Type.Number);
                        }
                    default:
                        throw new VineConversionException(type, Type.Number);
                }
            }
        }

        public bool AsBool {
            get {
                switch (type) {
                    case Type.Bool:
                        return boolValue;
                    case Type.Null:
                        if (!strictMode) {
                            return false;
                        } else {
                            throw new VineConversionException(type, Type.Bool);
                        }
                    case Type.Int:
                        if (!strictMode) {
                            return intValue != 0;
                        } else {
                            throw new VineConversionException(type, Type.Bool);
                        }
                    case Type.Number:
                        if (!strictMode) {
                            return numberValue != 0.0;
                        } else {
                            throw new VineConversionException(type, Type.Bool);
                        }
                    case Type.String:
                        if (!strictMode) {
                            return stringValue.Length > 0;
                        } else {
                            throw new VineConversionException(type, Type.Bool);
                        }
                    default:
                        throw new VineConversionException(type, Type.Bool);
                }
            }
        }

        public string AsString {
            get {
                switch (type) {
                    case Type.Int:
                        return intValue.ToString();
                    case Type.Number:
                        if (double.IsNaN(numberValue)) {
                            return "NaN";
                        }

                        // VineScript is made to write stories, 
                        // and my (maybe bad) assumption is that writers don't care
                        // about bigs floating point numbers.
                        // The number is converted to string with an arbitrary number of 
                        // optional decimal digits (should it be expanded or reduced?)
                        return numberValue.ToString("0.0#########", CultureInfo.InvariantCulture);
                    case Type.String:
                        return stringValue;
                    case Type.Bool:
                        return boolValue.ToString();
                    case Type.Null:
                        return "";
                    case Type.Array:
                        string arr_str = "[";
                        for (int i = 0; i < arrayValue.Count; i++) {
                            if (arrayValue[i].IsString) {
                                arr_str += string.Format("\"{0}\"", arrayValue[i].ToString());
                            } else {
                                arr_str += arrayValue[i].ToString();
                            }
                            if (i < arrayValue.Count - 1) {
                                arr_str += ", ";
                            }
                        }
                        arr_str += "]";
                        return arr_str;
                    case Type.Dict:
                        string dm_str = "{";
                        for (int i = 0; i < dictValue.Count; i++) {
                            var el = dictValue.ElementAt(i);
                            dm_str += string.Format("\"{0}\": ", el.Key);
                            if (el.Value.IsString) {
                                dm_str += string.Format("\"{0}\"", el.Value.ToString());
                            } else {
                                dm_str += el.Value.ToString();
                            }
                            if (i < dictValue.Count - 1) {
                                dm_str += ", ";
                            }
                        }
                        dm_str += "}";
                        return dm_str;
                    default:
                        throw new VineConversionException(type, Type.String);
                }
            }
        }

        public object AsObject {
            get {
                switch (type) {
                    case Type.Int:
                        return intValue;
                    case Type.Number:
                        return numberValue;
                    case Type.String:
                        return stringValue;
                    case Type.Bool:
                        return boolValue;
                    case Type.Null:
                        return null;
                    case Type.Array:
                        return arrayValue;
                    case Type.Dict:
                        return dictValue;
                    default:
                        throw new VineConversionException();
                }
            }
        }

        public List<VineVar> AsArray {
            get {
                switch (type) {
                    case Type.Array:
                        return arrayValue;
                    default:
                        // not sure what's the best way to handle this with strictMode
                        if (!strictMode) {
                            return null;
                        } else {
                            throw new VineConversionException(type, Type.Array);
                        }
                }
            }
        }

        public Dictionary<string, VineVar> AsDict {
            get {
                switch (type) {
                    case Type.Dict:
                        return dictValue;
                    default:
                        // not sure what's the best way to handle this with strictMode
                        if (!strictMode) {
                            return null;
                        } else {
                            throw new VineConversionException(type, Type.Dict);
                        }
                }
            }
        }
        
        public VineVar this[int index]
        {
            get { return arrayValue[index]; }
            set { arrayValue[index] = value; }
        }
        
        public VineVar this[string key]
        {
            get { return dictValue[key]; }
            set { dictValue[key] = value; }
        }

        public override string ToString()
        {
            return AsString;
        }

        // Create a null value
        public VineVar() : this(null) { }

        // Create a value with a C# object
        public VineVar(object value, bool clone=false)
        {
            // Copy an existing value
            if (value is VineVar) {
                var otherValue = value as VineVar;
                type = otherValue.type;
                name = otherValue.name;
                switch (type) {
                    case Type.Int:
                        intValue = otherValue.intValue;
                        break;
                    case Type.Number:
                        numberValue = otherValue.numberValue;
                        break;
                    case Type.String:
                        stringValue = otherValue.stringValue;
                        break;
                    case Type.Bool:
                        boolValue = otherValue.boolValue;
                        break;
                    case Type.Null:
                        break;
                    case Type.Array:
                        // deep copy
                        arrayValue = otherValue.arrayValue.ConvertAll(
                            val => clone ? val.Clone() : val
                        );
                        break;
                    case Type.Dict:
                        // deep copy
                        dictValue = otherValue.AsDict.ToDictionary(
                            entry => entry.Key,
                            entry => clone ? entry.Value.Clone() : entry.Value
                        );
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                return;
            }

            name = "";

            if (value == null) {
                type = Type.Null;
                return;
            }
            if (value.GetType() == typeof(string)) {
                type = Type.String;
                stringValue = (string)value;
                return;
            }
            if (value.GetType() == typeof(int)) {
                type = Type.Int;
                intValue = (int)value;
                return;
            }
            if (value.GetType() == typeof(float)) {
                type = Type.Number;
                numberValue = (double)value;
                return;
            }
            if (value.GetType() == typeof(double)) {
                type = Type.Number;
                numberValue = (double)value;
                return;
            }
            if (value.GetType() == typeof(bool)) {
                type = Type.Bool;
                boolValue = (bool)value;
                return;
            }
            if (value.GetType() == typeof(List<VineVar>)) {
                type = Type.Array;
                arrayValue = (List<VineVar>)value;
                return;
            }
            if (value.GetType() == typeof(Dictionary<string, VineVar>)) {
                type = Type.Dict;
                dictValue = (Dictionary<string, VineVar>)value;
                return;
            }
            var error = string.Format("Attempted to create a Value using a {0}; currently, " +
                "Values can only be ints, numbers, strings, bools or null.", value.GetType().Name);
            throw new Exception(error);
        }

        public VineVar Clone()
        {
            return new VineVar(this, true);
        }

        public IEnumerator GetEnumerator()
        {
            switch (type) {
                case Type.Array:
                    foreach (var item in this.AsArray) {
                        yield return item;
                    }
                    break;
                case Type.Dict:
                    foreach (var item in this.AsDict) {
                        yield return item;
                    }
                    break;
                case Type.String:
                    for (int i = 0; i < this.AsString.Length; i++) {
                        yield return (VineVar)this.AsString.Substring(i, 1);
                    }
                    break;
                default:
                    throw new VineConversionException(
                        "'" + type + "' is not iterable"
                    );
            }
        }

        public override int GetHashCode()
        {
            return this.AsObject?.GetHashCode() ?? 0;
        }

        public int CompareTo(VineVar other)
        {
            if (other == null) {
                return 1;
            }

            if (other.type == type) {
                switch (type) 
                {
                    case Type.Bool:
                        return boolValue.CompareTo(other.boolValue);
                    case Type.Int:
                        return intValue.CompareTo(other.intValue);
                    case Type.Number:
                        return numberValue.CompareTo(other.numberValue);
                    case Type.String:
                        return stringValue.CompareTo(other.stringValue);
                    case Type.Null:
                        return 0;
                }
            }

            throw new VineComparisonException();
        }

        public int CompareTo(object obj)
        {
            if (obj == null) {
                return 1;
            }
            
            var other = obj as VineVar;

            // not a VineVar
            if (other == null) {
                throw new ArgumentException("Object is not of type VineVar");
            }

            // it's a VineVar
            return CompareTo(other);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType()) {
                return false;
            }

            var other = (VineVar)obj;
            
            // the order is important here:
            // 1. string == string
            // 2. string == (double | int | bool | null) => false
            // 3. either double == double or double == (double)int
            // 4. int == int
            // 5. bool == bool
            // 6. null == null
            // 7. null == (int | double | bool)

            // Both are strings
            if (IsString && other.IsString) {
                return AsString == other.AsString;
            }

            // They are not both strings, checks if at least one of them is:
            if (!VineVar.strictMode && (this.IsString || other.IsString)) {
                return false;
            }

            if ((this.IsNumber && (other.IsNumber || other.IsInt))
                || (other.IsNumber && (this.IsNumber || this.IsInt))
            ) {
                // TODO use SMALL_VALUE: Math.Abs(numberValue - other.numberValue) < SMALL_VALUE
                // if implemented here, should also be implemented in comparison  < > <= >=
                return AsNumber == other.AsNumber;
            }
            if (this.IsInt && other.IsInt) {
                return AsInt == other.AsInt;
            }
            if (this.IsBool && other.IsBool) {
                return AsBool == other.AsBool;
            }

            if (this.IsNull && other.IsNull) {
                return true;
            }
            if (this.IsNull || other.IsNull) {
                return false;
            }

            if (this.IsArray && other.IsArray) {
                return arrayValue.SequenceEqual(other.arrayValue);
            }

            if (this.IsDict && other.IsDict) {
                return dictValue.OrderBy(pair => pair.Key, StringComparer.Ordinal)
                    .SequenceEqual(other.dictValue.OrderBy(
                        pair => pair.Key, StringComparer.Ordinal));
            }

            // Finally, all other cases:
            if (VineVar.strictMode) {
                throw new VineComparisonException(this, other);
            } else {
                return false;
            }
        }

        public static VineVar operator + (VineVar a, VineVar b)
        {
            // Here's how additions with null are handled:
            // null + null => error
            // null + string => string
            // null + anything else => null
            
            // First, if both operands are null/Null => error
            if (    (object.ReferenceEquals(a, null) || a.IsNull)
                &&  (object.ReferenceEquals(b, null) || b.IsNull)
                ) {
                throw new VineArithmeticException(
                    "+", 
                    a?.type.ToString() ?? "<null>", 
                    b?.type.ToString() ?? "<null>"
                );
            }

            // String catches:
            // undefined + string
            // number + string
            // string + string
            // bool + string
            // null + string
            if (a?.type == Type.String || b?.type == Type.String) {
                // automatically becomes a string, even if only 1 of the 2 args is a string
                return new VineVar(a?.AsString + b?.AsString);
            }

            // strings are out of the way,
            // starting from here each cases are treated individually
            
            // Catches null and throw an exception if 1 operands is null
            Exception e = null;
            if (!VineVarUtils.CheckNullOp("+", a, b, out e)) {
                throw e;
            }

            // Can't add bools
            if (a.type == Type.Bool || b.type == Type.Bool) {
                throw new VineArithmeticException("+", a, b);
            }

            // if one of the two operands is a double, treat both as double
            if ((a.type == Type.Number || b.type == Type.Number)) {
                return new VineVar(a.AsNumber + b.AsNumber);
            }

            // Now that doubles are out of the way, check for integers
            if ((a.type == Type.Int || b.type == Type.Int)) {
                return new VineVar(a.AsInt + b.AsInt);
            }

            // Adding 2 arrays together is allowed: [a, b] + [c, d] = [a, b, c, d]
            if ((a.type == Type.Array && b.type == Type.Array)) {
                return new VineVar(a.AsArray.Concat(b.AsArray).ToList());
            }

            throw new VineArithmeticException("+", a, b);
        }

        public static VineVar operator - (VineVar a, VineVar b)
        {
            // Checks for null/Null operands
            Exception e = null;
            if (!VineVarUtils.CheckNullOp("-", a, b, out e)) {
                throw e;
            }

            // First checks for Numbers, Ints and Nulls
            if (    (a.type == Type.Number && (b.type == Type.Number || b.type == Type.Null || b.type == Type.Int))
                ||  (b.type == Type.Number && (a.type == Type.Number || a.type == Type.Null || a.type == Type.Int))
            ) {
                return new VineVar(a.AsNumber - b.AsNumber);
            }
            
            // If not Numbers, then checks for Int and Nulls
            if (    a.type == Type.Int && (b.type == Type.Int || b.type == Type.Null) 
                ||  b.type == Type.Int && (a.type == Type.Int || a.type == Type.Null)
            ) {
                return new VineVar(a.AsInt - b.AsInt);
            }
            
            throw new VineArithmeticException("-", a, b);
        }

        public static VineVar operator * (VineVar a, VineVar b)
        {
            // Checks for null/Null operands
            Exception e = null;
            if (!VineVarUtils.CheckNullOp("*", a, b, out e)) {
                throw e;
            }

            // First checks for Numbers, Ints and Nulls
            if (    (a.type == Type.Number && (b.type == Type.Number || b.type == Type.Null || b.type == Type.Int))
                ||  (b.type == Type.Number && (a.type == Type.Number || a.type == Type.Null || a.type == Type.Int))
            ) {
                return new VineVar(a.AsNumber * b.AsNumber);
            }
            
            // If not Numbers, then checks for Int and Nulls
            if (    a.type == Type.Int && (b.type == Type.Int || b.type == Type.Null) 
                ||  b.type == Type.Int && (a.type == Type.Int || a.type == Type.Null)
            ) {
                return new VineVar(a.AsInt * b.AsInt);
            }
            
            throw new VineArithmeticException("*", a, b);
        }

        public static VineVar operator / (VineVar a, VineVar b)
        {
            // Checks for null/Null operands
            Exception e = null;
            if (!VineVarUtils.CheckNullOp("/", a, b, out e)) {
                throw e;
            }

            // First checks for Numbers, Ints and Nulls
            if (    (a.type == Type.Number && (b.type == Type.Number || b.type == Type.Null || b.type == Type.Int))
                ||  (b.type == Type.Number && (a.type == Type.Number || a.type == Type.Null || a.type == Type.Int))
            ) {
                return new VineVar(a.AsNumber / b.AsNumber);
            }
            
            // If not Numbers, then checks for Int and Nulls
            if (    a.type == Type.Int && (b.type == Type.Int || b.type == Type.Null) 
                ||  b.type == Type.Int && (a.type == Type.Int || a.type == Type.Null)
            ) {
                return new VineVar(a.AsInt / b.AsInt);
            }

            throw new VineArithmeticException("/", a, b);
        }

        public static VineVar operator % (VineVar a, VineVar b)
        {
            // Checks for null/Null operands
            Exception e = null;
            if (!VineVarUtils.CheckNullOp("%", a, b, out e)) {
                throw e;
            }

            // First checks for Numbers, Ints and Nulls
            if (    (a.type == Type.Number && (b.type == Type.Number || b.type == Type.Null || b.type == Type.Int))
                ||  (b.type == Type.Number && (a.type == Type.Number || a.type == Type.Null || a.type == Type.Int))
            ) {
                return new VineVar(a.AsNumber % b.AsNumber);
            }
            
            // If not Numbers, then checks for Int and Nulls
            if (    a.type == Type.Int && (b.type == Type.Int || b.type == Type.Null) 
                ||  b.type == Type.Int && (a.type == Type.Int || a.type == Type.Null)
            ) {
                return new VineVar(a.AsInt % b.AsInt);
            }
            
            throw new VineArithmeticException("%", a, b);
        }

        public static VineVar operator ^ (VineVar a, VineVar b)
        {
            // Checks for null/Null operands
            Exception e = null;
            if (!VineVarUtils.CheckNullOp("^", a, b, out e)) {
                throw e;
            }

            // First checks for Numbers, Ints and Nulls
            if (    (a.type == Type.Number && (b.type == Type.Number || b.type == Type.Null || b.type == Type.Int))
                ||  (b.type == Type.Number && (a.type == Type.Number || a.type == Type.Null || a.type == Type.Int))
            ) {
                return new VineVar(Math.Pow(a.AsNumber, b.AsNumber));
            }
            
            // If not Numbers, then checks for Int and Nulls
            if (    a.type == Type.Int && (b.type == Type.Int || b.type == Type.Null) 
                ||  b.type == Type.Int && (a.type == Type.Int || a.type == Type.Null)
            ) {
                return new VineVar((int)Math.Pow(a.AsInt, b.AsInt));
            }
            
            throw new VineArithmeticException("^", a, b);
        }

        public static VineVar operator - (VineVar a)
        {
            // Checks for null/Null operand
            Exception e = null;
            if (!VineVarUtils.CheckNullOp("-", a, out e)) {
                throw e;
            }

            if (a.type == Type.Number) {
                return new VineVar(-a.AsNumber);
            }
            if (a.type == Type.Int) {
                return new VineVar(-a.AsInt);
            }
            if (a.type == Type.Null) {
                return new VineVar(-0); // Not really sure with that...
            }
            throw new VineArithmeticException("-", a);
        }

        public static VineVar operator ! (VineVar a)
        {
            // Checks for null/Null operand
            Exception e = null;
            if (!VineVarUtils.CheckNullOp("!", a, out e)) {
                throw e;
            }

            if (a.type == Type.Bool) {
                return new VineVar(!a.AsBool);
            }

            throw new VineArithmeticException("!", a);
        }
        
        public static bool operator == (VineVar a, VineVar b)
        {
            if (!object.ReferenceEquals(a, null)) {
                return a.Equals(b);
            }
            if (!object.ReferenceEquals(b, null)) {
                return b.Equals(a);
            }
            // they're both null
            return true;

            // Quicker but less readable way of doing the same thing:
            //return (a?.Equals(b) ?? b?.Equals(a)) ?? true;
        }

        public static bool operator != (VineVar a, VineVar b)
        {
            if (!object.ReferenceEquals(a, null)) {
                return !a.Equals(b);
            }
            if (!object.ReferenceEquals(b, null)) {
                return !b.Equals(a);
            }
            // they're both null: they're not different
            return false;

            // Quicker but less readable way of doing the same thing:
            //return (!a?.Equals(b) ?? !b?.Equals(a)) ?? false;
        }

        // Define the is greater than operator.
        public static bool operator > (VineVar a, VineVar b)
        {
            if (!object.ReferenceEquals(a, null) && !object.ReferenceEquals(b, null)) { 
                if (    (a.type == Type.Int || a.type == Type.Number)
                    &&  (b.type == Type.Int || b.type == Type.Number)
                ) {
                    return a.CompareTo(b) == 1;
                }
            }

            if (VineVar.strictMode) {
                throw new VineComparisonException(">", a, b);
            } else {
                return false;
            }
        }

        // Define the is less than operator.
        public static bool operator < (VineVar a, VineVar b)
        {
            if (!object.ReferenceEquals(a, null) && !object.ReferenceEquals(b, null)) { 
                if (    (a.type == Type.Int || a.type == Type.Number)
                    &&  (b.type == Type.Int || b.type == Type.Number)
                ) {
                    return a.CompareTo(b) == -1;
                }
            }
            
            if (VineVar.strictMode) {
                throw new VineComparisonException("<", a, b);
            } else {
                return false;
            }
        }

        // Define the is greater than or equal to operator.
        public static bool operator >= (VineVar a, VineVar b)
        {
            if (!object.ReferenceEquals(a, null) && !object.ReferenceEquals(b, null)) { 
                if (    (a.type == Type.Int || a.type == Type.Number)
                    &&  (b.type == Type.Int || b.type == Type.Number)
                ) {
                    return a.CompareTo(b) >= 0;
                }
            }
            
            if (VineVar.strictMode) {
                throw new VineComparisonException(">=", a, b);
            } else {
                return false;
            }
        }

        // Define the is less than or equal to operator.
        public static bool operator <= (VineVar a, VineVar b)
        {
            if (!object.ReferenceEquals(a, null) && !object.ReferenceEquals(b, null)) { 
                if (    (a.type == Type.Int || a.type == Type.Number)
                    &&  (b.type == Type.Int || b.type == Type.Number)
                ) {
                    return a.CompareTo(b) <= 0;
                }
            }
            
            if (VineVar.strictMode) {
                throw new VineComparisonException("<=", a, b);
            } else {
                return false;
            }
        }

        public static implicit operator VineVar(bool val)
        {
            return new VineVar(val);
        }

        public static implicit operator VineVar(int val)
        {
            return new VineVar(val);
        }

        public static implicit operator VineVar(double val)
        {
            return new VineVar(val);
        }

        public static implicit operator VineVar(float val)
        {
            return new VineVar(val);
        }

        public static implicit operator VineVar(string val)
        {
            return new VineVar(val);
        }

        public static implicit operator VineVar(List<VineVar> val)
        {
            return new VineVar(val);
        }

        public static implicit operator VineVar(Dictionary<string, VineVar> val)
        {
            return new VineVar(val);
        }

        //public static implicit operator bool(VineVar val)
        //{
        //    return val.AsBool;
        //}

        //public static implicit operator int(VineVar val)
        //{
        //    return val.AsInt;
        //}

        //public static implicit operator double(VineVar val)
        //{
        //    return val.AsNumber;
        //}

        //public static implicit operator string(VineVar val)
        //{
        //    return val.AsString;
        //}
    }
}
