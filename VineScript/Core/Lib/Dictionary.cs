using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VineScript.Binding;
using VineScript.Core;

namespace VineScript.Lib
{
    public class Dictionary
    {
        [VineBinding]
        public static bool ContainsKey(VineVar dict, string key)
        {
            if (!dict.IsDict) {
                throw new VineFunctionTypeError(
                    "ContainsKey", dict.type, "Please use a dictionary."
                );
            }
            return dict.AsDict.ContainsKey(key);
        }

        [VineBinding]
        public static bool ContainsValue(VineVar dict, VineVar value)
        {
            if (!dict.IsDict) {
                throw new VineFunctionTypeError(
                    "ContainsValue", dict.type, "Please use a dictionary."
                );
            }
            return dict.AsDict.ContainsValue(value);
        }
        
        /// <summary>
        /// Returns an array containing all the keys of the given dictionary.
        /// </summary>
        [VineBinding]
        public static VineVar Keys(VineVar dict)
        {
            if (!dict.IsDict) {
                throw new VineFunctionTypeError(
                    "Keys", dict.type, "Please use a dictionary."
                );
            }
            return new VineVar(dict.AsDict.Keys.ToList());
        }
        
        /// <summary>
        /// Returns an array containing all the values of the given dictionary.
        /// </summary>
        [VineBinding]
        public static VineVar Values(VineVar dict)
        {
            if (!dict.IsDict) {
                throw new VineFunctionTypeError(
                    "Values", dict.type, "Please use a dictionary."
                );
            }
            return new VineVar(dict.AsDict.Values.ToList());
        }
    }
}
