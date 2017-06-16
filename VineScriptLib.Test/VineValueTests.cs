using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VineScriptLib;
using VineScriptLib.Core;

namespace VineScriptLib.Test
{
    /// <summary>
    /// Summary description for VineValueTests
    /// </summary>
    [TestClass]
    public class VineValueTests
    {
        public VineValueTests()
        {
        }

        [TestMethod]
        [ExpectedException(typeof(VineArithmeticException))]
        public void BoolAdditionFails0()
        {
            VineValue a = new VineValue(true);
            VineValue b = new VineValue(false);
            var r = a + b;
        }

        [TestMethod]
        [ExpectedException(typeof(VineArithmeticException))]
        public void BoolAdditionFails1()
        {
            VineValue a = new VineValue(true);
            VineValue b = new VineValue(1);
            var r = a + b;
        }

        [TestMethod]
        [ExpectedException(typeof(VineArithmeticException))]
        public void BoolSubFails0()
        {
            VineValue a = new VineValue(true);
            VineValue b = new VineValue(false);
            var r = a - b;
        }

        [TestMethod]
        [ExpectedException(typeof(VineArithmeticException))]
        public void BoolSubFails1()
        {
            VineValue a = new VineValue(true);
            VineValue b = new VineValue(1);
            var r = a - b;
        }

        [TestMethod]
        public void NullArithmetic()
        {
            VineValue @null = null;
            VineValue NULL = VineValue.NULL;
            VineValue str = "foo";
            VineValue @int = 42;
            VineValue number = 16.66;
            VineValue @bool = true;
            VineValue array = VineValue.newArray;
            VineValue dict = VineValue.newDict;
            
            /**
             * Fails:
             **/

            // +
            try {
                var r = @null + @null;
                Assert.Fail();
            } catch (VineArithmeticException) { /* Valid */ }

            try {
                var r = NULL + NULL;
                Assert.Fail();
            } catch (VineArithmeticException) { /* Valid */ }

            try {
                var r = @null + NULL;
                Assert.Fail();
            } catch (VineArithmeticException) { /* Valid */ }

            try {
                var r = @null + @int;
                Assert.Fail();
            } catch (VineArithmeticException) { /* Valid */ }

            try {
                var r = @null + number;
                Assert.Fail();
            } catch (VineArithmeticException) { /* Valid */ }

            try {
                var r = @null + array;
                Assert.Fail();
            } catch (VineArithmeticException) { /* Valid */ }

            try {
                var r = @null + dict;
                Assert.Fail();
            } catch (VineArithmeticException) { /* Valid */ }

            // -
            try {
                var r = str - @null;
                Assert.Fail();
            } catch (VineArithmeticException) { /* Valid */ }

            try {
                var r = @int - @null;
                Assert.Fail();
            } catch (VineArithmeticException) { /* Valid */ }

            // *
            try {
                var r = @null * @null;
                Assert.Fail();
            } catch (VineArithmeticException) { /* Valid */ }

            try {
                var r = @int * @null;
                Assert.Fail();
            } catch (VineArithmeticException) { /* Valid */ }

            // /
            try {
                var r = @int / @null;
                Assert.Fail();
            } catch (VineArithmeticException) { /* Valid */ }

            // unary
            try {
                var r = -@null;
                Assert.Fail();
            } catch (VineArithmeticException) { /* Valid */ }
            try {
                var r = !@null;
                Assert.Fail();
            } catch (VineArithmeticException) { /* Valid */ }
            
            /**
             * Succeeds:
             **/
            Assert.AreEqual((VineValue)"foo", str + @null);
            Assert.AreEqual((VineValue)"foo", str + NULL);
        }

        [TestMethod]
        public void NullGtEqLt()
        {
            VineValue @null = null;
            VineValue NULL = VineValue.NULL;
            VineValue str = "foo";
            VineValue @int = 42;
            VineValue number = 16.66;
            VineValue @bool = true;
            VineValue array = VineValue.newArray;
            VineValue dict = VineValue.newDict;

            if (VineValue.strictMode) {
                try {
                    var r = @int > @null;
                    Assert.Fail();
                } catch (VineComparisonException) { /* Valid */ }

                try {
                    var r = NULL < @null;
                    Assert.Fail();
                } catch (VineComparisonException) { /* Valid */ }

                try {
                    var r = NULL >= @null;
                    Assert.Fail();
                } catch (VineComparisonException) { /* Valid */ }

                try {
                    var r = NULL <= @null;
                    Assert.Fail();
                } catch (VineComparisonException) { /* Valid */ }
            } else {
                Assert.AreEqual(false, @null > NULL);
                Assert.AreEqual(false, @null < @int);
                Assert.AreEqual(false, @null >= number);
                Assert.AreEqual(false, @null <= array);
            }
        }

