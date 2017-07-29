using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VineScript.Core;

namespace VineScript.Test
{
    [TestClass]
    public class FormattingTests
    {
        [TestMethod]
        public void LineEnding01()
        {
            Utils.CompareFile("scripts/formatting/line_ending01.vine");
        }

        [TestMethod]
        public void LineEnding02()
        {
            Utils.CompareFile("scripts/formatting/line_ending02.vine");
        }

        [TestMethod]
        public void LineEnding03()
        {
            Utils.CompareFile("scripts/formatting/line_ending03.vine");
        }

        [TestMethod]
        public void LineEnding04()
        {
            Utils.CompareFile("scripts/formatting/line_ending04.vine");
        }

        [TestMethod]
        public void LineEnding05()
        {
            Utils.CompareFile("scripts/formatting/line_ending05.vine");
        }

        [TestMethod]
        public void Comments01()
        {
            Utils.CompareFile("scripts/formatting/comments01.vine");
        }

        [TestMethod]
        public void InlineComments01()
        {
            Utils.CompareFile("scripts/formatting/inline_comments01.vine");
        }

        [TestMethod]
        public void EscapedChars01()
        {
            Utils.CompareFile("scripts/formatting/escape_tags01.vine");
        }

        [TestMethod]
        public void EscapedChars02()
        {
            Utils.CompareFile("scripts/formatting/escape_tags02.vine");
        }
    }
}
