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
    public class SequencesTest
    {
        VineStory story;

        public SequencesTest()
        {
            story = new VineStory("");
        }

        [TestMethod]
        public void SequenceCmpArrayGetSet01()
        {
            string input = "scripts/sequences/arraygetset01";
            StreamReader cmp = File.OpenText("scripts/sequences/arraygetset01.cmp");
            
            string output = story.RunPassage(input).text;

            Assert.AreEqual(cmp.ReadToEnd(), output);
        }

        [TestMethod]
        public void SequenceCmpArrayGetSet02()
        {
            string input = "scripts/sequences/arraygetset02";
            StreamReader cmp = File.OpenText("scripts/sequences/arraygetset02.cmp");
            
            string output = story.RunPassage(input).text;

            Assert.AreEqual(cmp.ReadToEnd(), output);
        }

        [TestMethod]
        public void SequenceInvalidIndexes()
        {
            try {
                // Negative index
                string input = "<< set $arr = [1, 2, 3] >>\n"
                    + "{{ $arr[-1] >>";

                Loader loader = new Loader();
                loader.LoadCode(input, "test");
                VineStory newstory = new VineStory(loader);

                string output = newstory.RunPassage("test").text;
                Assert.Fail();
            } catch (Exception) {
                // It's ok
            } 

            try {
                // Out of range index
                string input = "<< set $arr = [1, 2, 3] >>\n"
                    + "{{ $arr[3] >>";

                Loader loader = new Loader();
                loader.LoadCode(input, "test");
                VineStory newstory = new VineStory(loader);

                string output = newstory.RunPassage("test").text;
                Assert.Fail();
            } catch (Exception) {
                // It's ok
            } 
        }

        [TestMethod]
        public void SequenceStringGetSet01()
        {
            string input = "<< set str = \"FooBar\" >>\n"
                + "{{ str }}, "
                + "{{ str[0] }}, "
                + "{{ str[1] }}, "
                + "{{ str[2] }}, "
                + "{{ str[3] }}";

            Loader loader = new Loader();
            loader.LoadCode(input, "test");
            VineStory newstory = new VineStory(loader);
            string output = newstory.RunPassage("test").text;

            Assert.AreEqual("FooBar, F, o, o, B", output);
        }

        [TestMethod]
        public void SequenceStringSetInvalid()
        {
            // Strings don't support item assignment
            string input = "<< set str = \"FooBar\" >>\n"
                + "<< set str[0] = \"B\" >>";
            
            try {
                Loader loader = new Loader();
                loader.LoadCode(input, "test");
                VineStory newstory = new VineStory(loader);
                
                string output = newstory.RunPassage("test").text;
                Assert.Fail();
            } catch (Exception) {
                // It's ok
            } 
        }

        [TestMethod]
        public void SequenceSetArray2DForLoop01()
        {
            // 'arr' is an array containing 3 sub-arrays, each containing 2 ints.
            // As 'el' is a reference to 'arr', it's possible to modify the content
            // of each sub-array.

            //<< set arr = [[0, 1], [2, 3], [4, 5]] >>
            //<< for el in arr >>
            //    << set el[0] = el[0] + 1 >>
            //    << set el[1] = el[1] + 1 >>
            //<< end >>

            string input = "scripts/sequences/array2d_set_for01";
            
            string output = story.RunPassage(input).text;

            Assert.AreEqual(1, story.vars["arr"][0][0]);
            Assert.AreEqual(2, story.vars["arr"][0][1]);
            Assert.AreEqual(3, story.vars["arr"][1][0]);
            Assert.AreEqual(4, story.vars["arr"][1][1]);
            Assert.AreEqual(5, story.vars["arr"][2][0]);
            Assert.AreEqual(6, story.vars["arr"][2][1]);
        }

        [TestMethod]
        public void SequenceSetArray2DForLoop02()
        {
            // 'arr' is an array containing 3 sub-arrays, each containing 2 ints.
            // Here, 'el' is still a reference, but it's a ref to a cloned 'arr',
            // so 'el' won't be able change the content of the sub-arrays of 'arr'
            // as it's actually modifying the content of the cloned 'arr'.

            //<< set arr = [[0, 1], [2, 3], [4, 5]] >>
            //<< for el in Clone(arr) >>
            //    << set el[0] = el[0] + 1 >>
            //    << set el[1] = el[1] + 1 >>
            //<< end >>

            string input = "scripts/sequences/array2d_set_for02";
            
            string output = story.RunPassage(input).text;

            Assert.AreEqual(0, story.vars["arr"][0][0]);
            Assert.AreEqual(1, story.vars["arr"][0][1]);
            Assert.AreEqual(2, story.vars["arr"][1][0]);
            Assert.AreEqual(3, story.vars["arr"][1][1]);
            Assert.AreEqual(4, story.vars["arr"][2][0]);
            Assert.AreEqual(5, story.vars["arr"][2][1]);
        }
    }
}
