using Algol_interpreter.Grammar;
using Algol_interpreter.Visitor;
using Antlr4.Runtime;

var fileName = "priklad1.txt";

var fileContents = File.ReadAllText(fileName);

var inputStream = new AntlrInputStream(fileContents);
var algolLexer = new Algol60Lexer(inputStream);
var commonTokenStream = new CommonTokenStream(algolLexer);
var algolParser = new Algol60Parser(commonTokenStream);

var algolContext = algolParser.program();
var visitor = new AlgolVisitor();

visitor.Visit(algolContext);

Console.ReadKey();