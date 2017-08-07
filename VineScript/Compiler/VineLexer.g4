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

// From Harlowe:
// This includes all forms of Unicode 6 whitespace except \n, \r, and Ogham space mark.
//WS_CODE:    [ \f\t\v\u00a0\u2000-\u200a\u2028\u2029\u202f\u205f\u3000]+ -> skip ;
// Unicode whitespace https://github.com/antlr/antlr4/blob/master/doc/lexer-rules.md
// list : https://en.wikipedia.org/wiki/Whitespace_character
//UNICODE_WS : [\p{White_Space}] -> skip; // match all Unicode whitespace (only in Antlr >= 4.6)
WS:     [ \t\f\r\n]+ -> channel(HIDDEN) ;

VAR_PREFIX: '$' ;

// Unicode ID https://github.com/antlr/antlr4/blob/master/doc/lexer-rules.md
// match full Unicode alphabetic ids (only in Antlr >= 4.6)
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
