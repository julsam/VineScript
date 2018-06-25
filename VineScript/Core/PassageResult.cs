using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VineScript.Core
{
    public class PassageResult
    {
        /// <summary>Passage name.</summary>
        public string PassageName { get; internal set; }

        /// <summary>Result of the passage, after it's been interpreted.</summary>
        public string Text { get; internal set; }

        /// <summary>Links contained in this passage, after it's been interpreted.</summary>
        public List<PassageLink> Links { get; internal set; }

        // Should this also contains passages errors?

        public PassageResult(string passageName, string text="")
        {
            this.PassageName = passageName;
            this.Text = text;
            this.Links = new List<PassageLink>();
        }

        public PassageResult(string passageName, string otherText, List<PassageLink> otherLinks)
        {
            this.PassageName = passageName;
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
