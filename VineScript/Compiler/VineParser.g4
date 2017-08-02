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

// {{ foo }} is equal to << print foo >>


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


//<< autoescape off >>
//    {{ body }}
//    << autoescape on >>
//        {{ foo }}
//    << endautoescape >>
//<< endautoescape >>

// preformatted: (on by default)
<< formatted on >>
<< formatted off >>
// or like html:
<< pre on >>
<< pre off >>
// maybe:
<< tab >>
<< br >>

'<<' 'set' ID ('='|'to') expr '>>'
*/
@members{
    public enum EVineParseMode {
        SINGLE_PASSAGE,
        EVAL_EXPR
    }

    // by default, parse as a single passage
    public EVineParseMode ParseMode = EVineParseMode.SINGLE_PASSAGE;

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
    :   {ParseMode == EVineParseMode.EVAL_EXPR}? evalExprMode NL? EOF // active only if we're expr parse mode
    |   block* NL? EOF
    |   { NotifyErrorListeners(errReservedChar); } RESERVED_CHARS 
    //|   { NotifyErrorListeners("Error char"); } ERROR_CHAR
    ;

evalExprMode
    :   expr
    ;

// directOutput will add the text/code markups to the output.
// The output will then be parsed by the formatter.
block
    :   NL              # directOutput
    |   text            # directOutput  // foobar
    |   display         # noOutput      // {{ foo }}
    |   controlStmt     # noOutput      // << open stmt >> something << close stmt >>
    |   simpleStmtBlock # directOutput  // << set foo = 0 >>
    |   link            # noOutput      // [[label|link]]
    |   BLOCK_COMMENT   # directOutput  // /* comment */
    |   LINE_COMMENT    # directOutput  // // inline comment
    ;

text
    :   TXT
    ;

simpleStmtBlock
    :   '<<' setStmt '>>'
    |   '<<' unsetStmt '>>'
    |   '<<' funcCall '>>'
    ;

link
    :   '[[' title=linkContent+ ']]'
    |   '[[' title=linkContent+ '|' passageName=linkContent+ ']]'
    |   '[[' title=linkContent+ '->' passageName=linkContent+ ']]'
    |   '[[' passageName=linkContent+ '<-' title=linkContent+ ']]'
    ;

linkContent
    :   LINK_TEXT+
    ;

/**
 * Display something in the text (variable, expression, function return, ...)
 **/
display
    :   '{{' expr '}}'
    ;

setStmt
    :   'set' assignList
    ;

assignList
    :   assign (',' assign)*
    |   assignList { NotifyErrorListeners("Missing ',' separator"); } assign (',' assign)*
    |   assignList { NotifyErrorListeners("Too many ','"); } ',' ',' assignList
    ;

assign
    :   variable (sequenceAccess)* op=('='|'to') expr
    |   variable (sequenceAccess)* op=('+='|'-='|'*='|'/='|'%=') expr
    |   variable (sequenceAccess)* { NotifyErrorListeners("Missing assignation operator"); } expr
    |   variable (sequenceAccess)* op=('='|'to'|'+='|'-='|'*='|'/='|'%=')
        { NotifyErrorListeners("Missing expression after the operator"); }
    ;

unsetStmt
    :   'unset' unsetList
    ;

unsetList
    :   variable (',' variable)*
    |   unsetList { NotifyErrorListeners("Missing ',' separator"); } variable (',' variable)*
    |   unsetList { NotifyErrorListeners("Too many ','"); } ',' ',' unsetList
    ;

funcCall
    :   ID '(' expressionList? ')'
    |   ID '(' expressionList? ')' { NotifyErrorListeners("Too many parentheses"); } ')'
    |   ID '(' expressionList?     { NotifyErrorListeners("Missing closing ')'"); }
    ;

newSequence
    :   LBRACK expressionList? RBRACK   # newArray
    |   LBRACE keyValueList? RBRACE     # newDict
    // array errors:
    |   LBRACK expressionList? RBRACK { NotifyErrorListeners("Too many brackets"); } RBRACK # newArrayError
    |   LBRACK expressionList?     { NotifyErrorListeners("Missing closing ']'"); }   # newArrayError
    // dict errors:
    |   LBRACE keyValueList? RBRACE { NotifyErrorListeners("Too many braces"); } RBRACE # newDictError
    |   LBRACE keyValueList?        { NotifyErrorListeners("Missing closing '}'"); }    # newDictError
    ;

// if, elif, else, for, end
controlStmt
    :   ifStmt (elifStmt)* (elseStmt)? endIfStmt    # ifCtrlStmt
    |   forStmt endForStmt                          # forCtrlStmt
    ;

ifStmt
    :   '<<' 'if' wsa expr '>>' block*
    ;

elifStmt
    :   '<<' 'elif' wsa expr '>>' block*
    ;

elseStmt
    :   '<<' 'else' '>>' block*
    ;

endIfStmt
    :   '<<' 'end' '>>'
    ;

forStmt
    :   '<<' 'for' wsa variable 'in' expr '>>' NL? block*
    //|   '<<' 'for' wsa key=variable ',' val=variable 'in' expr '>>' NL? block*
    |   '<<' 'for' wsa variable 'in' interval '>>' NL? block*
    ;

endForStmt
    :   '<<' 'end' '>>'
    ;

expr
    :   <assoc=right> left=expr '^' right=expr      # powExpr
    |   op=(MINUS|'!') expr                         # unaryExpr 
    |   left=expr op=('*' | DIV | '%') right=expr   # mulDivModExpr
    |   left=expr op=('+'|MINUS) right=expr         # addSubExpr
    |   left=expr op=(LT|GT|'<='|'>=') right=expr   # relationalExpr
    |   left=expr op=('=='|'!=') right=expr         # equalityExpr
    |   left=expr ('&&'|wsb 'and' wsa) right=expr   # andExpr
    |   left=expr ('||'|wsb 'or' wsa) right=expr    # orExpr
    |   '(' expr ')'                                # parensExpr
    |   newSequence                                 # sequenceExpr
    |   funcCall                                    # funcCallExpr
    |   atom                                        # atomExpr
    |   variable (sequenceAccess)*                  # varExpr
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
    :   '$'? ID ('.' ID)*           
    |   { NotifyErrorListeners(errVarDefReservedKw); }
        reservedKeyword
    ;

sequenceAccess
    :   LBRACK expr RBRACK
    ;

interval
    :   left=expr '...' right=expr
    ;

// Call to force whitespace. Kind of hacky?
// If the current token is not a white space => error.
// We use semantic predicates here because WS is in a different
// channel and the parser can't access directly
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
    |   END
    |   KW_AND
    |   KW_OR
    |   TO
    |   SET
    |   UNSET
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
