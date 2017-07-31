using System;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System.Collections.Generic;
using VineScript.Core;
using System.Globalization;
using System.Reflection;
using System.Runtime.ExceptionServices;

namespace VineScript.Compiler
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

    class VineVisitor : VineParserBaseVisitor<VineVar> 
    {
        public PassageResult passageResult { get; private set; }
        private VineStory story;

        private ParserRuleContext lastEnteredContext;

        public void printOutput()
        {
            Console.WriteLine("### EVALUATE OUTPUT: ###");
            if (passageResult.text.Length > 0) {
                Console.WriteLine(passageResult.text);
            }
            Console.WriteLine("### END ###");
        }
        
        private void AddToPassageResult(string text)
        {
            passageResult.text += text;
        }

        public VineVisitor(VineStory story)
        {
            this.story = story;
            passageResult = new PassageResult();
        }

        /// <summary>
        /// Expression Evaluation Mode. Different from normal mode, it
        /// only accepts simples expressions (no text, no comments, no
        /// code markups!)
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public override VineVar VisitEvalExprMode(VineParser.EvalExprModeContext context)
        {
            lastEnteredContext = context;

            VineVar value = Visit(context.expr());
            AddToPassageResult(value.AsString);
            return value;
        }

        public override VineVar VisitPassage(VineParser.PassageContext context)
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

        public override VineVar VisitDirectOutput(VineParser.DirectOutputContext context)
        {
            lastEnteredContext = context;
            VisitChildren(context);
                AddToPassageResult(context.GetText());

        public override VineVar VisitLink(VineParser.LinkContext context)
        {
            // '[[' title=linkContent+ '|' passageName=linkContent+ ']]'
            lastEnteredContext = context;

            // Link Title
            string title = Visit(context.title).AsString.Trim(new char[] { '\t', ' ' });
            title = Util.UnescapeLinkContent(title);

            // Passage Name
            string passageName = title;
            if (context.passageName != null) { 
                passageName = Visit(context.passageName).AsString.Trim(new char[] { '\t', ' ' });
                passageName = Util.UnescapeLinkContent(passageName);
            }

            // Check for empty title/link like [[mytitle| ]] or [[ |mylink]]
            if (string.IsNullOrWhiteSpace(title)) {
                throw new VineRuntimeException("The title of a link can't be empty!", context);
            } else if (string.IsNullOrWhiteSpace(passageName)) {
                throw new VineRuntimeException("A link can't be empty!", context);
            }

            // DEBUG: print in passage
            //AddToPassageResult("title: " + title + ", link: " + passageName);

            // Add it back to the output passage as a statement and will be treated
            // accordingly by the VineFormatter.
            AddToPassageResult("<< " + title + " | " + passageName + " >>");

            passageResult.links.Add(
                new PassageLink(title, passageName, passageResult.links.Count)
            );

            return null;
        }

        public override VineVar VisitLinkContent(VineParser.LinkContentContext context)
        {
            lastEnteredContext = context;
            string result = "";
            foreach (var item in context.LINK_TEXT()) {
                result += item.GetText();
            }
            return result;
        }
            return null;
        }

        public override VineVar VisitDisplay(VineParser.DisplayContext context)
        {
            lastEnteredContext = context;
            VineVar value = Visit(context.expr());
            Console.WriteLine("> DISPLAY: " + context.expr().GetText() + " = " + value);

            // marks the start of the output of the display command
            AddToPassageResult("\u001E");
            // Get every lines in an array
            string[] outputLines = value.AsString.Split('\n');
            for (int i = 0; i < outputLines.Length; i++) {
                //  add the line to the output
                AddToPassageResult(outputLines[i]);

                // if not the last line
                if (i < outputLines.Length - 1) {
                    // control character '\u000B' that replace '\n' 
                    // and will be used in LineFormatter to distinguish
                    // between line returns that are in the source code
                    // and line returns added by displaying the return value
                    // of a function containing '\n'
                    AddToPassageResult("\u000B");
                }
            }
            // marks the end of the output of the display command
            AddToPassageResult("\u001F");
            
            return null;
        }

        #region Commands

        public override VineVar VisitAssign(VineParser.AssignContext context)
        {
            // '<<' 'set' ID 'to' expr '>>'
            lastEnteredContext = context;
            AddToPassageResult("<< set >>");

            var variable = Visit(context.variable());
            string id = variable.name;
            VineVar value = Visit(context.expr());

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
                
                SetValueInSequence(story.vars[id], context.sequenceAccess(), 
                    value, context.op.Type);
            }
            else
            {
                Console.WriteLine("STMT SET " + id + " TO " + value);
                if (context.op.Type == VineLexer.ASSIGN || context.op.Type == VineLexer.TO) {
                    story.vars[id] = AssignOp(context.op.Type, null, value);
                } else {
                    story.vars[id] = AssignOp(context.op.Type, story.vars[id], value);
                }
            }

            return value;
        }

        public override VineVar VisitFuncCall(VineParser.FuncCallContext context)
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
            VineVar value = story.CallFunction(funcName, list.ToArray());
           
            return value;
        }

        #endregion Commands

        #region Control Statements

        public override VineVar VisitIfCtrlStmt(VineParser.IfCtrlStmtContext context)
        {
            lastEnteredContext = context;
            Console.WriteLine("IF CONTROL STATEMENT");
            AddToPassageResult("<< if >>");

            bool ifvalue = Visit(context.ifStmt()).AsBool;
            if (!ifvalue) {
                bool elifvalue = false;
                for (int i = 0; i < context.elifStmt().Length; i++) {
                    AddToPassageResult("<< elif >>");
                    elifvalue = Visit(context.elifStmt(i)).AsBool;
                    if (elifvalue) {
                        break;
                    }
                }
                if (!elifvalue && context.elseStmt() != null) {
                    AddToPassageResult("<< else >>");
                    Visit(context.elseStmt());
                }
            }
            AddToPassageResult("<< endif >>");
            return ifvalue;
        }

        public override VineVar VisitIfStmt(VineParser.IfStmtContext context)
        {
            lastEnteredContext = context;
            Console.WriteLine("IF STATEMENT");
            VineVar ifvalue = Visit(context.expr());
            if (ifvalue.AsBool) {
                for (int i = 0; i < context.block().Length; i++) {
                    Console.WriteLine(">>> " + Util.Escape(context.block(i).GetText()));
                    Visit(context.block(i));
                    Console.WriteLine("\r\n-------------\r\n");
                }
            }
            return ifvalue;
        }
        
        public override VineVar VisitElifStmt(VineParser.ElifStmtContext context)
        {
            lastEnteredContext = context;
            Console.WriteLine("ELIF STATEMENT");
            VineVar elifvalue = Visit(context.expr());
            if (elifvalue.AsBool) {
                for (int i = 0; i <  context.block().Length; i++) {
                    Console.WriteLine(">>> " + Util.Escape(context.block(i).GetText()));
                    Visit(context.block(i));
                    Console.WriteLine("\r\n-------------\r\n");
                }
            }
            return elifvalue;
        }
        
        public override VineVar VisitElseStmt(VineParser.ElseStmtContext context)
        {
            lastEnteredContext = context;
            Console.WriteLine("ELSE STATEMENT");
            for (int i = 0; i <  context.block().Length; i++) {
                Console.WriteLine(">>> " + Util.Escape(context.block(i).GetText()));
                Visit(context.block(i));
                Console.WriteLine("\r\n-------------\r\n");
            }
            return null;
        }

        public override VineVar VisitForCtrlStmt(VineParser.ForCtrlStmtContext context)
        {
            lastEnteredContext = context;
            Console.WriteLine("FOR CONTROL STATEMENT");
            AddToPassageResult("<< for >>");
            VineVar forvalue = Visit(context.forStmt());
            AddToPassageResult("<< endfor >>");
            return null;
        }

        public override VineVar VisitForStmt(VineParser.ForStmtContext context)
        {
            lastEnteredContext = context;
            Console.WriteLine("FOR STATEMENT");
            
            // TODO scope with temp vars
            VineVar iterator = null;
            if (context.expr() != null) {
                iterator = Visit(context.expr());
            } else {
                iterator = Visit(context.interval());
            }

            if (iterator.IsArray || iterator.IsDict || iterator.IsString) {
                var tempForVar = Visit(context.variable());
                string id = tempForVar.name;
                foreach (var item in iterator) {
                    if (iterator.IsDict) {
                        story.vars[id] =
                            ((KeyValuePair<string, VineVar>)item).Value;
                    } else {
                        story.vars[id] = item as VineVar;
                    }
                    for (int i = 0; i < context.block().Length; i++) {
                        Console.WriteLine(">>> " + Util.Escape(context.block(i).GetText()));
                        Visit(context.block(i));
                        Console.WriteLine("\r\n-------------\r\n");
                    }
                }
            } else {
                throw new VineRuntimeException(
                    "'" + iterator.type + "' is not iterable", context
                );
            }

            return null;
        }

        public override VineVar VisitInterval(VineParser.IntervalContext context)
        {
            lastEnteredContext = context;
            Console.WriteLine("> INTERVAL");
            VineVar left = Visit(context.left);
            VineVar right = Visit(context.right);
            if (!left.IsInt || !right.IsInt) {
                throw new VineRuntimeException("Intervals must be integers values", context);
            }
            var interval = new VineVar(VineVarUtils.ConvertList(
                VineScript.Utils.Range(left.AsInt, right.AsInt)
            ));
            return interval;
        }

        #endregion Control Statements

        #region Expr

        public override VineVar VisitUnaryExpr(VineParser.UnaryExprContext context)
        {
            lastEnteredContext = context;
            Console.WriteLine("UNARY EXPR " + context.GetText());
            VineVar left = Visit(context.expr());
            return context.op.Type == VineParser.MINUS ? -left : !left;
        }

        public override VineVar VisitPowExpr(VineParser.PowExprContext context)
        {
            lastEnteredContext = context;
            Console.WriteLine("POW EXPR " + context.GetText());
            VineVar left = Visit(context.left);
            VineVar right = Visit(context.right);
            return (left ^ right);
        }

        public override VineVar VisitMulDivModExpr(VineParser.MulDivModExprContext context)
        {
            lastEnteredContext = context;
            Console.WriteLine("Expr MulDivMod " + context.GetText());
            VineVar left = Visit(context.left);
            VineVar right = Visit(context.right);
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

        public override VineVar VisitAddSubExpr(VineParser.AddSubExprContext context)
        {
            lastEnteredContext = context;
            Console.WriteLine("Expr AddSub " + context.GetText());
            VineVar left = Visit(context.left);
            VineVar right = Visit(context.right);
            return context.op.Type == VineParser.ADD
                ? left + right
                : left - right;
        }

        public override VineVar VisitEqualityExpr(VineParser.EqualityExprContext context)
        {
            lastEnteredContext = context;
            Console.WriteLine("Expr Equality " + context.GetText());
            bool value = false;
            VineVar left = Visit(context.left);
            VineVar right = Visit(context.right);
            if (context.op.Type == VineParser.EQ) {
                value = (left == right);
            } else if (context.op.Type == VineParser.NEQ) {
                value = (left != right);
            } else {
                throw new VineRuntimeException("Unknown operator", context);
            }
            return value;
        }

        public override VineVar VisitRelationalExpr(VineParser.RelationalExprContext context)
        {
            lastEnteredContext = context;
            Console.WriteLine("Expr Comparison " + context.GetText());
            bool value = false;
            VineVar left = Visit(context.left);
            VineVar right = Visit(context.right);
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

        public override VineVar VisitAndExpr(VineParser.AndExprContext context)
        {
            lastEnteredContext = context;
            Console.WriteLine("AND EXPR " + context.GetText());
            VineVar left = Visit(context.left);
            // Short-circuit evaluation (minimal evaluation)
            if (left.AsBool == false) {
                // stop here, 'left' is false, we don't need to check 'right'
                return false;
            }
            VineVar right = Visit(context.right);
            return (left.AsBool && right.AsBool);
        }

        public override VineVar VisitOrExpr(VineParser.OrExprContext context)
        {
            lastEnteredContext = context;
            Console.WriteLine("OR EXPR " + context.GetText());
            VineVar left = Visit(context.left);
            // Short-circuit evaluation (minimal evaluation)
            if (left.AsBool == true) {
                // stop here, 'left' is true, we don't need to check 'right'
                return true;
            }
            // 'left' is false, we need to check 'right'
            VineVar right = Visit(context.right);
            return right.AsBool;
        }

        public override VineVar VisitParensExpr(VineParser.ParensExprContext context)
        {
            lastEnteredContext = context;
            Console.WriteLine("PARENS EXPR " + context.GetText());
            VineVar value = Visit(context.expr());
            return value;
        }

        public override VineVar VisitVarExpr(VineParser.VarExprContext context)
        {
            lastEnteredContext = context;
            Console.WriteLine("VAR EXPR " + context.GetText());

            VineVar variable = Visit(context.variable());
            string name = variable.name;
            VineVar value = null;

            if (context.sequenceAccess() != null && context.sequenceAccess().Count() > 0) {
                value = GetValueInSequence(variable, context.sequenceAccess());
            } else {
                value = variable;
            }

            value.name = name;
            return value;
        }
        
        #endregion Expr

        #region Atom

        public override VineVar VisitIntAtom(VineParser.IntAtomContext context)
        {
            lastEnteredContext = context;
            Console.WriteLine("ATOM INT " + context.INT().GetText());
            return int.Parse(context.INT().GetText());
        }

        public override VineVar VisitFloatAtom(VineParser.FloatAtomContext context)
        {
            lastEnteredContext = context;
            Console.WriteLine("ATOM FLOAT " + context.FLOAT().GetText());
            return double.Parse(context.FLOAT().GetText(), CultureInfo.InvariantCulture);
        }

        public override VineVar VisitBoolAtom(VineParser.BoolAtomContext context)
        {
            lastEnteredContext = context;
            Console.WriteLine("ATOM BOOL " + context.GetText());
            return bool.Parse(context.GetText());
        }

        public override VineVar VisitNullAtom(VineParser.NullAtomContext context)
        {
            lastEnteredContext = context;
            Console.WriteLine("NULL STRING " + context.NULL().GetText());
            return VineVar.NULL;
        }

        #endregion Atom

        public override VineVar VisitStringLiteral(VineParser.StringLiteralContext context)
        {
            lastEnteredContext = context;
            Console.WriteLine("ATOM STRING " + context.STRING().GetText());

            // Get the string literal
            string str = context.STRING().GetText();
            
            // Removes starting and ending '"'
            str = str.Substring(1, str.Length - 2);
            
            // Unescape special chars starting with '\'. E.g. \n, \t, etc
            str = Compiler.Util.UnescapeAuthorizedChars(str);

            return str;
        }

        public override VineVar VisitVariable(VineParser.VariableContext context)
        {
            lastEnteredContext = context;
            string id = context.GetText();
            
            // Remove optional '$'
            if (context.GetToken(VineLexer.VAR_PREFIX, 0) != null) {
                id = id.Remove(0, 1);
            }
            VineVar value = story.vars.ContainsKey(id) ? story.vars[id] : VineVar.NULL;
            value.name = id;
            Console.WriteLine("SimpleVar: " + id + " = \"" + value + "\"");
            return value;
        }
        
        public override VineVar VisitNewArray(VineParser.NewArrayContext context)
        {
            // '[' expressionList? ']'
            lastEnteredContext = context;
            Console.WriteLine("> NEW ARRAY: ");

            VineVar vineArray = VineVar.newArray;
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

        public override VineVar VisitNewDict(VineParser.NewDictContext context)
        {
            // '{' keyValueList? '}'
            lastEnteredContext = context;
            Console.WriteLine("> NEW DICT: ");

            VineVar vineDict = VineVar.newDict;
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

        private VineVar GetValueInSequence(VineVar startingVar, 
            VineParser.SequenceAccessContext[] sequences)
        {
            // Trying to get to the last sequence, which is n-1 in the list.
            // It should be either an array, a dictionnary or a string.
            // E.g.: myvar[3] ["abc"] [0] [42]
            //       |        |       |   |   
            //       n-3      n-2     |   n   
            //                        n-1     
            // ResolveSequencesAccess returns n-1:
            VineVar lastSequence = ResolveSequencesAccess(startingVar, sequences);

            // The last sequence (n-1) is now into lastSequence,
            // we can access n using the last index

            // Get the last index
            var lastIndex = Visit(sequences[sequences.Count() - 1].expr());

            // Finally get the content of [n], the value
            VineVar value = null;
            if (lastSequence.IsString) {
                value = lastSequence.AsString.Substring(lastIndex.AsInt, 1);
            } else if (lastSequence.IsArray) {
                value = lastSequence[lastIndex.AsInt];
            } else if (lastSequence.IsString) {
                value = lastSequence[lastIndex.AsString];
            } else {
                throw new VineRuntimeException(
                    "Can't access element with [] because the variable '"
                    + lastSequence.name + "' is neither an array nor a dictionnary"
                    + " nor a string",
                    sequences[0]
                );
            }
            return value;
        }

        private void SetValueInSequence(VineVar startingVar, 
            VineParser.SequenceAccessContext[] sequences, VineVar value,
            int opToken)
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
            VineVar lastSequence = ResolveSequencesAccess(startingVar, sequences);

            // The last sequence (n-1) is now into lastSequence,
            // we can access n using the last index

            // Get the last index
            var lastIndex = Visit(sequences[sequences.Count() - 1].expr());

            // Finally set the content of [n]
            if (lastSequence.IsArray)
            {
                var result = AssignOp(opToken, lastSequence[lastIndex.AsInt], value);
                lastSequence[lastIndex.AsInt] = result;
            }
            else if (lastSequence.IsDict)
            {
                var result = AssignOp(opToken, lastSequence[lastIndex.AsString], value);
                lastSequence[lastIndex.AsString] = result;
            }
            else if (lastSequence.IsString)
            {
                throw new VineRuntimeException(
                    "Strings don't support item assignment", sequences[sequences.Count() - 1]
                );
            }
            else
            {
                throw new VineRuntimeException(
                    "Can't access element with [] because the variable '"
                    + lastSequence.name + "' is neither an array nor a dictionnary"
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
        private VineVar ResolveSequencesAccess(VineVar startingVar, 
            VineParser.SequenceAccessContext[] sequences)
        {
            VineVar lastSequence = startingVar;

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

        private VineVar AssignOp(int opToken, VineVar left, VineVar right)
        {
            switch (opToken) {
                case VineLexer.ASSIGN:
                case VineLexer.TO:
                    left = right;
                    break;
                case VineLexer.ADDASSIGN:
                    left += right;
                    break;
                case VineLexer.SUBASSIGN:
                    left -= right;
                    break;
                case VineLexer.MULASSIGN:
                    left *= right;
                    break;
                case VineLexer.DIVASSIGN:
                    left /= right;
                    break;
                case VineLexer.MODASSIGN:
                    left %= right;
                    break;
                default:
                    throw new Exception("Unhandled assignment operator");
            }
            return left;
        }
    }
    
}