        [TestMethod]
        public void CreateValueInValue()
        {
            VineValue val = new VineValue(new VineValue());
        }

        [TestMethod]
        public void CreateBools()
        {
            VineValue boolean_true = new VineValue(true);
            VineValue boolean_false = new VineValue(false);
            Assert.AreEqual(VineValue.Type.Bool, boolean_true.type);
            Assert.AreEqual(VineValue.Type.Bool, boolean_false.type);
        }

        [TestMethod]
        public void CreateInts()
        {
            VineValue integer_42 = new VineValue(42);
            VineValue integer_minus77 = new VineValue(-77);
            VineValue integer_0 = new VineValue(0);
            VineValue integer_minus_0 = new VineValue(-0);
            
            VineValue integer_42_copy = integer_42;
            VineValue integer_42_copy_asnum = integer_42.AsNumber + 0.1;
            Assert.AreEqual(integer_42, new VineValue(42));
            Assert.AreEqual(integer_42, 42);
            Assert.AreNotEqual((int)42, (double)42.1);
            Assert.AreNotEqual(integer_42, new VineValue(42.1));

            Assert.AreSame(integer_42, integer_42_copy);
            Assert.AreNotSame(integer_42, new VineValue(42));
            Assert.AreEqual(integer_0, integer_minus_0);
        }

        [TestMethod]
        public void CreateNumbers()
        {
            VineValue number_42 = new VineValue(42.0);
            VineValue number_minus77 = new VineValue(-77.0);
            VineValue number_42_01234 = new VineValue(42.01234);
            VineValue number_minus77_9876 = new VineValue(-77.9876);

            // check type
            Assert.AreEqual(VineValue.Type.Number, number_42.type);
            Assert.AreEqual(VineValue.Type.Number, number_minus77.type);
            Assert.AreEqual(VineValue.Type.Number, number_42_01234.type);
            Assert.AreEqual(VineValue.Type.Number, number_minus77_9876.type);
            
            // check value
            Assert.AreEqual(42.0, number_42);
            Assert.AreEqual(-77.0, number_minus77);
            Assert.AreEqual(42.01234, number_42_01234);
            Assert.AreEqual(-77.9876, number_minus77_9876);

            
            var nan = new VineValue(double.NaN);
            Assert.AreEqual(VineValue.Type.Number, nan.type);
            Assert.AreEqual(double.NaN, nan.AsNumber);


            // Copying
            //VineValue number_positive_copy = number_positive;
            ////var a = number_positive_copy - 2 + 2;
            //Assert.AreSame(number_positive, new VineValue(42.0));
            //Assert.AreNotSame(number_positive, 42.0);
            //Assert.AreSame(number_positive, number_positive_copy);
            //Assert.AreNotSame(number_positive, number_positive_float);
            //Assert.AreSame(number_positive, number_positive_copy - 2 + 2);
        }

        [TestMethod]
        public void CreateStrings()
        {
            VineValue str = new VineValue("foo bar");
        }

        [TestMethod]
        public void CreateNulls()
        {
            VineValue val = new VineValue();
            VineValue val2 = VineValue.NULL;

            // Equality
            Assert.AreEqual(0, val.AsInt);
            Assert.AreEqual(0.0, val.AsNumber);
            Assert.AreEqual(false, val.AsBool);
            Assert.AreEqual(null, val.AsString);
            Assert.AreEqual(val, val2);

            Assert.AreEqual(new VineValue(0), val.AsInt);
        }

