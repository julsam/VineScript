using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VineScript.Compiler
{
    internal class WhiteSpace
    {
        // See VineLexer.g4 for more infos about the selected white spaces
        internal static readonly char[] WhiteSpaceList = new char[] {
            ' ',
            '\f',
            '\t',
            '\v',
            '\u00A0',
            '\u2000',
            '\u2001',
            '\u2002',
            '\u2003',
            '\u2004',
            '\u2005',
            '\u2006',
            '\u2007',
            '\u2008',
            '\u2009',
            '\u200A',
            '\u2028',
            '\u2029',
            '\u202F',
            '\u205F',
            '\u3000'
        };
        
        public static string Trim(string text)
        {
#if GRAMMAR_VERBOSE
            Console.WriteLine("--- WS removed: ---");
#endif
            StringBuilder wsRemoved = new StringBuilder();
            string[] lines = text.Replace("\r", "").Split(new char[] { '\n'});
            for (int i = 0; i < lines.Length; i++) {
                // removes starting & ending whitespaces
                // doesn't use the default empty Trim() because Vine is using
                // some unusual whitespace characters as markups for formatting
                wsRemoved.Append(lines[i].Trim(new char[] { '\t', ' ' }));
                if (i < lines.Length - 1) {
                    // add new line if it's not the last one
                    wsRemoved.Append(Environment.NewLine);
                }
            }
#if GRAMMAR_VERBOSE
            Console.WriteLine(wsRemoved);
            Console.WriteLine("--- ---");
#endif
            return wsRemoved.ToString();
        }

        public static string CollapseWordsSpacing(string input)
        {
            var trim = input.Split(
                WhiteSpaceList, StringSplitOptions.RemoveEmptyEntries
            );
            return string.Join(" ", trim);
        }

        public static string CollapseWordsSpacingLn(string input)
        {
            List<char> ws = new List<char>(WhiteSpaceList);
            ws.Add('\r');
            ws.Add('\n');
            var trim = input.Split(
                ws.ToArray(), StringSplitOptions.RemoveEmptyEntries
            );
            return string.Join(" ", trim);
        }

        internal static bool IsOnlyWhiteSpace(string str)
        {
            foreach (var c in str) {
                if (!char.IsWhiteSpace(c)) {
                    return false;
                }
            }
            return true;
        }
    }
}
