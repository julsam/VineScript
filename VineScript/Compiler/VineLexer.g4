lexer grammar VineLexer;

@header {
using System;
}
@members{
    //public override IToken Emit(){
    //    switch (_type) {
    //        case RESERVED_CHARS:
    //            //setType(RESERVED_CHARS);
    //            IToken result = base.Emit();
    //            // you'll need to define this method
    //            System.Console.WriteLine("Unterminated string literal"); 
    //            //reportError(result, "Unterminated string literal");
    //            return result;
    //        default:
    //            return base.Emit();
    //    }
    //}

    public static bool IsVerbatim(string str)
    {
        if (ToVerbatim(str) != null) {
            return true;
        } else {
            return false;
        }
    }

    public static string ToVerbatim(string str)
    {
        try {
            while ( str.Length >= 3 && str.StartsWith("`") && str.EndsWith("`")
                &&  !Util.IsCharAtEscaped(str, str.Length - 1)
                ) {
                str = str.Remove(0, 1);
                str = str.Remove(str.Length - 1, 1);
            }
            if  (   str.Length > 0 && !str.StartsWith("`")
                &&  (!str.EndsWith("`") ||  Util.IsCharAtEscaped(str, str.Length - 1))
                ) {
                return str;
            } else {
                return null;
            }
        } catch (Exception) {
            throw new Exception("Internal error VineLexer:ToVerbatim");
        }
    }
}

/*
 * Lexer Rules
 */

// Default "mode" (text mode) : Everything that is outside of tags '{{ .. }}', '<< .. >>' or '/* .. */'

// Escape '\' or '`'. For every char added here, must think to add it to UnescapeVineSequence too
TXT_ESC
    :   ('\\\\' | '\\`') -> type(TXT)
    ;

// Escape everything between ` ` or `` `` or ``` ```, etc
VERBATIM
    :   ('`')+ ALL_BUT_RESERVED_CHARS*? ('`')+ {IsVerbatim(Text)}?
    ;

LOUTPUT:        '{{' -> pushMode(VineCode) ;
LSTMT:          '<<' -> pushMode(VineCode) ;
LLINK:          '[[' -> pushMode(LinkMode) ;
BLOCK_COMMENT:  '/*' .*? '*/' ;
LINE_COMMENT:   '//' ~[\r\n]* ;

NL:     '\r'? '\n' ;

// Reserved / illegal characters:
//  * '\u000B': \v vertical tabulation, marks '\n' in strings returned by a function
//              and then used by LinesFormatter to keep those '\n' in place
//  * '\u001E': marks the start of the output of the display command
//  * '\u001F': marks the end of the output of the display command
RESERVED_CHARS: [\u000B\u001E\u001F] ;
fragment
ALL_BUT_RESERVED_CHARS: ~[\u000B\u001E\u001F] ;

TXT_SPECIALS
    :   [`\\<\[{/] -> type(TXT)
    ;
TXT :   ~[`\\<\[{/\r\n\u000B\u001E\u001F]+
    ;

ERROR_CHAR: . ;

// ----------------------------------------------------------
mode LinkMode;

LINK_ESC
    :   (   '\\\\'  // [[my \\ title|mylink]]
        |   '\\]'   // [[my [[own\]] title|mylink]]
        |   '\\|'   // [[my \| title|mylink]]
        |   '\\<'   // [[mylink<-my \<- title]]
        |   '\\-'   // [[my \-> title->mylink]]
        )
        -> type(LINK_TEXT)
    ;

RLINK: ']]' -> popMode ;

LINK_RESERVED_CHARS: RESERVED_CHARS -> type(RESERVED_CHARS) ;

LINK_PIPE:  '|' ;
LINK_LEFT:  '<-' ;
LINK_RIGHT: '->' ;

LINK_TEXT_SPECIALS: [\\<\]-] -> type(LINK_TEXT) ;
LINK_TEXT:  ~[\\|<\]\r\n\u000B\u001E\u001F-]+ ;

LinkMode_ERROR_CHAR: ERROR_CHAR -> type(ERROR_CHAR) ;

// ----------------------------------------------------------
mode VineCode;
ROUTPUT:    '}}' -> popMode ;
RSTMT:      '>>' -> popMode ;

// Parentheses, square brackets, curly braces
LPAREN:     '(' ;
RPAREN:     ')' ;
LBRACK:     '[' ;
RBRACK:     ']' ;
LBRACE:     '{' ;
RBRACE:     '}' ;

// Reserved keywords
IF:     'if' ;
ELIF:   'elif' ;
ELSE:   'else' ;
END:    'end' ;
FOR:    'for' ;
IN:     'in' ;
KW_AND: 'and' ;
KW_OR:  'or' ;
TO:     'to' ;
SET:    'set' ;
UNSET:  'unset' ;

// Separators
INTERVAL_SEP:   '...' ;
DOT:            '.' ;
COMMA:          ',' ;
COLON:          ':' ;

// Assign
ADDASSIGN:  '+=' ;
SUBASSIGN:  '-=' ;
MULASSIGN:  '*=' ;
DIVASSIGN:  '/=' ;
MODASSIGN:  '%=' ;
ASSIGN:     '=' ;

// Unary op
MINUS:  '-' ;
NOT:    '!' ;
POW:    '^' ; // right assoc

// Arithmetic op
MUL:    '*' ;
DIV:    '/' ;
ADD:    '+' ;
MOD:    '%' ;

// Equality op
EQ:     '==' ;
NEQ:    '!=' ;

// Logical op
AND:    '&&' ;
OR:     '||' ;

// Comparison op
LT:     '<' ;
GT:     '>' ;
LTE:    '<=' ;
GTE:    '>=' ;

