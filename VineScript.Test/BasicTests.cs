using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VineScript;
using VineScript.Core;

namespace VineScript.Test
{
    /// <summary>
    /// Summary description for BasicTests
    /// </summary>
    [TestClass]
    public class BasicTests
    {
        VineStory story;

        public BasicTests()
        {
            story = new VineStory("");
        }

        [TestMethod]
        public void Empty()
        {
            string input = "";

            Loader loader = new Loader();
            loader.LoadCode(input, "test");
            VineStory newstory = new VineStory(loader);

            string output = newstory.RunPassage("test").Text;
            Assert.AreEqual("", output);
        }

        [TestMethod]
        public void SimpleComment()
        {
            string input = "/* comment */";

            Loader loader = new Loader();
            loader.LoadCode(input, "test");
            VineStory newstory = new VineStory(loader);

            string output = newstory.RunPassage("test").Text;
            Assert.AreEqual("", output);
        }

        [TestMethod]
        public void SimplePrintInt()
        {
            string input = "{{ $var1 }}";

            Loader loader = new Loader();
            loader.LoadCode(input, "test");
            VineStory newstory = new VineStory(loader);

            newstory.vars["var1"] = 42;
            string output = newstory.RunPassage("test").Text;
            Assert.AreEqual("42", output);
        }

        [TestMethod]
        public void PrintDefinedVars()
        {
            string input = "{{ $varStr }}";
            input += " {{ $varInt }}";
            input += " {{ $varNumber }}";
            input += " {{ $varBool }}";
            input += " {{ $varNull }}";
            
            Loader loader = new Loader();
            loader.LoadCode(input, "test");
            VineStory newstory = new VineStory(loader);

            newstory.vars["varStr"] = "Foo bar";
            newstory.vars["varInt"] = 42;
            newstory.vars["varNumber"] = 4.669;
            newstory.vars["varBool"] = true;
            newstory.vars["varNull"] = VineVar.NULL;
            
            string output = newstory.RunPassage("test").Text;
            Assert.AreEqual("Foo bar 42 4.669 True", output);
        }

        [TestMethod]
        public void CmpFilePrint01()
        {
            string input = "scripts/basic/print01";
            StreamReader cmp = File.OpenText("scripts/basic/print01.cmp");
            
            string output = story.RunPassage(input).Text;

            Assert.AreEqual(cmp.ReadToEnd(), output);
        }

        [TestMethod]
        public void CmpFileSet01()
        {
            string input = "scripts/basic/set01";
            StreamReader cmp = File.OpenText("scripts/basic/set01.cmp");
            
            string output = story.RunPassage(input).Text;

            Assert.AreEqual(cmp.ReadToEnd(), output);
        }

        [TestMethod]
        public void CmpFileSet02()
        {
            // Var reassign
            string input = "scripts/basic/set02";
            StreamReader cmp = File.OpenText("scripts/basic/set02.cmp");
            
            string output = story.RunPassage(input).Text;

            Assert.AreEqual(cmp.ReadToEnd(), output);
        }

        [TestMethod]
        public void CmpFileSet03()
        {
            // +=, -=, *=, /=, %=
            string input = "scripts/basic/set03";
            StreamReader cmp = File.OpenText("scripts/basic/set03.cmp");
            
            string output = story.RunPassage(input).Text;

            Assert.AreEqual(cmp.ReadToEnd(), output);
        }

        [TestMethod]
        public void AssignList()
        {
            string input = "<< set v1 = 12, v2 = 2.0, v3 = true, v4 = null, ";
            input += "v5 = \"Foo\", v6 = [1,2,3], v7 = {\"a\": 4, \"b\": 5 } >>";
            
            Loader loader = new Loader();
            loader.LoadCode(input, "test");
            VineStory newstory = new VineStory(loader);

            string output = newstory.RunPassage("test").Text;
            
            Assert.AreEqual(12, newstory.vars["v1"]);
            Assert.AreEqual(2.0, newstory.vars["v2"]);
            Assert.AreEqual(true, newstory.vars["v3"]);
            Assert.AreEqual(VineVar.NULL, newstory.vars["v4"]);
            Assert.AreEqual("Foo", newstory.vars["v5"]);
            Assert.AreEqual(3, newstory.vars["v6"][2]);
            Assert.AreEqual(4, newstory.vars["v7"]["a"]);
        }

        [TestMethod]
        public void Unset01()
        {
            string input = "scripts/basic/unset01";
            
            string output = story.RunPassage(input).Text;
            
            Assert.IsFalse(story.vars.ContainsKey("a"));
            Assert.IsFalse(story.vars.ContainsKey("b"));
            Assert.IsFalse(story.vars.ContainsKey("c"));
            Assert.IsFalse(story.vars.ContainsKey("d"));
            Assert.IsFalse(story.vars.ContainsKey("e"));
            Assert.IsFalse(story.vars.ContainsKey("f"));
            Assert.IsFalse(story.vars.ContainsKey("g"));
        }

