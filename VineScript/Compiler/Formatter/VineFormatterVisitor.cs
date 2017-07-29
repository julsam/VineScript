using System;
using System.Collections.Generic;
using Antlr4.Runtime.Misc;

namespace VineScriptLib.Compiler.Formatter
{
    class VineFormatterVisitor : VineFormatterBaseVisitor<string> 
    {
        public string output { get; private set; } = "";
        
        public void printOutput()
        {
            Console.WriteLine("### FORMATTER OUTPUT: ###");
            Console.WriteLine(output);
            Console.WriteLine("### END ###");
        }

        public override string VisitContainsText(VineFormatterParser.ContainsTextContext context)
        {
            //Console.WriteLine("ContainsText " + context.GetText());
            string text = "";
            for (int i = 0; i < context.children.Count; i++) {
                text += Visit(context.children[i]);
            }
            return text;
        }

        public override string VisitText(VineFormatterParser.TextContext context)
        {
            //Console.WriteLine("Text " + context.GetText());
            string text = "";
            for (int i = 0; i < context.children.Count; i++) {
                text += context.children[i].GetText();
            }
            return text;
        }

        /// <summary>
        /// Print line without line return.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override string VisitConsumeLn(VineFormatterParser.ConsumeLnContext context)
        {
            string text = "";
            for (int i = 0; i < context.children.Count; i++) {
                text += Visit(context.children[i]);
            }
            output += text;
            return null;
        }

        /// <summary>
        /// Print line with line return.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override string VisitPrintLn(VineFormatterParser.PrintLnContext context)
        {
            string text = "";
            for (int i = 0; i < context.children.Count; i++) {
                text += Visit(context.children[i]);
            }
            output += text + Environment.NewLine;
            return null;
        }
    }
}
