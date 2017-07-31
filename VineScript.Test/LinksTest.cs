using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VineScript;
using VineScript.Core;

namespace VineScriptTest
{
    /// <summary>
    /// Summary description for UnitTest1
    /// </summary>
    [TestClass]
    public class LinksTest
    {
        [TestMethod]
        public void LinksBasic01()
        {
            StreamReader input = File.OpenText("scripts/links/basic01.vine");

            VineStory story = new VineStory();
            PassageResult result = story.RunPassage(input);

            
            Assert.AreEqual("my title 01", result.links[0].title);
            Assert.AreEqual("mylink01", result.links[0].passageName);

            Assert.AreEqual("my title 02", result.links[1].title);
            Assert.AreEqual("my link 02", result.links[1].passageName);

            Assert.AreEqual("both a link and a title", result.links[2].title);
            Assert.AreEqual("both a link and a title", result.links[2].passageName);

            Assert.AreEqual("BothALinkAndATitle", result.links[3].title);
            Assert.AreEqual("BothALinkAndATitle", result.links[3].passageName);
        }

        [TestMethod]
        public void LinksEscapeText01()
        {
            StreamReader input = File.OpenText("scripts/links/escape_text01.vine");

            VineStory story = new VineStory();
            PassageResult result = story.RunPassage(input);

            
            Assert.AreEqual("not need to escape this >]-<", result.links[0].title);
            Assert.AreEqual("link", result.links[0].passageName);

            Assert.AreEqual("my [[own]] title", result.links[1].title);
            Assert.AreEqual("mylink", result.links[1].passageName);

            Assert.AreEqual("my | title", result.links[2].title);
            Assert.AreEqual("mylink", result.links[2].passageName);

            Assert.AreEqual("my <- title", result.links[3].title);
            Assert.AreEqual("mylink", result.links[3].passageName);

            Assert.AreEqual("my -> title", result.links[4].title);
            Assert.AreEqual("mylink", result.links[4].passageName);

            Assert.AreEqual("my \\\\ title", result.links[5].title);
            Assert.AreEqual("mylink", result.links[5].passageName);
        }
    }
}
