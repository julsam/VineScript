using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VineScript.Binding;
using VineScript.Core;

namespace VineScript.Lib
{
    public class Rand
    {
        internal static readonly System.Random rand = new System.Random();
        
        /// <summary>
        /// Picks and return one of the values randomly.
        /// </summary>
        [VineBinding]
        public static VineVar Either(params VineVar[] values)
        {
            if (values == null || values.Length == 0) {
                throw new VineFunctionCallException(
                    "Values needs to be initialized and non-empty."
                );
            }
            return values[rand.Next(values.Length)];
        }

        /// <summary>
        /// Returns a nonnegative random integer that is less than the specified maximum.
        /// </summary>
        /// <param name="max">Exclusive upper bound.</param>
        /// <returns>An integer greater than or equal to zero, and less than maxValue.</returns>
        [VineBinding]
        public static int Random(int maxValue=int.MaxValue)
        {
            return rand.Next(maxValue);
        }

        /// <summary>
        /// Returns a random integer that is within a specified range.
        /// </summary>
        /// <param name="min">Inclusive lower bound.</param>
        /// <param name="max">Exclusive upper bound.</param>
        /// <returns>An integer greater than or equal to minValue and less than maxValue.</returns>
        [VineBinding]
        public static int Random(int minValue, int maxValue)
        {
            return rand.Next(minValue, maxValue);
        }

        /// <summary>
        /// Returns a random floating-point number between 0.0 and 1.0.
        /// </summary>
        /// <returns>A double-precision floating point number greater than
        /// or equal to 0.0, and less than 1.0.</returns>
        [VineBinding]
        public static double Randomf()
        {
            return rand.NextDouble();
        }
    }
}
