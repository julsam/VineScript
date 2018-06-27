using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VineScript.Core
{
    public abstract class VineException : Exception
    {
        public string Title = "";
        public string Details = "";

        public VineException() : base() { }
        public VineException(string msg) : base(msg)
        {
            Title = msg;
        }
        public VineException(string msg, Exception innerException)
            : base(msg, innerException)
        {
            Title = msg;
        }
    }
}
