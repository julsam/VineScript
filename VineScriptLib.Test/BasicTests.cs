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
            story.vars["varNull"] = VineValue.NULL;

            string input = "{{ $varStr }}";
            input += " {{ $varInt }}";
            input += " {{ $varNumber }}";
            input += " {{ $varBool }}";
            input += " {{ $varNull }}";

            string output = story.RunPassage(input);
            Assert.AreEqual("Foo bar 42 4.669 True", output);
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
            string input = "{% if $var == 0 %}Zero{% elif $var == 42%}Forty Two{% else %}Other{% endif %}";
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

        [TestMethod]
        public void DisplayAdditionPriority()
        {
            VineStory story = new VineStory();
            story.vars["varInt1"] = 1;
            story.vars["varInt2"] = 2;
            story.vars["varStr"] = "Foo";

            string input = "{{ \"Foo\" + 1 + 2 }}";
            input += " {{ 1 + 2 + \"Foo\" }}";
            input += " {{ varStr + varInt1 + varInt2 }}";
            input += " {{ varInt1 + varInt2 + varStr}}";
            input += " {{ \"Foo\" + 1.0 + 2.0 }}";
            input += " {{ 1.0 + 2.0 + \"Foo\" }}";
            input += " {{ \"Foo\" + 1.5 + 2.5 }}";
            input += " {{ 1.5 + 2.5 + \"Foo\" }}";

            string output = story.RunPassage(input);
            Assert.AreEqual("Foo12 3Foo Foo12 3Foo Foo1.02.0 3.0Foo Foo1.52.5 4.0Foo", output);
        }

        [TestMethod]
        public void ArrayCreation01()
        {
            VineStory story = new VineStory();

            string input = "{% set $arr = [1, 2.0, \"Three\", false, null] %}";

            string output = story.RunPassage(input);
            
            Assert.IsNotNull(story.vars["arr"]);
            Assert.IsTrue(story.vars["arr"].IsArray);
            Assert.AreEqual(1, story.vars["arr"][0]);
            Assert.AreEqual(2.0, story.vars["arr"][1]);
            Assert.AreEqual("Three", story.vars["arr"][2]);
            Assert.AreEqual(false, story.vars["arr"][3]);
            Assert.AreEqual(VineValue.NULL, story.vars["arr"][4]);
        }

        [TestMethod]
        public void DictCreation01()
        {
            VineStory story = new VineStory();

            string input = "{% set $dict = { \"a\": 1, \"b\": 2.0, \"c\": \"Three\", \"d\": false, \"e\": null } %}";

            string output = story.RunPassage(input);
            
            Assert.IsNotNull(story.vars["dict"]);
            Assert.IsTrue(story.vars["dict"].IsDict);
            Assert.AreEqual(1, story.vars["dict"]["a"]);
            Assert.AreEqual(2.0, story.vars["dict"]["b"]);
            Assert.AreEqual("Three", story.vars["dict"]["c"]);
            Assert.AreEqual(false, story.vars["dict"]["d"]);
            Assert.AreEqual(VineValue.NULL, story.vars["dict"]["e"]);
        }

        [TestMethod]
        public void AssignList()
        {
            VineStory story = new VineStory();

            string input = "{% set v1 = 12, v2 = 2.0, v3 = true, v4 = null, ";
            input += "v5 = \"Foo\", v6 = [1,2,3], v7 = {\"a\": 4, \"b\": 5 } %}";

            string output = story.RunPassage(input);
            
            Assert.AreEqual(12, story.vars["v1"]);
            Assert.AreEqual(2.0, story.vars["v2"]);
            Assert.AreEqual(true, story.vars["v3"]);
            Assert.AreEqual(VineValue.NULL, story.vars["v4"]);
            Assert.AreEqual("Foo", story.vars["v5"]);
            Assert.AreEqual(3, story.vars["v6"][2]);
            Assert.AreEqual(4, story.vars["v7"]["a"]);
        }

        [TestMethod]
        public void CmpFileFor01()
        {
            StreamReader input = File.OpenText("scripts/basic/for01.vine");
            StreamReader cmp = File.OpenText("scripts/basic/for01.cmp");

            VineStory story = new VineStory();
            string output = story.RunPassage(input);

            Assert.AreEqual(cmp.ReadToEnd(), output);
        }

        [TestMethod]
        public void CmpFileFor02()
        {
            StreamReader input = File.OpenText("scripts/basic/for02.vine");
            StreamReader cmp = File.OpenText("scripts/basic/for02.cmp");

            VineStory story = new VineStory();
            string output = story.RunPassage(input);

            Assert.AreEqual(cmp.ReadToEnd(), output);
        }

        [TestMethod]
        public void CmpFileFor03()
        {
            StreamReader input = File.OpenText("scripts/basic/for03.vine");
            StreamReader cmp = File.OpenText("scripts/basic/for03.cmp");

            VineStory story = new VineStory();
            string output = story.RunPassage(input);

            Assert.AreEqual(cmp.ReadToEnd(), output);
        }

        [TestMethod]
        public void CmpFileFor04()
        {
            StreamReader input = File.OpenText("scripts/basic/for04.vine");
            StreamReader cmp = File.OpenText("scripts/basic/for04.cmp");

            VineStory story = new VineStory();
            string output = story.RunPassage(input);

            Assert.AreEqual(cmp.ReadToEnd(), output);
        }

        [TestMethod]
        public void CmpFileInterval01()
        {
            StreamReader input = File.OpenText("scripts/basic/interval01.vine");
            StreamReader cmp = File.OpenText("scripts/basic/interval01.cmp");

            VineStory story = new VineStory();
            string output = story.RunPassage(input);

            Assert.AreEqual(cmp.ReadToEnd(), output);
        }

        [TestMethod]
        public void CmpFileUnescapeStringLiteral01()
        {
            StreamReader input = File.OpenText("scripts/basic/unescape_string01.vine");
            StreamReader cmp = File.OpenText("scripts/basic/unescape_string01.cmp");

            VineStory story = new VineStory();
            string output = story.RunPassage(input);

            Assert.AreEqual(cmp.ReadToEnd(), output);
        }

        [TestMethod]
        public void CmpFileUnescapeStringLiteral02()
        {
            try {
                StreamReader input = File.OpenText("scripts/basic/unescape_string02.vine");
                VineStory story = new VineStory();
                string output = story.RunPassage(input);
                Assert.Fail();
            }
            catch (Exception) {
                /* All good, the '\r' character should make it fail */
            }
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