        [TestMethod]
        public void UnaryNot()
        {
            // Int
            try {
                VineValue val = 6;
                val = !val;
                Assert.Fail();
            } catch (VineArithmeticException) { /* Valid */ } 

            // Number
            try {
                VineValue val = 6.1;
                val = !val;
                Assert.Fail();
            } catch (VineArithmeticException) { /* Valid */ } 

            // String
            try {
                VineValue val = "foo";
                val = !val;
                Assert.Fail();
            } catch (VineArithmeticException) { /* Valid */ } 
            
            VineValue bool_true = true;
            VineValue bool_false = false;
            Assert.AreEqual(false, !bool_true);
            Assert.AreNotEqual(true, !bool_true);
            Assert.AreEqual(true, !bool_false);
            Assert.AreNotEqual(false, !bool_false);
        }

        [TestMethod]
        public void CompareEqual()
        {
            // ==
            Assert.IsTrue(new VineValue(11) == 11);
            Assert.IsTrue(new VineValue(-200) == -200);
            Assert.IsTrue(new VineValue(42.0) == 42);
            Assert.IsTrue(new VineValue(42) == 42.0);
            Assert.IsTrue(new VineValue(42.0) == 42.0);
            Assert.IsTrue(new VineValue(42.1234) == 42.1234);
            Assert.IsTrue(new VineValue(true) == true);
            Assert.IsTrue(new VineValue(false) == false);
            Assert.IsTrue(new VineValue("foo") == "foo");
            Assert.IsFalse(new VineValue("foo") == 1); // Will fail in strict mode
            Assert.IsFalse(new VineValue("foo") == 1.1); // Will fail in strict mode
            Assert.IsFalse(new VineValue("foo") == VineValue.NULL);

            // == 
            // NULL == NULL
            Assert.IsTrue(new VineValue() == VineValue.NULL);
            Assert.IsTrue(new VineValue(11) == new VineValue(11));
            Assert.IsTrue(new VineValue(-200) == -(new VineValue(200)));
            Assert.IsTrue(new VineValue(-200) == new VineValue(-200));
            Assert.IsTrue(new VineValue(42.0) == new VineValue(42.0));
            Assert.IsTrue(new VineValue(42.1234) == new VineValue(42.1234));
            Assert.IsTrue(new VineValue(42.1234) == new VineValue(42.123400000));
            Assert.IsTrue(new VineValue(true) == new VineValue(true));
            Assert.IsTrue(new VineValue(false) == new VineValue(false));
        }

        [TestMethod]
        public void CompareNotEqual()
        {
            // !=
            Assert.IsTrue(new VineValue(11) != 22);
            Assert.IsTrue(new VineValue(42.0) != 0.0);
            Assert.IsTrue(new VineValue(42.12340) != 42.12341);
            Assert.IsTrue(new VineValue(true) != false);
            Assert.IsTrue(new VineValue(false) != true);
            Assert.IsTrue(new VineValue("foo") != "bar");
            Assert.IsFalse(new VineValue("foo") != "foo");
            Assert.IsTrue(new VineValue("foo") != 1); // Will fail in strict mode
            Assert.IsTrue(new VineValue("foo") != 1.1); // Will fail in strict mode
            Assert.IsTrue(new VineValue("foo") != VineValue.NULL);
            
            // != NULL
            Assert.IsTrue(new VineValue(11) != VineValue.NULL);
            Assert.IsTrue(new VineValue(11.0) != VineValue.NULL);
            Assert.IsTrue(new VineValue(true) != VineValue.NULL);
            Assert.IsTrue(new VineValue(false) != VineValue.NULL);
            Assert.IsTrue(new VineValue("") != VineValue.NULL);
            Assert.IsTrue(new VineValue("foo") != VineValue.NULL);

            //
            Assert.IsTrue(new VineValue(10) != new VineValue(11));
            Assert.IsTrue(new VineValue(200) != -(new VineValue(200)));
            Assert.IsTrue(new VineValue(200) != new VineValue(-200));
            Assert.IsTrue(new VineValue(42.0) != new VineValue(41));
            Assert.IsTrue(new VineValue(42.1234) != new VineValue(42.12345));
            Assert.IsTrue(new VineValue(true) != new VineValue(false));
        }

