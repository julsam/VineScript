using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VineScript.Core
{
    public class PassageResult
    {
        public string text { get; internal set; }
        public List<PassageLink> links { get; internal set; }

        public PassageResult(string text="")
        {
            this.text = text;
            this.links = new List<PassageLink>();
        }

        public PassageResult(string otherText, List<PassageLink> otherLinks)
        {
            this.text = otherText;
            this.links = otherLinks.ConvertAll(
                val => val
            );
        }

        public PassageResult(List<PassageLink> otherLinks)
        {
            this.text = "";
            this.links = otherLinks.ConvertAll(
                val => val
            );
        }
    }
}
