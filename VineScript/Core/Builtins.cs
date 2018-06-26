using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VineScript.Core
{
    public class Builtins
    {
        public static List<int> Range(int stop)
        {
            return Range(0, stop);
        }

        public static List<int> Range(int start, int stop)
        {
            var list = new List<int>();

            foreach (int i in YieldRange(start, stop)) {
                list.Add(i);
            }

            return list;
        }

        public static List<int> Range(int start, int stop, int step)
        {
            var list = new List<int>();

            foreach (int i in YieldRange(start, stop, step)) {
                list.Add(i);
            }

            return list;
        }

        public static IEnumerable<int> YieldRange(int end)
        {
            return YieldRange(0, end);
        }

        public static IEnumerable<int> YieldRange(int start, int stop)
        {
            if (start < stop) {
                return YieldRange(start, stop, 1);
            } else {
                return YieldRange(start, stop, -1);
            }
        }

        public static IEnumerable<int> YieldRange(int start, int stop, int step)
        {
            if (step == 0) {
                throw new ArgumentOutOfRangeException("step can't be equal to 0");
            }

            if (start < stop)
            {
                if (step < 0) {
                    throw new ArgumentOutOfRangeException(
                        "step can't be inferior to 0 when start < stop"
                    );
                }
                for (int i = start; i < stop; i += step) {
                    yield return i;
                }
            }
            else if (start > stop)
            {
                if (step > 0) {
                    throw new ArgumentOutOfRangeException(
                        "step can't be superior to 0 when start > stop"
                    );
                }
                for (int i = start; i > stop; i += step) {
                    yield return i;
                }
            }
        }

        public static string LowercaseFirst(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) {
                return string.Empty;
            }
            char[] a = s.ToCharArray();
            a[0] = char.ToLower(a[0]);
            return new string(a);
        }

        public static string UppercaseFirst(string s)
        {
            if (string.IsNullOrWhiteSpace(s)) {
                return string.Empty;
            }
            char[] a = s.ToCharArray();
            a[0] = char.ToUpper(a[0]);
            return new string(a);
        }

        public static char[] ToCharArray(params string[] strings)
        {
            List<char> chars = new List<char>();
            foreach (var word in strings) {
                chars.AddRange(word.ToCharArray());
            }
            return chars.ToArray();
        }
    }
}
