parser grammar VineParser;
/*
// django syntax: https://docs.djangoproject.com/en/1.10/topics/templates/#the-django-template-language
// django:
// * var:        {{ foo }}
// * code:        {% if foo %}
// * comment:    {# comment #}
// detail: https://github.com/benjohnson/atom-django-templates/blob/master/grammars/html%20(django).cson

grammar VineScript;

script: sequence+ EOF;

sequence:    text
        |    code
        |    
        ;

code:    stmt
    |    text
    ;

// conditionStmt or controlStmt (control structure)
stmt:    conditionStmt
    |    assign
    |    print
    |    stmt ';'
    |    variable
    ;

// {{ foo }} is equal to {% print foo %}


// built-in filters:
// * upper
// * lower
// * truncatechars:8
// * truncatewords:2
// * pluralize
// * default:"nothing"
// * length
// * random
// * stringformat:"#.0"
// * yesno:"yeah,no,maybe"
// ---- added by myself: ----
// * min
// * max


// built-in functions:
//  * ParseInt(string)
//  * ParseNumber(string)
//  * Range(int):[]


// custom template tags
// https://docs.djangoproject.com/en/1.10/howto/custom-template-tags/#howto-writing-custom-template-tags
// custom template filters : 
// https://docs.djangoproject.com/en/1.10/howto/custom-template-tags/#howto-writing-custom-template-filters


//{% autoescape off %}
//    {{ body }}
//    {% autoescape on %}
//        {{ foo }}
//    {% endautoescape %}
//{% endautoescape %}

// preformatted: (on by default)
{% formatted on %}
{% formatted off %}
// or like html:
{% pre on %}
{% pre off %}
// maybe:
{% tab %}
{% br %}

'{%' 'set' ID ('='|'to') expr '%}'
*/
@members{
    private static readonly string reserved_char =
        "'\u000B' (vertical tabulation) is a reserved character and is not allowed to be used!";
}
options { tokenVocab=VineLexer; }

/*
 * Parser Rules
 */
passage
    :   block* NL? EOF
    |   { NotifyErrorListeners(reserved_char); } RESERVED_CHARS 
    //|   { NotifyErrorListeners("Error char"); } ERROR_CHAR
    ;

block
    :   /*stmtBlock+ NL (stmtBlock* NL+)  # noPrintBlock
    |   stmtBlock+ NL                   # noPrintBlock
    |   stmt text (stmt|text)* NL   # printBlockLn
    |   text stmt (stmt|text)* NL   # printBlockLn
    |   text NL                     # printBlockLn
    |   */NL                          # printBlockLn
    |   text                        # printBlock
    |   stmt                        # noPrintBlock
    ;

text:   TXT
    ;

stmt:   display
    |   stmtBlock
    ;

stmtBlock
    :   controlStmt
    |   command
    |   '{%' funcCall '%}'
    //|   LINE_COMMENT
    |   BLOCK_COMMENT
    ;

/**
 * Display something in the text (variable, expression, function return, ...)
 **/
display: '{{' expr '}}' ;

command
    :   '{%' 'set' variable ('='|'to') expr '%}' # assignStmt
    |   '{%' COMMAND expressionList? '%}'   # langCmd // {% formatted on %}, {% br %}, ...
    ;

funcCall
    :   ID '(' expressionList? ')'
    |   ID '(' expressionList? ')' { NotifyErrorListeners("Too many parentheses"); } ')'
    |   ID '(' expressionList?     { NotifyErrorListeners("Missing closing ')'"); }
    ;

// if, elif, else, for, end
controlStmt
    :   ifStmt (elifStmt)* (elseStmt)? endIfStmt 
    //|    '{%' 'for' ID 'in' expr '%}'
    ;

ifStmt:   '{%' 'if ' expr '%}' block* ;
elifStmt:   '{%' 'elif ' expr '%}' block* ;
elseStmt:   '{%' 'else' '%}' block* ;

endIfStmt:  '{%' 'end' '%}' ;

expr:   <assoc=right> left=expr '^' right=expr      # powExpr
    |   op=(MINUS|NOT) expr                         # unaryExpr 
    |   left=expr op=('*' | '/' | '%') right=expr   # mulDivModExpr
    |   left=expr op=('+'|'-') right=expr           # addSubExpr
    |   left=expr op=('<'|'>'|'<='|'>=') right=expr # relationalExpr
    |   left=expr op=('=='|'!=') right=expr         # equalityExpr
    |   left=expr '&&' right=expr                   # andExpr
    |   left=expr '||' right=expr                   # orExpr
    |   '(' expr ')'                                # parensExpr
    |   funcCall                                    # funcCallExpr
    |   atom                                        # atomExpr
    |   variable                                    # varExpr
    ;

expressionList
    :   expr (',' expr)*
    |   expr (','        { NotifyErrorListeners("Too many comma separators"); } ','+ expr)+
    |   expr (',' expr)* { NotifyErrorListeners("Too many comma separators"); } ',' 
    ;

atom:   INT             # intAtom
    |   FLOAT           # floatAtom
    |   (TRUE | FALSE)  # boolAtom
    |   STRING          # stringAtom
    |   NULL            # nullAtom
    |   { NotifyErrorListeners(reserved_char); }
        ILLEGAL_STRING  # errorAtom
    ;

// Variable access. The '$' prefix is optional
variable
    :   '$'? ID ('.' ID)*           # simpleVar
//    |   variable ('[' expr ']')+    # collectionVar
    ;

/*
variable:   ID                      # variableValue
        |   variable Dot variable   # combinedVariable
//        |   ID[expr]+    
        ;

postfixExpression
    :   atom //should be ID (and maybe allow STRING too)
    |   postfixExpression '[' expr ']'
    |   postfixExpression '(' argumentExpressionList? ')'
    |   postfixExpression '.' ID                            # attributeLookup 
    |   postfixExpression '++'
    |   postfixExpression '--'
    ;
*/
