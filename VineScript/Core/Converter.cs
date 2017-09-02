using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VineScript.Core.VineValue;

namespace VineScript.Core
{
    public class Converter
    {
        // --------------------------------------------------------------------
        // VineValue
        // --------------------------------------------------------------------

        public static dynamic ToVineValue(object value)
        {
            if (value is VineVar)
            {
                VineVar vinevar = value as VineVar;
                if (vinevar.type == VineVar.Type.Bool) {
                return new VineBool(vinevar);
                }
                else if (vinevar.type == VineVar.Type.Int) {
                    return new VineInt(vinevar);
                }
                else if (vinevar.type == VineVar.Type.Number) {
                    return new VineNumber(vinevar);
                }
                else if (vinevar.type == VineVar.Type.String) {
                    return new VineString(vinevar);
                }
                else if (vinevar.type == VineVar.Type.Array) {
                    return new VineArray(vinevar);
                }
                else if (vinevar.type == VineVar.Type.Dict) {
                    return new VineDictionary(vinevar);
                }
                else if (vinevar.type == VineVar.Type.Null) {
                    return null;
                }
            }
            else
            {
                if (value is IVineValue) {
                    return value;
                }
                else if (value is bool) {
                    return new VineBool((bool)value);
                }
                else if (value is int) {
                    return new VineInt((int)value);
                }
                else if (value is double || value is float) {
                    return new VineNumber((double)value);
                }
                else if (value is string) {
                    return new VineString((string)value);
                }
                else if (value is IList)
                {
                    if (value is List<VineVar>) {
                        return new VineArray(new VineVar((List<VineVar>)value));
                    }
                    else {
                        return new VineArray((VineVar)value);
                    }
                }
                else if (value is IDictionary)
                {
                    if (value is Dictionary<string, VineVar>) {
                        var vinevar = new VineVar((Dictionary<string, VineVar>)value);
                        return new VineDictionary(vinevar);
                    }
                    else {
                        return new VineDictionary((VineVar)value);
                    }
                }
                else if (value == null)
                {
                    return null;
                }
            }
            
            throw new Exception(string.Format(
                "Can't convert type \"{0}\" to VineValue.\nAccepted types are:"
                + " bool, int, double, string, IList, IDictionnary,"
                + " VineVar, VineBool, VineInt, VineNumber, VineString,"
                + " VineArray or VineDictionary",
                value.GetType()
            ));
        }

        /// <summary>Convert a List of VineVar to a List of VineValue</summary>
        public static List<dynamic> ToListOfVineValues(List<VineVar> list)
        {
            List<dynamic> converted = new List<dynamic>(list.Count);
            foreach (var item in list) {
                converted.Add(ToVineValue(item));
            }
            return converted;
        }

        /// <summary>Convert a Dictionary of VineVar to a Dictionary of VineValue</summary>
        public static Dictionary<string, dynamic>
            ToDictOfVineValues(Dictionary<string, VineVar> dict)
        {
            var converted = new Dictionary<string, dynamic>(dict.Count);
            foreach (var item in dict) {
                converted.Add(item.Key, ToVineValue(item.Value));
            }
            return converted;
        }
        
        // --------------------------------------------------------------------
        // VineVar
        // --------------------------------------------------------------------

        /// <summary>Convert a List to VineVar</summary>
        public static VineVar ToVineVar(IList list)
        {
            return new VineVar(ToListOfVineVars(list));
        }

        /// <summary>Convert a Dictionary to VineVar</summary>
        public static VineVar ToVineVar(IDictionary dict)
        {
            return new VineVar(ToDictOfVineVars(dict));
        }

        /// <summary>Convert a List of C# values to a List of VineVar</summary>
        public static List<VineVar> ToListOfVineVars(IList list)
        {
            List<VineVar> converted = new List<VineVar>(list.Count);
            foreach (var item in list) {
                if (item is IList) {
                    var nested = ToListOfVineVars((IList)item);
                    converted.Add(new VineVar(nested));
                } else if (item is IDictionary) {
                    var nested = ToDictOfVineVars((IDictionary)item);
                    converted.Add(new VineVar(nested));
                } else {
                    converted.Add(new VineVar(item));
                }
            }
            return converted;
        }

        /// <summary>Convert Dictionary to Dictionary of VineVar</summary>
        public static Dictionary<string, VineVar> ToDictOfVineVars(IDictionary dict)
        {
            // TODO check key 
            // TODO check value type : IsAllowedType / IsCandidate
            //Type key_type = dict.GetType().GetGenericArguments()[0];
            //Type value_type = dict.GetType().GetGenericArguments()[1];
            Dictionary<string, VineVar> converted = new Dictionary<string, VineVar>(dict.Count);
            foreach (string key in dict.Keys) {
                if (dict[key] is IDictionary) {
                    var nested = ToDictOfVineVars((IDictionary)dict[key]);
                    converted.Add(key, new VineVar(nested));
                } else if (dict[key] is IList) {
                    var nested = ToListOfVineVars((IList)dict[key]);
                    converted.Add(key, new VineVar(nested));
                } else {
                    converted.Add(key, new VineVar(dict[key]));
                }
            }
            return converted;
        }

        //public static List<object> ConvertFromVineVarList(List<VineVar> list)
        //{
        //    List<object> converted = new List<object>(list.Count);
        //    foreach (var item in list) {
        //        if (item.type == VineVar.Type.Array) {
        //            var nested = ConvertFromVineVarList(item.AsArray);
        //            converted.Add(nested);
        //        } else if (item.type == VineVar.Type.Dict) {
        //            var nested = ConvertFromVineVarDictionary(item.AsDict);
        //            converted.Add(nested);
        //        } else {
        //            converted.Add(item.AsObject);
        //        }
        //    }
        //    return converted;
        //}

        //public static Dictionary<string, object> ConvertFromVineVarDictionary(Dictionary<string, VineVar> dict)
        //{
        //    Dictionary<string, object> converted = new Dictionary<string, object>(dict.Count);
        //    foreach (string key in dict.Keys) {
        //        if (dict[key].type == VineVar.Type.Dict) {
        //            var nested = ConvertFromVineVarDictionary(dict[key].AsDict);
        //            converted.Add(key, nested);
        //        } else if (dict[key].type == VineVar.Type.Array) {
        //            var nested = ConvertFromVineVarList(dict[key].AsArray);
        //            converted.Add(key, nested);
        //        } else {
        //            converted.Add(key, dict[key].AsObject);
        //        }
        //    }
        //    return converted;
        //}
    }
}
