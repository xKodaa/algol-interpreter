using Algol_interpreter.Grammar;
using Algol_interpreter.Visitor;
using Antlr4.Runtime;

namespace Algol_interpreter
{
    internal static class Program
    {
        private static void Main()
        {
            RunAllExamples();
            Console.WriteLine("Stiskněte libovolné tlačítko pro ukončení programu...");
            Console.ReadKey();
        }

        private static void RunAllExamples()
        {
            RunInterpreter("../../../AlgolExamples/priklad1.txt");
            RunInterpreter("../../../AlgolExamples/priklad2.txt");
            RunInterpreter("../../../AlgolExamples/priklad3.txt");
        }

        private static void RunInterpreter(string fileName)
        {
            Console.WriteLine("Kód ze souboru: " + Path.GetFileName(fileName));
            var fileContents = File.ReadAllText(fileName);

            var inputStream = new AntlrInputStream(fileContents);
            var algolLexer = new Algol60Lexer(inputStream);
            var commonTokenStream = new CommonTokenStream(algolLexer);
            var algolParser = new Algol60Parser(commonTokenStream);

            var algolContext = algolParser.program();
            var visitor = new AlgolVisitor();

            visitor.Visit(algolContext);
            Console.WriteLine();
        }
    }  
}

