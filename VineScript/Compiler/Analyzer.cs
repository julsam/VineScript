using System;
using System.IO;
using System.Collections.Generic;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using VineScript.Core;

namespace VineScript.Compiler
{
    public class Analyzer
    {
        public List<SyntaxErrorReport> CheckSyntaxFile(string filename)
        {
            // if file exists:
            // CheckSyntax(File.OpenText(inputFile));
            throw new NotImplementedException();
        }

        public List<SyntaxErrorReport> CheckSyntaxFile(StreamReader istream)
        {
            // if file exists:
            // CheckSyntax(Open.File(filename));
            throw new NotImplementedException();
        }

        public List<SyntaxErrorReport> CheckSyntaxDirectory(string dir, string ext=".vine")
        {
            // if directory exists:
            // list = "dir/*.vine"
            // foreach (file in list)
            // CheckSyntaxFile(file);
            throw new NotImplementedException();
        }

        public List<SyntaxErrorReport> CheckSyntax(string vinecode)
        {
            var errors = VineCompiler.CheckSyntax(vinecode);
            Console.WriteLine("Errors found: " + errors.Count);
            if (errors.Count > 0) {
                Console.WriteLine(ParserErrorFormatter.Format(errors));
            }
            return errors;
        }
    }
}