        [TestMethod]
        public void SimpleIf()
        {
            string input = "<< if $var == 0 >>Zero<< elif $var == 42>>Forty Two<< else >>Other<< end >>";
            
            Loader loader = new Loader();
            loader.LoadCode(input, "test");
            VineStory newstory = new VineStory(loader);

            newstory.vars["var"] = 42;
            string output = newstory.RunPassage("test").Text;
            Assert.AreEqual("Forty Two", output);
        }

        [TestMethod]
        public void FileIf01()
        {
            story.vars.Clear();

            story.RunPassage("scripts/basic/if01");
            Assert.AreEqual("var is null", story.vars["result"]);

            story.vars["var"] = "str";
            story.RunPassage("scripts/basic/if01");
            Assert.AreEqual("var is a string", story.vars["result"]);

            story.vars["var"] = 42;
            story.RunPassage("scripts/basic/if01");
            Assert.AreEqual("var is an int", story.vars["result"]);

            story.vars["var"] = 4.669;
            story.RunPassage("scripts/basic/if01");
            Assert.AreEqual("var is a number", story.vars["result"]);

            story.vars["var"] = true;
            story.RunPassage("scripts/basic/if01");
            Assert.AreEqual("var is a bool", story.vars["result"]);

            story.vars["var"] = false;
            story.RunPassage("scripts/basic/if01");
            Assert.AreEqual("var is a bool", story.vars["result"]);
        }

        [TestMethod]
        public void FileIf02()
        {
            /*
            << if ($hello_world == "Hello, World!" || 1 > 2) 
                and (0 < 1) && $my_int >= 42 && $my_int != "42"
            >>
            */
            string filename = "scripts/basic/if02";
            
            story.vars["hello_world"] = "Hello, World!";
            story.vars["my_int"] = 0;
            story.RunPassage(filename);
            Assert.AreEqual(new VineVar(false), story.vars["result"]);
            
            story.vars["hello_world"] = "abc";
            story.vars["my_int"] = 43;
            story.RunPassage(filename);
            Assert.AreEqual(new VineVar(false), story.vars["result"]);
            
            story.vars["hello_world"] = "Hello, World!";
            story.vars["my_int"] = 43;
            story.RunPassage(filename);
            Assert.AreEqual(new VineVar(true), story.vars["result"]);
        }

        [TestMethod]
        public void CmpFileLangChars01()
        {
            string input = "scripts/basic/lang_chars01";
            StreamReader cmp = File.OpenText("scripts/basic/lang_chars01.cmp");
            
            string output = story.RunPassage(input).Text;

            Assert.AreEqual(cmp.ReadToEnd(), output);
        }

        [TestMethod]
        public void FileComments01()
        {
            VineStory story = new VineStory();

            story.RunPassage("scripts/basic/comments01");

            // TODO checks syntax error when implemented
            // If there's not error reading this file, we're good.
        }

        [TestMethod]
        public void DisplayAdditionPriority()
        {
            string input = "{{ \"Foo\" + 1 + 2 }}";
            input += " {{ 1 + 2 + \"Foo\" }}";
            input += " {{ varStr + varInt1 + varInt2 }}";
            input += " {{ varInt1 + varInt2 + varStr}}";
            input += " {{ \"Foo\" + 1.0 + 2.0 }}";
            input += " {{ 1.0 + 2.0 + \"Foo\" }}";
            input += " {{ \"Foo\" + 1.5 + 2.5 }}";
            input += " {{ 1.5 + 2.5 + \"Foo\" }}";
            
            Loader loader = new Loader();
            loader.LoadCode(input, "test");
            VineStory newstory = new VineStory(loader);

            newstory.vars["varInt1"] = 1;
            newstory.vars["varInt2"] = 2;
            newstory.vars["varStr"] = "Foo";

            string output = newstory.RunPassage("test").Text;
            Assert.AreEqual("Foo12 3Foo Foo12 3Foo Foo1.02.0 3.0Foo Foo1.52.5 4.0Foo", output);
        }

