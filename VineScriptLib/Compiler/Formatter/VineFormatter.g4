grammar VineFormatter;

/*
 * Parser Rules
 */

passage
    :   line* EOF?
    ;

line
    :   NL EOF                              # printLn
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

BACKSLASH_ESC
    :   '\\\\' -> type(TXT)
    ;
TXT_ESC_LBRACE
    :   '\\{' -> type(TXT)
    ;
TXT_ESC_SLASH
    :   '\\/' -> type(TXT)
    ;
TXT_ESC_LT
    :   '\\<' -> type(TXT)
    ;

STMT:           '<<' .*? '>>' ;
BLOCK_COMMENT:  '/*' .*? '*/' ;
LINE_COMMENT:   '//' ~[\r\n]* ;

NL:     '\r'? '\n' ;
WS:     [ \t]+ -> skip ;

TXT_LBRACE
    :   '{' -> type(TXT)
    ;
TXT_RBRACE
    :   '}' -> type(TXT)
    ;
TXT_LT
    :   '<' -> type(TXT)
    ;
TXT_GT
    :   '>' -> type(TXT)
    ;
TXT_SLASH
    :   '/' -> type(TXT)
    ;
TXT_ESC
    :   '\\' -> type(TXT)
    ;
TXT :   ~[\\<>{}/\r\n]+
    ;

ERROR_CHAR: . ;
