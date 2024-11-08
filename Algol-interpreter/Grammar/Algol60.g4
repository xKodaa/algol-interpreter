grammar Algol60;

/* 
    ANTLR => generator parseru, lexeru, ANTLR vytváří gramatiku
    pomocí syntaxe podobné BNF (metoda zápisu gramatiky)
    Vysvětlivky:
        * => 0 nebo více
        + => alespoň jednou
        | => or
        ? => volitelná část
        ( ) => spojení nererminálnů
        # => label, vytvotri zaroven metodu ve visitorovi
        'program' => název lexikálního pravidla: reprezentace pravidel;
*/

/* LEXIKÁLNÍ PRAVIDLA */

// struktura 
program: 'BEGIN' declaration* 'END'; 
block: 'BEGIN' declaration* statement* return_statement? 'END'; 
declaration: statement | variable_declaration | variable_type | array_declaration | array_2d_declaration | function_declaration | if_block | while_block;
statement: (asignment | function_call) ';'; 
asignment: IDENTIFIER ASSIGN expression; 
return_statement: 'RETURN' expression ';'; 
constant_type: DIGIT | NUMBER | STRING; 
expression: 
    constant_type                                       #constant_expression
    | IDENTIFIER                                        #identifier_expression
    | function_call                                     #function_call_expression
    | expression MULTIPLICATIVE_OPPERANDS expression    #multiplicative_expression  
    | expression ADDITIVE_OPPERANDS expression          #additive_expression
    | expression COMPARISON_OPPERANDS expression        #comparison_expression 
    | expression LOGICAL_OPPERANDS expression           #logical_expression
    | array_access                                      #array_expression
    | array_2d_access                                   #array_2d_expression;

// cykly a podmínky 
if_block: 'IF' expression 'THEN' block ('ELSE' else_if_block)?;
else_if_block: block | if_block; 
while_block: 'WHILE' expression block; 

// lokální a globální typované proměnné
variable_declaration: variable_type IDENTIFIER (ASSIGN expression)? ';'; 
variable_type: data_type | 'GLOBAL ' data_type; 
data_type: 'INTEGER' | 'REAL' | 'STRING'; 

// pole
array_declaration: variable_type IDENTIFIER '[' DIGIT ']' (ASSIGN array_inicialization)? ';'; 
array_inicialization: '[' expression (',' expression)* ']'; 
array_access: IDENTIFIER '[' expression ']';

// 2D pole
array_2d_declaration: variable_type IDENTIFIER '[' DIGIT ']' '[' DIGIT ']' (ASSIGN array_2d_inicialization)? ';';
array_2d_inicialization: '[' ('[' expression (',' expression)* ']' ','? )* ']';
array_2d_access: IDENTIFIER '[' expression ']' '[' expression ']';

// funkce s parametry
function_declaration: 'PROCEDURE' IDENTIFIER '(' parameter_list? ')' block; 
parameter_list: parameter (',' parameter)*; 
parameter: variable_type IDENTIFIER; 
function_call: IDENTIFIER '(' (expression (',' expression)*)? ')'; 

/* TERMINÁLY */ 

// ignorování whitespaců, zakončení řádků a komentářů
WS: [ \t\r\n]+ -> skip;
COMMENT: '(*' .*? '*)' -> skip;
IDENTIFIER: [a-zA-Z_][a-zA-Z0-9_]*;  // 1. znak musi byt pismeno nebo '_', dalši muže byt pismeno/čislice/'_'
DIGIT: [0-9]+;
NUMBER: DIGIT '.' DIGIT;  // muže začinat '+' nebo '-', pak musi nasledovat čislice, pak muže nasledovat desetinná čiselna čast
STRING: '"' (~["])* '"';  // musi začinat "" a uvnitr muze byt libovolny pocet znaku krome ""
ASSIGN: ':=';
MULTIPLICATIVE_OPPERANDS: '*' | '%' | '/';
ADDITIVE_OPPERANDS: '+' | '-';
COMPARISON_OPPERANDS: '==' | '!=' | '<' | '>' | '<=' | '>=';
LOGICAL_OPPERANDS: 'AND' | 'OR';