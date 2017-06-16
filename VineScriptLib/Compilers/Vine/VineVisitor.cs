using System;
using System.Linq;
using Antlr4.Runtime.Tree;
using System.Collections.Generic;
using VineScriptLib.Core;
using System.Globalization;

namespace VineScriptLib.Compilers.Vine
{
    class VineVisitor : VineParserBaseVisitor<VineValue> 
    {
        private VineStory story;
        public string output { get; private set; }

        public void printOutput()
        {
            Console.WriteLine("### EVALUATE OUTPUT: ###");
            if (output.Length > 0)
                Console.WriteLine(output);
            Console.WriteLine("### END ###");
        }

        public VineVisitor(VineStory story)
        {
            this.story = story;
            this.output = "";
        }

        public override VineValue VisitPassage(VineParser.PassageContext context)
        {
            Console.WriteLine("VineVisitor VisitPassage");
            
            return VisitChildren(context);
        }

        //public override VineValue VisitBlock(VineParser.BlockContext context)
        //{
        //    return VisitChildren(context);
        //}

        public override VineValue VisitText(VineParser.TextContext context)
        {
            string value;
            //if (context.RuleIndex == VineParser.TXT) {
                value = context.TXT().GetText();
            //} else {
            //    value = "\n";//context.NL().GetText();
            //}
            Console.WriteLine("> TEXT:");
            Console.WriteLine(value);
            output += value;
            return null;
        }

        //public override VineValue VisitPrintBlock(VineParser.PrintBlockContext context)
        //{
        //    // Do nothing
        //    VisitChildren(context);
        //    return null;
        //}

        public override VineValue VisitPrintBlockLn(VineParser.PrintBlockLnContext context)
        {
            // Force a new line at the end
            VisitChildren(context);
            output += Environment.NewLine;
            return null;
        }

        //public override VineValue VisitPrintStmtWS(VineParser.PrintStmtWSContext context)
        //{
        //    VisitChildren(context);
        //    if (context.ws() != null)
        //        output += " ";
        //    return null;
        //}

        public override VineValue VisitOutputVariable(VineParser.OutputVariableContext context)
        {
            //var id = context.ID().GetText().Remove(0, 1); // Remove '$'
            object value = Visit(context.expr());
            Console.WriteLine("> VAR: " + context.expr().GetText() + " = " + value);
            output += value;
            //if (context.END_OUTPUT_WS() != null)
            //    output += " ";
            return value as VineValue;
        }

        #region Commands

        public override VineValue VisitAssignStmt(VineParser.AssignStmtContext context)
        {
            // '{%' 'set' ID 'to' expr '%}'
            var id = context.VAR().GetText().Remove(0, 1); // Remove '$'
            VineValue value = Visit(context.expr());
            Console.WriteLine("STMT SET " + id + " TO " + value);
            if (story.vars.ContainsKey(id)) { 
                Console.WriteLine(string.Format("[!!] Warning, the variable '{0}' is already defined! Its value '{1}' will be overridden.", id, value));
            }
            story.vars[id] = value;
            return value as VineValue;
        }

        public override VineValue VisitFuncCall(VineParser.FuncCallContext context)
        {
            // ID '(' expressionList? ')'
            var funcName = context.ID().GetText();
            Console.WriteLine("> FUNCALL: " + funcName);
            List<object> list = new List<object>();
            if (context.expressionList() != null)
            {
                for (int i = 0; i < context.expressionList().expr().Length; i++)
                {
                    var el = Visit(context.expressionList().expr(i));
                    list.Add((object)el);
                }
            }
            VineValue value = story.CallFunction(funcName, list.ToArray());
           
            return value;
        }

        #endregion Commands

        #region Control Statements

