namespace Algol_interpreter;

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
}