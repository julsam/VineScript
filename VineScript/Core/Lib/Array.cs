using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VineScript.Binding;
using VineScript.Core;

namespace VineScript.Lib
{
    public class Array
    {
        [VineBinding]
        public static void AddRange(ref VineVar array, params VineVar[] items)
        {
            if (!array.IsArray) {
                throw new VineFunctionTypeError(
                    "AddRange", array.type, "Please use an array."
                );
            }
            array.AsArray.AddRange(items);
        }

        [VineBinding]
        public static void Insert(ref VineVar array, int index, VineVar item)
        {
            if (!array.IsArray) {
                throw new VineFunctionTypeError(
                    "Insert", array.type, "Please use an array."
                );
            }
            if (index < 0 || index > array.AsArray.Count - 1) {
                throw new VineFunctionCallException(
                    "The index '" + index + "' is out of range."
                );
            }
            array.AsArray.Insert(index, item);
        }
        
        [VineBinding]
        public static VineVar Range(int stop)
        {
            return Builtins.Range(stop);
        }
        
        [VineBinding]
        public static VineVar Range(int start, int stop)
        {
            return Builtins.Range(start, stop);
        }
        
        [VineBinding]
        public static VineVar Range(int start, int stop, int step)
        {
            return Builtins.Range(start, stop, step);
        }

        // --------------------------------------------------------------------

        [VineBinding]
        public static VineVar Repeat(int times, params VineVar[] values)
        {
            if (times <= 0) {
                throw new VineFunctionCallException(
                    "Can't Repeat values " + times + " times."
                );
            }
            VineVar repeat = VineVar.newArray;
            while (times > 0) {
                repeat.AsArray.AddRange(values);
                times--;
            }
            return repeat;
        }

        [VineBinding]
        public static VineVar Reverse(params VineVar[] values)
        {
            var clone = values.ToList();
                clone.Reverse();
            return clone;
        }

        [VineBinding]
        public static VineVar Rotate(int shift, params VineVar[] values)
        {
            if (shift == 0 || values.Length == 0 || shift % values.Length == 0) {
                return values;
            }

            VineVar[] copy = new VineVar[values.Length];
            
            // Don't need to loop more than necessary
            shift = shift % values.Length;

            for (int i = 0; i < values.Length; i++)
            {
                int j = i + shift;
                if (j < 0) {
                    j += values.Length;
                } else if (j > values.Length - 1) {
                    j -= values.Length;
                }
                copy.SetValue(values[i], j);
            }

            return copy;
        }

        //[VineBinding]
        //public static VineVar Rotate(int shift, VineArray array)
        //{
        //    if (shift == 0 || array.Count == 0 || shift % array.Count == 0) {
        //        return array.ToVineVar();
        //    }

        //    // Don't need to loop more than necessary
        //    shift = shift % array.Count;

        //    LinkedList<VineVar> ll = new LinkedList<VineVar>(array.ToVineVar().AsArray);
        //    if (shift < 0)
        //    {
        //        for (int i = 0; i > shift; i--) {
        //            var first = ll.First;
        //            ll.RemoveFirst();
        //            ll.AddLast(first);
        //        }
        //    }
        //    else
        //    {
        //        for (int i = 0; i < shift; i++) {
        //            var last = ll.Last;
        //            ll.RemoveLast();
        //            ll.AddFirst(last);
        //        }
        //    }
        //    return ll.ToList();
        //}

        /// <summary>
        /// Randomly shuffle the given array.
        /// </summary>
        [VineBinding]
        public static VineVar Shuffle(params VineVar[] values)
        {
            // https://stackoverflow.com/questions/273313/randomize-a-listt

            int n = values.Length;
            VineVar shuffled = values.ToList();
            while (n > 1)
            {
                n--;
                int k = Rand.rand.Next(n + 1);
                VineVar value = shuffled[k];
                shuffled[k] = shuffled[n];
                shuffled[n] = value;
            }
            return shuffled;
        }

        [VineBinding]
        public static VineVar Sort(params string[] values)
        {
            return values
                .OrderBy(v => v, StringComparer.InvariantCulture)
                .Select(v => new VineVar(v))
                .ToList();
        }

        [VineBinding]
        public static VineVar SubArray(VineVar array, int start, int length)
        {
            if (!array.IsArray) {
                throw new VineFunctionTypeError(
                    "SubArray", array.type, "Please use an array."
                );
            }
            var subarray = array.AsArray.GetRange(start, length);
            return subarray;
        }
    }
}
