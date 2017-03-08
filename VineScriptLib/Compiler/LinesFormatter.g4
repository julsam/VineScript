grammar LinesFormatter;

/*
 * Parser Rules
 */

compileUnit
    :   line*
    ;

line
    :   NL EOF                      # consumeLn
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
    ;

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

NL:         '\r'? '\n' ;
WS:         [ \t]+ -> skip ;
TXT:        ('{'? ~('{'|'%'|'#'|'?'|'\r'|'\n'))+ ;
/*
TXT:        (   ('{' ~('{'|'%'|'#'|'?'|'\r'|'\n'))
            |   ~('{') .
            )+ ;
*/
