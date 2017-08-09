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
        public void EscapedTags01()
        {
            Utils.CompareFile("scripts/formatting/escape_tags01.vine");
        }

        [TestMethod]
        public void EscapedTags02()
        {
            Utils.CompareFile("scripts/formatting/escape_tags02.vine");
        }

        [TestMethod]
        public void Collapse01()
        {
            Utils.CompareFile("scripts/formatting/collapse01.vine");
        }

        [TestMethod]
        public void Collapse02()
        {
            Utils.CompareFile("scripts/formatting/collapse02.vine");
        }
    }
}
