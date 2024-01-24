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
    'program' => název lexikálního pravidla: reprezentace pravidel
*/

program: statement* EOF;

statement: variable_declaration | function_definition | expression_statement | if_statement | while_statement;

variable_declaration: data_type IDENTIFIER (ASSIGN expression)? ';';

data_type: 'INTEGER' | 'REAL' | 'STRING';

function_definition: 'PROCEDURE' IDENTIFIER '(' parameter_list? ')' block;

parameter_list: variable_declaration (',' variable_declaration)*;

expression_statement: expression ';';

if_statement: 'IF' expression block ('ELSE' block)?;

while_statement: 'WHILE' expression block;

expression: additive_expression;

additive_expression: multiplicative_expression (ADDITIVE_OPPERANDS multiplicative_expression)*; 

multiplicative_expression: primary_expression (MULTIPLICATIVE_OPPERANDS primary_expression)*; 

primary_expression: IDENTIFIER | NUMBER | STRING | '(' expression ')';

block: '{' statement* '}';

// ignorování whitespaců a zástupných znaků pro tabulátor (\t) a zakončení řádku (\r\n)
WS: [ \t\r\n]+ -> skip;
// ignorování komentářů '(*' -> začatek komentáře, '.*?' -> libovolný počet znaků, '*)' -> konec komentáře
COMMENT: '(*' .*? '*)' -> skip;
// prvni musi byt pismeno nebo '_', dalši muže byt pismeno/čislice/'_' 
IDENTIFIER: [a-zA-Z_][a-zA-Z0-9_]*; 
// muže začinat '+' nebo '-', pak musi nasledovat čislice, pak muže nasledovat desetinná část s číslicemi
NUMBER: (ADDITIVE_OPPERANDS)? (DIGIT)+ ('.' (DIGIT)+)?; 
// musí začínat "" uvozovkama, uvnitř uvozovek muže obsahovat jakýkoliv počet libovolných znaků, ale nesmí obsahovat další uvozovky
STRING: '"' (~["])* '"'; 
DIGIT: [0-9]; 
ASSIGN: ':=';
MULTIPLICATIVE_OPPERANDS: '*' | '%' | '/';
ADDITIVE_OPPERANDS: '+' | '-';