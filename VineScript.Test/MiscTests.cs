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
            VineStory story = new VineStory();
            try {
                story.RunPassage(input);
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
            VineStory story = new VineStory();
            try {
                story.RunPassage(input);
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
            VineStory story = new VineStory();
            try {
                story.RunPassage(input);
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
            VineStory story = new VineStory();
            try {
                story.RunPassage(input);
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
            VineStory story = new VineStory();
            try {
                story.RunPassage(input);
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
            VineStory story = new VineStory();
            try {
                story.RunPassage(input);
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
            VineStory story = new VineStory();
            try {
                story.RunPassage(input);
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
            VineStory story = new VineStory();
            try {
                story.RunPassage(input);
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
            VineStory story = new VineStory();
            try {
                story.RunPassage(input);
                Assert.Fail();
            } catch (VineParseException e) {
                if (e.errorReport.ErrorMessage != VineParser.errReservedChar001F) {
                    Assert.Fail();
                }
            } catch (Exception) {
                Assert.Fail();
            }
        }
    }
}
