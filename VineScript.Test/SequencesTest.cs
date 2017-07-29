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
        [TestMethod]
        public void SequenceCmpArrayGetSet01()
        {
            
            StreamReader input = File.OpenText("scripts/sequences/arraygetset01.vine");
            StreamReader cmp = File.OpenText("scripts/sequences/arraygetset01.cmp");

            VineStory story = new VineStory();
            string output = story.RunPassage(input);

            Assert.AreEqual(cmp.ReadToEnd(), output);
        }

        [TestMethod]
        public void SequenceCmpArrayGetSet02()
        {
            
            StreamReader input = File.OpenText("scripts/sequences/arraygetset02.vine");
            StreamReader cmp = File.OpenText("scripts/sequences/arraygetset02.cmp");

            VineStory story = new VineStory();
            string output = story.RunPassage(input);

            Assert.AreEqual(cmp.ReadToEnd(), output);
        }

        [TestMethod]
        public void SequenceCmpArrayGetSet03()
        {
            
            StreamReader input = File.OpenText("scripts/sequences/arraygetset03.vine");
            StreamReader cmp = File.OpenText("scripts/sequences/arraygetset03.cmp");

            VineStory story = new VineStory();
            string output = story.RunPassage(input);

            Assert.AreEqual(cmp.ReadToEnd(), output);
        }

        [TestMethod]
        public void SequenceInvalidIndexes()
        {
            try {
                // Negative index
                VineStory story = new VineStory();
                string input = "<< set $arr = [1, 2, 3] >>\n"
                    + "{{ $arr[-1] >>";
                story.RunPassage(input);
                Assert.Fail();
            } catch (Exception) {
                // It's ok
            } 

            try {
                // Out of range index
                VineStory story = new VineStory();
                string input = "<< set $arr = [1, 2, 3] >>\n"
                    + "{{ $arr[3] >>";
                story.RunPassage(input);
                Assert.Fail();
            } catch (Exception) {
                // It's ok
            } 
        }

        [TestMethod]
        public void SequenceStringGetSet01()
        {
            VineStory story = new VineStory();

            string input = "<< set str = \"FooBar\" >>\n"
                + "{{ str }}, "
                + "{{ str[0] }}, "
                + "{{ str[1] }}, "
                + "{{ str[2] }}, "
                + "{{ str[3] }}";

            string output = story.RunPassage(input);

            Assert.AreEqual("FooBar, F, o, o, B", output);
        }

        [TestMethod]
        public void SequenceStringSetInvalid()
        {
            VineStory story = new VineStory();

            string input = "<< set str = \"FooBar\" >>\n"
                + "<< set str[0] = \"B\" >>";
            
            try {
                // Strings don't support item assignment
                story.RunPassage(input);
                Assert.Fail();
            } catch (Exception) {
                // It's ok
            } 
        }

        [TestMethod]
        public void SequenceSetArray2DForLoop01()
        {
            VineStory story = new VineStory();
            
            // 'arr' is an array containing 3 sub-arrays, each containing 2 ints.
            // As 'el' is a reference to 'arr', it's possible to modify the content
            // of each sub-array.

            //<< set arr = [[0, 1], [2, 3], [4, 5]] >>
            //<< for el in arr >>
            //    << set el[0] = el[0] + 1 >>
            //    << set el[1] = el[1] + 1 >>
            //<< endfor >>

            StreamReader input = File.OpenText("scripts/sequences/array2d_set_for01.vine");
            story.RunPassage(input);
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
            VineStory story = new VineStory();

            // 'arr' is an array containing 3 sub-arrays, each containing 2 ints.
            // Here, 'el' is still a reference, but it's a ref to a cloned 'arr',
            // so 'el' won't be able change the content of the sub-arrays of 'arr'
            // as it's actually modifying the content of the cloned 'arr'.

            //<< set arr = [[0, 1], [2, 3], [4, 5]] >>
            //<< for el in Clone(arr) >>
            //    << set el[0] = el[0] + 1 >>
            //    << set el[1] = el[1] + 1 >>
            //<< endfor >>

            StreamReader input = File.OpenText("scripts/sequences/array2d_set_for02.vine");
            story.RunPassage(input);
            Assert.AreEqual(0, story.vars["arr"][0][0]);
            Assert.AreEqual(1, story.vars["arr"][0][1]);
            Assert.AreEqual(2, story.vars["arr"][1][0]);
            Assert.AreEqual(3, story.vars["arr"][1][1]);
            Assert.AreEqual(4, story.vars["arr"][2][0]);
            Assert.AreEqual(5, story.vars["arr"][2][1]);
        }
    }
}