        [TestMethod]
        public void ArrayCreation01()
        {
            string input = "<< set $arr = [1, 2.0, \"Three\", false, null] >>";
            
            Loader loader = new Loader();
            loader.LoadCode(input, "test");
            VineStory newstory = new VineStory(loader);

            string output = newstory.RunPassage("test").Text;
            
            Assert.IsNotNull(newstory.vars["arr"]);
            Assert.IsTrue(newstory.vars["arr"].IsArray);
            Assert.AreEqual(1, newstory.vars["arr"][0]);
            Assert.AreEqual(2.0, newstory.vars["arr"][1]);
            Assert.AreEqual("Three", newstory.vars["arr"][2]);
            Assert.AreEqual(false, newstory.vars["arr"][3]);
            Assert.AreEqual(VineVar.NULL, newstory.vars["arr"][4]);
        }

        [TestMethod]
        public void DictCreation01()
        {
            string input = "<< set $dict = { \"a\": 1, \"b\": 2.0, \"c\": \"Three\", \"d\": false, \"e\": null } >>";
            
            Loader loader = new Loader();
            loader.LoadCode(input, "test");
            VineStory newstory = new VineStory(loader);

            string output = newstory.RunPassage("test").Text;
            
            Assert.IsNotNull(newstory.vars["dict"]);
            Assert.IsTrue(newstory.vars["dict"].IsDict);
            Assert.AreEqual(1, newstory.vars["dict"]["a"]);
            Assert.AreEqual(2.0, newstory.vars["dict"]["b"]);
            Assert.AreEqual("Three", newstory.vars["dict"]["c"]);
            Assert.AreEqual(false, newstory.vars["dict"]["d"]);
            Assert.AreEqual(VineVar.NULL, newstory.vars["dict"]["e"]);
        }

        [TestMethod]
        public void CmpFileFor01()
        {
            string input = "scripts/basic/for01";
            StreamReader cmp = File.OpenText("scripts/basic/for01.cmp");
            
            string output = story.RunPassage(input).Text;

            Assert.AreEqual(cmp.ReadToEnd(), output);
        }

        [TestMethod]
        public void CmpFileFor02()
        {
            string input = "scripts/basic/for02";
            StreamReader cmp = File.OpenText("scripts/basic/for02.cmp");
            
            string output = story.RunPassage(input).Text;

            Assert.AreEqual(cmp.ReadToEnd(), output);
        }

        [TestMethod]
        public void CmpFileFor03()
        {
            string input = "scripts/basic/for03";
            StreamReader cmp = File.OpenText("scripts/basic/for03.cmp");
            
            string output = story.RunPassage(input).Text;

            Assert.AreEqual(cmp.ReadToEnd(), output);
        }

        [TestMethod]
        public void CmpFileFor04()
        {
            string input = "scripts/basic/for04";
            StreamReader cmp = File.OpenText("scripts/basic/for04.cmp");
            
            string output = story.RunPassage(input).Text;

            Assert.AreEqual(cmp.ReadToEnd(), output);
        }

        [TestMethod]
        public void CmpFileForKeyValue01()
        {
            string input = "scripts/basic/forkeyvalue01";
            StreamReader cmp = File.OpenText("scripts/basic/forkeyvalue01.cmp");
            
            string output = story.RunPassage(input).Text;

            Assert.AreEqual(cmp.ReadToEnd(), output);
        }

        [TestMethod]
        public void CmpFileForRange01()
        {
            string input = "scripts/basic/for_range01";
            StreamReader cmp = File.OpenText("scripts/basic/for_range01.cmp");
            
            string output = story.RunPassage(input).Text;

            Assert.AreEqual(cmp.ReadToEnd(), output);
        }

        [TestMethod]
        public void CmpFileInterval01()
        {
            string input = "scripts/basic/interval01";
            StreamReader cmp = File.OpenText("scripts/basic/interval01.cmp");
            
            string output = story.RunPassage(input).Text;

            Assert.AreEqual(cmp.ReadToEnd(), output);
        }

        [TestMethod]
        public void CmpFileUnescapeStringLiteral01()
        {
            string input = "scripts/basic/unescape_string01";
            StreamReader cmp = File.OpenText("scripts/basic/unescape_string01.cmp");
            
            string output = story.RunPassage(input).Text;

            Assert.AreEqual(cmp.ReadToEnd(), output);
        }

        [TestMethod]
        public void CmpFileUnescapeStringLiteral02()
        {
            try {
                string input = "scripts/basic/unescape_string02";
                string output = story.RunPassage(input).Text;
                Assert.Fail();
            }
            catch (Exception) {
                /* All good, the '\r' character should make it fail */
            }
        }

        [TestMethod]
        public void CmpFileUnescapeStringLiteral03()
        {
            string input = "scripts/basic/unescape_string03";
            StreamReader cmp = File.OpenText("scripts/basic/unescape_string03.cmp");
            
            string output = story.RunPassage(input).Text;

            Assert.AreEqual(cmp.ReadToEnd(), output);
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