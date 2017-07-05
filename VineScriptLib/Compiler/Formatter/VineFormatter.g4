grammar VineFormatter;

/*
 * Parser Rules
 */

passage
    :   line* EOF?
    ;

line
    :   NL EOF                              # printLn
    |   containsOutput NL (block NL?)+ EOF  # consumeLn // foobar {{ $var }}\n  <= consume \n
                                                        // {% endif %}EOF       <= consume this line
    |   containsOutput NL                   # printLn
    |   containsOutput EOF                  # consumeLn
    |   block+ (NL|EOF)                     # consumeLn
    |   NL                                  # printLn
    ;

containsOutput
    :   (output|block)* output+ block+
    |   (output|block)* block+ output+
    |   output+
    ;

output
    :   varPrint
    |   text
    |   output output
    ;

varPrint
    :   OUTPUT
    ;

block
    :   STMT
    //|   LINE_COMMENT
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
    :   '{' -> type(TXT)
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
