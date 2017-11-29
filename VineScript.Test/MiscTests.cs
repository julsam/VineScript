using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VineScript;
using VineScript.Core;
using VineScript.Compiler;

namespace VineScript.Test
{
    [TestClass]
    public class MiscTests
    {
        [TestMethod]
        public void IllegalCharacter01()
        {
            // the first character (sometimes displayed as a whitespace)
            // is '\u000B' (vertical tabulation) and is not allowed in text
            string input = "{{ 1 }}";
            try {
                Loader loader = new Loader();
                loader.LoadCode(input, "test");
                Assert.Fail();
            } catch (VineParseException e) {
                if (e.errorReport.ErrorMessage != VineParser.errReservedChar000B) {
                    Assert.Fail();
                }
            } catch (Exception) {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void IllegalCharacter02()
        {
            // the character between 'foo' and 'bar'(sometimes displayed as a whitespace)
            // is '\u000B' (vertical tabulation) and is not in a string literal
            string input = "{{ \"foobar\" }}";
            try {
                Loader loader = new Loader();
                loader.LoadCode(input, "test");
                Assert.Fail();
            } catch (VineParseException e) {
                if (e.errorReport.ErrorMessage != VineParser.errReservedChar000B) {
                    Assert.Fail();
                }
            } catch (Exception) {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void IllegalCharacter03()
        {
            // the character between 'foo' and 'bar'(sometimes displayed as a whitespace)
            // is '\u000B' (vertical tabulation) and is not in a string literal
            string input = "[[ foobar ]]";
            try {
                Loader loader = new Loader();
                loader.LoadCode(input, "test");
                Assert.Fail();
            } catch (VineParseException e) {
                if (e.errorReport.ErrorMessage != VineParser.errReservedChar000B) {
                    Assert.Fail();
                }
            } catch (Exception) {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void IllegalCharacter04()
        {
            // the character \u001E (record separator): ''
            string input = "Record separator: '' (invisible in most editors)";
            try {
                Loader loader = new Loader();
                loader.LoadCode(input, "test");
                Assert.Fail();
            } catch (VineParseException e) {
                if (e.errorReport.ErrorMessage != VineParser.errReservedChar001E) {
                    Assert.Fail();
                }
            } catch (Exception) {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void IllegalCharacter05()
        {
            // the character \u001E (record separator): ''
            string input = "Record separator: {{ \"''\" }} (invisible in most editors)";
            try {
                Loader loader = new Loader();
                loader.LoadCode(input, "test");
                Assert.Fail();
            } catch (VineParseException e) {
                if (e.errorReport.ErrorMessage != VineParser.errReservedChar001E) {
                    Assert.Fail();
                }
            } catch (Exception) {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void IllegalCharacter06()
        {
            // the character \u001E (record separator): ''
            string input = "Record separator: [[ foo''bar ]] (invisible in most editors)";
            try {
                Loader loader = new Loader();
                loader.LoadCode(input, "test");
                Assert.Fail();
            } catch (VineParseException e) {
                if (e.errorReport.ErrorMessage != VineParser.errReservedChar001E) {
                    Assert.Fail();
                }
            } catch (Exception) {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void IllegalCharacter07()
        {
            // the character \u001F (unit separator): ''
            string input = "Unit separator: '' (invisible in most editors)";
            try {
                Loader loader = new Loader();
                loader.LoadCode(input, "test");
                Assert.Fail();
            } catch (VineParseException e) {
                if (e.errorReport.ErrorMessage != VineParser.errReservedChar001F) {
                    Assert.Fail();
                }
            } catch (Exception) {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void IllegalCharacter08()
        {
            // the character \u001F (unit separator): ''
            string input = "Unit separator: {{ \"''\" }} (invisible in most editors)";
            try {
                Loader loader = new Loader();
                loader.LoadCode(input, "test");
                Assert.Fail();
            } catch (VineParseException e) {
                if (e.errorReport.ErrorMessage != VineParser.errReservedChar001F) {
                    Assert.Fail();
                }
            } catch (Exception) {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void IllegalCharacter09()
        {
            // the character \u001F (unit separator): ''
            string input = "Unit separator: [[ foo''bar ]] (invisible in most editors)";
            try {
                Loader loader = new Loader();
                loader.LoadCode(input, "test");
                Assert.Fail();
            } catch (VineParseException e) {
                if (e.errorReport.ErrorMessage != VineParser.errReservedChar001F) {
                    Assert.Fail();
                }
            } catch (Exception) {
                Assert.Fail();
            }
        }

        [TestMethod]
        public void MissingSpaceSeparator01()
        {
            // 4 Errors here:
            string input = "<< if!false and 1<2and 0==0or!true >>FOO<<end>>";

            //  1. space after 'if'
            string errMsg1 = VineParser.errMissingSpaceAfter + "'if'";
            //  2. space between '2' and 'and'
            string errMsg2 = VineParser.errMissingSpaceBefore + "'and'";
            //  3. space between '0' and 'or'
            string errMsg3 = VineParser.errMissingSpaceBefore + "'or'";
            //  4. space after 'or'
            string errMsg4 = VineParser.errMissingSpaceAfter + "'or'";
            
            try {
                Loader loader = new Loader();
                loader.LoadCode(input, "test");
                Assert.Fail();
            } catch (VineParseException e) {
                if (e.errorReports[0].ErrorMessage != errMsg1) {
                    Assert.Fail();
                }
                if (e.errorReports[1].ErrorMessage != errMsg2) {
                    Assert.Fail();
                }
                if (e.errorReports[2].ErrorMessage != errMsg3) {
                    Assert.Fail();
                }
                if (e.errorReports[3].ErrorMessage != errMsg4) {
                    Assert.Fail();
                }
            } catch (Exception) {
                Assert.Fail();
            }
        }
    }
}
