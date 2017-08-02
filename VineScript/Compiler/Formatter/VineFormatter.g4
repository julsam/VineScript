grammar VineFormatter;

/*
 * Parser Rules
 */

passage
    :   line* EOF?
    ;

line
    :   NL EOF                              # printLn
    |   block+ WS block+ (NL|EOF)           # consumeFullLine
    |   containsText NL (block NL?)+ EOF    # consumeLn // foobar {{ $var }}\n  <= consume \n
                                                        // << endif >>EOF       <= consume this line
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
    :   TXT
    |   WS
    //|   DISPLAY
    ;

block
    :   STMT
    |   LINE_COMMENT
    |   BLOCK_COMMENT
    ;

/*
 * Lexer Rules
 */

//DISPLAY: '\u001E' .*? '\u001F' ; // output of {{ ... }}

// Escaped commands, eg: \<< this is text >> 
TXT_ESC
    :   ('\\\\' | '\\{' | '\\/' | '\\<') -> type(TXT)
    ;

STMT:           '<<' .*? '>>' ;
BLOCK_COMMENT:  '/*' .*? '*/' ;
LINE_COMMENT:   '//' ~[\r\n]* ;

NL:     '\r'? '\n' ;
WS:     [ \t]+ ;

TXT_SPECIALS
    :   [\\/<>{}] -> type(TXT)
    ;
TXT :   ~[\\<>{}/\r\n]+
    ;

ERROR_CHAR: . ;
