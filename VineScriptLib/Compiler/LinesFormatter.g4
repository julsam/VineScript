grammar LinesFormatter;

/*
 * Parser Rules
 */

compileUnit
    :   line*
    ;

line
    :   NL EOF                      # printLn
    |   inline NL block+ EOF        # consumeLn
    |   inline NL (block NL?)+ EOF  # consumeLn
    |   block+ (NL|EOF)             # consumeLn
    |   inline NL                   # printLn
    |   inline EOF                  # consumeLn
    |   NL                          # printLn
    ;

inline
    :   varPrint (inline|block)*
    |   block+ (varPrint|text)+ block*
    |   text (inline|block)* 
    |   inline inline
    ;

//inline
//    :   varPrint
//    |   text
//    |   block
//    |   inline inline
//    ;

varPrint
    :   OUTPUT
    ;

block
    :   STMT
    |   LINE_COMMENT
    |   BLOCK_COMMENT
    ;

text
    :   TXT
    ;

/*
 * Lexer Rules
 */

OUTPUT:         '{{' .*? '}}' ;
STMT:           '{%' .*? '%}' ;
//LINE_COMMENT:   '#' ~('#')*? NL ;
BLOCK_COMMENT:  '{#' .*? '#}' ;

NL:     '\r'? '\n' ;
WS:     [ \t]+ -> skip ;

TXT_LBRACE
	:	'{' -> type(TXT)
    ;
TXT :   ~[{\r\n]+
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

ERROR_CHAR: . ;