        [TestMethod]
        public void CompareGreaterThan()
        {
            // >
            Assert.IsTrue(new VineValue(22) > 11);
            Assert.IsTrue(new VineValue(-2) > -3);
            Assert.IsTrue(new VineValue(42.0) > -42.0);
            Assert.IsTrue(new VineValue(42.13) > 42.12);

            if (VineValue.strictMode) 
            {
                try {
                    Assert.IsTrue(new VineValue("") > 11);
                    Assert.Fail();
                } catch (VineComparisonException) { /* Valid */ }

                try {
                    Assert.IsTrue(new VineValue("abc") > "a");
                    Assert.Fail();
                } catch (VineComparisonException) { /* Valid */ }

                try {
                    Assert.IsTrue(new VineValue(true) > false);
                    Assert.Fail();
                } catch (VineComparisonException) { /* Valid */ }
            }
            else
            {
                Assert.IsFalse(new VineValue("") > 11);
                Assert.IsFalse(new VineValue("abc") > "a");
                Assert.IsFalse(new VineValue(true) > false);
            }
        }

        [TestMethod]
        public void CompareGreaterThanOrEqual()
        {
            // >=
            Assert.IsTrue(new VineValue(22) >= 11);
            Assert.IsTrue(new VineValue(22) >= 22);
            Assert.IsTrue(new VineValue(-2) >= -3);
            Assert.IsTrue(new VineValue(-2) >= -2);
            Assert.IsTrue(new VineValue(42.0) >= -42.0);
            Assert.IsTrue(new VineValue(42.0) >= 42.0);
            Assert.IsTrue(new VineValue(42.13) >= 42.12);
            Assert.IsTrue(new VineValue(42.13) >= 42.13);

            if (VineValue.strictMode) 
            {
                try {
                    Assert.IsTrue(new VineValue("") >= 11);
                    Assert.Fail();
                } catch (VineComparisonException) { /* Valid */ }

                try {
                    Assert.IsTrue(new VineValue("abc") >= "a");
                    Assert.Fail();
                } catch (VineComparisonException) { /* Valid */ }

                try {
                    Assert.IsTrue(new VineValue(true) >= false);
                    Assert.Fail();
                } catch (VineComparisonException) { /* Valid */ }
            }
            else
            {
                Assert.IsFalse(new VineValue("") >= 11);
                Assert.IsFalse(new VineValue("abc") >= "a");
                Assert.IsFalse(new VineValue(true) >= false);
            }
        }

        [TestMethod]
        public void CompareLowerThan()
        {
            // >
            Assert.IsTrue(new VineValue(11) < 22);
            Assert.IsTrue(new VineValue(-3) < -2);
            Assert.IsTrue(new VineValue(-42.0) < 42.0);
            Assert.IsTrue(new VineValue(42.12) < 42.13);

            if (VineValue.strictMode) 
            {
                try {
                    Assert.IsTrue(new VineValue("") < 11);
                    Assert.Fail();
                } catch (VineComparisonException) { /* Valid */ }

                try {
                    Assert.IsTrue(new VineValue("abc") < "a");
                    Assert.Fail();
                } catch (VineComparisonException) { /* Valid */ }

                try {
                    Assert.IsTrue(new VineValue(true) < false);
                    Assert.Fail();
                } catch (VineComparisonException) { /* Valid */ }
            }
            else
            {
                Assert.IsFalse(new VineValue("") < 11);
                Assert.IsFalse(new VineValue("abc") < "a");
                Assert.IsFalse(new VineValue(true) < false);
            }
        }

        [TestMethod]
        public void CompareLowerThanOrEqual()
        {
            // >=
            Assert.IsTrue(new VineValue(11) <= 22);
            Assert.IsTrue(new VineValue(11) <= 11);
            Assert.IsTrue(new VineValue(-3) <= -2);
            Assert.IsTrue(new VineValue(-3) <= -3);
            Assert.IsTrue(new VineValue(-42.0) <= 42.0);
            Assert.IsTrue(new VineValue(-42.0) <= -42.0);
            Assert.IsTrue(new VineValue(42.12) <= 42.13);
            Assert.IsTrue(new VineValue(42.12) <= 42.12);
            
            if (VineValue.strictMode) 
            {
                try {
                    Assert.IsTrue(new VineValue("") <= 11);
                    Assert.Fail();
                } catch (VineComparisonException) { /* Valid */ }

                try {
                    Assert.IsTrue(new VineValue("abc") <= "a");
                    Assert.Fail();
                } catch (VineComparisonException) { /* Valid */ }

                try {
                    Assert.IsTrue(new VineValue(true) <= false);
                    Assert.Fail();
                } catch (VineComparisonException) { /* Valid */ }
            }
            else
            {
                Assert.IsFalse(new VineValue("") <= 11);
                Assert.IsFalse(new VineValue("abc") <= "a");
                Assert.IsFalse(new VineValue(true) <= false);
            }
        }

