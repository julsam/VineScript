using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using VineScript;
using VineScript.Core;

namespace VineScript.Test
{
    /// <summary>
    /// Summary description for VineValueTests
    /// </summary>
    [TestClass]
    public class VineVarTests
    {
        public VineVarTests()
        {
        }

        [TestMethod]
        [ExpectedException(typeof(VineArithmeticException))]
        public void BoolAdditionFails0()
        {
            VineVar a = new VineVar(true);
            VineVar b = new VineVar(false);
            var r = a + b;
        }

        [TestMethod]
        [ExpectedException(typeof(VineArithmeticException))]
        public void BoolAdditionFails1()
        {
            VineVar a = new VineVar(true);
            VineVar b = new VineVar(1);
            var r = a + b;
        }

        [TestMethod]
        [ExpectedException(typeof(VineArithmeticException))]
        public void BoolSubFails0()
        {
            VineVar a = new VineVar(true);
            VineVar b = new VineVar(false);
            var r = a - b;
        }

        [TestMethod]
        [ExpectedException(typeof(VineArithmeticException))]
        public void BoolSubFails1()
        {
            VineVar a = new VineVar(true);
            VineVar b = new VineVar(1);
            var r = a - b;
        }

        [TestMethod]
        public void NullArithmetic()
        {
            VineVar @null = null;
            VineVar NULL = VineVar.NULL;
            VineVar str = "foo";
            VineVar @int = 42;
            VineVar number = 16.66;
            VineVar @bool = true;
            VineVar array = VineVar.newArray;
            VineVar dict = VineVar.newDict;
            
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
            Assert.AreEqual((VineVar)"foo", str + @null);
            Assert.AreEqual((VineVar)"foo", str + NULL);
        }