        public override VineValue VisitControlStmt(VineParser.ControlStmtContext context)
        {
            Console.WriteLine("CONTROL STATEMENT");
            bool ifvalue = Visit(context.ifStmt()).AsBool;
            if (!ifvalue) {
                //for (int i = 0; i < context.stat().Count; i++) {
                //    Console.WriteLine(">>> " + context.stat(i).GetText());
                //    var value = Visit(context.stat(i));
                //    Console.WriteLine(">>> " + value);
                //    Console.WriteLine("\r\n-------------\r\n");
                //}
                bool elifvalue = false;
                for (int i = 0; i < context.elifStmt().Length; i++) {
                    elifvalue = Visit(context.elifStmt(i)).AsBool;
                    if (elifvalue) {
                        break;
                    }
                }
                if (!elifvalue && context.elseStmt() != null) {
                    Visit(context.elseStmt());
                }
            } 
            return ifvalue;
        }
        

	    public override VineValue VisitIfStmt(VineParser.IfStmtContext context)
        {
            Console.WriteLine("IF STATEMENT");
            bool ifvalue = Visit(context.expr()).AsBool;
            if (ifvalue) {
                for (int i = 0; i < context.block().Length; i++) {
                    Console.WriteLine(">>> " + context.block(i).GetText());
                    //var value = Visit(context.block(i));
                    //Console.WriteLine(">>> " + value);
                    Visit(context.block(i));
                    Console.WriteLine("\r\n-------------\r\n");
                }
            }
            return ifvalue;
        }
        
	    public override VineValue VisitElifStmt(VineParser.ElifStmtContext context)
        {
            Console.WriteLine("ELIF STATEMENT");
            bool elifvalue = Visit(context.expr()).AsBool;
            if (elifvalue) {
                for (int i = 0; i <  context.block().Length; i++) {
                    Console.WriteLine(">>> " + context.block(i).GetText());
                    //var value = Visit(context.block(i));
                    //Console.WriteLine(">>> " + value);
                    Visit(context.block(i));
                    Console.WriteLine("\r\n-------------\r\n");
                }
            }
            return elifvalue;
        }
        
	    public override VineValue VisitElseStmt(VineParser.ElseStmtContext context)
        {
            Console.WriteLine("ELSE STATEMENT");
            for (int i = 0; i <  context.block().Length; i++) {
                Console.WriteLine(">>> " + context.block(i).GetText());
                //object value = Visit(context.block(i));
                //Console.WriteLine(">>> " + value);
                Visit(context.block(i));
                Console.WriteLine("\r\n-------------\r\n");
            }
            return 0;
        }

        #endregion Control Statements

        #region Expr

        public override VineValue VisitUnaryExpr(VineParser.UnaryExprContext context)
        {
            Console.WriteLine("OR EXPR " + context.GetText());
            VineValue left = Visit(context.expr());
            return context.op.Type == VineParser.MINUS ? -left : !left;
        }

        public override VineValue VisitPowExpr(VineParser.PowExprContext context)
        {
            Console.WriteLine("POW EXPR " + context.GetText());
            VineValue left = Visit(context.left);
            VineValue right = Visit(context.right);
            return (left ^ right);
        }

        // expr op=('*' | '/'/* | '%'*/) expr		# mulDivExpr
        // expr op=('+'|'-') expr					# addSubExpr

        public override VineValue VisitMulDivModExpr(VineParser.MulDivModExprContext context)
        {
            Console.WriteLine("Expr MulDivMod " + context.GetText());
            VineValue left = Visit(context.left);
            VineValue right = Visit(context.right);
            if (context.op.Type == VineParser.MUL) {
                return left * right;
            } else if (context.op.Type == VineParser.DIV) {
                return left / right;
            } else if (context.op.Type == VineParser.MOD) {
                return left % right;
            } else {
                throw new Exception("Unknown operator");
            }
        }
        public override VineValue VisitAddSubExpr(VineParser.AddSubExprContext context)
        {
            Console.WriteLine("Expr AddSub " + context.GetText());
            VineValue left = Visit(context.left);
            VineValue right = Visit(context.right);
            return context.op.Type == VineParser.ADD
                ? left + right
                : left - right;
        }

