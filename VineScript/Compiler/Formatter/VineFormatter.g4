grammar VineFormatter;

/*
 * Parser Rules
 */
@lexer::members {
}

passage
    :   line* EOF?
    ;

line
    :   NL EOF                              # printLn
    |   block+ WS block+ (NL|EOF)           # consumeFullLine
    |   containsText NL (block NL?)+ EOF    # consumeLn // foobar {{ $var }}\n  <= consume \n
                                                        // << end >>EOF         <= consume this line
    |   containsText NL                     # printLn
    |   containsText EOF                    # consumeLn
    |   block+ (NL|EOF)                     # consumeLn
    |   NL                                  # printLn
    ;

containsText
    :   (text|block)* text+ block+
    |   (text|block)* block+ text+
    |   text+
    ;

text
    :   DISPLAY
    |   TXT
    |   WS
    |   verbatim
    ;

verbatim
    :   VERBATIM
    ;

block
    :   STMT
    |   LINE_COMMENT
    |   BLOCK_COMMENT
    ;

/*
 * Lexer Rules
 */

DISPLAY: '\u001E' .*? '\u001F' ; // output of {{ ... }}
 
TXT_ESC
    :   ('\\\\' | '\\`') -> type(TXT)
    ;

// Escape everything between ` ` or `` `` or ``` ```, etc
VERBATIM
    :   ('`')+ .*? ('`')+ {VineLexer.IsVerbatim(Text)}?
    ;

STMT:           '<<' .*? '>>' ;
BLOCK_COMMENT:  '/*' .*? '*/' ;
LINE_COMMENT:   '//' ~[\r\n]* ;

NL:     '\r'? '\n' ;
WS:     [ \t]+ ;

TXT_SPECIALS
    :   [`\\/<>*{}] -> type(TXT)
    ;
TXT :   ~[`\\<>*{}/\r\n\u001E\u001F]+
    ;

ERROR_CHAR: . ;
