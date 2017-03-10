lexer grammar VineLexer;

tokens { TYPE_STMT_END }
/*
 * Lexer Rules
 */

//channels { WSCHANNEL, MYHIDDEN }

// Default "mode": Everything that is outside of tags '{{ .. }}', '{% .. %}' or '{# .. #}'

OUTPUT:         '{{' -> pushMode(MODE_CODE) ;
STMT:           '{%' -> pushMode(MODE_CODE) ;
//LINE_COMMENT:   '#' ~('#')*? NL -> channel(HIDDEN);
BLOCK_COMMENT:  '{#' .*? '#}' ;

// From Harlowe:
// This includes all forms of Unicode 6 whitespace except \n, \r, and Ogham space mark.
//WS:                    [ \f\t\v\u00a0\u2000-\u200a\u2028\u2029\u202f\u205f\u3000]+ -> skip ;
//WS:         [ \t]+ ;
WS:     [ \t]+ -> channel(HIDDEN) ;
NL:     '\r'? '\n' ;

// A text is either :
//  1. anything that's not {, \r, \n
//  2. or it is { but then it's not followed by {, %, #. ?
// TODO: should allow escaping tags
TXT :   (       ~('{'|'\r'|'\n')
            |   '{' ~('{'|'%'|'#'|'?')
        )+ 
    |  '{' ~('{'|'%'|'#'|'?')*? // special case when { is not followed by anything (EOF)
    ;

//TXT:      ('{'? ~('{'|'%'|'#'|'?'|'\r'|'\n'))+ ;
//TXT:    (   '{' ~('{'|'%'|'#'|'?')
//        |   {_input.La(-1) != '{'}? ~('{'|'\r'|'\n')
//        )+ ;

ANY:    . ;

// ----------------------------------------------------------
//mode MODE_COMMENT;

// ----------------------------------------------------------
mode MODE_CODE;
END_OUTPUT_WS:  '}}' WS -> popMode ; 
END_OUTPUT:     '}}' -> popMode ; 
END_STMT:       '%}' -> popMode ; 

IF:             'if ' ;
ELSE:           'elif ' ;
ELIF:           'else' ;
END:            'end' ;
TRUE:           'true' ;
FALSE:          'false' ;
NULL:           'null' ;
AND:            '&&' ;
OR:             '||' ;
AND2:           ' and ' ;
OR2:            ' or ' ;
DOT:            '.' ;
COMMA:          ',' ;
LeftParen:      '(' ;
RightParen:     ')' ;
LeftBracket:    '[' ;
RightBracket:   ']' ;

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

//LeftBrace : '{';
//RightBrace : '}';

// TODO complete list. Commands are built-in functions
COMMAND:    'array' | 'TODO' ;
SET:        'set' ;
TO:         'to' ;
ASSIGN:     '=' ;

STRING:     '"' (ESC|.)*? '"' ;
ID:         '$' ID_LETTER (ID_LETTER | DIGIT)* ;
INT:        DIGIT+ ;
FLOAT:      DIGIT+ '.' DIGIT+ ;

// From Harlowe:
// This includes all forms of Unicode 6 whitespace except \n, \r, and Ogham space mark.
//WS:                    [ \f\t\v\u00a0\u2000-\u200a\u2028\u2029\u202f\u205f\u3000]+ -> skip ;
WS_CODE:        [ \t]+ -> channel(HIDDEN) ;
//NL:         '\r'? '\n' -> channel(HIDDEN) ;

// fragments
fragment ESC:           '\\"' | '\\\\' ; // 2-char sequences \" and \\
//fragment ESC:           '\\' [trn"\\] ; // \t \r \n \" and \\
fragment DIGIT:         [0-9] ;
fragment ID_LETTER:     [A-Za-z\u0080-\uFFFF_] ; // goes from 41 ('A') to z then 128 to 65535 (unicode)