// TODO complete list. Commands are built-in functions
COMMAND:    'array' | 'TODO' ;

// Atoms
TRUE:       'true' ;
FALSE:      'false' ;
NULL:       'null' ;


STRING:         '"' (ESC | ALL_BUT_RESERVED_CHARS)*? '"' ;

// catches strings containing '\u000B' or '\u001E' or '\u001F':
ILLEGAL_STRING: '"' .*? '"' ;

//tokens { STRING }
//DOUBLE : '"' .*? '"'   -> type(STRING) ;
//SINGLE : '\'' .*? '\'' -> type(STRING) ;
//STRING_SQUOTE:    '\'' (ESC_SQUOTE|.)*? '\'' ;
//STRING_DQUOTE:    '"' (ESC_DQUOTE|.)*? '"' ;


// Antlr defined unicode whitespace (only in Antlr >= 4.6 or 4.7)
// https://github.com/antlr/antlr4/blob/master/doc/lexer-rules.md
//UNICODE_WS : [\p{White_Space}] -> skip; // match all Unicode whitespace 
WS: (WS_Restricted|WS_SpaceSeparator|WS_LineSeparator|WS_ParagraphSeparator)+ -> channel(HIDDEN) ;


// ** WHITE SPACE DEFINITIONS **
// http://www.unicode.org/Public/UNIDATA/UnicodeData.txt
// https://msdn.microsoft.com/en-us/library/system.char.iswhitespace(v=vs.110).aspx
// https://en.wikipedia.org/wiki/Whitespace_character

// Contains SPACE (0020), CHARACTER TABULATION (0009), FORM FEED (U+000C),
// CARRIAGE RETURN (U+000D) and LINE FEED (000A)
// It's missing:
// * LINE TABULATION (U+000B aka \v), used as a reserved char
// * NEXT LINE (U+0085), which i'm not sure I should include
fragment WS_Restricted:     [ \t\f\r\n] ;

// SpaceSeparator "Zs" from the unicode list:
//    0020;SPACE;Zs;0;WS;;;;;N;;;;;
//    00A0;NO-BREAK SPACE;Zs;0;CS;<noBreak> 0020;;;;N;NON-BREAKING SPACE;;;;
//    1680;OGHAM SPACE MARK;Zs;0;WS;;;;;N;;;;;
//    2000;EN QUAD;Zs;0;WS;2002;;;;N;;;;;
//    2001;EM QUAD;Zs;0;WS;2003;;;;N;;;;;
//    2002;EN SPACE;Zs;0;WS;<compat> 0020;;;;N;;;;;
//    2003;EM SPACE;Zs;0;WS;<compat> 0020;;;;N;;;;;
//    2004;THREE-PER-EM SPACE;Zs;0;WS;<compat> 0020;;;;N;;;;;
//    2005;FOUR-PER-EM SPACE;Zs;0;WS;<compat> 0020;;;;N;;;;;
//    2006;SIX-PER-EM SPACE;Zs;0;WS;<compat> 0020;;;;N;;;;;
//    2007;FIGURE SPACE;Zs;0;WS;<noBreak> 0020;;;;N;;;;;
//    2008;PUNCTUATION SPACE;Zs;0;WS;<compat> 0020;;;;N;;;;;
//    2009;THIN SPACE;Zs;0;WS;<compat> 0020;;;;N;;;;;
//    200A;HAIR SPACE;Zs;0;WS;<compat> 0020;;;;N;;;;;
//    202F;NARROW NO-BREAK SPACE;Zs;0;CS;<noBreak> 0020;;;;N;;;;;
//    205F;MEDIUM MATHEMATICAL SPACE;Zs;0;WS;<compat> 0020;;;;N;;;;;
//    3000;IDEOGRAPHIC SPACE;Zs;0;WS;<wide> 0020;;;;N;;;;;

// Does not include SPACE (it's in rule WS_Restricted) 
// and OGHAM SPACE MARK (does not look like a space)
fragment WS_SpaceSeparator:     [\u00A0\u2000-\u200A\u202F\u205F\u3000] ;

//LineSeparator "Zl" from the unicode list:
//    2028;LINE SEPARATOR;Zl;0;WS;;;;;N;;;;;
fragment WS_LineSeparator:      '\u2028' ;

//ParagraphSeparator "Zp" from the unicode list:
//    2029;PARAGRAPH SEPARATOR;Zp;0;B;;;;;N;;;;;
fragment WS_ParagraphSeparator: '\u2029' ;


// ** IDENTIFIERS **
VAR_PREFIX: '$' ;

// Unicode ID https://github.com/antlr/antlr4/blob/master/doc/lexer-rules.md
// match full Unicode alphabetic ids (only in Antlr >= 4.6 or 4.7)
//UNICODE_ID : [\p{Alpha}\p{General_Category=Other_Letter}] [\p{Alnum}\p{General_Category=Other_Letter}]* ;
ID:         ID_LETTER (ID_LETTER | DIGIT)* ;
INT:        DIGIT+ ;
FLOAT:      DIGIT+ '.' DIGIT+ ;

VineCode_ERROR_CHAR: ERROR_CHAR -> type(ERROR_CHAR) ;

// fragments
fragment ESC:       '\\"' | '\\\\' ; // 2-char sequences \" and \\
//fragment ESC_SQUOTE:    '\\\'' | '\\\\' ; // 2-char sequences \" and \\
//fragment ESC_DQUOTE:    '\\"' | '\\\\' ; // 2-char sequences \" and \\
fragment DIGIT:     [0-9] ;
fragment ID_LETTER: [A-Za-z\u0080-\uFFFF_] ; // goes from 41 ('A') to z then 128 to 65535 (unicode)
