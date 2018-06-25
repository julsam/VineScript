using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VineScript.Core
{
    public class PassageResult
    {
        /// <summary>Result of the passage, after it's been interpreted.</summary>
        public string Text { get; internal set; }

        /// <summary>Links contained in this passage, after it's been interpreted.</summary>
        public List<PassageLink> Links { get; internal set; }

        // Should this also contains passages errors?

        public PassageResult(string text="")
        {
            this.Text = text;
            this.Links = new List<PassageLink>();
        }

        public PassageResult(string otherText, List<PassageLink> otherLinks)
        {
            this.Text = otherText;
            this.Links = otherLinks.ConvertAll(
                val => val
            );
        }

        public PassageResult(List<PassageLink> otherLinks)
        {
            this.Text = "";
            this.Links = otherLinks.ConvertAll(
                val => val
            );
        }
    }
}