        [TestMethod]
        public void BoolAs()
        {
            VineValue bool_true = true;
            VineValue bool_false = false;
            
            // AsBool
            Assert.AreEqual(true, bool_true.AsBool);
            Assert.AreEqual(false, bool_false.AsBool);
            
            // AsInt
            try {
                // Shouldn't work
                var asInt = bool_true.AsInt;
                if (VineValue.strictMode) {
                    Assert.Fail();
                }
            } catch (VineConversionException) {
                // It's ok
            } 
            
            // AsNumber
            try {
                // Shouldn't work
                var asNumber = bool_true.AsNumber;
                if (VineValue.strictMode) {
                    Assert.Fail();
                }
            } catch (VineConversionException) {
                // It's ok
            } 

            // AsString
            Assert.AreEqual("True", bool_true.AsString);
            Assert.AreEqual("False", bool_false.AsString);
        }

        [TestMethod]
        public void IntAs()
        {
            VineValue int_val = 13;
            Assert.AreEqual(VineValue.Type.Int, int_val.type);

            // AsInt
            Assert.AreEqual(13, int_val.AsInt);
            // AsNumber
            Assert.AreEqual(13.0, int_val.AsNumber);
            // AsString
            Assert.AreEqual("13", int_val.AsString);

            // AsBool
            try {
                // Shouldn't work
                var asBool = int_val.AsBool;
                if (VineValue.strictMode) {
                    Assert.Fail();
                }
            } catch (VineConversionException) {
                // It's ok
            } 
        }

        [TestMethod]
        public void NumberAs()
        {
            VineValue num_val = 17.0;
            Assert.AreEqual(VineValue.Type.Number, num_val.type);
            Assert.AreNotEqual(VineValue.Type.Int, num_val.type);

            // AsInt
            Assert.AreEqual(17, num_val.AsInt);
            // AsNumber
            Assert.AreEqual(17.0, num_val.AsNumber);
            // AsString
            Assert.AreEqual("17", num_val.AsString);

            // AsBool
            try {
                // Shouldn't work
                var asBool = num_val.AsBool;
                if (VineValue.strictMode) {
                    Assert.Fail();
                }
            } catch (VineConversionException) {
                // It's ok
            } 
        }

        [TestMethod]
        public void StringAs()
        {
            // AsString
            Assert.AreEqual("foobar", (new VineValue("foobar")).AsString);
            Assert.AreEqual("ABCDEF abcdef 0123456789 !@#$%^&*()_+{}[]'", 
                (new VineValue("ABCDEF abcdef 0123456789 !@#$%^&*()_+{}[]'")).AsString);

            var empty = new VineValue("");
            Assert.AreEqual("", empty.AsString);

            // AsInt
            try {
                // "foobar" AsInt shouldn't work
                VineValue str_val = "foobar";
                var asInt = str_val.AsInt;
                if (VineValue.strictMode) {
                    Assert.Fail();
                }
            } catch (VineConversionException) {
                // It's ok
            } 
            
            try {
                // "23" AsInt shouldn't work
                VineValue str_val = "23";
                var asInt = str_val.AsInt;
                if (VineValue.strictMode) {
                    Assert.Fail();
                }
            } catch (VineConversionException) {
                // It's ok
            }
            
            // AsNumber
            try {
                // "foobar" AsNumber shouldn't work
                VineValue str_val = "foobar";
                var asInt = str_val.AsNumber;
                if (VineValue.strictMode) {
                    Assert.Fail();
                }
            } catch (VineConversionException) {
                // It's ok
            } 
            
            try {
                // "23" AsNumber shouldn't work
                VineValue str_val = "23";
                var asInt = str_val.AsNumber;
                if (VineValue.strictMode) {
                    Assert.Fail();
                }
            } catch (VineConversionException) {
                // It's ok
            }
            
            // AsBool
            try {
                // Shouldn't work
                VineValue str_val = "True";
                var asBool = str_val.AsBool;
                if (VineValue.strictMode) {
                    Assert.Fail();
                }
            } catch (VineConversionException) {
                // It's ok
            } 

            try {
                // Shouldn't work
                VineValue str_val = "false";
                var asBool = str_val.AsBool;
                if (VineValue.strictMode) {
                    Assert.Fail();
                }
            } catch (VineConversionException) {
                // It's ok
            } 
        }

