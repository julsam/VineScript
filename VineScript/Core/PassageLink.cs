using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VineScript.Core
{
    public struct PassageLink
    {
        /// <summary>Link's text.</summary>
        public string title { get; }

        /// <summary>Link's destination (passage name).</summary>
        public string destination { get; }

        /// <summary>Link's code (not executed).</summary>
        public string code { get; }

        /// <summary>Order in which the link was created.</summary>
        public int order { get; }

        public PassageLink(string title, string destination, string code, int order=0)
        {
            this.title = title;
            this.destination = destination;
            this.code = code;
            this.order = order;
        }
    }
}
