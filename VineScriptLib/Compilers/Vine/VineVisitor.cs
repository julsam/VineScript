using System;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System.Collections.Generic;
using VineScriptLib.Core;
using System.Globalization;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace VineScriptLib.Compilers.Vine
{
    internal class VineRuntimeException : Exception
    {
        ParserRuleContext runtimeContext;

        public VineRuntimeException(string msg, ParserRuleContext ctx)
            : base(ExceptionUtil.UnderlineError(typeof(VineRuntimeException), msg, ctx))
        {
            runtimeContext = ctx;
        }

        public VineRuntimeException(string msg, Exception innerException)
            : base(msg, innerException) { }
    }

    class VineVisitor : VineParserBaseVisitor<VineValue> 
    {
        private VineStory story;
        public string output { get; private set; }

        private ParserRuleContext lastEnteredContext;

        public void printOutput()
        {
            Console.WriteLine("### EVALUATE OUTPUT: ###");
            if (output.Length > 0) {
                Console.WriteLine(output);
            }
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

            try {
                return VisitChildren(context);
            }
            catch (VineRuntimeException e) {
                // Rethrow the exception without modifications
                ExceptionDispatchInfo.Capture(e).Throw();
                throw;
            }
            catch (Exception e) {
                // Reformat the error message
                string formatted = ExceptionUtil.UnderlineError(
                    e.GetType(),
                    e.Message,
                    lastEnteredContext
                );
                // Rethrow the same exception type with the formatted message
                throw (Exception)Activator.CreateInstance(e.GetType(), formatted);
            }
            // TODO: shouldn't reformat message whith underline when the error
            // is happening in a called c# function?
        }

        public override VineValue VisitText(VineParser.TextContext context)
        {
            lastEnteredContext = context;
            string value = context.TXT().GetText();
            Console.WriteLine("> TEXT:\n" + value);
            output += value;
            return null;
        }

        public override VineValue VisitPrintBlockLn(VineParser.PrintBlockLnContext context)
        {
            lastEnteredContext = context;
            // Force a new line at the end
            VisitChildren(context);
            output += Environment.NewLine;
            return null;
        }

        public override VineValue VisitDisplay(VineParser.DisplayContext context)
        {
            lastEnteredContext = context;
            VineValue value = Visit(context.expr());
            Console.WriteLine("> VAR: " + context.expr().GetText() + " = " + value);

            // Get every lines in an array
            string[] outputLines = value.AsString.Split('\n');
            for (int i = 0; i < outputLines.Length; i++) {
                //  add the line to the output
                output += outputLines[i];

                // if not the last line
                if (i < outputLines.Length - 1) {
                    // control character '\u000B' that replace '\n' 
                    // and will be used in LineFormatter to distinguish
                    // between line returns that are in the source code
                    // and line returns added by displaying the return value
                    // of a function containing '\n'
                    output += '\u000B';
                }
            }

            // value does not contain '\u000B', it shouldn't be needed
            return value;
        }

        #region Commands

        public override VineValue VisitAssignStmt(VineParser.AssignStmtContext context)
        {
            // '{%' 'set' ID 'to' expr '%}'
            lastEnteredContext = context;
            var variable = Visit(context.variable());
            string id = variable.name;
            VineValue value = Visit(context.expr());

            if (story.vars.ContainsKey(id)) { 
                Console.WriteLine(string.Format(
                    "[!!] Warning, the variable '{0}' is already defined!"
                    + " Its value '{1}' will be overridden.", id, value
                ));
            }

            if (context.sequenceAccess() != null && context.sequenceAccess().Count() > 0)
            {
                // Debug
                Console.Write("STMT SET " + id);
                for (int i = 0; i < context.sequenceAccess().Count(); i++) {
                    Console.Write(context.sequenceAccess(i).GetText());
                }
                Console.WriteLine(" TO " + value);
                
                SetValueInSequence(story.vars[id], context.sequenceAccess(), value);
            }
            else
            {
                Console.WriteLine("STMT SET " + id + " TO " + value);
                story.vars[id] = value;
            }

            return value;
        }

        public override VineValue VisitFuncCall(VineParser.FuncCallContext context)
        {
            // ID '(' expressionList? ')'
            lastEnteredContext = context;
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
            lastEnteredContext = context;
            Console.WriteLine("CONTROL STATEMENT");
            bool ifvalue = Visit(context.ifStmt()).AsBool;
            if (!ifvalue) {
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
            lastEnteredContext = context;
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
            lastEnteredContext = context;
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
            lastEnteredContext = context;
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
            lastEnteredContext = context;
            Console.WriteLine("UNARY EXPR " + context.GetText());
            VineValue left = Visit(context.expr());
            return context.op.Type == VineParser.MINUS ? -left : !left;
        }

        public override VineValue VisitPowExpr(VineParser.PowExprContext context)
        {
            lastEnteredContext = context;
            Console.WriteLine("POW EXPR " + context.GetText());
            VineValue left = Visit(context.left);
            VineValue right = Visit(context.right);
            return (left ^ right);
        }

        public override VineValue VisitMulDivModExpr(VineParser.MulDivModExprContext context)
        {
            lastEnteredContext = context;
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
                throw new VineRuntimeException("Unknown operator", context);
            }
        }

        public override VineValue VisitAddSubExpr(VineParser.AddSubExprContext context)
        {
            lastEnteredContext = context;
            Console.WriteLine("Expr AddSub " + context.GetText());
            VineValue left = Visit(context.left);
            VineValue right = Visit(context.right);
            return context.op.Type == VineParser.ADD
                ? left + right
                : left - right;
        }

        public override VineValue VisitEqualityExpr(VineParser.EqualityExprContext context)
        {
            lastEnteredContext = context;
            Console.WriteLine("Expr Equality " + context.GetText());
            bool value = false;
            VineValue left = Visit(context.left);
            VineValue right = Visit(context.right);
            if (context.op.Type == VineParser.EQ) {
                value = (left == right);
            } else if (context.op.Type == VineParser.NEQ) {
                value = (left != right);
            } else {
                throw new VineRuntimeException("Unknown operator", context);
            }
            return value;
        }

        public override VineValue VisitRelationalExpr(VineParser.RelationalExprContext context)
        {
            lastEnteredContext = context;
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
                throw new VineRuntimeException("Unknown operator", context);
            }
            return value;
        }

        public override VineValue VisitAndExpr(VineParser.AndExprContext context)
        {
            lastEnteredContext = context;
            Console.WriteLine("AND EXPR " + context.GetText());
            VineValue left = Visit(context.left);
            // Short-circuit evaluation (minimal evaluation)
            if (left.AsBool == false) {
                // stop here, 'left' is false, we don't need to check 'right'
                return false;
            }
            VineValue right = Visit(context.right);
            return (left.AsBool && right.AsBool);
        }

        public override VineValue VisitOrExpr(VineParser.OrExprContext context)
        {
            lastEnteredContext = context;
            Console.WriteLine("OR EXPR " + context.GetText());
            VineValue left = Visit(context.left);
            // Short-circuit evaluation (minimal evaluation)
            if (left.AsBool == true) {
                // stop here, 'left' is true, we don't need to check 'right'
                return true;
            }
            // 'left' is false, we need to check 'right'
            VineValue right = Visit(context.right);
            return right.AsBool;
        }

        public override VineValue VisitParensExpr(VineParser.ParensExprContext context)
        {
            lastEnteredContext = context;
            Console.WriteLine("PARENS EXPR " + context.GetText());
            VineValue value = Visit(context.expr());
            return value;
        }

        public override VineValue VisitVarExpr(VineParser.VarExprContext context)
        {
            lastEnteredContext = context;
            Console.WriteLine("VAR EXPR " + context.GetText());

            VineValue variable = Visit(context.variable());
            string name = variable.name;
            VineValue value = null;

            if (context.sequenceAccess() != null && context.sequenceAccess().Count() > 0) {
                value = GetValueInSequence(variable, context.sequenceAccess());
            } else {
                value = variable.Clone();
            }

            value.name = name;
            return value;
        }
        
        #endregion Expr

        #region Atom

        public override VineValue VisitIntAtom(VineParser.IntAtomContext context)
        {
            lastEnteredContext = context;
            Console.WriteLine("ATOM INT " + context.INT().GetText());
            return int.Parse(context.INT().GetText());
        }

        public override VineValue VisitFloatAtom(VineParser.FloatAtomContext context)
        {
            lastEnteredContext = context;
            Console.WriteLine("ATOM FLOAT " + context.FLOAT().GetText());
            return double.Parse(context.FLOAT().GetText(), CultureInfo.InvariantCulture);
        }

        public override VineValue VisitBoolAtom(VineParser.BoolAtomContext context)
        {
            lastEnteredContext = context;
            Console.WriteLine("ATOM BOOL " + context.GetText());
            return bool.Parse(context.GetText());
        }

        public override VineValue VisitNullAtom(VineParser.NullAtomContext context)
        {
            lastEnteredContext = context;
            Console.WriteLine("NULL STRING " + context.NULL().GetText());
            return VineValue.NULL;
        }

        #endregion Atom

        public override VineValue VisitStringLiteral(VineParser.StringLiteralContext context)
        {
            lastEnteredContext = context;
            Console.WriteLine("ATOM STRING " + context.STRING().GetText());
            string str = context.STRING().GetText();
            str = str.Substring(1, str.Length - 2);
            // TODO: remove '\' in '\\n', '\\t' etc
            return str;
        }

        public override VineValue VisitVariable(VineParser.VariableContext context)
        {
            lastEnteredContext = context;
            string id = context.GetText();
            
            // Remove optional '$'
            if (context.GetToken(VineLexer.VAR_PREFIX, 0) != null) {
                id = id.Remove(0, 1);
            }
            VineValue value = story.vars.ContainsKey(id) ? story.vars[id] : VineValue.NULL;
            value.name = id;
            Console.WriteLine("SimpleVar: " + id + " = \"" + value + "\"");
            return value;
        }
        
        public override VineValue VisitNewArray(VineParser.NewArrayContext context)
        {
            // '[' expressionList? ']'
            lastEnteredContext = context;
            Console.WriteLine("> NEW ARRAY: ");

            VineValue vineArray = VineValue.newArray;
            if (context.expressionList() != null)
            {
                for (int i = 0; i < context.expressionList().expr().Length; i++)
                {
                    var el = Visit(context.expressionList().expr(i));
                    vineArray.AsArray.Add(el);
                }
            }
           
            return vineArray;
        }

        public override VineValue VisitNewDict(VineParser.NewDictContext context)
        {
            // '{' keyValueList? '}'
            lastEnteredContext = context;
            Console.WriteLine("> NEW DICT: ");

            VineValue vineDict = VineValue.newDict;
            if (context.keyValueList() != null && context.keyValueList().keyValue() != null)
            {
                for (int i = 0; i < context.keyValueList().keyValue().Length; i++)
                {
                    var key = Visit(context.keyValueList().keyValue(i).stringLiteral());
                    var value = Visit(context.keyValueList().keyValue(i).expr());
                    vineDict.AsDict.Add(key.AsString, value);
                }
            }
           
            return vineDict;
        }

        private void Visit(TerminalNodeImpl node)
        {
            Console.WriteLine(" Visit Symbol={0}", node.Symbol.Text);
        }

        /// <summary>
        /// Eval expr mode.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override VineValue VisitEvalExprMode(VineParser.EvalExprModeContext context)
        {
            lastEnteredContext = context;
            VineValue value = Visit(context.expr());
            output += value.AsString;
            return value;
        }

        private VineValue GetValueInSequence(VineValue startingVar, 
            VineParser.SequenceAccessContext[] sequences)
        {
            // Trying to get to the last sequence, which is n-1 in the list.
            // It should be either an array, a dictionnary or a string.
            // E.g.: myvar[3] ["abc"] [0] [42]
            //       |        |       |   |   
            //       n-3      n-2     |   n   
            //                        n-1     
            // ResolveSequencesAccess returns n-1:
            VineValue lastSequence = ResolveSequencesAccess(startingVar, sequences);

            // The last sequence (n-1) is now into lastSequence,
            // we can access n using the last index

            // Get the last index
            var lastIndex = Visit(sequences[sequences.Count() - 1].expr());

            // Finally get the content of [n], the value
            VineValue value = null;
            if (lastSequence.IsString) {
                value = lastSequence.AsString.Substring(lastIndex.AsInt, 1);
            } else if (lastSequence.IsArray) {
                value = lastSequence[lastIndex.AsInt];
            } else if (lastSequence.IsString) {
                value = lastSequence[lastIndex.AsString];
            } else {
                throw new VineRuntimeException(
                    "Can't access element with [] because the variable '"
                    + lastSequence.name + "' is neither an array nor a dictionnary "
                    + " nor a string",
                    sequences[0]
                );
            }
            return value;
        }

        private void SetValueInSequence(VineValue startingVar, 
            VineParser.SequenceAccessContext[] sequences, VineValue value)
        {
            if (startingVar.IsString) {
                // not allowed: string[0] = 'a'
                throw new VineRuntimeException(
                    "Strings don't support item assignment", sequences[0]
                );
            }
            
            // Trying to get to the last sequence, which is n-1 in the list.
            // It should be either an array, a dictionnary or a string.
            // E.g.: myvar[3] ["abc"] [0] [42]
            //       |        |       |   |   
            //       n-3      n-2     |   n   
            //                        n-1     
            // ResolveSequencesAccess returns n-1:
            VineValue lastSequence = ResolveSequencesAccess(startingVar, sequences);

            // The last sequence (n-1) is now into lastSequence,
            // we can access n using the last index

            // Get the last index
            var lastIndex = Visit(sequences[sequences.Count() - 1].expr());

            // Finally set the content of [n]
            if (lastSequence.IsArray) {
                lastSequence[lastIndex.AsInt] = value;
            } else if (lastSequence.IsDict) {
                lastSequence[lastIndex.AsString] = value;
            } else {
                throw new VineRuntimeException(
                    "Can't access element with [] because the variable '"
                    + lastSequence.name + "' is neither an array nor a dictionnary "
                    + " nor a string",
                    sequences[0]
                );
            }
        }

        /// <summary>
        /// Get to the last sequence in a list of sequences, which is n-1 in the list.
        /// It should be either an array, a dictionnary or a string.
        /// </summary>
        /// <param name="startingVar">The left-most variable in the sequence access.</param>
        /// <param name="sequences">Array of sequences (array, dictionnary or string)</param>
        /// <returns>Return the [n-1] sequence</returns>
        private VineValue ResolveSequencesAccess(VineValue startingVar, 
            VineParser.SequenceAccessContext[] sequences)
        {
            VineValue lastSequence = startingVar;

            // Trying to get to the n-1 element, aka the last sequence,
            // which should be an array, a dictionnary or a string.
            // E.g.: myvar[3] ["abc"] [0] [42]
            //       |        |       |   |   
            //       n-3      n-2     |   n   
            //                        n-1     
            for (int i = 0; i < sequences.Count() - 1; i++)
            {
                var indexExpr = Visit(sequences[i].expr());
                if (lastSequence.IsArray || lastSequence.IsString) {
                    if (!indexExpr.IsInt) {
                        throw new VineRuntimeException(
                            "An array element can only be accessed by an integer",
                            sequences[0]
                        );
                    }
                    try {
                        if (lastSequence.IsArray) {
                            lastSequence = lastSequence[indexExpr.AsInt];
                        } else {
                            // it's a string
                            // make sure we are in the n-1 sequence,
                            // or else it's an error
                            //if (i != sequences.Count() - 2) {
                            //    throw new Exception(
                            //        "Trying to access a string using [] too many times!"
                            //    );
                            //}
                            break;
                        }
                    } catch (ArgumentOutOfRangeException) {
                        throw new VineRuntimeException(
                            "Index was out of range. Must be non-negative and"
                            + " less than the size of the array.",
                            sequences[0]
                        );
                    }
                } else if (lastSequence.IsDict) {
                    // no check on the expression type? not for now, 
                    // as anything can be converted to a string, we'll see
                    // if it's a problem or not.
                    lastSequence = lastSequence[indexExpr.AsString];
                } else {
                    throw new VineRuntimeException(
                        "Can't access element with [] because the variable '"
                        + lastSequence.name + "' is neither an array nor a dictionnary "
                        + " nor a string",
                        sequences[0]
                    );
                }
            }
            return lastSequence;
        }
    }
    
}