        [TestMethod]
        public void Additions()
        {
            // ints
            var int_val = new VineValue(10) + new VineValue(2);
            Assert.AreEqual(12, int_val);

            int_val = 0;
            Assert.AreEqual(0, int_val);
            int_val += 1;
            Assert.AreEqual(1, int_val);
            int_val += 10;
            Assert.AreEqual(11, int_val);
            Assert.AreEqual(VineValue.Type.Int, int_val.type);
            int_val += 1.1;

            // To Number
            Assert.AreEqual(12.1, int_val);
            Assert.AreEqual(VineValue.Type.Number, int_val.type);
            
            // Back to Int
            int_val = 0;
            Assert.AreEqual(0, int_val);
            Assert.AreEqual(VineValue.Type.Int, int_val.type);
            Assert.AreEqual(0.0, int_val.AsInt); // auto cast for comparison with number
            Assert.AreNotEqual(VineValue.Type.Number, int_val.type);
            
            // To String
            int_val = new VineValue(42) + new VineValue("foo");
            Assert.AreEqual("42foo", int_val);
            Assert.AreEqual(VineValue.Type.String, int_val.type);


            // String combination
            VineValue foo = "foo";
            VineValue bar = "bar";
            VineValue foobar = foo + bar;
            Assert.AreEqual("foobar", foobar);

            // Copying
            //VineValue number_positive_copy = number_positive;
            ////var a = number_positive_copy - 2 + 2;
            //Assert.AreSame(number_positive, new VineValue(42.0));
            //Assert.AreNotSame(number_positive, 42.0);
            //Assert.AreSame(number_positive, number_positive_copy);
            //Assert.AreNotSame(number_positive, number_positive_float);
            //Assert.AreSame(number_positive, number_positive_copy - 2 + 2);
        }

        [TestMethod]
        public void Subtractions()
        {
            // ints
            VineValue int_val = new VineValue(20) - new VineValue(2);
            Assert.AreEqual(18, int_val);
            
            int_val = new VineValue(20) - new VineValue(-2);
            Assert.AreEqual(22, int_val);

            int_val = new VineValue(-20) - new VineValue(2);
            Assert.AreEqual(-22, int_val);

            int_val = new VineValue(-20) - new VineValue(-2);
            Assert.AreEqual(-18, int_val);

            // String
            try {
                // Shouldn't work
                VineValue foo = "foo";
                VineValue bar = "bar";
                VineValue foobar = foo - bar;
                Assert.Fail();
            } catch (VineArithmeticException) {
                // It's ok
            } 
        }

        [TestMethod]
        public void Multiplications()
        {
            // ints
            VineValue int_val = new VineValue(21) * new VineValue(2);
            Assert.AreEqual(42, int_val);

            // String
            try {
                // Shouldn't work
                VineValue foo = "foo";
                VineValue bar = "bar";
                VineValue foobar = foo * bar;
                Assert.Fail();
            } catch (VineArithmeticException) {
                // It's ok
            } 
        }

        [TestMethod]
        public void Divisions()
        {
            // ints
            VineValue int_val = new VineValue(25) / new VineValue(5);
            Assert.AreEqual(5, int_val);

            // String
            try {
                // Shouldn't work
                VineValue foo = "foo";
                VineValue bar = "bar";
                VineValue foobar = foo / bar;
                Assert.Fail();
            } catch (VineArithmeticException) {
                // It's ok
            } 
        }

        [TestMethod]
        public void BigInts()
        {
            // OverflowException is not thrown in an unchecked context
            {
                // int.MaxValue == 2147483647
                // int.MinValue == -2147483648
                VineValue int_val = 2147483647;
                VineValue int_val2 = 1;
                // it overflows and becomes the min negative value
                Assert.AreEqual(-2147483648, int_val + int_val2);
            }

            checked // Checked context, throws overflow exceptions
            {
                try {
                    VineValue int_val = 2147483647;
                    VineValue int_val2 = 1;
                    Assert.AreEqual(-2147483648, int_val + int_val2);
                } catch (OverflowException) {
                    // OK
                }
            }
        }

