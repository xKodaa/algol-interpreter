grammar Algol60;

/* 
ANTLR => generator parseru
Vysvětlivky:
    * => 0 nebo více
    | => or
    
*/
 
program: line* EOF; // EOF => end of file

line: statement | if_block | whileBlock;
statement: assignment | function ';';
assignment: IDENTIFIER ':=' expression;
function: ' ';

if_block: 'if' expression block ('else' else_if_block) ;
expression: ' ';
else_if_block: block | if_block;
block: '{' line * '}';
whileBlock: 'while' expression block;


// "proměnné pro usnadnění zápisu... antlr4 thing"
WS: [ \t\r\n]+ -> skip; // whitespace, \t - tabulator, \r\n - konec radku
IDENTIFIER: [a-zA-Z_][a-zA-Z0-9_]*;