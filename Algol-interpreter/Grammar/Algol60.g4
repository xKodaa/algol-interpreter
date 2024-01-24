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

/* LEXIKÁLNÍ PRAVIDLA */

// obecná pravidla
program: statement* EOF;
statement: variable_declaration | function_definition | expression_statement | if_statement | while_statement;
variable_declaration: data_type IDENTIFIER (ASSIGN expression)? ';';
expression_statement: expression ';';
expression: additive_expression;
// za primární výraz je možno dosazovat identifier - např. název proměnné, number - nějaké číslo, string, či výraz v závorkách (x+5...)  
primary_expression: IDENTIFIER | NUMBER | STRING | '(' expression ')';  
block: '{' statement* '}';

// cykly a podmínky 
if_statement: 'IF' expression block ('ELSE' block)?;
while_statement: 'WHILE' expression block;

// lokální a globální typované proměnné
// TODO dodělat scope lokalnich a globalnich
data_type: 'INTEGER' | 'REAL' | 'STRING';

// funkce s parametry
function_definition: 'PROCEDURE' IDENTIFIER '(' parameter_list? ')' block;
parameter_list: variable_declaration (',' variable_declaration)*;

// aritmetické výrazy
additive_expression: multiplicative_expression (ADDITIVE_OPPERANDS multiplicative_expression)*; 
multiplicative_expression: primary_expression (MULTIPLICATIVE_OPPERANDS primary_expression)*; 

/* TERMINÁLY (klíčová slova) */

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