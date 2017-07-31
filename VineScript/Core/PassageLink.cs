using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VineScript.Core
{
    public struct PassageLink
    {
        string title;
        string link;
        int order;

        public PassageLink(string title, string link, int order=0)
        {
            this.title = title;
            this.link = link;
            this.order = order;
        }
    }
}
