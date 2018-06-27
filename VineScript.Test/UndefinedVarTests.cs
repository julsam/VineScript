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
    public class UndefinedVarTests
    {
        // inner class defining an userlib
        public class TestRefOutUserLib
        {
            [Binding.VineBinding]
            public static void MyRefFunc(ref VineVar value)
            {
                // can't be called if the argument is undefined
                value = "won't work";
            }

            [Binding.VineBinding]
            public static void MyOutFunc(out VineVar value)
            {
                // if the argument is undefined this will still work
                value = "it's working";
            }
        }

        [TestMethod]
        public void TestUndefinedRefArg()
        {
            UserLib userlib = new UserLib();
            userlib.Bind(typeof(TestRefOutUserLib));

            // the parameter of MyRefFunc is 'ref',
            // undefined args are not allowed (unlike for 'out')
            string input = "<< MyRefFunc(myvar) >>";
            try {
                Loader loader = new Loader();
                loader.LoadCode(input, "test");
                VineStory story = new VineStory(loader, userlib);
                story.RunPassage("test");
            } catch (VineUndefinedVarException e) {
                // OK
            } catch (Exception) {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void TestUndefinedOutArg()
        {
            UserLib userlib = new UserLib();
            userlib.Bind(typeof(TestRefOutUserLib));

            // the parameter of MyOutFunc is 'out', so in this case only,
            // the argument is allowed to be undefined
            string input = "<< MyOutFunc(myvar) >>";
            try {
                Loader loader = new Loader();
                loader.LoadCode(input, "test");
                VineStory story = new VineStory(loader, userlib);
                story.RunPassage("test");
                Assert.AreEqual("it's working", story.vars["myvar"]);
            } catch (VineUndefinedVarException e) {
                Assert.Fail();
            } catch (Exception) {
                Assert.Fail();
            }
        }
        [TestMethod]
        public void TestUndefined01()
        {
            // can't use undefined variable 'myvar'
            string input = "<<if myvar == null>>foobar<<end>>";
            try {
                Loader loader = new Loader();
                loader.LoadCode(input, "test");
                VineStory story = new VineStory(loader);
                story.RunPassage("test");
                Assert.Fail();
            } catch (VineUndefinedVarException e) {
                // OK
            } catch (Exception) {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void TestUndefined02()
        {
            // can't assign undefined variable 'myvar2'
            string input = "<<set myvar = myvar2>>";
            try {
                Loader loader = new Loader();
                loader.LoadCode(input, "test");
                VineStory story = new VineStory(loader);
                story.RunPassage("test");
                Assert.Fail();
            } catch (VineUndefinedVarException e) {
                // OK
            } catch (Exception) {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void TestUndefined03()
        {
            // can't pass undefined args to function (unless the parameter is 'Out')
            string input = "<< IsNull(myvar) >>";
            try {
                Loader loader = new Loader();
                loader.LoadCode(input, "test");
                VineStory story = new VineStory(loader);
                story.RunPassage("test");
                Assert.Fail();
            } catch (VineUndefinedVarException e) {
                // OK
            } catch (Exception) {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void TestUndefined04()
        {
            string input = "{{ myvar + 1 }}";
            try {
                Loader loader = new Loader();
                loader.LoadCode(input, "test");
                VineStory story = new VineStory(loader);
                story.RunPassage("test");
                Assert.Fail();
            } catch (VineUndefinedVarException e) {
                // OK
            } catch (Exception) {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void TestUndefined05()
        {
            string input = "{{ \"foo\" + myvar }}";
            try {
                Loader loader = new Loader();
                loader.LoadCode(input, "test");
                VineStory story = new VineStory(loader);
                story.RunPassage("test");
                Assert.Fail();
            } catch (VineUndefinedVarException e) {
                // OK
            } catch (Exception) {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void TestUndefined06()
        {
            UserLib userlib = new UserLib();
            userlib.Bind(typeof(TestRefOutUserLib));

            // trying to access the sequence element of an undefined
            // variable inside a function call (that allows undefined)
            string input = "<< MyOutFunc(myvar[0]) >>";
            try {
                Loader loader = new Loader();
                loader.LoadCode(input, "test");
                VineStory story = new VineStory(loader, userlib);
                story.RunPassage("test");
            } catch (VineUndefinedVarException e) {
                // OK
            } catch (Exception) {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void TestUndefined07()
        {
            UserLib userlib = new UserLib();
            userlib.Bind(typeof(TestRefOutUserLib));

            // undefined variable used as an index for accessing an array
            string input = "<<set myvar = [1,2]>>"
                + "<< MyOutFunc(myvar[b]) >>";
            try {
                Loader loader = new Loader();
                loader.LoadCode(input, "test");
                VineStory story = new VineStory(loader, userlib);
                story.RunPassage("test");
            } catch (VineUndefinedVarException e) {
                // OK
            } catch (Exception) {
                Assert.Fail();
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