using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VineScriptLib;
using VineScriptLib.Core;

namespace VineScriptLib.Test
{
    /// <summary>
    /// Summary description for BasicTests
    /// </summary>
    [TestClass]
    public class BasicTests
    {
        public BasicTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        [TestMethod]
        public void Empty()
        {
            string input = "";
            VineStory story = new VineStory();
            string output = story.RunPassage(input);
            Assert.AreEqual("", output);
        }

        [TestMethod]
        public void SimpleComment()
        {
            string input = "{# comment #}";
            VineStory story = new VineStory();
            string output = story.RunPassage(input);
            Assert.AreEqual("", output);
        }

        [TestMethod]
        public void SimplePrintInt()
        {
            VineStory story = new VineStory();
            story.vars["var1"] = 42;
            string input = "{{ $var1 }}";
            string output = story.RunPassage(input);
            Assert.AreEqual("42", output);
        }

        [TestMethod]
        public void PrintDefinedVars()
        {
            VineStory story = new VineStory();
            story.vars["varStr"] = "Foo bar";
            story.vars["varInt"] = 42;
            story.vars["varNumber"] = 4.669;
            story.vars["varBool"] = true;
            story.vars["varNull"] = null;

            string input = "{{ $varStr }}";
            input += " {{ $varInt }}";
            input += " {{ $varNumber }}";
            input += " {{ $varBool }}";
            input += " {{ $varNull }}";

            string output = story.RunPassage(input);
            Assert.AreEqual("Foo bar 42 4.669 True ", output);
        }

        [TestMethod]
        public void CmpFilePrint01()
        {
            StreamReader input = File.OpenText("scripts/basic/print01.vine");
            StreamReader cmp = File.OpenText("scripts/basic/print01.cmp");

            VineStory story = new VineStory();
            string output = story.RunPassage(input);

            Assert.AreEqual(cmp.ReadToEnd(), output);
        }

        [TestMethod]
        public void CmpFileSet01()
        {
            StreamReader input = File.OpenText("scripts/basic/set01.vine");
            StreamReader cmp = File.OpenText("scripts/basic/set01.cmp");

            VineStory story = new VineStory();
            string output = story.RunPassage(input);

            Assert.AreEqual(cmp.ReadToEnd(), output);
        }

        [TestMethod]
        public void CmpFileSet02()
        {
            // Var reassign
            StreamReader input = File.OpenText("scripts/basic/set02.vine");
            StreamReader cmp = File.OpenText("scripts/basic/set02.cmp");

            VineStory story = new VineStory();
            string output = story.RunPassage(input);

            Assert.AreEqual(cmp.ReadToEnd(), output);
        }

        [TestMethod]
        public void SimpleIf()
        {
            VineStory story = new VineStory();
            story.vars["var"] = 42;
            string input = "{% if $var == 0 %}Zero{% elif $var == 42%}Forty Two{% else %}Other{% end %}";
            string output = story.RunPassage(input);
            Assert.AreEqual("Forty Two", output);
        }

        [TestMethod]
        public void FileIf01()
        {
            VineStory story = new VineStory();
            
            story.RunPassage(File.OpenText("scripts/basic/if01.vine"));
            Assert.AreEqual("var is null", story.vars["result"]);

            story.vars["var"] = "str";
            story.RunPassage(File.OpenText("scripts/basic/if01.vine"));
            Assert.AreEqual("var is a string", story.vars["result"]);

            story.vars["var"] = 42;
            story.RunPassage(File.OpenText("scripts/basic/if01.vine"));
            Assert.AreEqual("var is an int", story.vars["result"]);

            story.vars["var"] = 4.669;
            story.RunPassage(File.OpenText("scripts/basic/if01.vine"));
            Assert.AreEqual("var is a number", story.vars["result"]);

            story.vars["var"] = true;
            story.RunPassage(File.OpenText("scripts/basic/if01.vine"));
            Assert.AreEqual("var is a bool", story.vars["result"]);

            story.vars["var"] = false;
            story.RunPassage(File.OpenText("scripts/basic/if01.vine"));
            Assert.AreEqual("var is a bool", story.vars["result"]);
        }

        [TestMethod]
        public void CmpFileLangChars01()
        {
            StreamReader input = File.OpenText("scripts/basic/lang_chars01.vine");
            StreamReader cmp = File.OpenText("scripts/basic/lang_chars01.cmp");

            VineStory story = new VineStory();
            string output = story.RunPassage(input);

            Assert.AreEqual(cmp.ReadToEnd(), output);
        }

        [TestMethod]
        public void FileComments01()
        {
            VineStory story = new VineStory();

            story.RunPassage(File.OpenText("scripts/basic/comments01.vine"));

            // TODO checks syntax error when implemented
            // If there's not error reading this file, we're good.
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion
    }
}