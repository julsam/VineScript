grammar WhiteSpace;

/**
 * This grammar is not used in Vine, it was first made as an helper to format text and code.
 */

/*
 * Parser Rules
 */
 
/*
 * v1
 */
/*
compileUnit
    :   NL? block* NL? EOF    
    ;

block
    :   code NL (code NL)+            # NoPrint
    |   code text (code|text)* NL    # Print
    |   text code (code|text)* NL    # Print
    |   code NL                        # NoPrint
    |   text NL                        # Print
    |   NL                            # Print
    ;

code
    :   OUTPUT
    |   STMT
    |   COMMENT
    ;

text
    :   TXT
    ;
*/

/*
 * v2
 */
compileUnit
    :   NL? block* NL? EOF
    ;

block
    :   code+ NL                    # NoPrint
    |   code NL                     # NoPrint
    |   code text (code|text)* NL   # PrintLn
    |   text code (code|text)* NL   # PrintLn
    //|   code text NL                # PrintLn
    //|   text code NL                # PrintLn
    |   text NL                     # PrintLn
    |   NL                          # PrintLn
    |   text                        # Print
    ;

code
    :   OUTPUT
    |   STMT
    |   COMMENT
    ;

text
    :   TXT
    //|    NL
    ;

/*
 * Lexer Rules
 */

OUTPUT:     '{{' .*? '}}' ;
STMT:       '{%' .*? '%}' ;
COMMENT:    '{#' .*? '#}' ;

NL:         '\r'? '\n' ;
TXT:        ('{'? ~('{'|'%'|'#'|'?'|'\r'|'\n'))+ ; 
WS:         [ \t]+ -> channel(HIDDEN) ;
