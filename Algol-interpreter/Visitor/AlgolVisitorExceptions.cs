namespace Algol_interpreter.Visitor;

public abstract class AlgolVisitorExceptions
{
    public class AlgolInterpreterException : Exception
    {
        protected AlgolInterpreterException(string message) : base(message) { }
    }

    public class UnsupportedDataTypeException : AlgolInterpreterException
    {
        public UnsupportedDataTypeException(object? datatype) : base($"Nepodporovaný datový typ: {datatype}") { }
    }

    public class IncorrectAssignmentException : AlgolInterpreterException
    {
        public IncorrectAssignmentException(object? identifier, object? value, object? datatype)
            : base($"Nelze přiřadit proměnnou {identifier} s hodnotou {value} do datového typu {datatype}") { }
    }

    public class UnsupportedAdditiveOpperatorException : AlgolInterpreterException
    {
        public UnsupportedAdditiveOpperatorException(object? obj) : base($"Nepodporovaný opperand pro sčítání/odčítání: {obj}") { }
    }

    public class UnsupportedMultiplicativeOpperatorException : AlgolInterpreterException
    {
        public UnsupportedMultiplicativeOpperatorException(object? obj) : base($"Nepodporovaný opperand pro násobení/dělení: {obj}") { }
    }
    
    public class UnsupportedComparisonOpperatorException : AlgolInterpreterException
    {
        public UnsupportedComparisonOpperatorException(object? obj) : base($"Nepodporovaný opperand pro porovnávání: {obj}") { }
    }    
    
    public class UnsupportedSpecificComparisonOpperatorException : AlgolInterpreterException
    {
        public UnsupportedSpecificComparisonOpperatorException(object? obj) : base($"Nepodporovaný specifický operátor: {obj}") { }
    }

    public class InccorectArgumentsCountException : Exception
    {
        public InccorectArgumentsCountException(object? procName) : base($"Nesprávný počet argumentů funkce: {procName}") { }
    }

    public class NonDeclaredMemberException : Exception
    {
        public NonDeclaredMemberException(object? procName) : base($"Člen {procName} není deklarován") { }
    }  
    
    public class NonDeclaredVariableException : Exception
    {
        public NonDeclaredVariableException(object? varName) : base($"Proměnná {varName} není deklarováná") { }
    }    
    
    public class ArrayOutOfBoundsException : Exception
    {
        public ArrayOutOfBoundsException(object? arraySize) : base($"Velikost pole {arraySize} překročena") { }
    }   
}