        [TestMethod]
        public void BigNumbers()
        {
            // todo
            //double.MaxValue
            //double.MinValue
            double a = double.NaN;
        }

        [TestMethod]
        public void LargeStrings()
        {
            StreamReader istream = File.OpenText("../../VineValueTests.cs");
            string input = istream.ReadToEnd();
            var str_val = new VineValue(input);
            Assert.AreEqual(input, str_val);
        }

        [TestMethod]
        public void VineValueHashCode()
        {
            // Quick test to make sure VineValue.GetHashCode() is working
            // An exception will be thrown if it isn't.
            HashSet<VineValue> set = new HashSet<VineValue>();
            set.Add(new VineValue(0));
            set.Add(new VineValue(0.0));
            set.Add(new VineValue(false));
            set.Add(new VineValue(""));
            set.Add(VineValue.NULL);
            set.Add(VineValue.newArray);
            VineValue arr = new List<VineValue> { 1, 2, VineValue.NULL, "foo", true, 42.2 };
            set.Add(new VineValue(arr));
            set.Add(VineValue.newDict);
            foreach (var el in set) {
                Console.WriteLine(
                    string.Format("{0}: {1}", el.GetHashCode(), el)
                );
            }
        }

        [TestMethod]
        public void ArrayValues()
        {
            VineValue arr = new List<VineValue> { 1, 2, VineValue.NULL, "foo", true, 42.2 };
            Assert.AreEqual(1, arr[0]);
            arr[0] = 22;
            Assert.AreEqual(22, arr[0]);
            Assert.AreEqual(2, arr[1]);
            Assert.AreEqual(VineValue.NULL, arr[2]);
            Assert.AreEqual("foo", arr[3]);
            Assert.AreEqual(true, arr[4]);
            Assert.AreEqual(42.2, arr[5]);
        }

        [TestMethod]
        public void ArrayPrint()
        {
            VineValue arr = new List<VineValue> { 1, 2, VineValue.NULL, "foo", true, 42.2 };
            Assert.AreEqual("[1, 2, , \"foo\", True, 42.2]", arr.ToString());
        }

        [TestMethod]
        public void ArrayEquality()
        {
            VineValue arr1 = new List<VineValue> { 1, 2, 3 };
            VineValue arr2 = new List<VineValue> { 1, 2, 3 };
            VineValue arr3 = new List<VineValue> { 2, 3, 1 };
            Assert.AreEqual(arr1, arr2);
            Assert.AreNotEqual(arr1, arr3);
        }

        [TestMethod]
        public void ArrayAddition()
        {
            VineValue arr1 = new List<VineValue> { 1, 2, 3 };
            VineValue arr2 = new List<VineValue> { 4, 5, 6 };
            VineValue arr3 = arr1 + arr2;
            Assert.AreEqual(1, arr3[0]);
            Assert.AreEqual(2, arr3[1]);
            Assert.AreEqual(3, arr3[2]);
            Assert.AreEqual(4, arr3[3]);
            Assert.AreEqual(5, arr3[4]);
            Assert.AreEqual(6, arr3[5]);
        }

        [TestMethod]
        public void Array2D()
        {
            VineValue arr1 = new List<VineValue> { 1, 2, 3 };
            VineValue arr2 = new List<VineValue> { 4, 5, 6 };
            VineValue arr3 = new List<VineValue> { 7, 8, 9 };
            VineValue array2d = new List<VineValue> { arr1, arr2, arr3 };

            Console.WriteLine(array2d);
            
            Assert.AreEqual(1, array2d[0][0]);
            Assert.AreEqual(2, array2d[0][1]);
            Assert.AreEqual(3, array2d[0][2]);

            Assert.AreEqual(4, array2d[1][0]);
            Assert.AreEqual(5, array2d[1][1]);
            Assert.AreEqual(6, array2d[1][2]);

            Assert.AreEqual(7, array2d[2][0]);
            Assert.AreEqual(8, array2d[2][1]);
            Assert.AreEqual(9, array2d[2][2]);
        }

