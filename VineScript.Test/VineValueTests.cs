using System;
using System.Text;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using VineScript;
using VineScript.Core;
using VineScript.Core.VineValue;

namespace VineScript.Test
{
    [TestClass]
    public class VineValueTests
    {
        [TestMethod]
        [ExpectedException(typeof(VineConversionException))]
        public void FailNull01()
        {
            VineVar vinevar = null;
            new VineBool(vinevar);
        }

        [TestMethod]
        [ExpectedException(typeof(VineConversionException))]
        public void FailNull02()
        {
            new VineBool(VineVar.NULL);
        }

        [TestMethod]
        public void ArrayCreation()
        {
            VineArray a = new List<VineVar>();
            a = new List<bool>();
            a = new List<int>();
            a = new List<double>();
            a = new List<string>();
            a = new List<dynamic>();

            VineVar nestedintlist = new List<VineVar>() {
                new VineVar(new List<int>() { 1,2,3,4 }),
                new VineVar(new List<int>() { 5,6,7,8 }),
            };
            a = nestedintlist;

            var vinevarlist = new List<VineVar>() {
                new VineVar(new List<VineVar> { 1,2,3,4 }),
                new VineVar(new List<VineVar> { 5,6,7,8 }),
            };
            a = vinevarlist;

            var nestedlist = new List<List<VineVar>>() {
                new List<VineVar> { 1,2,3,4 },
                new List<VineVar> { 5,6,7,8 },
            };
            a = new VineArray(new VineVar(nestedlist));

            var nestedlist2 = new List<dynamic>() {
                new List<VineVar> { 1,2,3,4 },
                new List<VineVar> { 5,6,7,8 },
            };
            a = nestedlist2;

            var nestedlist3 = new List<dynamic>() {
                new Dictionary<string, VineVar>() {
                        { "a", 1 },
                        { "b", 2 }
                },
                new Dictionary<string, VineVar>() {
                        { "c", 3 },
                        { "d", 4 }
                }
            };
            a = nestedlist3;
        }

        [TestMethod]
        public void DictCreation()
        {
            VineDictionary d = new Dictionary<string, VineVar>();
            d = new Dictionary<string, bool>();
            d = new Dictionary<string, int>();
            d = new Dictionary<string, float>();
            d = new Dictionary<string, string>();
            d = new Dictionary<string, dynamic>();

            var nested_dict = new Dictionary<string, dynamic>() {
                { "a", new VineVar(new Dictionary<string, VineVar>() {
                        { "a", 1 },
                        { "b", 2 }
                    })
                },
                { "b", new VineVar(new Dictionary<string, VineVar>() {
                        { "a", 1 },
                        { "b", 2 }
                    })
                }
            };
        }

        [TestMethod]
        public void ArrayManipulation()
        {
            VineVar vinevar_array = VineVar.newArray;
            VineArray array = new VineArray(vinevar_array);
            
            // [[1, 2], [3, 4]]
            vinevar_array.AsArray.Add(VineVar.newArray);
            vinevar_array.AsArray.Add(VineVar.newArray);
            vinevar_array[0].AsArray.Add(1);
            vinevar_array[0].AsArray.Add(2);
            vinevar_array[1].AsArray.Add(3);
            vinevar_array[1].AsArray.Add(4);

            // Adding values to the array
            VineInt one = 1;
            array.Add(one); // index 2
            array.Add((VineInt)2); // index 3
            array.Add(3.14); // index 4
            array.Add("Foo"); // index 5

            // Should look like:
            // [[1, 2], [3, 4], 1, 2, 3.14, "Foo"]

            // Modifying
            array[3] += 1;
            array[1][0]++;

            // Should look like:
            // [[1, 2], [4, 4], 1, 3, 3.14, "Foo"]

            // Accessing nested array
            VineArray array2 = array[1];
            array2.Add("Bar");

            // Should look like:
            // [[1, 2], [4, 4, "Bar"], 1, 3, 3.14, "Foo"]

            // Convert to VineVar
            var array_converted = array.ToVineVar();
            array_converted.AsArray.Add(55);
            array_converted[0] = 97;

            // Should look like:
            // [97, [4, 4, "Bar"], 1, 3, 3.14, "Foo", 55]

            // Adding a new array
            var newArray = new VineArray(new List<int>());
            newArray.Add(0);
            array.Add(newArray);

            // Should look like:
            // [97, [4, 4, "Bar"], 1, 3, 3.14, "Foo", 55, [0]]

            // Adding itself
            array.Add(array[0]);
            array.Add(array2);

            // Should look like:
            // [97, [4, 4, "Bar"], 1, 3, 3.14, "Foo", 55, [0], 97, [4, 4, "Bar"]]

            Assert.AreEqual(
                "[97, [4, 4, \"Bar\"], 1, 3, 3.14, \"Foo\", 55, [0], 97, [4, 4, \"Bar\"]]",
                array.ToVineVar().ToString()
            );
        }

