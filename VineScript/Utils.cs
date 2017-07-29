using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VineScript.Core;

namespace VineScript
{
    class Utils
    {
        public static List<int> Range(int end)
        {
            return Range(0, end);
        }

        public static List<int> Range(int start, int end, int steps=1)
        {
            var range = new List<int>();
            var step = 1;
            
            if (start > end)
            {
                var i = start;
                while (i > end) {
                    range.Add(i);
                    i -= step;
                }
            }
            else
            {
                var i = start;
                while (i < end) {
                    range.Add(i);
                    i += step;
                }
            }
            return range;
        }
    }
}
