using System;

namespace VineScriptLib.Compiler
{
    /// <summary>
    /// Visitor for WhiteSpace grammar. Not used in Vine, it's here to help me format inputs.
    /// </summary>
    class WhiteSpaceVisitor : WhiteSpaceBaseVisitor<string> 
    {
        public string output { get; private set; }

        public void printOutput()
        {
            Console.WriteLine("### OUTPUT: ###");
            Console.WriteLine(output);
            Console.WriteLine("### END ###");
        }

        public override string VisitCompileUnit(WhiteSpaceParser.CompileUnitContext context)
        {
            Console.WriteLine("WhiteSpaceVisitor VisitCompileUnit");
            return VisitChildren(context);
        }

        public override string VisitBlock(WhiteSpaceParser.BlockContext context)
        {
            //Console.WriteLine("Rule: " + context.RuleIndex);
            return VisitChildren(context);
        }

        public override string VisitCode(WhiteSpaceParser.CodeContext context)
        {
            //output += context.GetText();
            return context.GetText();
        }

        public override string VisitText(WhiteSpaceParser.TextContext context)
        {
            //output += context.GetText();
            return context.GetText();
        }

        public override string VisitPrint(WhiteSpaceParser.PrintContext context)
        {
            string value = "";
            value += Visit(context.text());
            output += value;
            return value;
        }

        public override string VisitPrintLn(WhiteSpaceParser.PrintLnContext context)
        {
            string value = "";
            for (int i = 0; i < context.text().Length; i++) {
                value += Visit(context.text(i));
            }
            output += value + Environment.NewLine;
            return value + Environment.NewLine;
        }
    }
    
}