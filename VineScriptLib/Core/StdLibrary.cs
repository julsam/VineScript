using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VineScriptLib.Core
{
    public interface IVineLibrary
    {
        FunctionsCollection functions { get; set; }
        FunctionsCollection filters { get; set; }
        void RegisterFunctions();
        void RegisterFilters();
    }

    public class StdLibrary : IVineLibrary
    {
        public FunctionsCollection functions { get; set; }
        public FunctionsCollection filters { get; set; }

        public StdLibrary()
        {
            functions = new FunctionsCollection();
            filters = new FunctionsCollection();
        }

        public void RegisterFunctions()
        {
            functions.Register("Hello", typeof(StdLibrary));
            functions.Register("Add", typeof(StdLibrary));
            functions.Register("Upper", typeof(StdLibrary));
            functions.Register("Clone", typeof(StdLibrary));
            functions.Register("Int", typeof(StdLibrary));
            functions.Register("Number", typeof(StdLibrary));
            functions.Register("String", typeof(StdLibrary));
            functions.Register("IsString", typeof(StdLibrary));
            functions.Register("IsBool", typeof(StdLibrary));
            functions.Register("IsInt", typeof(StdLibrary));
            functions.Register("IsNumber", typeof(StdLibrary));
            functions.Register("IsNull", typeof(StdLibrary));
            functions.Register("IsArray", typeof(StdLibrary));
            functions.Register("IsDict", typeof(StdLibrary));
        }

        public void RegisterFilters()
        {
            filters.Register("Upper", typeof(StdLibrary));
        }

        public static VineValue Hello(object context)
        {
            return "Hello, World!";
        }

        public static VineValue Add(object context, VineValue a, VineValue b)
        {
            return a + b;
        }
        
        public static VineValue Upper(object context, VineValue value)
        {
            return value.AsString.ToUpper();
        }
        
        public static VineValue Clone(object context, VineValue value)
        {
            if (value == null) {
                throw new Exception("Can't clone Null value");
            }
            return value.Clone();
        }

        public static int Int(object context, VineValue value)
        {
            return value.AsInt;
        }

        public static double Number(object context, VineValue value)
        {
            return value.AsNumber;
        }

        public static string String(object context, VineValue value)
        {
            return value.AsString;
        }

        public static bool IsBool(object context, VineValue value)
        {
            return value.IsBool;
        }

        public static bool IsInt(object context, VineValue value)
        {
            return value.IsInt;
        }

        public static bool IsNumber(object context, VineValue value)
        {
            return value.IsNumber;
        }

        public static bool IsString(object context, VineValue value)
        {
            return value.IsString;
        }

        public static bool IsNull(object context, VineValue value)
        {
            return value.IsNull;
        }

        public static bool IsArray(object context, VineValue value)
        {
            return value.IsArray;
        }

        public static bool IsDict(object context, VineValue value)
        {
            return value.IsDict;
        }

        // Using c# types instead of VineValue:

        //public static string Upper(object context, string value)
        //{
        //    return value.ToUpper();
        //}

        //public static int DebugArgsInt(object context, int value)
        //{
        //    return value;
        //}
    }

    public class UserLib : IVineLibrary
    {
        public FunctionsCollection functions { get; set; }
        public FunctionsCollection filters { get; set; }

        public UserLib()
        {
            functions = new FunctionsCollection();
            filters = new FunctionsCollection();
        }

        public virtual void RegisterFunctions()
        {
        }

        public virtual void RegisterFilters()
        {
        }
    }
}
