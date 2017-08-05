using System;
using Antlr4.Runtime;

namespace VineScript.Compiler
{
    internal class ParserErrorListener : BaseErrorListener
    {
        public override void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, 
            int line, int column, string errmsg, RecognitionException e)
        {
            throw new VineParseException(
                recognizer, offendingSymbol, line, column,  Utils.UppercaseFirst(errmsg), e
            );
        }
    }
}
