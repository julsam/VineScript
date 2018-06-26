using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VineScript.Binding;
using VineScript.Core;

namespace VineScript.Lib
{
    public class String
    {
        /// <summary>
        /// Concatenates the elements of an array, using the specified separator between
        ///  each element.
        /// </summary>
        [VineBinding]
        public static string Join(string separator, VineVar values)
        {
            return System.String.Join("|", values.AsArray);
        }
        
        /// <summary>
        /// Indicates whether the specified string is null, empty, or consists only
        /// of white-space characters.
        /// </summary>
        [VineBinding]
        public static bool IsEmpty(string str)
        {
            return System.String.IsNullOrWhiteSpace(str);
        }
        
        /// <summary>
        /// Returns a new string in which all occurrences of a specified string
        /// in the current instance are replaced with another specified string.
        /// </summary>
        [VineBinding]
        public static string Replace(string str, string oldValue, string newValue)
        {
            return str.Replace(oldValue, newValue);
        }
        
        /// <summary>
        /// Returns a new string in which a specified number of characters in the specified
        /// string beginning at a specified position have been deleted.
        /// </summary>
        [VineBinding]
        public static string Remove(string str, int startIndex, int length)
        {
            return str.Remove(startIndex, length);
        }
        
        /// <summary>
        /// Splits a string at each occurence of separator.
        /// </summary>
        [VineBinding]
        public static VineVar Split(string str, string separator, bool removeEmptyEntries=true)
        {
            var r = str.Split(new string[] { separator },
                removeEmptyEntries
                    ? StringSplitOptions.RemoveEmptyEntries
                    : StringSplitOptions.None
            );
            return r;
        }
        
        /// <summary>
        /// Returns a substring of the specified string, starting at position startIndex.
        /// </summary>
        [VineBinding]
        public static string SubString(string str, int startIndex)
        {
            return str.Substring(startIndex);
        }
        
        /// <summary>
        /// Returns a substring of the specified string, starting at position startIndex
        /// and of the specified length.
        /// </summary>
        [VineBinding]
        public static string SubString(string str, int startIndex, int length)
        {
            return str.Substring(startIndex, length);
        }

        [VineBinding]
        public static bool StartsWith(string str, string value)
        {
            return str.StartsWith(value);
        }

        [VineBinding]
        public static bool EndsWith(string str, string value)
        {
            return str.EndsWith(value);
        }

        /// <summary>
        /// Remove all leading and trailing occurences of a set of characters.
        /// </summary>
        [VineBinding]
        public static string Trim(string str, params string[] trimChars)
        {
            return str.Trim(Builtins.ToCharArray(trimChars));
        }

        /// <summary>
        /// Remove all leading occurences of a set of characters.
        /// </summary>
        [VineBinding]
        public static string TrimStart(string str, params string[] trimChars)
        {
            return str.TrimStart(Builtins.ToCharArray(trimChars));
        }

        /// <summary>
        /// Remove all trailing occurences of a set of characters.
        /// </summary>
        [VineBinding]
        public static string TrimEnd(string str, params string[] trimChars)
        {
            return str.TrimEnd(Builtins.ToCharArray(trimChars));
        }

        /// <summary>
        /// Remove whitespaces characters at the start and end of each lines.
        /// </summary>
        [VineBinding]
        public static string TrimWhiteSpace(string str)
        {
            StringBuilder wsRemoved = new StringBuilder();
            string[] lines = str.Replace("\r", "").Split(new char[] { '\n'});
            for (int i = 0; i < lines.Length; i++) {
                // removes starting & ending whitespaces
                wsRemoved.Append(lines[i].Trim());
                if (i < lines.Length - 1) {
                    // add new line if it's not the last one
                    wsRemoved.Append(Environment.NewLine);
                }
            }
            return wsRemoved.ToString();
        }
        
        /// <summary>
        /// Returns a copy of the string converted to lowercase.
        /// </summary>
        [VineBinding]
        public static string Lowercase(string str)
        {
            return str.ToLower();
        }
        
        /// <summary>
        /// Returns a copy of the string converted to uppercase.
        /// </summary>
        [VineBinding]
        public static string Uppercase(string str)
        {
            return str.ToUpper();
        }
        
        /// <summary>
        /// Returns a copy of the string with the first letter converted to lowercase.
        /// </summary>
        [VineBinding]
        public static string LowerFirst(string str)
        {
            return Builtins.LowercaseFirst(str);
        }
        
        /// <summary>
        /// Returns a copy of the string with the first letter converted to uppercase.
        /// </summary>
        [VineBinding]
        public static string UpperFirst(string str)
        {
            return Builtins.UppercaseFirst(str);
        }
    }
}
