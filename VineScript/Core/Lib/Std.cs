using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VineScript.Binding;
using VineScript.Core;

namespace VineScript.Lib
{
    public class Std
    {
        /// <summary>
        /// Print any value on the standard output.
        /// </summary>
        /// <param name="value"></param>
        [VineBinding]
        public static void Console(VineVar value=default(VineVar))
        {
            System.Console.Write(value);
        }

        /// <summary>
        /// Print any value on the standard output, followed by a newline
        /// </summary>
        /// <param name="value"></param>
        [VineBinding]
        public static void ConsoleLine(VineVar value=default(VineVar))
        {
            System.Console.WriteLine(value);
        }
        
        [VineBinding]
        public static VineVar Clone(VineVar value)
        {
            return value.Clone();
        }
        
        [VineBinding]
        public static VineVar ReferenceEquals(VineVar a, VineVar b)
        {
            return object.ReferenceEquals(a, b);
        }
        
        [VineBinding]
        public static VineVar Int(VineVar value)
        {
            return value.AsInt;
        }
        
        [VineBinding]
        public static VineVar Number(VineVar value)
        {
            return value.AsNumber;
        }
        
        [VineBinding]
        public static VineVar String(VineVar value)
        {
            return value.AsString;
        }
        
        [VineBinding]
        public static bool IsBool(VineVar value)
        {
            return value.IsBool;
        }
        
        [VineBinding]
        public static bool IsInt(VineVar value)
        {
            return value.IsInt;
        }
        
        [VineBinding]
        public static bool IsNumber(VineVar value)
        {
            return value.IsNumber;
        }
        
        [VineBinding]
        public static bool IsString(VineVar value)
        {
            return value.IsString;
        }
        
        [VineBinding]
        public static bool IsNull(VineVar value)
        {
            return value.IsNull;
        }
        
        [VineBinding]
        public static bool IsArray(VineVar value)
        {
            return value.IsArray;
        }
        
        [VineBinding]
        public static bool IsDict(VineVar value)
        {
            return value.IsDict;
        }
    }
}
