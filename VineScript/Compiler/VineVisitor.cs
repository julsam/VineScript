using System;
using System.Linq;
using Antlr4.Runtime;
using Antlr4.Runtime.Tree;
using System.Collections.Generic;
using VineScript.Core;
using System.Globalization;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Text;

namespace VineScript.Compiler
{
    public class VineRuntimeException : Exception
    {
        ParserRuleContext runtimeContext;

        public VineRuntimeException(string msg, ParserRuleContext ctx)
            : base(RuntimeErrorFormatter.Format(typeof(VineRuntimeException), msg, ctx))
        {
            runtimeContext = ctx;
        }

        public VineRuntimeException(string msg, Exception innerException)
            : base(msg, innerException) {}
    }

    class VineVisitor : VineParserBaseVisitor<VineVar> 
    {
        public PassageResult passageResult { get; private set; }

        private ParserOutputBuilder outputBuilder;
        private VineStory story;

        private ParserRuleContext lastEnteredContext;

        public VineVisitor(VineStory story)
        {
            this.story = story;
            passageResult = new PassageResult();
            outputBuilder = new ParserOutputBuilder();
        }

#if GRAMMAR_VERBOSE
        public void printOutput()
        {
            Console.WriteLine("### EVALUATE OUTPUT: ###");
            if (passageResult.text.Length > 0) {
                Console.WriteLine(passageResult.text);
            }
            Console.WriteLine("### END ###");
        }
#endif

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
            outputBuilder.PushText(value.AsString);
            passageResult.text = outputBuilder.Build();
            return value;
        }

