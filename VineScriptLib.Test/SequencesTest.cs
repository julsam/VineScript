using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VineScriptLib;
using VineScriptLib.Core;

namespace VineScriptLib.Test
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
        public void SequenceInvalidIndexes()
        {
            try {
                // Negative index
                VineStory story = new VineStory();
                string input = "{% set $arr = [1, 2, 3] %}\n"
                    + "{{ $arr[-1] %}";
                story.RunPassage(input);
                Assert.Fail();
            } catch (Exception) {
                // It's ok
            } 

            try {
                // Out of range index
                VineStory story = new VineStory();
                string input = "{% set $arr = [1, 2, 3] %}\n"
                    + "{{ $arr[3] %}";
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

            string input = "{% set str = \"FooBar\" %}\n"
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

            string input = "{% set str = \"FooBar\" %}\n"
                + "{% set str[0] = \"B\" %}";
            
            try {
                // Strings don't support item assignment
                story.RunPassage(input);
                Assert.Fail();
            } catch (Exception) {
                // It's ok
            } 
        }
    }
}