        [TestMethod]
        public void NullGtEqLt()
        {
            VineVar @null = null;
            VineVar NULL = VineVar.NULL;
            VineVar str = "foo";
            VineVar @int = 42;
            VineVar number = 16.66;
            VineVar @bool = true;
            VineVar array = VineVar.newArray;
            VineVar dict = VineVar.newDict;

            if (VineVar.strictMode) {
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
            VineVar val = new VineVar(new VineVar());
        }

        [TestMethod]
        public void CreateBools()
        {
            VineVar boolean_true = new VineVar(true);
            VineVar boolean_false = new VineVar(false);
            Assert.AreEqual(VineVar.Type.Bool, boolean_true.type);
            Assert.AreEqual(VineVar.Type.Bool, boolean_false.type);
        }

        [TestMethod]
        public void CreateInts()
        {
            VineVar integer_42 = new VineVar(42);
            VineVar integer_minus77 = new VineVar(-77);
            VineVar integer_0 = new VineVar(0);
            VineVar integer_minus_0 = new VineVar(-0);
            
            VineVar integer_42_copy = integer_42;
            VineVar integer_42_copy_asnum = integer_42.AsNumber + 0.1;
            Assert.AreEqual(integer_42, new VineVar(42));
            Assert.AreEqual(integer_42, 42);
            Assert.AreNotEqual((int)42, (double)42.1);
            Assert.AreNotEqual(integer_42, new VineVar(42.1));

            Assert.AreSame(integer_42, integer_42_copy);
            Assert.AreNotSame(integer_42, new VineVar(42));
            Assert.AreEqual(integer_0, integer_minus_0);
        }

        [TestMethod]
        public void CreateNumbers()
        {
            VineVar number_42 = new VineVar(42.0);
            VineVar number_minus77 = new VineVar(-77.0);
            VineVar number_42_01234 = new VineVar(42.01234);
            VineVar number_minus77_9876 = new VineVar(-77.9876);

            // check type
            Assert.AreEqual(VineVar.Type.Number, number_42.type);
            Assert.AreEqual(VineVar.Type.Number, number_minus77.type);
            Assert.AreEqual(VineVar.Type.Number, number_42_01234.type);
            Assert.AreEqual(VineVar.Type.Number, number_minus77_9876.type);
            
            // check value
            Assert.AreEqual(42.0, number_42);
            Assert.AreEqual(-77.0, number_minus77);
            Assert.AreEqual(42.01234, number_42_01234);
            Assert.AreEqual(-77.9876, number_minus77_9876);

            
            var nan = new VineVar(double.NaN);
            Assert.AreEqual(VineVar.Type.Number, nan.type);
            Assert.AreEqual(double.NaN, nan.AsNumber);


            // Copying
            //VineVar number_positive_copy = number_positive;
            ////var a = number_positive_copy - 2 + 2;
            //Assert.AreSame(number_positive, new VineVar(42.0));
            //Assert.AreNotSame(number_positive, 42.0);
            //Assert.AreSame(number_positive, number_positive_copy);
            //Assert.AreNotSame(number_positive, number_positive_float);
            //Assert.AreSame(number_positive, number_positive_copy - 2 + 2);
        }

        [TestMethod]
        public void CreateStrings()
        {
            VineVar str = new VineVar("foo bar");
        }

        [TestMethod]
        public void CreateNulls()
        {
            VineVar val = new VineVar();
            VineVar val2 = VineVar.NULL;

            // Equality
            Assert.AreEqual(0, val.AsInt);
            Assert.AreEqual(0.0, val.AsNumber);
            Assert.AreEqual(false, val.AsBool);
            Assert.AreEqual("", val.AsString);
            Assert.AreEqual(val, val2);

            Assert.AreEqual(new VineVar(0), val.AsInt);
        }

        [TestMethod]
        public void UnaryNot()
        {
            // Int
            try {
                VineVar val = 6;
                val = !val;
                Assert.Fail();
            } catch (VineArithmeticException) { /* Valid */ } 

            // Number
            try {
                VineVar val = 6.1;
                val = !val;
                Assert.Fail();
            } catch (VineArithmeticException) { /* Valid */ } 

            // String
            try {
                VineVar val = "foo";
                val = !val;
                Assert.Fail();
            } catch (VineArithmeticException) { /* Valid */ } 
            
            VineVar bool_true = true;
            VineVar bool_false = false;
            Assert.AreEqual(false, !bool_true);
            Assert.AreNotEqual(true, !bool_true);
            Assert.AreEqual(true, !bool_false);
            Assert.AreNotEqual(false, !bool_false);
        }

        [TestMethod]
        public void CompareEqual()
        {
            // ==
            Assert.IsTrue(new VineVar(11) == 11);
            Assert.IsTrue(new VineVar(-200) == -200);
            Assert.IsTrue(new VineVar(42.0) == 42);
            Assert.IsTrue(new VineVar(42) == 42.0);
            Assert.IsTrue(new VineVar(42.0) == 42.0);
            Assert.IsTrue(new VineVar(42.1234) == 42.1234);
            Assert.IsTrue(new VineVar(true) == true);
            Assert.IsTrue(new VineVar(false) == false);
            Assert.IsTrue(new VineVar("foo") == "foo");
            Assert.IsFalse(new VineVar("foo") == 1); // Will fail in strict mode
            Assert.IsFalse(new VineVar("foo") == 1.1); // Will fail in strict mode
            Assert.IsFalse(new VineVar("foo") == VineVar.NULL);

            // == 
            // NULL == NULL
            Assert.IsTrue(new VineVar() == VineVar.NULL);
            Assert.IsTrue(new VineVar(11) == new VineVar(11));
            Assert.IsTrue(new VineVar(-200) == -(new VineVar(200)));
            Assert.IsTrue(new VineVar(-200) == new VineVar(-200));
            Assert.IsTrue(new VineVar(42.0) == new VineVar(42.0));
            Assert.IsTrue(new VineVar(42.1234) == new VineVar(42.1234));
            Assert.IsTrue(new VineVar(42.1234) == new VineVar(42.123400000));
            Assert.IsTrue(new VineVar(true) == new VineVar(true));
            Assert.IsTrue(new VineVar(false) == new VineVar(false));
        }

        [TestMethod]
        public void CompareNotEqual()
        {
            // !=
            Assert.IsTrue(new VineVar(11) != 22);
            Assert.IsTrue(new VineVar(42.0) != 0.0);
            Assert.IsTrue(new VineVar(42.12340) != 42.12341);
            Assert.IsTrue(new VineVar(true) != false);
            Assert.IsTrue(new VineVar(false) != true);
            Assert.IsTrue(new VineVar("foo") != "bar");
            Assert.IsFalse(new VineVar("foo") != "foo");
            Assert.IsTrue(new VineVar("foo") != 1); // Will fail in strict mode
            Assert.IsTrue(new VineVar("foo") != 1.1); // Will fail in strict mode
            Assert.IsTrue(new VineVar("foo") != VineVar.NULL);
            
            // != NULL
            Assert.IsTrue(new VineVar(11) != VineVar.NULL);
            Assert.IsTrue(new VineVar(11.0) != VineVar.NULL);
            Assert.IsTrue(new VineVar(true) != VineVar.NULL);
            Assert.IsTrue(new VineVar(false) != VineVar.NULL);
            Assert.IsTrue(new VineVar("") != VineVar.NULL);
            Assert.IsTrue(new VineVar("foo") != VineVar.NULL);

            //
            Assert.IsTrue(new VineVar(10) != new VineVar(11));
            Assert.IsTrue(new VineVar(200) != -(new VineVar(200)));
            Assert.IsTrue(new VineVar(200) != new VineVar(-200));
            Assert.IsTrue(new VineVar(42.0) != new VineVar(41));
            Assert.IsTrue(new VineVar(42.1234) != new VineVar(42.12345));
            Assert.IsTrue(new VineVar(true) != new VineVar(false));
        }

        [TestMethod]
        public void CompareGreaterThan()
        {
            // >
            Assert.IsTrue(new VineVar(22) > 11);
            Assert.IsTrue(new VineVar(-2) > -3);
            Assert.IsTrue(new VineVar(42.0) > -42.0);
            Assert.IsTrue(new VineVar(42.13) > 42.12);

            if (VineVar.strictMode) 
            {
                try {
                    Assert.IsTrue(new VineVar("") > 11);
                    Assert.Fail();
                } catch (VineComparisonException) { /* Valid */ }

                try {
                    Assert.IsTrue(new VineVar("abc") > "a");
                    Assert.Fail();
                } catch (VineComparisonException) { /* Valid */ }

                try {
                    Assert.IsTrue(new VineVar(true) > false);
                    Assert.Fail();
                } catch (VineComparisonException) { /* Valid */ }
            }
            else
            {
                Assert.IsFalse(new VineVar("") > 11);
                Assert.IsFalse(new VineVar("abc") > "a");
                Assert.IsFalse(new VineVar(true) > false);
            }
        }

        [TestMethod]
        public void CompareGreaterThanOrEqual()
        {
            // >=
            Assert.IsTrue(new VineVar(22) >= 11);
            Assert.IsTrue(new VineVar(22) >= 22);
            Assert.IsTrue(new VineVar(-2) >= -3);
            Assert.IsTrue(new VineVar(-2) >= -2);
            Assert.IsTrue(new VineVar(42.0) >= -42.0);
            Assert.IsTrue(new VineVar(42.0) >= 42.0);
            Assert.IsTrue(new VineVar(42.13) >= 42.12);
            Assert.IsTrue(new VineVar(42.13) >= 42.13);

            if (VineVar.strictMode) 
            {
                try {
                    Assert.IsTrue(new VineVar("") >= 11);
                    Assert.Fail();
                } catch (VineComparisonException) { /* Valid */ }

                try {
                    Assert.IsTrue(new VineVar("abc") >= "a");
                    Assert.Fail();
                } catch (VineComparisonException) { /* Valid */ }

                try {
                    Assert.IsTrue(new VineVar(true) >= false);
                    Assert.Fail();
                } catch (VineComparisonException) { /* Valid */ }
            }
            else
            {
                Assert.IsFalse(new VineVar("") >= 11);
                Assert.IsFalse(new VineVar("abc") >= "a");
                Assert.IsFalse(new VineVar(true) >= false);
            }
        }

        [TestMethod]
        public void CompareLowerThan()
        {
            // >
            Assert.IsTrue(new VineVar(11) < 22);
            Assert.IsTrue(new VineVar(-3) < -2);
            Assert.IsTrue(new VineVar(-42.0) < 42.0);
            Assert.IsTrue(new VineVar(42.12) < 42.13);

            if (VineVar.strictMode) 
            {
                try {
                    Assert.IsTrue(new VineVar("") < 11);
                    Assert.Fail();
                } catch (VineComparisonException) { /* Valid */ }

                try {
                    Assert.IsTrue(new VineVar("abc") < "a");
                    Assert.Fail();
                } catch (VineComparisonException) { /* Valid */ }

                try {
                    Assert.IsTrue(new VineVar(true) < false);
                    Assert.Fail();
                } catch (VineComparisonException) { /* Valid */ }
            }
            else
            {
                Assert.IsFalse(new VineVar("") < 11);
                Assert.IsFalse(new VineVar("abc") < "a");
                Assert.IsFalse(new VineVar(true) < false);
            }
        }

        [TestMethod]
        public void CompareLowerThanOrEqual()
        {
            // >=
            Assert.IsTrue(new VineVar(11) <= 22);
            Assert.IsTrue(new VineVar(11) <= 11);
            Assert.IsTrue(new VineVar(-3) <= -2);
            Assert.IsTrue(new VineVar(-3) <= -3);
            Assert.IsTrue(new VineVar(-42.0) <= 42.0);
            Assert.IsTrue(new VineVar(-42.0) <= -42.0);
            Assert.IsTrue(new VineVar(42.12) <= 42.13);
            Assert.IsTrue(new VineVar(42.12) <= 42.12);
            
            if (VineVar.strictMode) 
            {
                try {
                    Assert.IsTrue(new VineVar("") <= 11);
                    Assert.Fail();
                } catch (VineComparisonException) { /* Valid */ }

                try {
                    Assert.IsTrue(new VineVar("abc") <= "a");
                    Assert.Fail();
                } catch (VineComparisonException) { /* Valid */ }

                try {
                    Assert.IsTrue(new VineVar(true) <= false);
                    Assert.Fail();
                } catch (VineComparisonException) { /* Valid */ }
            }
            else
            {
                Assert.IsFalse(new VineVar("") <= 11);
                Assert.IsFalse(new VineVar("abc") <= "a");
                Assert.IsFalse(new VineVar(true) <= false);
            }
        }

        [TestMethod]
        public void BoolAs()
        {
            VineVar bool_true = true;
            VineVar bool_false = false;
            
            // AsBool
            Assert.AreEqual(true, bool_true.AsBool);
            Assert.AreEqual(false, bool_false.AsBool);
            
            // AsInt
            try {
                // Shouldn't work
                var asInt = bool_true.AsInt;
                if (VineVar.strictMode) {
                    Assert.Fail();
                }
            } catch (VineConversionException) {
                // It's ok
            } 
            
            // AsNumber
            try {
                // Shouldn't work
                var asNumber = bool_true.AsNumber;
                if (VineVar.strictMode) {
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
            VineVar int_val = 13;
            Assert.AreEqual(VineVar.Type.Int, int_val.type);

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
                if (VineVar.strictMode) {
                    Assert.Fail();
                }
            } catch (VineConversionException) {
                // It's ok
            } 
        }

        [TestMethod]
        public void NumberAs()
        {
            VineVar num_val = 17.0;
            Assert.AreEqual(VineVar.Type.Number, num_val.type);
            Assert.AreNotEqual(VineVar.Type.Int, num_val.type);

            // AsInt
            Assert.AreEqual(17, num_val.AsInt);
            // AsNumber
            Assert.AreEqual(17.0, num_val.AsNumber);
            // AsString
            Assert.AreEqual("17.0", num_val.AsString);

            // AsBool
            try {
                // Shouldn't work
                var asBool = num_val.AsBool;
                if (VineVar.strictMode) {
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
            Assert.AreEqual("foobar", (new VineVar("foobar")).AsString);
            Assert.AreEqual("ABCDEF abcdef 0123456789 !@#$%^&*()_+{}[]'", 
                (new VineVar("ABCDEF abcdef 0123456789 !@#$%^&*()_+{}[]'")).AsString);

            var empty = new VineVar("");
            Assert.AreEqual("", empty.AsString);

            // AsInt
            try {
                // "foobar" AsInt shouldn't work
                VineVar str_val = "foobar";
                var asInt = str_val.AsInt;
                if (VineVar.strictMode) {
                    Assert.Fail();
                }
            } catch (VineConversionException) {
                // It's ok
            } 
            
            try {
                // "23" AsInt shouldn't work
                VineVar str_val = "23";
                var asInt = str_val.AsInt;
                if (VineVar.strictMode) {
                    Assert.Fail();
                }
            } catch (VineConversionException) {
                // It's ok
            }
            
            // AsNumber
            try {
                // "foobar" AsNumber shouldn't work
                VineVar str_val = "foobar";
                var asInt = str_val.AsNumber;
                if (VineVar.strictMode) {
                    Assert.Fail();
                }
            } catch (VineConversionException) {
                // It's ok
            } 
            
            try {
                // "23" AsNumber shouldn't work
                VineVar str_val = "23";
                var asInt = str_val.AsNumber;
                if (VineVar.strictMode) {
                    Assert.Fail();
                }
            } catch (VineConversionException) {
                // It's ok
            }
            
            // AsBool
            try {
                // Shouldn't work
                VineVar str_val = "True";
                var asBool = str_val.AsBool;
                if (VineVar.strictMode) {
                    Assert.Fail();
                }
            } catch (VineConversionException) {
                // It's ok
            } 

            try {
                // Shouldn't work
                VineVar str_val = "false";
                var asBool = str_val.AsBool;
                if (VineVar.strictMode) {
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
            var int_val = new VineVar(10) + new VineVar(2);
            Assert.AreEqual(12, int_val);

            int_val = 0;
            Assert.AreEqual(0, int_val);
            int_val += 1;
            Assert.AreEqual(1, int_val);
            int_val += 10;
            Assert.AreEqual(11, int_val);
            Assert.AreEqual(VineVar.Type.Int, int_val.type);
            int_val += 1.1;

            // To Number
            Assert.AreEqual(12.1, int_val);
            Assert.AreEqual(VineVar.Type.Number, int_val.type);
            
            // Back to Int
            int_val = 0;
            Assert.AreEqual(0, int_val);
            Assert.AreEqual(VineVar.Type.Int, int_val.type);
            Assert.AreEqual(0.0, int_val.AsInt); // auto cast for comparison with number
            Assert.AreNotEqual(VineVar.Type.Number, int_val.type);
            
            // To String
            int_val = new VineVar(42) + new VineVar("foo");
            Assert.AreEqual("42foo", int_val);
            Assert.AreEqual(VineVar.Type.String, int_val.type);


            // String combination
            VineVar foo = "foo";
            VineVar bar = "bar";
            VineVar foobar = foo + bar;
            Assert.AreEqual("foobar", foobar);

            // Copying
            //VineVar number_positive_copy = number_positive;
            ////var a = number_positive_copy - 2 + 2;
            //Assert.AreSame(number_positive, new VineVar(42.0));
            //Assert.AreNotSame(number_positive, 42.0);
            //Assert.AreSame(number_positive, number_positive_copy);
            //Assert.AreNotSame(number_positive, number_positive_float);
            //Assert.AreSame(number_positive, number_positive_copy - 2 + 2);
        }

        [TestMethod]
        public void Subtractions()
        {
            // ints
            VineVar int_val = new VineVar(20) - new VineVar(2);
            Assert.AreEqual(18, int_val);
            
            int_val = new VineVar(20) - new VineVar(-2);
            Assert.AreEqual(22, int_val);

            int_val = new VineVar(-20) - new VineVar(2);
            Assert.AreEqual(-22, int_val);

            int_val = new VineVar(-20) - new VineVar(-2);
            Assert.AreEqual(-18, int_val);

            // String
            try {
                // Shouldn't work
                VineVar foo = "foo";
                VineVar bar = "bar";
                VineVar foobar = foo - bar;
                Assert.Fail();
            } catch (VineArithmeticException) {
                // It's ok
            } 
        }

        [TestMethod]
        public void Multiplications()
        {
            // ints
            VineVar int_val = new VineVar(21) * new VineVar(2);
            Assert.AreEqual(42, int_val);

            // String
            try {
                // Shouldn't work
                VineVar foo = "foo";
                VineVar bar = "bar";
                VineVar foobar = foo * bar;
                Assert.Fail();
            } catch (VineArithmeticException) {
                // It's ok
            } 
        }

        [TestMethod]
        public void Divisions()
        {
            // ints
            VineVar int_val = new VineVar(25) / new VineVar(5);
            Assert.AreEqual(5, int_val);

            // String
            try {
                // Shouldn't work
                VineVar foo = "foo";
                VineVar bar = "bar";
                VineVar foobar = foo / bar;
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
                VineVar int_val = 2147483647;
                VineVar int_val2 = 1;
                // it overflows and becomes the min negative value
                Assert.AreEqual(-2147483648, int_val + int_val2);
            }

            checked // Checked context, throws overflow exceptions
            {
                try {
                    VineVar int_val = 2147483647;
                    VineVar int_val2 = 1;
                    Assert.AreEqual(-2147483648, int_val + int_val2);
                } catch (OverflowException) {
                    // OK
                }
            }
        }

        [TestMethod]
        public void LargeStrings()
        {
            StreamReader istream = File.OpenText("../../VineVarTests.cs");
            string input = istream.ReadToEnd();
            var str_val = new VineVar(input);
            Assert.AreEqual(input, str_val);
        }

        [TestMethod]
        public void VineValueHashCode()
        {
            // Quick test to make sure VineVar.GetHashCode() is working
            // An exception will be thrown if it isn't.
            HashSet<VineVar> set = new HashSet<VineVar>();
            set.Add(new VineVar(0));
            set.Add(new VineVar(0.0));
            set.Add(new VineVar(false));
            set.Add(new VineVar(""));
            set.Add(VineVar.NULL);
            set.Add(VineVar.newArray);
            VineVar arr = new List<VineVar> { 1, 2, VineVar.NULL, "foo", true, 42.2 };
            set.Add(new VineVar(arr));
            set.Add(VineVar.newDict);
            foreach (var el in set) {
                Console.WriteLine(
                    string.Format("{0}: {1}", el.GetHashCode(), el)
                );
            }
        }

        [TestMethod]
        public void ArrayValues()
        {
            VineVar arr = new List<VineVar> { 1, 2, VineVar.NULL, "foo", true, 42.2 };
            Assert.AreEqual(1, arr[0]);
            arr[0] = 22;
            Assert.AreEqual(22, arr[0]);
            Assert.AreEqual(2, arr[1]);
            Assert.AreEqual(VineVar.NULL, arr[2]);
            Assert.AreEqual("foo", arr[3]);
            Assert.AreEqual(true, arr[4]);
            Assert.AreEqual(42.2, arr[5]);
        }

        [TestMethod]
        public void ArrayPrint()
        {
            VineVar arr = new List<VineVar> { 1, 2, VineVar.NULL, "foo", true, 42.2 };
            Assert.AreEqual("[1, 2, , \"foo\", True, 42.2]", arr.ToString());
        }

        [TestMethod]
        public void ArrayEquality()
        {
            VineVar arr1 = new List<VineVar> { 1, 2, 3 };
            VineVar arr2 = new List<VineVar> { 1, 2, 3 };
            VineVar arr3 = new List<VineVar> { 2, 3, 1 };
            Assert.AreEqual(arr1, arr2);
            Assert.AreNotEqual(arr1, arr3);
        }

        [TestMethod]
        public void ArrayAddition()
        {
            VineVar arr1 = new List<VineVar> { 1, 2, 3 };
            VineVar arr2 = new List<VineVar> { 4, 5, 6 };
            VineVar arr3 = arr1 + arr2;
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
            VineVar arr1 = new List<VineVar> { 1, 2, 3 };
            VineVar arr2 = new List<VineVar> { 4, 5, 6 };
            VineVar arr3 = new List<VineVar> { 7, 8, 9 };
            VineVar array2d = new List<VineVar> { arr1, arr2, arr3 };

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
            VineVar inner1 = new List<VineVar> { 1, 2, 3 };
            VineVar inner2 = new List<VineVar> { 4, 5, 6 };
            VineVar inner3 = new List<VineVar> { 7, 8, 9 };
            VineVar array = new List<VineVar> { inner1, inner2, inner3 };
            
            VineVar refcopy = array;
            VineVar clone = array.Clone();

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
            Dictionary<string, VineVar> dictlst = new Dictionary<string, VineVar>();
            dictlst.Add("k1", "foo");
            dictlst.Add("k2", 32);
            dictlst.Add("k3", 0.43);
            dictlst.Add("k4", true);
            VineVar dict = new VineVar(dictlst);
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
            Dictionary<string, VineVar> dictlst = new Dictionary<string, VineVar>();
            dictlst.Add("k1", "foo");
            dictlst.Add("k2", 32);
            dictlst.Add("k3", 0.43);
            dictlst.Add("k4", true);
            VineVar dict = new VineVar(dictlst);
            Assert.AreEqual(
                "{\"k1\": \"foo\", \"k2\": 32, \"k3\": 0.43, \"k4\": True}",
                dict.ToString()
            );
        }

        [TestMethod]
        public void DictEquality()
        {
            Dictionary<string, VineVar> dictlst1 = new Dictionary<string, VineVar>();
            dictlst1.Add("k1", 1);
            dictlst1.Add("k2", 2);
            dictlst1.Add("k3", 3);
            VineVar dict1 = new VineVar(dictlst1);
            Dictionary<string, VineVar> dictlst2 = new Dictionary<string, VineVar>();
            dictlst2.Add("k1", 1);
            dictlst2.Add("k3", 3);
            dictlst2.Add("k2", 2);
            VineVar dict2 = new VineVar(dictlst2);
            
            // Values and keys are the same
            Assert.AreEqual(dict1, dict2);

            VineVar dict3 = VineVar.newDict;
            dict3.AsDict.Add("k1", 1);
            dict3.AsDict.Add("k2", 2);
            dict3.AsDict.Add("k3.2", 3);
            VineVar dict4 = VineVar.newDict;
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
            VineVar dict = VineVar.newDict;
            dict.AsDict.Add("a", new Dictionary<string, VineVar>() {
                { "a.a", 1 }, { "a.b", "foo" }, { "a.c", true }
            });
            dict.AsDict.Add("b", VineVar.newDict);

            dict.AsDict["b"].AsDict.Add("b.a", VineVar.NULL);
            
            VineVar refcopy = dict;
            VineVar clone = dict.Clone();

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
