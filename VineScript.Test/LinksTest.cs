using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VineScript;
using VineScript.Core;

namespace VineScriptTest
{
    [TestClass]
    public class LinksTest
    {
        VineStory story;

        public LinksTest()
        {
            story = new VineStory("");
        }

        [TestMethod]
        public void LinksBasic01()
        {
            string input = "scripts/links/basic01";

            PassageResult result = story.RunPassage(input);

            Assert.AreEqual("my title 01", result.Links[0].title);
            Assert.AreEqual("mylink01", result.Links[0].destination);

            Assert.AreEqual("my title 02", result.Links[1].title);
            Assert.AreEqual("my link 02", result.Links[1].destination);

            Assert.AreEqual("both a link and a title", result.Links[2].title);
            Assert.AreEqual("both a link and a title", result.Links[2].destination);

            Assert.AreEqual("BothALinkAndATitle", result.Links[3].title);
            Assert.AreEqual("BothALinkAndATitle", result.Links[3].destination);
        }

        [TestMethod]
        public void LinksEscapeText01()
        {
            string input = "scripts/links/escape_text01";

            PassageResult result = story.RunPassage(input);

            Assert.AreEqual("not need to escape this \\ ]", result.Links[0].title);
            Assert.AreEqual("link", result.Links[0].destination);

            Assert.AreEqual("my [[own]] title", result.Links[1].title);
            Assert.AreEqual("mylink", result.Links[1].destination);

            Assert.AreEqual("my | title", result.Links[2].title);
            Assert.AreEqual("mylink", result.Links[2].destination);

            Assert.AreEqual("my \\\\ title", result.Links[3].title);
            Assert.AreEqual("mylink", result.Links[3].destination);
        }

        [TestMethod]
        public void LinksCode01()
        {
            string input = "scripts/links/linkcode01";

            Loader loader = new Loader();
            loader.LoadFromDir();
            VineStory story = new VineStory(loader);
            PassageResult result = story.RunPassage(input);

            Assert.AreEqual("title", result.Links[0].title);
            Assert.AreEqual("mylink", result.Links[0].destination);

            string sub1 = result.Links[0].code;
            loader.LoadCode(sub1, "nested1");
            result = story.RunPassage("nested1");
            
            Assert.AreEqual("1+1", result.Links[0].title);
            Assert.AreEqual("nestedlink1", result.Links[0].destination);

            Assert.AreEqual("3+3", result.Links[1].title);
            Assert.AreEqual("nestedlink3", result.Links[1].destination);

            string sub2 = result.Links[1].code;
            loader.LoadCode(sub2, "nested2");
            result = story.RunPassage("nested2");
            
            Assert.AreEqual("nested code 3", result.Text);
        }
    }
}