        [TestMethod]
        public void ArrayEquals()
        {
            VineVar vinevar_array = VineVar.newArray;
            VineArray array = new VineArray(vinevar_array);
            // [1, 2]
            var nested = VineVar.newArray;
            nested.AsArray.Add(1);
            nested.AsArray.Add(2);
            
            // [1, 2.2, "Foo", true, [1, 2]]
            vinevar_array.AsArray.Add(1);
            vinevar_array.AsArray.Add(2.2);
            vinevar_array.AsArray.Add("Foo");
            vinevar_array.AsArray.Add(true);
            vinevar_array.AsArray.Add(nested);


            Assert.IsTrue(array[0] == 1);
            Assert.IsTrue(array[1] == 2.2);
            Assert.IsTrue(array[2] == "Foo");
            Assert.IsTrue(array[3] == true);
            Assert.IsTrue(array[4] == nested);
            
            Assert.IsTrue(array[0] == (VineInt)1);
            Assert.IsTrue(array[1] == (VineNumber)2.2);
            Assert.IsTrue(array[2] == (VineString)"Foo");
            Assert.IsTrue(array[3] == (VineBool)true);
            Assert.IsTrue(array[4] == (VineArray)nested);

            Assert.IsTrue(array[0] != "F");
            Assert.IsTrue(array[1] != true);
            Assert.IsTrue(array[2] != 1);
            Assert.IsTrue(array[3] != nested);
            Assert.IsTrue(array[4] != 2.2);

            Assert.IsFalse(array[0] == "F");
            Assert.IsFalse(array[1] == true);
            Assert.IsFalse(array[2] == 1);
            Assert.IsFalse(array[3] == nested);
            Assert.IsFalse(array[4] == 2.2);
        }

        [TestMethod]
        public void ArrayContains()
        {
            VineVar vinevar_array = VineVar.newArray;
            VineArray array = new VineArray(vinevar_array);
            
            // [[42, 56], [3, 4]]
            vinevar_array.AsArray.Add(VineVar.newArray);
            vinevar_array.AsArray.Add(VineVar.newArray);
            vinevar_array[0].AsArray.Add(42);
            vinevar_array[0].AsArray.Add(56);
            vinevar_array[1].AsArray.Add(3);
            vinevar_array[1].AsArray.Add(4);
            
            Assert.IsTrue(array[0].Contains(42));
            Assert.IsTrue(array[0].Contains((VineInt)42));
            Assert.IsTrue(array[0].Contains((VineVar)42));
            Assert.IsTrue(array[0].Contains(array[0][1]));
            Assert.IsTrue(array.Contains(array[0]));
            Assert.IsFalse(array.Contains(null));
            Assert.IsFalse(array.Contains(VineVar.NULL));
            
            // [[42, 56], [3, 4], null]
            vinevar_array.AsArray.Add(null);
            Assert.IsTrue(array.Contains(null));
            
            // [[42, 56, null], [3, 4], null]
            vinevar_array[0].AsArray.Add(VineVar.NULL);
            Assert.IsTrue(array[0].Contains(VineVar.NULL));
        }

        [TestMethod]
        public void ArrayRemove()
        {
            // [[1, 2, null], [3, 4]]
            VineVar vinevar_array = VineVar.newArray;
            vinevar_array.AsArray.Add(VineVar.newArray);
            vinevar_array.AsArray.Add(VineVar.newArray);
            vinevar_array[0].AsArray.Add(1);
            vinevar_array[0].AsArray.Add(2);
            vinevar_array[0].AsArray.Add(null);
            vinevar_array[1].AsArray.Add(3);
            vinevar_array[1].AsArray.Add(4);

            VineArray array = new VineArray(vinevar_array);
            
            Assert.IsFalse(array.Remove(null));
            Assert.IsTrue(array[0].Remove(null));
            Assert.IsTrue(array[1].Remove(3));
            Assert.IsFalse(array[1].Remove(42));

            array[0].RemoveAt(0);
            
            Assert.AreEqual(
                "[[2], [4]]",
                array.ToVineVar().ToString()
            );
        }

        [TestMethod]
        public void DictManipulation()
        {
            //TODO
            // {"a": 1, "b": 2, "c": {"c.a": 3, "c.b": 4 }}
            //VineVar vinevar_dict = VineVar.newDict;
            //vinevar_dict.AsDict.Add("a", 1);
            //vinevar_dict.AsDict.Add("b", 2);
            //vinevar_dict.AsDict.Add("c", VineVar.newDict);
            //vinevar_dict["c"].AsDict.Add("c.a", 3);
            //vinevar_dict["c"].AsDict.Add("c.b", 4);

            //VineDictionary val1 = new VineDictionary(VineVar.newDict);
            //var l = new Dictionary<string, int>();
            //VineDictionary arr = l;
            //val1.Add("foo", arr);
            
            //val1.Add("d", (VineInt)1);
            //VineInt one = 1;
            //val1.Add("one", one);
            //val1.Add("e", 1);
            //val1.Value.Add("f", 1.2);
            //var enumerat = val1.GetEnumerator();
            //enumerat.MoveNext();
            //val1.Remove(enumerat.Current);

            //var kvp = new KeyValuePair<string, object>("d", (VineInt)21);
            //var isRemoved1 = val1.Remove(kvp);
            //var isRemoved2 = val1.Remove("e");
        }
    }
}