        public override VineValue VisitEqualityExpr(VineParser.EqualityExprContext context)
        {
            Console.WriteLine("Expr Comparison " + context.GetText());
            bool value = false;
            VineValue left = Visit(context.left);
            VineValue right = Visit(context.right);
            if (context.op.Type == VineParser.EQ) {
                value = (left == right);
            } else if (context.op.Type == VineParser.NEQ) {
                value = (left != right);
            } else {
                throw new Exception("Unknown operator");
            }
            return value;
        }

        public override VineValue VisitRelationalExpr(VineParser.RelationalExprContext context)
        {
            Console.WriteLine("Expr Comparison " + context.GetText());
            bool value = false;
            VineValue left = Visit(context.left);
            VineValue right = Visit(context.right);
            if (context.op.Type == VineParser.LT) {
                value = (left < right);
            } else if (context.op.Type == VineParser.LTE) {
                value = (left <= right);
            } else if (context.op.Type == VineParser.GT) {
                value = (left > right);
            } else if (context.op.Type == VineParser.GTE) {
                value = (left >= right);
            } else {
                throw new Exception("Unknown operator");
            }
            return value;
        }

        public override VineValue VisitAndExpr(VineParser.AndExprContext context)
        {
            Console.WriteLine("AND EXPR " + context.GetText());
            VineValue left = Visit(context.left);
            VineValue right = Visit(context.right);
            return (left.AsBool && right.AsBool);
        }

        public override VineValue VisitOrExpr(VineParser.OrExprContext context)
        {
            Console.WriteLine("OR EXPR " + context.GetText());
            VineValue left = Visit(context.left);
            VineValue right = Visit(context.right);
            return (left.AsBool || right.AsBool);
        }

        public override VineValue VisitParensExpr(VineParser.ParensExprContext context)
        {
            Console.WriteLine("PARENS EXPR " + context.GetText());
            VineValue value = Visit(context.expr());
            return value;
        }
        
        #endregion Expr

        #region Atom

        public override VineValue VisitIntAtom(VineParser.IntAtomContext context)
        {
            Console.WriteLine("ATOM INT " + context.INT().GetText());
            return int.Parse(context.INT().GetText());
        }

        public override VineValue VisitFloatAtom(VineParser.FloatAtomContext context)
        {
            Console.WriteLine("ATOM FLOAT " + context.FLOAT().GetText());
            return double.Parse(context.FLOAT().GetText(), CultureInfo.InvariantCulture);
        }

        public override VineValue VisitBoolAtom(VineParser.BoolAtomContext context)
        {
            Console.WriteLine("ATOM BOOL " + context.GetText());
            return bool.Parse(context.GetText());
        }

        public override VineValue VisitVarAtom(VineParser.VarAtomContext context)
        {
            var id = context.VAR().GetText().Remove(0, 1); // Remove '$'
            object value = story.vars.ContainsKey(id) ? story.vars[id] : VineValue.NULL;
            Console.WriteLine("VariableValue: " + id + " = \"" + value + "\"");
            return value as VineValue;
        }

        public override VineValue VisitStringAtom(VineParser.StringAtomContext context)
        {
            Console.WriteLine("ATOM STRING " + context.STRING().GetText());
            string str = context.STRING().GetText();
            str = str.Substring(1, str.Length - 2);
            return str;
        }

        public override VineValue VisitNullAtom(VineParser.NullAtomContext context)
        {
            Console.WriteLine("NULL STRING " + context.NULL().GetText());
            return VineValue.NULL;
        }

        #endregion Atom

        private void Visit(TerminalNodeImpl node)
        {
            Console.WriteLine(" Visit Symbol={0}", node.Symbol.Text);
        }
        
        // test
        //public override VineValue VisitAaa(VineParser.AaaContext context)
        //{
        //    Console.WriteLine("Output Variable " + context.outputVariable().GetText());
        //    VineValue value = Visit(context.outputVariable());

        //    return value;
        //}
    }
    
}