        public override VineVar VisitPassage(VineParser.PassageContext context)
        {
            try {
                var v = VisitChildren(context);
                passageResult.text = outputBuilder.Build();
                return v;
            }
            catch (VineRuntimeException e) {
                // Rethrow the exception without modifications
                ExceptionDispatchInfo.Capture(e).Throw();
                throw;
            }
            catch (Exception e) {
                // Reformat the error message
                string formatted = RuntimeErrorFormatter.Format(
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

            if (    outputBuilder.IsCollapsed 
                && (    context.NL() != null
                    ||  context.BLOCK_COMMENT() != null 
                    ||  context.LINE_COMMENT() != null
                    )
                ){
                // if it's collapsed and it's a line return or a comment, we add a space
                outputBuilder.PushText(" ");
            } else {
                outputBuilder.PushText(context.GetText());
            }
            return null;
        }

        public override VineVar VisitCollapseStmt(VineParser.CollapseStmtContext context)
        {
            lastEnteredContext = context;
            outputBuilder.PushStmt("<< collapse >>");

            if (context.LCOLLAPSE() != null) {
                outputBuilder.EnterCollapse();
            } else {
                outputBuilder.ExitCollapse();
            }
            return null;
        }

        public override VineVar VisitVerbatimStmt(VineParser.VerbatimStmtContext context)
        {
            lastEnteredContext = context;
            string verbatim = context.VERBATIM().GetText();
            outputBuilder.PushVerbatim(verbatim);
            return null;
        }

        public override VineVar VisitLink(VineParser.LinkContext context)
        {
            // '[[' title=linkContent+ '|' address=linkContent+ '|' block* ']]'
            lastEnteredContext = context;

            // Link Title
            string title = Visit(context.title).AsString.Trim(new char[] { '\t', ' ' });
            title = Escape.UnescapeLinkContent(title);

            // Link Destination
            string destination = title;
            if (context.destination != null) { 
                destination = Visit(context.destination).AsString.Trim(new char[] { '\t', ' ' });
                destination = Escape.UnescapeLinkContent(destination);
            }

            // Check for empty title/link like [[mytitle| ]] or [[ |mylink]]
            if (string.IsNullOrWhiteSpace(title)) {
                throw new VineRuntimeException("The title of a link can't be empty!", context);
            } else if (string.IsNullOrWhiteSpace(destination)) {
                throw new VineRuntimeException("A link can't be empty!", context);
            }

            // Get the optional code content
            string linkcode = "";
            foreach (var block in context.block()) {
                string input = block.Start.InputStream.ToString();
                int start = block.Start.StartIndex;
                int stop = block.Stop.StopIndex;
                string blockText = "";
                if (start >= 0 && stop >= 0) {
                    for (int i = start; i <= stop; i++) {
                        blockText += input[i];
                    }
                }
                linkcode += blockText;
            }

            // DEBUG: print in passage
            //outputBuilder.PushText(
            //    "title: " + title
            //    + ", link: " + passageName
            //    + ", code: `" + linkcode + "`"
            //);
            //Console.WriteLine(
            //    "title: " + title 
            //    + ", link: " + destination
            //    + ", code: \"" + linkcode + "\""
            //);

            // Add it back to the output passage as a statement and will be treated
            // accordingly by the VineFormatter.
            outputBuilder.PushStmt("<< link >>");

            passageResult.links.Add(
                new PassageLink(title, destination, linkcode, passageResult.links.Count)
            );

            return null;
        }

        public override VineVar VisitLinkContent(VineParser.LinkContentContext context)
        {
            lastEnteredContext = context;
            StringBuilder result = new StringBuilder();
            foreach (var item in context.LINK_TEXT()) {
                result.Append(item.GetText());
            }
            return result.ToString();
        }

        public override VineVar VisitDisplay(VineParser.DisplayContext context)
        {
            lastEnteredContext = context;
            VineVar value = Visit(context.expr());
            
            // marks the start of the output of the display command
            var display_output = "\u001E";
            // Get every lines in an array
            string[] outputLines = value.AsString.Split('\n');
            for (int i = 0; i < outputLines.Length; i++) {
                //  add the line to the output
                display_output += outputLines[i];

                // if not the last line
                if (i < outputLines.Length - 1) {
                    // control character '\u000B' that replace '\n' 
                    // and will be used in LineFormatter to distinguish
                    // between line returns that are in the source code
                    // and line returns added by displaying the return value
                    // of a function containing '\n'
                    display_output += "\u000B";
                }
            }
            // marks the end of the output of the display command
            display_output += "\u001F";

            // Push it as a verbatim even if it's not one. We want the
            // displayed text to be outputed as it is, even if it's in
            // a collapsed block. So white spaces, line returns, etc
            // won't be collapsed.
            outputBuilder.PushVerbatim(display_output);

            return null;
        }

        #region Commands

        public override VineVar VisitAssign(VineParser.AssignContext context)
        {
            // '<<' 'set' ID 'to' expr '>>'
            lastEnteredContext = context;
            outputBuilder.PushStmt("<< set >>");

            var variable = Visit(context.variable());
            string id = variable.name;
            VineVar value = Visit(context.expr());

#if DEBUG
            if (story.vars.ContainsKey(id)) {
                Console.WriteLine(string.Format(
                    "[!!] Warning, the variable '{0}' is already defined!"
                    + " Its value '{1}' will be overridden.", id, value
                ));
            }
#endif

            if (context.sequenceAccess() != null && context.sequenceAccess().Count() > 0)
            {
                SetValueInSequence(story.vars[id], context.sequenceAccess(), 
                    value, context.op.Type);
            }
            else
            {
                if (context.op.Type == VineLexer.ASSIGN || context.op.Type == VineLexer.TO) {
                    story.vars[id] = AssignOp(context.op.Type, null, value);
                } else {
                    story.vars[id] = AssignOp(context.op.Type, story.vars[id], value);
                }
            }

            return value;
        }

        public override VineVar VisitUnsetList(VineParser.UnsetListContext context)
        {
            // '<<' 'unset' ID '>>'
            lastEnteredContext = context;
            outputBuilder.PushStmt("<< unset >>");

            foreach (var item in context.variable()) {
                var variable = Visit(item);
                string id = variable.name;

                if (story.vars.ContainsKey(id)) {
                    story.vars.Remove(id);
                } else {
                    Console.WriteLine(string.Format(
                        "[!!] Warning, the variable '{0}' is not defined!"
                        + " Its value cannot be unset.", id
                    ));
                }
            }

            return null;
        }

        public override VineVar VisitFuncCall(VineParser.FuncCallContext context)
        {
            // ID '(' expressionList? ')'
            lastEnteredContext = context;
            var funcName = context.ID().GetText();
            outputBuilder.PushStmt(string.Format("<< call func \"{0}\" >>", funcName));

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

            outputBuilder.PushStmt("<< if >>");
            bool ifvalue = Visit(context.ifStmt()).AsBool;
            if (!ifvalue) {
                bool elifvalue = false;
                for (int i = 0; i < context.elifStmt().Length; i++) {
                    outputBuilder.PushStmt("<< elif >>");
                    elifvalue = Visit(context.elifStmt(i)).AsBool;
                    if (elifvalue) {
                        break;
                    }
                }
                if (!elifvalue && context.elseStmt() != null) {
                    outputBuilder.PushStmt("<< else >>");
                    Visit(context.elseStmt());
                }
            }
            outputBuilder.PushStmt("<< end >>");
            return ifvalue;
        }

        public override VineVar VisitIfStmt(VineParser.IfStmtContext context)
        {
            lastEnteredContext = context;
            VineVar ifvalue = Visit(context.expr());
            if (ifvalue.AsBool) {
                for (int i = 0; i < context.block().Length; i++) {
                    Visit(context.block(i));
                }
            }
            return ifvalue;
        }
        
        public override VineVar VisitElifStmt(VineParser.ElifStmtContext context)
        {
            lastEnteredContext = context;
            VineVar elifvalue = Visit(context.expr());
            if (elifvalue.AsBool) {
                for (int i = 0; i <  context.block().Length; i++) {
                    Visit(context.block(i));
                }
            }
            return elifvalue;
        }
        
        public override VineVar VisitElseStmt(VineParser.ElseStmtContext context)
        {
            lastEnteredContext = context;
            for (int i = 0; i <  context.block().Length; i++) {
                Visit(context.block(i));
            }
            return null;
        }

        public override VineVar VisitForCtrlStmt(VineParser.ForCtrlStmtContext context)
        {
            lastEnteredContext = context;
            outputBuilder.PushStmt("<< for >>");
            VineVar forvalue = Visit(context.forStmt());
            outputBuilder.PushStmt("<< end >>");
            return null;
        }

        public override VineVar VisitForValueStmt(VineParser.ForValueStmtContext context)
        {
            lastEnteredContext = context;
            
            // TODO scope with temp vars
            ParserRuleContext iteratorCtx = null;
            if (context.expr() != null) {
                iteratorCtx = context.expr();
            } else {
                iteratorCtx = context.interval();
            }
            VineVar iterator = Visit(iteratorCtx);

            if (iterator.IsArray || iterator.IsDict || iterator.IsString) {
                var tempVarValue = Visit(context.variable());
                string id = tempVarValue.name;
                foreach (var item in iterator) {
                    if (iterator.IsDict) {
                        story.vars[id] = ((KeyValuePair<string, VineVar>)item).Value;
                    } else {
                        story.vars[id] = item as VineVar;
                    }
                    for (int i = 0; i < context.block().Length; i++) {
                        Visit(context.block(i));
                    }
                }
            } else {
                throw new VineRuntimeException(
                    "'" + iterator.type + "' is not iterable", iteratorCtx
                );
            }

            return null;
        }

        public override VineVar VisitForKeyValueStmt(VineParser.ForKeyValueStmtContext context)
        {
            lastEnteredContext = context;
            
            ParserRuleContext iteratorCtx = context.expr();
            VineVar iterator = Visit(iteratorCtx);
            
            // TODO scope with temp vars
            VineVar tempVarKey = Visit(context.key);
            VineVar tempVarValue = Visit(context.val);
            string keyId = tempVarKey.name;
            string valueId = tempVarValue.name;

            if (iterator.IsDict)
            {
                foreach (var item in iterator) {
                    story.vars[keyId] = ((KeyValuePair<string, VineVar>)item).Key;
                    story.vars[valueId] = ((KeyValuePair<string, VineVar>)item).Value;
                    for (int i = 0; i < context.block().Length; i++) {
                        Visit(context.block(i));
                    }
                }
            }
            else if (iterator.IsArray || iterator.IsString)
            {
                story.vars[keyId] = 0;
                foreach (var item in iterator)
                {
                    story.vars[valueId] = item as VineVar;
                    for (int i = 0; i < context.block().Length; i++) {
                        Visit(context.block(i));
                    }
                    story.vars[keyId] += 1;
                }
            }
            else
            {
                throw new VineRuntimeException(
                    "'" + iterator.type + "' is not iterable", iteratorCtx
                );
            }

            return null;
        }

        public override VineVar VisitInterval(VineParser.IntervalContext context)
        {
            lastEnteredContext = context;
            VineVar left = Visit(context.left);
            VineVar right = Visit(context.right);
            if (!left.IsInt || !right.IsInt) {
                throw new VineRuntimeException("Intervals must be integers values", context);
            }
            var interval = Converter.ToVineVar(
                Builtins.Range(left.AsInt, right.AsInt)
            );
            return interval;
        }

#endregion Control Statements

#region Expr

        public override VineVar VisitUnaryExpr(VineParser.UnaryExprContext context)
        {
            lastEnteredContext = context;
            VineVar left = Visit(context.expr());
            return context.op.Type == VineParser.MINUS ? -left : !left;
        }

        public override VineVar VisitPowExpr(VineParser.PowExprContext context)
        {
            lastEnteredContext = context;
            VineVar left = Visit(context.left);
            VineVar right = Visit(context.right);
            return (left ^ right);
        }

        public override VineVar VisitMulDivModExpr(VineParser.MulDivModExprContext context)
        {
            lastEnteredContext = context;
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
            VineVar left = Visit(context.left);
            VineVar right = Visit(context.right);
            return context.op.Type == VineParser.ADD
                ? left + right
                : left - right;
        }

        public override VineVar VisitEqualityExpr(VineParser.EqualityExprContext context)
        {
            lastEnteredContext = context;
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
            VineVar value = Visit(context.expr());
            return value;
        }

        public override VineVar VisitVarExpr(VineParser.VarExprContext context)
        {
            lastEnteredContext = context;

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
            return int.Parse(context.INT().GetText());
        }

        public override VineVar VisitFloatAtom(VineParser.FloatAtomContext context)
        {
            lastEnteredContext = context;
            return double.Parse(context.FLOAT().GetText(), CultureInfo.InvariantCulture);
        }

        public override VineVar VisitBoolAtom(VineParser.BoolAtomContext context)
        {
            lastEnteredContext = context;
            return bool.Parse(context.GetText());
        }

        public override VineVar VisitNullAtom(VineParser.NullAtomContext context)
        {
            lastEnteredContext = context;
            return VineVar.NULL;
        }

#endregion Atom

        public override VineVar VisitStringLiteral(VineParser.StringLiteralContext context)
        {
            lastEnteredContext = context;

            // Get the string literal
            string str = context.STRING().GetText();
            
            // Removes starting and ending '"'
            str = str.Substring(1, str.Length - 2);
            
            // Unescape special chars starting with '\'. E.g. \n, \t, etc
            str = Escape.UnescapeStringLiteral(str);

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
            return value;
        }
        
        public override VineVar VisitNewArray(VineParser.NewArrayContext context)
        {
            // '[' expressionList? ']'
            lastEnteredContext = context;

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

        /// <summary>
        /// Error message when trying to access a sequence element.
        /// </summary>
        /// <param name="varname">The variable's name in which the lookup takes place.
        /// This can be empty because some variable are anonymous, for e.g. when
        /// trying to access an element of a function return {{ foo()[0] }}.</param>
        /// <returns>Formatted error message.</returns>
        private string SequenceTypeError(string varname)
        {
            string str = "";
            if (!string.IsNullOrWhiteSpace(varname)) {
                str = " '" + varname + "'";
            }
            return "Can't access element with [] because the sequence"
                    + str + " is neither an array nor a dictionnary"
                    + " nor a string.";
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
            } else if (lastSequence.IsDict) {
                value = lastSequence[lastIndex.AsString];
            } else {
                throw new VineRuntimeException(
                    SequenceTypeError(lastSequence.name),
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
                    SequenceTypeError(lastSequence.name),
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
                        SequenceTypeError(lastSequence.name),
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