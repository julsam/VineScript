using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace VineScriptLib.Core
{
    // VineArithmeticException
    public class VineArithmeticException : Exception
    {
        public VineArithmeticException() : base("Not supported operation") { }

        public VineArithmeticException(string msg) : base(msg) { }

        public VineArithmeticException(string op, VineValue a, VineValue b) 
            : base(string.Format("Operator '{0}' cannot be applied to operands of type '{1}' and '{2}'",
                op, a.type, b.type)) { }

        public VineArithmeticException(string op, VineValue a) 
            : base(string.Format("Unary operator '{0}' cannot be applied to operand of type '{1}' ",
                op, a.type)) { }
    }

    public class VineComparisonException : Exception
    {
        public VineComparisonException() : base("Not supported comparison") { }

        public VineComparisonException(VineValue a, VineValue b) 
            : base(string.Format("Cannot compare equality between type '{0}' and '{1}'", a.type, b.type)) { }

        public VineComparisonException(string op, VineValue a, VineValue b) 
            : base(string.Format("Comparison '{0}' cannot be applied to operands of type '{1}' and '{2}'",
                op, a.type, b.type)) { }
    }

    public class VineConversionException : Exception
    {
        public VineConversionException() : base("Not supported cast") { }

        public VineConversionException(string msg) : base(msg) { }

        public VineConversionException(VineValue.Type from, VineValue.Type to) 
            : base(string.Format("Cannot convert type '{0}' to '{1}'", from, to)) { }

        public VineConversionException(VineValue from, VineValue to) 
            : base(string.Format("Cannot convert type '{0}' to '{1}'", from.type, to.type)) { }
    }

    /// <summary>
    /// VineScript's main variable. Can be an int, a double, a bool, a string,
    /// an array or a dictionnary.
    /// </summary>
    public class VineValue : IComparable, IComparable<VineValue>
    {
        public static bool strictMode = false;

		public static readonly VineValue NULL = new VineValue();
        //public static VineValue newInt {
        //    get {
        //        return new VineValue(0);
        //    }
        //}
        //public static VineValue newNumber {
        //    get {
        //        return new VineValue(0.0);
        //    }
        //}
        //public static VineValue newString {
        //    get {
        //        return new VineValue("");
        //    }
        //}
        //public static VineValue newBool {
        //    get {
        //        return new VineValue(false);
        //    }
        //}
        public static VineValue newArray {
            get {
                return new VineValue(new List<VineValue>());
            }
        }
        public static VineValue newDict {
            get {
                return new VineValue(new Dictionary<string, VineValue>());
            }
        }
        
        // used to compare floating point numbers
        public static readonly double SMALL_VALUE = 0.00000000001;

        public enum Type {
            Bool,       // bool
            Int,        // int
            Number,     // double
            String,     // string
            Null,       // null
            Array,      // List<VineValue>
            Dict,       // Dictionnary<string, VineValue>
            //Dataset // should be an Hashset*/
        }
        
		public Type type { get; internal set; }
        
        // Immutables inner values
		private readonly bool boolValue;
        private readonly int intValue;
		private readonly double numberValue;
		private readonly string stringValue;
		private readonly List<VineValue> arrayValue;
		private readonly Dictionary<string, VineValue> dictValue;
        
        public bool IsBool()    { return type == Type.Bool;     }
        public bool IsInt()     { return type == Type.Int;      }
        public bool IsNumber()  { return type == Type.Number;   }
        public bool IsString()  { return type == Type.String;   }
        public bool IsNull()    { return type == Type.Null;     }
        public bool IsArray()   { return type == Type.Array;    }
        public bool IsDict()    { return type == Type.Dict;     }

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
					    return numberValue.ToString(CultureInfo.InvariantCulture);
				    case Type.String:
					    return stringValue;
				    case Type.Bool:
					    return boolValue.ToString();
				    case Type.Null:
					    return null;
                    case Type.Array:
                        string arr_str = "[";
                        for (int i = 0; i < arrayValue.Count; i++) {
                            if (arrayValue[i].IsString()) {
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
                            if (el.Value.IsString()) {
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

		public List<VineValue> AsArray {
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

		public Dictionary<string, VineValue> AsDict {
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
        
        public VineValue this[int index]
        {
            get { return arrayValue[index]; }
            set { arrayValue[index] = value; }
        }
        
        public VineValue this[string key]
        {
            get { return dictValue[key]; }
            set { dictValue[key] = value; }
        }

        public override string ToString()
		{
            return AsString;
		}

        // Create a null value
		public VineValue() : this(null) { }

		// Create a value with a C# object
		public VineValue(object value)
		{
			// Copy an existing value
			if (typeof(VineValue).IsInstanceOfType(value)) {
				var otherValue = value as VineValue;
				type = otherValue.type;
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
					    arrayValue = new List<VineValue>(otherValue.arrayValue);
					    break;
				    case Type.Dict:
					    dictValue = new Dictionary<string, VineValue>(otherValue.dictValue);
					    break;
				    default:
					    throw new ArgumentOutOfRangeException ();
				}
				return;
			}
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
			if (value.GetType() == typeof(List<VineValue>)) {
				type = Type.Array;
				arrayValue = (List<VineValue>)value;
				return;
			}
			if (value.GetType() == typeof(Dictionary<string, VineValue>)) {
				type = Type.Dict;
				dictValue = (Dictionary<string, VineValue>)value;
				return;
			}
			var error = string.Format("Attempted to create a Value using a {0}; currently, " +
				"Values can only be ints, numbers, strings, bools or null.", value.GetType().Name);
			throw new Exception(error);
		}

        public override int GetHashCode()
        {
            return this.AsObject?.GetHashCode() ?? 0;
        }

        public int CompareTo(VineValue other)
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

			// soft, fast coercion
			var other = obj as VineValue;

            // not a value
            if (other == null) {
                throw new ArgumentException("Object is not a Value");
            }

			// it is a value!
			return CompareTo(other);
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType()) {
	            return false;
            }

            var other = (VineValue)obj;
            
            // the order is important here:
            // 1. string == string
            // 2. string == (double | int | bool | null) => false
            // 3. either double == double or double == (double)int
            // 4. int == int
            // 5. bool == bool
            // 6. null == null
            // 7. null == (int | double | bool)

            // Both are strings
            if (IsString() && other.IsString()) {
                return AsString == other.AsString;
            }

            // They are not both strings, checks if at least one of them is:
            if (!VineValue.strictMode && (IsString() || other.IsString())) {
                return false;
            }

            //if (    (IsNumber() && (other.IsNumber() || other.IsInt()))
            //    ||	(other.IsNumber() && (IsNumber() || IsInt()))
            //) {
            //    return AsNumber == other.AsNumber;
            //}

            if (    (type == Type.Number && (other.type == Type.Number || other.type == Type.Int))
                ||	(other.type == Type.Number && (type == Type.Number || type == Type.Int))
            ) {
                // try using SMALL_VALUE: Math.Abs(numberValue - other.numberValue) < SMALL_VALUE
                // if implemented here, should also be implemented in comparison  < > <= >=
                return AsNumber == other.AsNumber;
            }
            if (IsInt() && other.IsInt()) {
                return AsInt == other.AsInt;
            }
            if (IsBool() && other.IsBool()) {
                return AsBool == other.AsBool;
            }
            if (IsNull() && other.IsNull()) {
                return true;
            }
            if (IsNull() || other.IsNull()) {
                return false;
            }

            if (IsArray() && other.IsArray()) {
                return arrayValue.SequenceEqual(other.arrayValue);
            }

            if (IsDict() && other.IsDict()) {
                return dictValue.OrderBy(pair => pair.Key, StringComparer.Ordinal)
                    .SequenceEqual(other.dictValue.OrderBy(
                        pair => pair.Key, StringComparer.Ordinal));
            }

            // Finally, all other cases:
            if (VineValue.strictMode) {
                throw new VineComparisonException(this, other);
            } else {
                return false;
            }
        }

		public static VineValue operator + (VineValue a, VineValue b)
        {
			// catches:
			// undefined + string
			// number + string
			// string + string
			// bool + string
			// null + string

            // we're headed for string town!
			if (a.type == Type.String || b.type == Type.String) {
				// automatically becomes a string, even if only 1 of the 2 args is a string
				return new VineValue(a.AsString + b.AsString);
			}

            // strings are out of the way, now each cases are treated individually

            // Can't add bools
            if (a.type == Type.Bool || b.type == Type.Bool) {
                //throw new ArgumentException(string.Format("Cannot add types {0} and {1}.", a.type, b.type));
	            throw new VineArithmeticException("+", a, b);
            }

            // if one of the two operands is a double, treat both as double
			if ((a.type == Type.Number || b.type == Type.Number)) {
				return new VineValue(a.AsNumber + b.AsNumber);
			}

            // Now that doubles are out of the way, check for integers
			if ((a.type == Type.Int || b.type == Type.Int)) {
				return new VineValue(a.AsInt + b.AsInt);
			}

            // Adding 2 arrays together is allowed: [a, b] + [c, d] = [a, b, c, d]
			if ((a.type == Type.Array && b.type == Type.Array)) {
				return new VineValue(a.AsArray.Concat(b.AsArray).ToList());
			}

            // Should not happen, right?
			//throw new ArgumentException(string.Format("Cannot add types {0} and {1}.", a.type, b.type));
	        throw new VineArithmeticException("+", a, b);
		}

		public static VineValue operator - (VineValue a, VineValue b)
        {
            // First checks for Numbers, Ints and Nulls
			if (    (a.type == Type.Number && (b.type == Type.Number || b.type == Type.Null || b.type == Type.Int))
                ||  (b.type == Type.Number && (a.type == Type.Number || a.type == Type.Null || a.type == Type.Int))
			) {
				return new VineValue(a.AsNumber - b.AsNumber);
			}
            
            // If not Numbers, then checks for Int and Nulls
			if (    a.type == Type.Int && (b.type == Type.Int || b.type == Type.Null) 
                ||  b.type == Type.Int && (a.type == Type.Int || a.type == Type.Null)
			) {
				return new VineValue(a.AsInt - b.AsInt);
			}

			//throw new ArgumentException(string.Format("Cannot subtract types {0} and {1}.", a.type, b.type));
	        throw new VineArithmeticException("-", a, b);
		}

		public static VineValue operator * (VineValue a, VineValue b)
        {
            // First checks for Numbers, Ints and Nulls
			if (    (a.type == Type.Number && (b.type == Type.Number || b.type == Type.Null || b.type == Type.Int))
                ||  (b.type == Type.Number && (a.type == Type.Number || a.type == Type.Null || a.type == Type.Int))
			) {
				return new VineValue(a.AsNumber * b.AsNumber);
			}
            
            // If not Numbers, then checks for Int and Nulls
			if (    a.type == Type.Int && (b.type == Type.Int || b.type == Type.Null) 
                ||  b.type == Type.Int && (a.type == Type.Int || a.type == Type.Null)
			) {
				return new VineValue(a.AsInt * b.AsInt);
			}

			//throw new ArgumentException(string.Format("Cannot multiply types {0} and {1}.", a.type, b.type));
	        throw new VineArithmeticException("*", a, b);
		}

		public static VineValue operator / (VineValue a, VineValue b)
        {
            // First checks for Numbers, Ints and Nulls
			if (    (a.type == Type.Number && (b.type == Type.Number || b.type == Type.Null || b.type == Type.Int))
                ||  (b.type == Type.Number && (a.type == Type.Number || a.type == Type.Null || a.type == Type.Int))
			) {
				return new VineValue(a.AsNumber / b.AsNumber);
			}
            
            // If not Numbers, then checks for Int and Nulls
			if (    a.type == Type.Int && (b.type == Type.Int || b.type == Type.Null) 
                ||  b.type == Type.Int && (a.type == Type.Int || a.type == Type.Null)
			) {
				return new VineValue(a.AsInt / b.AsInt);
			}

			//throw new ArgumentException(string.Format("Cannot divide types {0} and {1}.", a.type, b.type));
	        throw new VineArithmeticException("/", a, b);
		}

		public static VineValue operator % (VineValue a, VineValue b)
        {
            // First checks for Numbers, Ints and Nulls
			if (    (a.type == Type.Number && (b.type == Type.Number || b.type == Type.Null || b.type == Type.Int))
                ||  (b.type == Type.Number && (a.type == Type.Number || a.type == Type.Null || a.type == Type.Int))
			) {
				return new VineValue(a.AsNumber % b.AsNumber);
			}
            
            // If not Numbers, then checks for Int and Nulls
			if (    a.type == Type.Int && (b.type == Type.Int || b.type == Type.Null) 
                ||  b.type == Type.Int && (a.type == Type.Int || a.type == Type.Null)
			) {
				return new VineValue(a.AsInt % b.AsInt);
			}
            
	        throw new VineArithmeticException("%", a, b);
		}

		public static VineValue operator ^ (VineValue a, VineValue b)
        {
            // First checks for Numbers, Ints and Nulls
			if (    (a.type == Type.Number && (b.type == Type.Number || b.type == Type.Null || b.type == Type.Int))
                ||  (b.type == Type.Number && (a.type == Type.Number || a.type == Type.Null || a.type == Type.Int))
			) {
				return new VineValue(Math.Pow(a.AsNumber, b.AsNumber));
			}
            
            // If not Numbers, then checks for Int and Nulls
			if (    a.type == Type.Int && (b.type == Type.Int || b.type == Type.Null) 
                ||  b.type == Type.Int && (a.type == Type.Int || a.type == Type.Null)
			) {
				return new VineValue(Math.Pow(a.AsInt, b.AsInt));
			}
            
	        throw new VineArithmeticException("^", a, b);
		}

		public static VineValue operator - (VineValue a)
        {
			if (a.type == Type.Number) {
				return new VineValue(-a.AsNumber);
			}
			if (a.type == Type.Int) {
				return new VineValue(-a.AsInt);
			}
			if (a.type == Type.Null) {
				return new VineValue(-0); // Not really sure with that...
			}
			//throw new ArgumentException(string.Format("Cannot negate type {0}.", a.type));
	        throw new VineArithmeticException("-", a);
		}

		public static VineValue operator ! (VineValue a)
        {
			if (a.type == Type.Bool) {
				return new VineValue(!a.AsBool);
			}

	        throw new VineArithmeticException("!", a);
		}
        
		public static bool operator == (VineValue a, VineValue b)
		{
			return a.Equals(b);
		}

		public static bool operator != (VineValue a, VineValue b)
		{
			return !a.Equals(b);
		}

		// Define the is greater than operator.
		public static bool operator > (VineValue a, VineValue b)
		{
            if (    (a.type == Type.Int || a.type == Type.Number)
                &&  (b.type == Type.Int || b.type == Type.Number)
            ) {
                return a.CompareTo(b) == 1;
            }

            if (VineValue.strictMode) {
                throw new VineComparisonException(">", a, b);
            } else {
                return false;
            }
		}

		// Define the is less than operator.
		public static bool operator < (VineValue a, VineValue b)
		{
            if (    (a.type == Type.Int || a.type == Type.Number)
                &&  (b.type == Type.Int || b.type == Type.Number)
            ) {
                return a.CompareTo(b) == -1;
            }
            
	        if (VineValue.strictMode) {
                throw new VineComparisonException("<", a, b);
            } else {
                return false;
            }
		}

		// Define the is greater than or equal to operator.
		public static bool operator >= (VineValue a, VineValue b)
		{
            if (    (a.type == Type.Int || a.type == Type.Number)
                &&  (b.type == Type.Int || b.type == Type.Number)
            ) {
                return a.CompareTo(b) >= 0;
            }
            
	        if (VineValue.strictMode) {
                throw new VineComparisonException(">=", a, b);
            } else {
                return false;
            }
		}

		// Define the is less than or equal to operator.
		public static bool operator <= (VineValue a, VineValue b)
		{
            if (    (a.type == Type.Int || a.type == Type.Number)
                &&  (b.type == Type.Int || b.type == Type.Number)
            ) {
                return a.CompareTo(b) <= 0;
            }
            
	        if (VineValue.strictMode) {
                throw new VineComparisonException("<=", a, b);
            } else {
                return false;
            }
		}

        public static implicit operator VineValue(bool val)
        {
	        return new VineValue(val);
        }

        public static implicit operator VineValue(int val)
        {
	        return new VineValue(val);
        }

        public static implicit operator VineValue(double val)
        {
	        return new VineValue(val);
        }

        public static implicit operator VineValue(float val)
        {
	        return new VineValue(val);
        }

        public static implicit operator VineValue(string val)
        {
	        return new VineValue(val);
        }

        public static implicit operator VineValue(List<VineValue> val)
        {
	        return new VineValue(val);
        }

        public static implicit operator VineValue(Dictionary<string, VineValue> val)
        {
	        return new VineValue(val);
        }

        //public static implicit operator bool(VineValue val)
        //{
        //    return val.AsBool;
        //}

        //public static implicit operator int(VineValue val)
        //{
        //    return val.AsInt;
        //}

        //public static implicit operator double(VineValue val)
        //{
        //    return val.AsNumber;
        //}

        //public static implicit operator string(VineValue val)
        //{
        //    return val.AsString;
        //}
    }
}
