using Algol_interpreter.Grammar;

namespace Algol_interpreter.Visitor;

public class AlgolVisitor : Algol60BaseVisitor<object>
{
    private Dictionary<string, object?> Variables { get; set; } = new()
    {
        ["Print"] = new Action<object?[]>(Print),
        ["PI"] = Math.PI
    };


    /* Override metody z AlgolBaseVisitor */

    public override object VisitStatement(Algol60Parser.StatementContext context)
    {
        return null;
    }

    public override object VisitReturn_statement(Algol60Parser.Return_statementContext context)
    {
        return null;
    }

    public override object VisitVariable_type(Algol60Parser.Variable_typeContext context)
    {
        return null;
    }

    public override object VisitArray_declaration(Algol60Parser.Array_declarationContext context)
    {
        return null;
    }

    public override object VisitArray_inicialization(Algol60Parser.Array_inicializationContext context)
    {
        return null;

    }

    public override object VisitArray_access(Algol60Parser.Array_accessContext context)
    {
        return null;
    }

    public override object VisitFunction_call(Algol60Parser.Function_callContext context)
    {
        return null;
    }

    public override object VisitVariable_declaration(Algol60Parser.Variable_declarationContext context)
    {
        var dataType = context.variable_type().GetText();   // datový typ proměnné
        var identifier = context.IDENTIFIER().GetText();    // nazev proměnné
        object? value = null;
        if (context.expression() != null)
        {
            value = Visit(context.expression());
            if (!IsExpectedDataType(dataType, value))
            {
                throw new AlgolVisitorExceptions.IncorrectAssignmentException(identifier, value, dataType);
            }
        }

        Variables[identifier] = value; // uložení proměnné do slovníku
        return value;
    }

    public override object VisitExpression_statement(Algol60Parser.Expression_statementContext context)
    {
        return null;

    }

    public override object VisitExpression(Algol60Parser.ExpressionContext context)
    {
        return null;

    }

    public override object VisitPrimary_expression(Algol60Parser.Primary_expressionContext context)
    {
        return null;

    }

    public override object VisitBlock(Algol60Parser.BlockContext context)
    {
        return null;

    }

    public override object VisitIf_statement(Algol60Parser.If_statementContext context)
    {
        return null;

    }

    public override object VisitWhile_statement(Algol60Parser.While_statementContext context)
    {
        return null;

    }

    public override object VisitData_type(Algol60Parser.Data_typeContext context)
    {
        return null;

    }

    public override object VisitFunction_definition(Algol60Parser.Function_definitionContext context)
    {
        return null;

    }

    public override object VisitParameter_list(Algol60Parser.Parameter_listContext context)
    {
        return null;

    }

    public override object VisitAdditive_expression(Algol60Parser.Additive_expressionContext context)
    {
        var left = Visit(context.multiplicative_expression(0));
        var right = Visit(context.multiplicative_expression(1));

        var op = context.ADDITIVE_OPPERANDS().ToString();
        return op switch
        {
            "+" => Addition(left, right),
            "-" => Substraction(left, right),
            _ => throw new AlgolVisitorExceptions.UnsupportedAdditiveOpperatorException(op)
        };
    }



    public override object VisitMultiplicative_expression(Algol60Parser.Multiplicative_expressionContext context)
    {

        var left = Visit(context.primary_expression(0));
        var right = Visit(context.primary_expression(1));

        var op = context.MULTIPLICATIVE_OPPERANDS().ToString();
        return op switch
        {
            "*" => Multiplication(left, right),
            "/" or "%" => Division(left, right, op),
            _ => throw new AlgolVisitorExceptions.UnsupportedMultiplicativeOpperatorException(op)
        };

    }


    /* Pomocné metody */

    // Výpis argumentů do konzole
    private static void Print(object?[] args)
    {
        foreach (var arg in args)
        {
            Console.WriteLine(arg);
        }
    }

    // Check datového typu
    private static bool IsExpectedDataType(string datatype, object obj)
    {
        return datatype switch
        {
            "INTEGER" => obj is int,
            "REAL" => obj is float,
            "STRING" => obj is string,
            _ => throw new AlgolVisitorExceptions.UnsupportedDataTypeException(datatype)
        };
    }

    /* Metody pro zpracování základních matematických operací */
    private static object Substraction(object left, object right)
    {
        switch (left)
        {
            case int l when right is int r:
                return l - r;
            case float l when right is float r:
                return l - r;
            case int l when right is float r:
                return l - r;
            case float l when right is int r:
                return l - r;
        }

        if (left is string || right is string)
        {
            return $"{left}+{right}";
        }
        throw new AlgolVisitorExceptions.UnsupportedDataTypeException($"{left.GetType()} nebo {right.GetType()}");
    }

    private static object Addition(object left, object right)
    {
        switch (left)
        {
            case int l when right is int r:
                return l + r;
            case float l when right is float r:
                return l + r;
            case float l when right is int r:
                return l - r;
            case int l when right is float r:
                return l - r;
        }

        if (left is string || right is string)
        {
            return $"{left}+{right}";
        }
        throw new AlgolVisitorExceptions.UnsupportedDataTypeException($"{left.GetType()} nebo {right.GetType()}");
    }


    private static object Division(object left, object right, object op)
    {
        if (!Equals(left, right))
        {
            throw new AlgolVisitorExceptions.UnsupportedMultiplicativeValueException();
        }

        return op.ToString() switch
        {
            "/" => // normal
                left switch
                {
                    int l when right is int r => l / r,
                    float l when right is float r => l / r,
                    int l when right is float r => l / r,
                    float l when right is int r => l / r,
                    _ => throw new AlgolVisitorExceptions.UnsupportedDataTypeException(
                        $"{left.GetType()} nebo {right.GetType()}")
                },
            "%" => // modulo
                left switch
                {
                    int l when right is int r => l % r,
                    float l when right is float r => l % r,
                    int l when right is float r => l % r,
                    float l when right is int r => l % r,
                    _ => throw new AlgolVisitorExceptions.UnsupportedDataTypeException(
                        $"{left.GetType()} nebo {right.GetType()}")
                },
            _ => 0
        };
    }

    private static object Multiplication(object left, object right)
    {
        return left switch
        {
            int l when right is int r => l * r,
            float l when right is float r => l * r,
            int l when right is float r => l * r,
            float l when right is int r => l * r,
            _ => throw new AlgolVisitorExceptions.UnsupportedDataTypeException($"{left.GetType()} nebo {right.GetType()}")
        };
    }

    private new static bool Equals(object? left, object? right)
    {
        if (left == null && right == null)
            return true;

        if (left == null || right == null)
            return false;

        return left switch
        {
            float l when right is float r => l == r,
            int l when right is int r => l == r,
            string l when right is string r => l == r,
            _ => throw new AlgolVisitorExceptions.UnsupportedDataTypeException($"{left.GetType()} nebo {right.GetType()}")
        };
    }
}