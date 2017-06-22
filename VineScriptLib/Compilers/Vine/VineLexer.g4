lexer grammar VineLexer;

@members{
    // Allow printing for debug in the lexer {print("foo")}?
    public bool print(string msg) { 
        System.Console.WriteLine(msg); 
        return true; 
    }
    
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
}

/*
 * Lexer Rules
 */

// Default "mode" (text mode) : Everything that is outside of tags '{{ .. }}', '{% .. %}' or '{# .. #}'

OUTPUT:         '{{' -> pushMode(VineCode) ;
STMT:           '{%' -> pushMode(VineCode) ;
//LINE_COMMENT:   '#' ~('#')*? NL -> channel(HIDDEN);
BLOCK_COMMENT:  '{#' .*? '#}' ;

// From Harlowe:
// This includes all forms of Unicode 6 whitespace except \n, \r, and Ogham space mark.
//WS:                    [ \f\t\v\u00a0\u2000-\u200a\u2028\u2029\u202f\u205f\u3000]+ -> skip ;
//WS:         [ \t]+ ;
//WS:     [ \t]+ -> channel(HIDDEN) ; // disabled, whitespace is considered as text
NL:     '\r'? '\n' ;

// Reserved/illegal characters:
//  * '\u000B': \v vertical tabulation, marks '\n' in strings returned by a function
//              and then used by LinesFormatter to keep those '\n' in place
RESERVED_CHARS: [\u000B]+ ;

TXT_LBRACE
    :   '{' -> type(TXT)
    ;
TXT :   ~[{\r\n\u000B]+
    ;

// A text is either :
//  1. anything that's not {, \r, \n
//  2. or it is { but then it's not followed by {, %, #. ?
// TODO: should allow escaping tags
//TXT :   (       ~('{'|'\r'|'\n')
//            |   '{' ~('{'|'%'|'#'|'?')
//        )+ 
//    |  '{' ~('{'|'%'|'#'|'?')*? // special case when { is not followed by anything (EOF)
//    ;
//TXT:      ('{'? ~('{'|'%'|'#'|'?'|'\r'|'\n'))+ ;
//TXT:    (   '{' ~('{'|'%'|'#'|'?')
//        |   {_input.La(-1) != '{'}? ~('{'|'\r'|'\n')
//        )+ ;

ERROR_CHAR: . ;

// ----------------------------------------------------------
mode VineCode;
END_OUTPUT:     '}}' -> popMode ; 
END_STMT:       '%}' -> popMode ; 

// Keywords
IF:         'if ' ;
ELSE:       'elif ' ;
ELIF:       'else' ;
END:        'end' ;
TRUE:       'true' ;
FALSE:      'false' ;
NULL:       'null' ;
AND2:       ' and ' ;
OR2:        ' or ' ;

AND:        '&&' ;
OR:         '||' ;
DOT:        '.' ;
COMMA:      ',' ;
LPAREN:     '(' ;
RPAREN:     ')' ;
LBRACK:     '[' ;
RBRACK:     ']' ;

//LeftBrace : '{';
//RightBrace : '}';

// unary op
MINUS:  '-' ;
NOT:    '!' ;
POW:    '^' ; // right assoc

// tern op
MUL:    '*' ;
DIV:    '/' ;
ADD:    '+' ;
MOD:    '%' ;

// Comparison
EQ:     '==' ;
NEQ:    '!=' ;
LT:     '<' ;
GT:     '>' ;
LTE:    '<=' ;
GTE:    '>=' ;

// TODO complete list. Commands are built-in functions
COMMAND:    'array' | 'TODO' ;
SET:        'set' ;
TO:         'to' ;
ASSIGN:     '=' ;


STRING:         '"' (ESC | ~('\u000B'))*? '"' ;

// catches string containing '\u000B':
ILLEGAL_STRING: '"' (ESC | .)*? '"' ;

//tokens { STRING }
//DOUBLE : '"' .*? '"'   -> type(STRING) ;
//SINGLE : '\'' .*? '\'' -> type(STRING) ;
//STRING_SQUOTE:    '\'' (ESC_SQUOTE|.)*? '\'' ;
//STRING_DQUOTE:    '"' (ESC_DQUOTE|.)*? '"' ;

VAR_PREFIX: '$' ;

// Unicode ID https://github.com/antlr/antlr4/blob/master/doc/lexer-rules.md
//UNICODE_ID : [\p{Alpha}\p{General_Category=Other_Letter}] [\p{Alnum}\p{General_Category=Other_Letter}]* ; // match full Unicode alphabetic ids
ID:         ID_LETTER (ID_LETTER | DIGIT)* ;
INT:        DIGIT+ ;
FLOAT:      DIGIT+ '.' DIGIT+ ;

// From Harlowe:
// This includes all forms of Unicode 6 whitespace except \n, \r, and Ogham space mark.
//WS_CODE:    [ \f\t\v\u00a0\u2000-\u200a\u2028\u2029\u202f\u205f\u3000]+ -> skip ;
// Unicode whitespace https://github.com/antlr/antlr4/blob/master/doc/lexer-rules.md
//UNICODE_WS : [\p{White_Space}] -> skip; // match all Unicode whitespace
WS_CODE:    [ \t]+ -> channel(HIDDEN) ;

VineCode_ERROR_CHAR: ERROR_CHAR -> type(ERROR_CHAR) ;

// fragments
fragment ESC:       '\\"' | '\\\\' ; // 2-char sequences \" and \\
//fragment ESC_SQUOTE:    '\\\'' | '\\\\' ; // 2-char sequences \" and \\
//fragment ESC_DQUOTE:    '\\"' | '\\\\' ; // 2-char sequences \" and \\
fragment DIGIT:     [0-9] ;
fragment ID_LETTER: [A-Za-z\u0080-\uFFFF_] ; // goes from 41 ('A') to z then 128 to 65535 (unicode)
