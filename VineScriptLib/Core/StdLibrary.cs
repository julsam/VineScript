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
            functions.Register("Add", typeof(StdLibrary));
        }

        public void RegisterFilters()
        {
            filters.Register("Upper", typeof(StdLibrary));
        }

        public static VineValue Add(VineValue a, VineValue b)
        {
            return a + b;
        }

        public static string Upper(string value)
        {
            return value.ToUpper();
        }
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
