using System;
using Antlr4.Runtime;

namespace VineScript.Compiler
{
    class UnderlineErrorListener : BaseErrorListener
    {
        public override void SyntaxError(IRecognizer recognizer, IToken offendingSymbol, 
            int line, int charPositionInLine, string msg, RecognitionException e)
        {
            var underline = UnderlineError(recognizer, offendingSymbol, line, charPositionInLine);
            var errmsg = string.Format(
                "[Parser] Invalid Expression '{0}' at {1}:{2}:\n{3}\n{4}", 
                Compiler.Util.Escape(offendingSymbol.Text),
                line, charPositionInLine, underline, msg
            );
            throw new Exception(errmsg, e);
        }

        protected string UnderlineError(IRecognizer recognizer, IToken offendingSymbol, 
            int line, int charPositionInLine)
        {
            string strRet = "";
            CommonTokenStream tokens = (CommonTokenStream)recognizer.InputStream;
            string input = tokens.TokenSource.InputStream.ToString();
            string[] lines = input.Split('\n');
            string errorLine = lines[line - 1];
            strRet += errorLine + "\n";
            for (int i = 0; i < charPositionInLine; i++) {
                strRet += " ";
            }
            int start = offendingSymbol.StartIndex;
            int stop = offendingSymbol.StopIndex;
            if (start >= 0 && stop >= 0) {
                for (int i = start; i <= stop; i++) {
                    strRet += "^";
                }
            }
            return strRet;
        }
    }
}
