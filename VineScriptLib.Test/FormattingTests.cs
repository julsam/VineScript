using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VineScriptLib.Core;

namespace VineScriptLib.Test
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
    }
}