        [TestMethod]
        public void ArrayClone()
        {
            VineValue inner1 = new List<VineValue> { 1, 2, 3 };
            VineValue inner2 = new List<VineValue> { 4, 5, 6 };
            VineValue inner3 = new List<VineValue> { 7, 8, 9 };
            VineValue array = new List<VineValue> { inner1, inner2, inner3 };
            
            VineValue refcopy = array;
            VineValue clone = array.Clone();

            array[0] = 42;
            array[1][0] = 99;
            
            // Share the same reference, should point to the same thing
            Assert.AreSame(array, refcopy);
            Assert.AreSame(array[0], refcopy[0]);
            Assert.AreSame(array[1], refcopy[1]);

            // Deep copy, the data should be completly cloned and independant
            Assert.AreNotSame(array, clone);
            Assert.AreNotSame(array[0], clone[0]);
            Assert.AreNotSame(array[1], clone[1]);
        }

        [TestMethod]
        public void DictValues()
        {
            Dictionary<string, VineValue> dictlst = new Dictionary<string, VineValue>();
            dictlst.Add("k1", "foo");
            dictlst.Add("k2", 32);
            dictlst.Add("k3", 0.43);
            dictlst.Add("k4", true);
            VineValue dict = new VineValue(dictlst);
            Assert.AreEqual("foo", dict["k1"]);
            dict["k1"] = "bar";
            Assert.AreEqual("bar", dict["k1"]);
            Assert.AreEqual(32, dict["k2"]);
            Assert.AreEqual(0.43, dict["k3"]);
            Assert.AreEqual(true, dict["k4"]);
        }

        [TestMethod]
        public void DictPrint()
        {
            Dictionary<string, VineValue> dictlst = new Dictionary<string, VineValue>();
            dictlst.Add("k1", "foo");
            dictlst.Add("k2", 32);
            dictlst.Add("k3", 0.43);
            dictlst.Add("k4", true);
            VineValue dict = new VineValue(dictlst);
            Assert.AreEqual(
                "{\"k1\": \"foo\", \"k2\": 32, \"k3\": 0.43, \"k4\": True}",
                dict.ToString()
            );
        }

        [TestMethod]
        public void DictEquality()
        {
            Dictionary<string, VineValue> dictlst1 = new Dictionary<string, VineValue>();
            dictlst1.Add("k1", 1);
            dictlst1.Add("k2", 2);
            dictlst1.Add("k3", 3);
            VineValue dict1 = new VineValue(dictlst1);
            Dictionary<string, VineValue> dictlst2 = new Dictionary<string, VineValue>();
            dictlst2.Add("k1", 1);
            dictlst2.Add("k3", 3);
            dictlst2.Add("k2", 2);
            VineValue dict2 = new VineValue(dictlst2);
            
            // Values and keys are the same
            Assert.AreEqual(dict1, dict2);

            VineValue dict3 = VineValue.newDict;
            dict3.AsDict.Add("k1", 1);
            dict3.AsDict.Add("k2", 2);
            dict3.AsDict.Add("k3.2", 3);
            VineValue dict4 = VineValue.newDict;
            dict4.AsDict.Add("k1", 1);
            dict4.AsDict.Add("k2", 2);
            dict4.AsDict.Add("k3", 4);

            // Values are the same but one key is different
            Assert.AreNotEqual(dict1, dict3);
            // Keys are the same but one value is different
            Assert.AreNotEqual(dict1, dict4);
        }

        [TestMethod]
        public void DictClone()
        {
            VineValue dict = VineValue.newDict;
            dict.AsDict.Add("a", new Dictionary<string, VineValue>() {
                { "a.a", 1 }, { "a.b", "foo" }, { "a.c", true }
            });
            dict.AsDict.Add("b", VineValue.newDict);

            dict.AsDict["b"].AsDict.Add("b.a", VineValue.NULL);
            
            VineValue refcopy = dict;
            VineValue clone = dict.Clone();

            dict["a"]["a.a"] += 100;
            dict["b"]["b.b"] = 0.0;
            
            // Share the same reference, should point to the same thing
            Assert.AreSame(dict, refcopy);
            Assert.AreSame(dict["a"], refcopy["a"]);
            Assert.AreSame(dict["b"], refcopy["b"]);

            // Deep copy, the data should be completly cloned and independant
            Assert.AreNotSame(dict, clone);
            Assert.AreNotSame(dict["a"], clone["a"]);
            Assert.AreNotSame(dict["b"], clone["b"]);
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion
    }
}
