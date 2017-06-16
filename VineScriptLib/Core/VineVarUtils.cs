using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VineScriptLib.Core
{
    internal class VineVarUtils
    {
        /// <summary>
        /// Check for null/Null value before doing an operation.
        /// If both operands are null/Null, it'll return false and 
        /// exception `e` should be thrown.
        /// If one of the operands is null/Null, it'll return false
        /// and depending on VineValue.strictMode, exception `e` 
        /// should be thrown or VineValue `@return` should be returned.
        /// If none are null/Null, the function will return true and
        /// the operation should be allowed.
        /// </summary>
        /// <param name="op">Operation type (as a string, used in the
        /// exception message)</param>
        /// <param name="a">Left hand operand</param>
        /// <param name="b">Right hand operand</param>
        /// <param name="e">Exception to eventually throw</param>
        /// <param name="return">VineValue to eventually return</param>
        /// <returns>`true` if none of the operands is null/Null. 
        /// `false` if at least one of them is null/Null; in that case, 
        /// if `e` contains an exception it should be thrown, else
        /// `@return` should be returned</returns>
        public static bool CheckNullOp(string op, VineValue a, VineValue b,
            out Exception e, out VineValue @return)
        {
            e = null;
            @return = null;

            if (    (object.ReferenceEquals(a, null) || a.IsNull())
                &&  (object.ReferenceEquals(b, null) || b.IsNull())) 
            {
                // if both are null, throw an exception
                e = new VineArithmeticException(
                    op, 
                    a?.type.ToString() ?? "<null>", 
                    b?.type.ToString() ?? "<null>"
                );
                return false;
            } 
            else if (object.ReferenceEquals(a, null) || a.IsNull()
                ||   object.ReferenceEquals(b, null) || b.IsNull()) 
            {
                // if at least one is null and depending on strictMode:
                // return Null or throw an exception
                if (VineValue.strictMode) {
                    e = new VineArithmeticException(
                        op, 
                        a?.type.ToString() ?? "<null>", 
                        b?.type.ToString() ?? "<null>"
                    );
                } else {
                    @return = VineValue.NULL;
                }
                return false;
            }

            return true;
        }
        /// <summary>
        /// Check for null/Null value before doing an operation.
        /// If the operand is null/Null, it'll return false and 
        /// exception `e` should be thrown.
        /// If noy, the function will return true and the operation
        /// should be allowed.
        /// </summary>
        /// <param name="op">Operation type (as a string, used in the
        /// exception message)</param>
        /// <param name="a">Unary operand</param>
        /// <param name="e">Exception to eventually throw</param>
        /// <param name="return">VineValue to eventually return</param>
        /// <returns>`true` if the operand is not null/Null. 
        /// `false` if the operand is null/Null; in that case, `e` will 
        /// contain an exception that should be thrown</returns>
        public static bool CheckNullOp(string op, VineValue a, out Exception e)
        {
            e = null;

            if ((object.ReferenceEquals(a, null) || a.IsNull())) 
            {
                // if both are null, throw an exception
                e = new VineArithmeticException(
                    op, a?.type.ToString() ?? "<null>"
                );
                return false;
            } 

            return true;
        }
    }
}
