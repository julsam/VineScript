using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VineScript;
using VineScript.Core;

namespace VineScript.Test
{
    [TestClass]
    public class MiscTests
    {
        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void IllegalCharacter01()
        {
            // the first character (sometimes displayed as a whitespace)
            // is '\u000B' (vertical tabulation) and is not allowed in text
            string input = "{{ 1 }}";
            VineStory story = new VineStory();
            string output = story.RunPassage(input);
        }
        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void IllegalCharacter02()
        {
            // the first character (sometimes displayed as a whitespace)
            // is '\u000B' (vertical tabulation) and is not in a string literal
            string input = "{{ \"foobar\" }}";
            VineStory story = new VineStory();
            string output = story.RunPassage(input);
        }
    }
}
