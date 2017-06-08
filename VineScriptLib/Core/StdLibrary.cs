using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            functions.Register("IsString", typeof(StdLibrary));
            functions.Register("IsBool", typeof(StdLibrary));
            functions.Register("IsInt", typeof(StdLibrary));
            functions.Register("IsDouble", typeof(StdLibrary));
            functions.Register("IsNull", typeof(StdLibrary));
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

        public static bool IsBool(object context, VineValue value)
        {
            return value.IsBool();
        }

        public static bool IsInt(object context, VineValue value)
        {
            return value.IsInt();
        }

        public static bool IsDouble(object context, VineValue value)
        {
            return value.IsNumber();
        }

        public static bool IsString(object context, VineValue value)
        {
            return value.IsString();
        }

        public static bool IsNull(object context, VineValue value)
        {
            return value.IsNull();
        }

        // Using c# types instead of VineValue:

        //public static string Upper(object context, string value)
        //{
        //    return value.ToUpper();
        //}

        //public static bool IsBool(object context, bool value)
        //{
        //    return typeof(bool).IsInstanceOfType(value);
        //}

        //public static bool IsInt(object context, int value)
        //{
        //    return typeof(int).IsInstanceOfType(value);
        //}

        //public static bool IsDouble(object context, double value)
        //{
        //    return typeof(double).IsInstanceOfType(value);
        //}

        //public static bool IsString(object context, string value)
        //{
        //    return typeof(string).IsInstanceOfType(value);
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
