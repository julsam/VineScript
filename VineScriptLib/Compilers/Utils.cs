﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VineScriptLib.Compilers
{
    public class Utils
    {
        static public string PrettyPrint(string stringTree)
        {
            string output = "";
            string str = stringTree.Replace(" (", "\n(");
            str = str.Replace(")", ")\n");
            string[] lines = str.Split(new char[] { '\n'}, StringSplitOptions.RemoveEmptyEntries);
            int tab = -1;
            foreach (string line in lines) {
                if (line[0] == '(') {
                    tab++;
                }
                for (int i = 0; i < tab; i++) {
                    output += "  ";
                }
                output += line.Trim() + "\n";
                if (line[line.Length - 1] == ')') {
                    tab--;
                }
            }
            return output;
        }

        static public string RemoveWhiteSpace(string text)
        {
            Console.WriteLine("--- WS removed: ---");
            string wsRemoved = "";
            string tmp = text.Replace("\r", "");
            string[] lines = tmp.Split(new char[] { '\n'});
            for (int i = 0; i < lines.Length; i++) {
                // removes starting & ending whitespaces
                wsRemoved += lines[i].Trim(new char[] { '\t', ' ' });
                if (i < lines.Length - 1) {
                    // add new line if it's not the last one
                    wsRemoved += Environment.NewLine;
                }
            }
            Console.WriteLine(wsRemoved);
            Console.WriteLine("--- ---");
            return wsRemoved;
        }
    }
}