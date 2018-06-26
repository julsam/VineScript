using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VineScript.Binding;
using VineScript.Core;

namespace VineScript.Lib
{
    public class Sequence
    {
        [VineBinding]
        public static void Add(ref VineVar array, VineVar items)
        {
            if (!array.IsArray) {
                throw new VineFunctionTypeError(
                    "Add", array.type, "Please use an array."
                );
            }
            array.AsArray.Add(items);
        }

        [VineBinding]
        public static void Add(ref VineVar dict, string key, VineVar item)
        {
            if (!dict.IsDict) {
                throw new VineFunctionTypeError(
                    "Add", dict.type, "Please use a dictionary."
                );
            }
            dict.AsDict.Add(key, item);
        }

        [VineBinding]
        public static void Clear(ref VineVar sequence)
        {
            if (sequence.IsArray) {
                sequence.AsArray.Clear();
            } else if (sequence.IsDict) {
                sequence.AsDict.Clear();
            } else {
                throw new VineFunctionTypeError("Clear", sequence.type);
            }
        }

        [VineBinding]
        public static bool Contains(VineVar sequence, VineVar item)
        {
            if (sequence.IsArray) {
                return sequence.AsArray.Contains(item);
            } else if (sequence.IsString) {
                return sequence.AsString.Contains(item.AsString);
            } else {
                throw new VineFunctionTypeError("Contains", sequence.type);
            }
        }

        [VineBinding]
        public static bool Contains(VineVar dict, string key, VineVar value)
        {
            if (!dict.IsDict) {
                throw new VineFunctionTypeError(
                    "Contains", dict.type, "Please use a dictionary."
                );
            }
            VineVar actualValue;
            if (!dict.AsDict.TryGetValue(key, out actualValue)) {
                return false;
            }
            return value == actualValue;
        }

        /// <summary>
        /// Returns the zero-based index of the leftmost occurence of
        /// the item within this array or string.
        /// </summary>
        [VineBinding]
        public static int IndexOf(VineVar sequence, VineVar item)
        {
            if (sequence.IsArray) {
                return sequence.AsArray.IndexOf(item);
            } else if (sequence.IsString) {
                return sequence.AsString.IndexOf(item.AsString);
            } else {
                throw new VineFunctionTypeError("IndexOf", sequence.type);
            }
        }

        /// <summary>
        /// Returns the zero-based index of the rightmost occurence of
        /// the item within this array or string.
        /// </summary>
        [VineBinding]
        public static int LastIndexOf(VineVar sequence, VineVar item)
        {
            if (sequence.IsArray) {
                return sequence.AsArray.LastIndexOf(item);
            } else if (sequence.IsString) {
                return sequence.AsString.LastIndexOf(item.AsString);
            } else {
                throw new VineFunctionTypeError("LastIndexOf", sequence.type);
            }
        }

        [VineBinding]
        public static VineVar Remove(ref VineVar sequence, VineVar item)
        {
            if (sequence.IsArray) {
                // item to remove
                return sequence.AsArray.Remove(item);
            } else if (sequence.IsDict) {
                // item is used as the key
                return sequence.AsDict.Remove(item.AsString);
            } else if (sequence.IsString) {
                // item is used as the start index
                if (!item.IsInt) {
                    throw new VineFunctionTypeError(
                        "Remove", sequence.type, 
                        "The second argument should be of type" + VineVar.Type.Int + "."
                    );
                }
                return sequence.AsString.Remove(item.AsInt);
            } else {
                throw new VineFunctionTypeError("Remove", sequence.type);
            }
        }

        [VineBinding]
        public static bool RemoveAt(ref VineVar sequence, VineVar index)
        {
            if (sequence.IsArray) {
                sequence.AsArray.RemoveAt(index.AsInt);
                return true;
            } else if (sequence.IsDict) {
                return sequence.AsDict.Remove(index.AsString);
            } else {
                throw new VineFunctionTypeError(
                    "RemoveAt", sequence.type, "Please use an array."
                );
            }
        }
        
        /// <summary>
        /// Count the number of elements in a VineVar of type array, dictionary
        /// or string.
        /// Same as the method Length.
        /// </summary>
        [VineBinding]
        public static int Count(VineVar value)
        {
            if (value.IsArray) {
                return value.AsArray.Count;
            } else if (value.IsDict) {
                return value.AsDict.Count;
            } else if (value.IsString) {
                return value.AsString.Length;
            } else {
                throw new VineFunctionTypeError("Count", value.type);
            }
        }

        /// <summary>
        /// Length of an VineVar of type array, dictionary or string.
        /// Same as the method Count.
        /// </summary>
        [VineBinding]
        public static int Length(VineVar value)
        {
            if (value.IsArray) {
                return value.AsArray.Count;
            } else if (value.IsDict) {
                return value.AsDict.Count;
            } else if (value.IsString) {
                return value.AsString.Length;
            } else {
                throw new VineFunctionTypeError("Length", value.type);
            }
        }
    }
}
