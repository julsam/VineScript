using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace VineScript.Compiler
{
    internal class Escape
    {
        public static string EscapeWhiteSpace(string input)
        {
            StringBuilder escaped = new StringBuilder(input.Length);
            foreach (var c in input)
            {
                switch (c)
                {
                    //case '\'': escaped.Append(@"\'"); break;
                    //case '\"': escaped.Append(@"\"""); break;
                    //case '\\': escaped.Append(@"\\"); break;
                    case '\0': escaped.Append(@"\0"); break; // Null (0x00)
                    case '\a': escaped.Append(@"\a"); break; // Bell (0x07)
                    case '\b': escaped.Append(@"\b"); break; // Backspace (0x08)
                    case '\f': escaped.Append(@"\f"); break; // From Feed (0x0C)
                    case '\n': escaped.Append(@"\n"); break;
                    case '\r': escaped.Append(@"\r"); break;
                    case '\t': escaped.Append(@"\t"); break;
                    case '\v': escaped.Append(@"\v"); break; // Vertical Tab, used internally (0x0B)
                    default: escaped.Append(c); break;
                }
            }
            return escaped.ToString();
        }

        /// <summary>
        /// Unescape escaped characters like \{ or \/ in the text of a sequence.
        /// When the escaped char \ is alone (not followed by '{}\/%<>['), 
        /// it's not considered as an escape char but as normal text.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string UnescapeVineSequence(string input)
        {
            StringBuilder unescaped = new StringBuilder(input.Length);
            bool escape = false;
            bool in_display_output = false;

            for (int i = 0; i < input.Length; i++)
            {
                char current = input[i];

                if (current == '\u001E') {
                    // marks the start of the output of the display command
                    in_display_output = true;
                    continue;
                } else if (current == '\u001F') {
                    // marks the end of the output of the display command
                    in_display_output = false;
                    continue;
                }

                // doesn't escape anything if it's between start/end of display command
                // just add it as it is to the output
                if (in_display_output) {
                    unescaped.Append(current);
                    continue;
                }

                if (!escape && current == '\\' && i < input.Length - 1) {
                    // next char will be escaped
                    escape = true;
                    continue;
                }
                
                if (escape)
                {
                    string unescaped_current = "";
                    switch (current)
                    {
                        case '\\':
                        case '`':
                            unescaped_current = current.ToString();
                            break;
                        default:
                            unescaped_current = "\\" + current.ToString();
                            break;
                    }
                    unescaped.Append(unescaped_current);
                    escape = false;
                }
                else
                {
                    unescaped.Append(current);
                }
            }
            return unescaped.ToString();
        }
        
        /// <summary>
        /// Is the char at the given index escaped? Escape character is '\'
        /// </summary>
        /// <param name="input">String to check</param>
        /// <param name="index">Index of the character to check in str</param>
        /// <returns>true if the char is escaped, else false.</returns>
        public static bool IsCharAtEscaped(string input, int index)
        {
            bool escape = false;

            for (int i = 0; i <= index; i++)
            {
                char current = input[i];

                if (!escape && current == '\\' && i < input.Length - 1) {
                    // next char is marked to be escaped
                    escape = true;
                    continue;
                }
                
                if (escape)
                {
                    if (index == i) {
                        return true;
                    }
                    escape = false;
                }
            }
            return false;
        }

        /// <summary>
        /// Unescape escaped characters like \] or \| in the text of a link.
        /// When the escaped char \ is alone (not followed by "<]|-\\")
        /// it's not considered as an escape char but as normal text.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string UnescapeLinkContent(string input)
        {
            StringBuilder unescaped = new StringBuilder(input.Length);
            bool escape = false;

            for (int i = 0; i < input.Length; i++)
            {
                char current = input[i];

                if (!escape && current == '\\' && i < input.Length - 1) {
                    // next char will be escaped
                    escape = true;
                    continue;
                }
                
                if (escape)
                {
                    string unescaped_current = "";
                    switch (current)
                    {
                        case '<':
                        case ']':
                        case '|':
                        case '-':
                        case '\\':
                            unescaped_current = current.ToString();
                            break;
                        default:
                            unescaped_current = "\\" + current.ToString();
                            break;
                    }
                    unescaped.Append(unescaped_current);
                    escape = false;
                }
                else
                {
                    unescaped.Append(current);
                }
            }
            return unescaped.ToString();
        }
        
        public static string UnescapeStringLiteral(string input)
        {
            if (string.IsNullOrEmpty(input)) {
                return input;
            }
            var unescaped = Regex.Replace(input, @"\\.", m => {
                switch (m.Value)
                {
                    case @"\'": return "\'";
                    case @"\""": return "\"";
                    case @"\\": return "\\";
                    case @"\n": return "\n";
                    case @"\t": return "\t";
                    default:
                        throw new Exception("Unrecognized escape sequence '" + m.Value + "'");
                }
            });
            return unescaped;
        }
    }
}
