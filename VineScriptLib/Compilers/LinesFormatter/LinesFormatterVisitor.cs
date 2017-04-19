using System;
using System.Collections.Generic;
using Antlr4.Runtime.Misc;

namespace VineScriptLib.Compilers.LinesFormatter
{
    /// <summary>
    /// Visitor for LinesFormatter grammar. Removes unwanted new lines from blocks of code, but tries to keep everything else formatted.
    /// </summary>
    class LinesFormatterVisitor : LinesFormatterBaseVisitor<string> 
    {
        public string output { get; private set; }
        public HashSet<int> linesToKeep = new HashSet<int>();

        public void printOutput()
        {
            Console.WriteLine("### LINES FORMATTER OUTPUT: ###");
            Console.WriteLine(output);
            Console.WriteLine("### END ###");
        }

        public override string VisitInline(LinesFormatterParser.InlineContext context)
        {
            //Console.WriteLine("Inline " + context.GetText());
            string value = "";
            for (int i = 0; i < context.children.Count; i++) {
                value += Visit(context.children[i]);
            }
            return value;
        }

        public override string VisitVarPrint(LinesFormatterParser.VarPrintContext context)
        {
            //Console.WriteLine("VarPrint " + context.GetText());
            return context.GetText();
        }

        public override string VisitBlock(LinesFormatterParser.BlockContext context)
        {
            //Console.WriteLine("Block " + context.GetText());
            return context.GetText();//.Replace("\r", "").Replace("\n", "");
        }

        public override string VisitText(LinesFormatterParser.TextContext context)
        {
            //Console.WriteLine("Text " + context.GetText());
            return context.GetText();
        }

        public override string VisitConsumeLn(LinesFormatterParser.ConsumeLnContext context)
        {
            //VisitChildren(context);
            //return null;
            string value = "";
            for (int i = 0; i < context.children.Count; i++) {
                value += Visit(context.children[i]);
            }
            //Console.WriteLine("ConsumeLn " + value);
            output += value;
            return value;
        }

        public override string VisitPrintLn(LinesFormatterParser.PrintLnContext context)
        {
            linesToKeep.Add(context.start.Line - 1);
            //VisitChildren(context);
            //return null;
            string value = "";
            for (int i = 0; i < context.children.Count; i++) {
                value += Visit(context.children[i]);
            }
            //Console.WriteLine("PrintLn " + value);
            output += value + Environment.NewLine;
            return value + Environment.NewLine;
        }
    }
    
}