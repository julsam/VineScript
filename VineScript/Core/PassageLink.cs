using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VineScript.Core
{
    public struct PassageLink
    {
        // link text
        public string title { get; }
        // passage name
        public string passageName { get; }
        // order in which the link was created
        public int order { get; }

        public PassageLink(string title, string passageName, int order=0)
        {
            this.title = title;
            this.passageName = passageName;
            this.order = order;
        }
    }
}
