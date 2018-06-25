parser grammar VineParser;

@members{
    public enum EVineParseMode {
        SINGLE_PASSAGE,
        EVAL_EXPR
    }

    // by default, parse as a single passage
    public EVineParseMode ParseMode = EVineParseMode.SINGLE_PASSAGE;
    
    internal static readonly string errReservedChar000B =
        "'\\v' (vertical tabulation) is a reserved character and is not allowed to be used!";
    internal static readonly string errReservedChar001E =
        "'\\u001E' (record separator) is a reserved character and is not allowed to be used!";
    internal static readonly string errReservedChar001F =
        "'\\u001F' (unit separator) is a reserved character and is not allowed to be used!";
    
    internal static readonly string errVarDefReservedKw =
        "Can't use a reserved keyword as a variable name!";
        
    internal static readonly string errMissingSpaceBefore = "Missing space before ";
    internal static readonly string errMissingSpaceAfter = "Missing space after ";

    internal static readonly string errAssignMissingSet =
        "Are you trying to assign a value to a variable without using the keyword 'set'?"
        + System.Environment.NewLine + "Here's a example of assignation: << set myvar = 0 >>";
        
    private void ReservedChar()
    {
        var token = _input.Lt(-1);
        ReservedChar(token);
    }

    private void ReservedChar(IToken token)
    {
        string msg = "";
        if (token.Text.Contains("\u000B")) {
            msg = errReservedChar000B;
        }
        else if (token.Text.Contains("\u001E")) {
            msg = errReservedChar001E;
        }
        else if (token.Text.Contains("\u001F")) {
            msg = errReservedChar001F;
        }
        NotifyPrev(token, msg);
    }

    private void NotifyPrev(string msg)
    {
        var token = _input.Lt(-1);
        NotifyPrev(token, msg);
    }

    private void NotifyPrev(IToken token, string msg)
    {
        NotifyErrorListeners(token, msg, null);
    }
}
options { tokenVocab=VineLexer; }

/*
 * Parser Rules
 */
passage
    :   {ParseMode == EVineParseMode.EVAL_EXPR}? evalExprMode NL? EOF // active only if we're expr parse mode
    |   block* NL? EOF
    |   RESERVED_CHARS { ReservedChar(); }
    |   { NotifyErrorListeners("Error char"); } ERROR_CHAR
    ;

evalExprMode
    :   expr
    ;

// directOutput will add the text/code markups to the output.
// The output will then be parsed by the formatter.
block
    :   NL              # directOutput
    |   verbatimStmt    # noOutput      // `as it is, escape << tags >> too`
    |   text            # directOutput  // everything else
    |   display         # noOutput      // {{ foo }}
    |   controlStmt     # noOutput      // << open stmt >> something << close stmt >>
    |   simpleStmtBlock # noOutput      // << set foo = 0 >>
    |   link            # noOutput      // [[label|link]]
    |   collapseStmt    # noOutput      // { foo\nbar } => foobar
    |   BLOCK_COMMENT   # directOutput  // /* comment */
    |   LINE_COMMENT    # directOutput  // // inline comment
    |   RESERVED_CHARS { ReservedChar(); } # blockError
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
    :   LLINK title=linkContent+ RLINK
    |   LLINK title=linkContent+ '|' '|' code=block* CLOSE_LINK
    |   LLINK title=linkContent+ '|' destination=linkContent+ RLINK
    |   LLINK title=linkContent+ '|' destination=linkContent+ '|' code=block* CLOSE_LINK
    ;

linkContent
    :   LINK_TEXT+
    |   RESERVED_CHARS { ReservedChar(); }
    ;

verbatimStmt
    :   VERBATIM
    ;

collapseStmt
    :   LCOLLAPSE
    |   RCOLLAPSE
    //|   LCOLLAPSE block* RCOLLAPSE // could use this rule if we want to be more strict
    ;

/**
 * Display something in the text (variable, expression, function return, ...)
 **/
display
    :   LOUTPUT expr ROUTPUT
    ;

setStmt
    :   'set' assignList
    |   { NotifyErrorListeners(errAssignMissingSet); }
        // using assignList creates too much problems (because of reservedKeywords
        // defined in the 'variable' rule). It's easier to specify it this way:
        ('$')? ID (sequenceAccess)* op=('='|'to'|'+='|'-='|'*='|'/='|'%=') expr
    ;

assignList
    :   assign (',' assign)*
    |   assignList { NotifyErrorListeners("Missing ',' separator"); } assign (',' assign)*
    |   assignList { NotifyErrorListeners("Too many ','"); } ',' ',' assignList
    ;

assign
    :   variable (sequenceAccess)* op=('='|'to') expr
    |   variable (sequenceAccess)* op=('+='|'-='|'*='|'/='|'%=') expr
    |   { NotifyErrorListeners("Missing assignation operator and expression after the variable"); }
        variable (sequenceAccess)* // this could be allowed to declare a var <<set myvar>>
    |   variable (sequenceAccess)*
        { NotifyErrorListeners("Missing assignation operator before expression"); } expr
    |   variable (sequenceAccess)* { NotifyErrorListeners("Missing expression after the operator"); }
        op=('='|'to'|'+='|'-='|'*='|'/='|'%=')
        
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
    :   ifStmt (elifStmt)* (elseStmt)? endStmt  # ifCtrlStmt
    |   forStmt endStmt                         # forCtrlStmt
    |   ifStmt (elifStmt)* (elseStmt)? {NotifyErrorListeners("'if' statement is missing a closing '<< end >>'");} # ctrlStmtError
    |   ifStmt (elifStmt)* elseStmt {NotifyErrorListeners("Too many 'else' statements");} (elseStmt)+  # ctrlStmtError
    |   ifStmt {NotifyErrorListeners("Misplaced '<< else >>'");} (elseStmt) (elifStmt)+ endStmt # ctrlStmtError
    |   forStmt {NotifyErrorListeners("'for' statement is missing a closing '<< end >>'");}  # ctrlStmtError
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

endStmt
    :   '<<' 'end' '>>'
    ;

forStmt
    :   '<<' 'for' wsa variable 'in' expr '>>' NL? block*                       # forValueStmt
    |   '<<' 'for' wsa variable 'in' interval '>>' NL? block*                   # forValueStmt
    |   '<<' 'for' wsa key=variable ',' val=variable 'in' expr '>>' NL? block*  # forKeyValueStmt
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
    |   expr (sequenceAccess)+                      # anonymSequence
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
    |   ILLEGAL_STRING { ReservedChar(); }
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
            string offendingSymbol = _input.Get(_input.Index).Text;
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
