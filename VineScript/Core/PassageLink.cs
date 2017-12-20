using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VineScript.Core
{
    public struct PassageLink
    {
        // Link's text
        public string title { get; }
        // Link's destination (passage name)
        public string destination { get; }
        // Link's code (not executed)
        public string code { get; }
        // Order in which the link was created
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
