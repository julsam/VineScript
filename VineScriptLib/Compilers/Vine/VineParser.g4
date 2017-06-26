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
    public enum EVineParseMode {
        FULL,
        EXPR
    }

    // by default, parse as a passage (full mode)
    public EVineParseMode ParseMode = EVineParseMode.FULL;

    private static readonly string errReservedChar =
        "'\u000B' (vertical tabulation) is a reserved character and is not allowed to be used!";
    
    private static readonly string errVarDefReservedKw =
        "Can't use a reserved keyword as a variable name!";
        
    private static readonly string errMissingSpaceBefore = "Missing space before ";
    private static readonly string errMissingSpaceAfter = "Missing space after ";
}
options { tokenVocab=VineLexer; }

/*
 * Parser Rules
 */
passage
    :   {ParseMode == EVineParseMode.EXPR}? evalExprMode NL? EOF // active only if we're expr parse mode
    |   block* NL? EOF
    |   { NotifyErrorListeners(errReservedChar); } RESERVED_CHARS 
    //|   { NotifyErrorListeners("Error char"); } ERROR_CHAR
    ;

evalExprMode
    :   expr
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

text
    :   TXT
    ;

stmt
    :   display
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
display
    : '{{' expr '}}'
    ;

command
    :   '{%' 'set' variable ('='|'to') expr '%}'    # assignStmt
    |   '{%' COMMAND expressionList? '%}'           # langCmd // {% formatted on %}, {% br %}, ...
    ;

funcCall
    :   ID '(' expressionList? ')'
    |   ID '(' expressionList? ')' { NotifyErrorListeners("Too many parentheses"); } ')'
    |   ID '(' expressionList?     { NotifyErrorListeners("Missing closing ')'"); }
    ;

newCollection
    :   '[' expressionList? ']'     # newArray
    |   LBRACE keyValueList? RBRACE # newDict
    // array errors:
    |   '[' expressionList? ']' { NotifyErrorListeners("Too many brackets"); } ']' # newArrayError
    |   '[' expressionList?     { NotifyErrorListeners("Missing closing ']'"); }   # newArrayError
    // dict errors:
    |   LBRACE keyValueList? RBRACE { NotifyErrorListeners("Too many braces"); } ']' # newDictError
    |   LBRACE keyValueList?        { NotifyErrorListeners("Missing closing '}'"); } # newDictError
    ;

// if, elif, else, for, end
controlStmt
    :   ifStmt (elifStmt)* (elseStmt)? endIfStmt 
    //|    '{%' 'for' ID 'in' expr '%}'
    ;

ifStmt
    :   '{%' 'if' wsa expr '%}' block*
    ;

elifStmt
    :   '{%' 'elif' wsa expr '%}' block*
    ;

elseStmt
    :   '{%' 'else' '%}' block*
    ;

endIfStmt
    :   '{%' 'endif' '%}'
    ;

expr
    :   <assoc=right> left=expr '^' right=expr      # powExpr
    |   op=('-'|'!') expr                           # unaryExpr 
    |   left=expr op=('*' | '/' | '%') right=expr   # mulDivModExpr
    |   left=expr op=('+'|'-') right=expr           # addSubExpr
    |   left=expr op=('<'|'>'|'<='|'>=') right=expr # relationalExpr
    |   left=expr op=('=='|'!=') right=expr         # equalityExpr
    |   left=expr ('&&'|wsb 'and' wsa) right=expr   # andExpr
    |   left=expr ('||'|wsb 'or' wsa) right=expr    # orExpr
    |   '(' expr ')'                                # parensExpr
    |   newCollection                               # collectionExpr
    |   funcCall                                    # funcCallExpr
    |   atom                                        # atomExpr
    |   variable                                    # varExpr
    ;

expressionList
    :   expr (',' expr)*
    |   expr (','        { NotifyErrorListeners("Too many comma separators"); } ','+ expr)+
    |   expr (',' expr)* { NotifyErrorListeners("Too many comma separators"); } ','
    ;

keyValue
    :   stringLiteral ':' expr
    |   { NotifyErrorListeners("Invalid key value: it should look like this: '\"key\": value'"); } .
    ;

keyValueList
    :   keyValue (',' keyValue)*
    |   keyValue (',' { NotifyErrorListeners("Too many comma separators"); } ','+ keyValue)+
    |   keyValue (',' keyValue)* { NotifyErrorListeners("Too many comma separators"); } ','
    ;

atom:   INT             # intAtom
    |   FLOAT           # floatAtom
    |   (TRUE | FALSE)  # boolAtom
    |   stringLiteral   # stringAtom
    |   NULL            # nullAtom
    ;

stringLiteral
    :   STRING
    |   { NotifyErrorListeners(errReservedChar); } ILLEGAL_STRING
    ;

// Variable access. The '$' prefix is optional
variable
    :   '$'? ID ('.' ID)*           # simpleVar
//    |   variable ('[' expr ']')+    # collectionVar
    |   { NotifyErrorListeners(errVarDefReservedKw); }
        reservedKeyword             # errVariable
    ;


// Call to force whitespace. Kind of hacky?
// If the current token is not a white space => error.
// We use semantic predicates here because WS is in a different
// channel that the parser can't access directly
wsb // before
    :   {
        if (_input.Get(_input.Index - 1).Type != WS) {
            string offendingSymbol = _input.Get(_input.Index - 1).Text;
            NotifyErrorListeners(errMissingSpaceBefore + "'" + offendingSymbol + "'");
        }
        }
    ;
wsa // after
    :   {
        if (_input.Get(_input.Index - 1).Type != WS) {
            string offendingSymbol = _input.Get(_input.Index - 1).Text;
            NotifyErrorListeners(errMissingSpaceAfter + "'" + offendingSymbol + "'");
        }
        }
    ;

reservedKeyword
    :   IF
    |   ELIF
    |   ELSE
    |   ENDIF
    |   KW_AND
    |   KW_OR
    |   TO
    |   SET
    |   TRUE
    |   FALSE
    |   NULL
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
