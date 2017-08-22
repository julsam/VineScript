using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VineScript.Core
{
    internal class VineVarUtils
    {
        /// <summary>
        /// Check for null/Null value before doing an operation.
        /// If one of the operands is null/Null, it'll return false and 
        /// exception `e` should be thrown.
        /// If none are null/Null, the function will return true and
        /// the operation should be allowed.
        /// </summary>
        /// <param name="op">Operation type (as a string, used in the
        /// exception message)</param>
        /// <param name="a">Left hand operand</param>
        /// <param name="b">Right hand operand</param>
        /// <param name="e">Exception to eventually throw</param>
        /// <returns>`true` if none of the operands is null/Null. 
        /// `false` if at least one of them is null/Null; in that case,
        /// `e` will contain an exception that should be thrown</returns>
        public static bool CheckNullOp(string op, VineVar a, VineVar b,
            out Exception e)
        {
            e = null;

            if (    object.ReferenceEquals(a, null) || a.IsNull
                ||  object.ReferenceEquals(b, null) || b.IsNull) 
            {
                e = new VineArithmeticException(
                    op, 
                    a?.type.ToString() ?? "<null>", 
                    b?.type.ToString() ?? "<null>"
                );
                return false;
            } 

            return true;
        }
        /// <summary>
        /// Check for null/Null value before doing an operation.
        /// If the operand is null/Null, it'll return false and 
        /// exception `e` should be thrown.
        /// If not, the function will return true and the operation
        /// should be allowed.
        /// </summary>
        /// <param name="op">Operation type (as a string, used in the
        /// exception message)</param>
        /// <param name="a">Unary operand</param>
        /// <param name="e">Exception to eventually throw</param>
        /// <returns>`true` if the operand is not null/Null. 
        /// `false` if the operand is null/Null; in that case, `e` will 
        /// contain an exception that should be thrown</returns>
        public static bool CheckNullOp(string op, VineVar a, out Exception e)
        {
            e = null;

            if ((object.ReferenceEquals(a, null) || a.IsNull)) 
            {
                // if both are null, throw an exception
                e = new VineArithmeticException(
                    op, a?.type.ToString() ?? "<null>"
                );
                return false;
            } 

            return true;
        }

        public static List<VineVar> ConvertList<T>(List<T> list)
        {
            var converted = new List<VineVar>(list.Count);
            foreach (var el in list) {
                converted.Add(new VineVar(el));
            }
            return converted;
        }

        public static string Stringify(List<VineVar> list)
        {
            StringBuilder str = new StringBuilder();
            str.Append("[");
            for (int i = 0; i < list.Count; i++) {
                if (list[i].IsString) {
                    str.AppendFormat("\"{0}\"", list[i].ToString());
                } else {
                    str.Append(list[i].ToString());
                }
                if (i < list.Count - 1) {
                    str.Append(", ");
                }
            }
            str.Append("]");
            return str.ToString();
        }

        public static string Stringify(Dictionary<string, VineVar> dict)
        {
            StringBuilder str = new StringBuilder();
            str.Append("{");
            for (int i = 0; i < dict.Count; i++) {
                var el = dict.ElementAt(i);
                str.AppendFormat("\"{0}\": ", el.Key);
                if (el.Value.IsString) {
                    str.AppendFormat("\"{0}\"", el.Value.ToString());
                } else {
                    str.Append(el.Value.ToString());
                }
                if (i < dict.Count - 1) {
                    str.Append(", ");
                }
            }
            str.Append("}");
            return str.ToString();
        }
    }
}
