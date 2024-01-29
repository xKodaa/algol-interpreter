using System.Globalization;
using Algol_interpreter.Grammar;

namespace Algol_interpreter.Visitor;

public class AlgolVisitor : Algol60BaseVisitor<object>
{
    private Dictionary<string, object?> Variables { get; set; } = new()
    {
        ["Print"] = new Func<object?[], object?>(Print)   // přidání předdefinované funkce print pro výpis do konzole
    };

    #region Struktura

    public override object VisitConstant_type(Algol60Parser.Constant_typeContext context)
    {
        if (context.DIGIT() is { } i)
        {
            return int.Parse(i.GetText());
        }

        if (context.NUMBER() is { } f)
        {
            return float.Parse(context.NUMBER().GetText(), CultureInfo.InvariantCulture);
        }

        if (context.STRING() is { } s)
        {
            return s.GetText()[1..^1];
        }

        throw new AlgolVisitorExceptions.UnsupportedDataTypeException(context.GetText());
    }

    public override object VisitIdentifier_expression(Algol60Parser.Identifier_expressionContext context)
    {
        var varName = context.IDENTIFIER().GetText();

        if (!Variables.ContainsKey(varName))
        {
            throw new AlgolVisitorExceptions.NonDeclaredVariableException(varName);
        }

        return Variables[varName] ?? "";
    }

    public override object VisitAsignment(Algol60Parser.AsignmentContext context)
    {
        var identifier = context.IDENTIFIER().GetText();
        var value = Visit(context.expression());
        Variables[identifier] = value;
        return value;
    }

    public override object VisitReturn_statement(Algol60Parser.Return_statementContext context)
    {
        Variables["_returnValue"] = Visit(context.expression());
        return Variables["_returnValue"] ?? "";
    }
    #endregion
    
    #region Pole

    public override object VisitArray_declaration(Algol60Parser.Array_declarationContext context)
    {
        var arrayName = context.IDENTIFIER().GetText();
        var size = int.Parse(context.DIGIT().GetText());
        var arrayType = context.variable_type().GetText();
        
        var array = new object?[size];
        Variables[arrayName] = array;

        if (context.array_inicialization() == null) return arrayName;
        var initializationValues = context.array_inicialization().expression();
        for (var i = 0; i < initializationValues.Length; i++)
        {
            var value = Visit(initializationValues[i]);
            if (!IsExpectedDataType(arrayType, value))
            {
                throw new AlgolVisitorExceptions.IncorrectAssignmentException(arrayName, value, value.GetType());
            }
                
            if (i >= size) throw new AlgolVisitorExceptions.ArrayOutOfBoundsException(size);
            array[i] = Visit(initializationValues[i]);
        }
        return arrayName;
    }

    public override object VisitArray_access(Algol60Parser.Array_accessContext context)
    {
        var arrayName = context.IDENTIFIER().GetText();
        var index = (int)(Visit(context.expression()) ?? throw new InvalidOperationException());

        if (!Variables.ContainsKey(arrayName))
            throw new AlgolVisitorExceptions.NonDeclaredMemberException(arrayName);

        var array = (object?[])Variables[arrayName]!;
        return array[index] ?? $"{arrayName}[index] = null";
    }

    public override object VisitArray_2d_declaration(Algol60Parser.Array_2d_declarationContext context)
    {
        var arrayName = context.IDENTIFIER().GetText();
        var rows = int.Parse(context.DIGIT(0).GetText());
        var cols = int.Parse(context.DIGIT(1).GetText());
        var arrayType = context.variable_type().GetText();

        var array2D = new object?[rows, cols];
        Variables[arrayName] = array2D;

        if (context.array_2d_inicialization() == null) return arrayName;

        var initializationValues = context.array_2d_inicialization().expression();
        for (var i = 0; i < rows; i++)
        {
            for (var j = 0; j < cols; j++)
            {
                var value = Visit(initializationValues[i * cols + j]);
                if (!IsExpectedDataType(arrayType, value))
                {
                    throw new AlgolVisitorExceptions.IncorrectAssignmentException(arrayName, value, value?.GetType());
                }
                array2D[i, j] = value;
            }
        }
        return arrayName;
    }

    public override object VisitArray_2d_access(Algol60Parser.Array_2d_accessContext context)
    {
        var arrayName = context.IDENTIFIER().GetText();
        var rowIndex = (int)(Visit(context.expression(0)) ?? throw new InvalidOperationException());
        var colIndex = (int)(Visit(context.expression(1)) ?? throw new InvalidOperationException());

        if (!Variables.ContainsKey(arrayName))
            throw new AlgolVisitorExceptions.NonDeclaredMemberException(arrayName);

        var array2D = (object?[,])Variables[arrayName]!;
        return array2D[rowIndex, colIndex] ?? $"{arrayName}[{rowIndex}, {colIndex}] = null";
    }

    #endregion

    #region Funkce

    public override object VisitFunction_declaration(Algol60Parser.Function_declarationContext context)
    {
        var procName = context.IDENTIFIER().GetText();
        var parameters = context.parameter_list()?.parameter().Select(p => p.IDENTIFIER().GetText()).ToList() ?? new List<string>();
        
        Variables[procName] = context;
        return context;
    }

    public override object VisitFunction_call(Algol60Parser.Function_callContext context)
    {
        var procName = context.IDENTIFIER().GetText();
        var args = context.expression().Select(Visit).ToArray();

        if (Variables.TryGetValue(procName, out var funcObj) && funcObj is Func<object?[], object?> func)
        {
            return func(args) ?? "";
        }
        if (Variables.TryGetValue(procName, out var procContextObj) && procContextObj is Algol60Parser.Function_declarationContext procContext)
        {
            var parameters = procContext.parameter_list()?.parameter().
                Select(p => p.IDENTIFIER().GetText()).ToList() ?? new List<string>();
            if (parameters.Count != args.Length) throw new AlgolVisitorExceptions.InccorectArgumentsCountException(procName);

            var localVars = new Dictionary<string, object?>();
            for (var i = 0; i < parameters.Count; i++)
            {
                localVars[parameters[i]] = args[i];
            }
        
            var originalVars = new Dictionary<string, object?>(Variables);
            Variables = localVars;
            Visit(procContext.block());
            var returnValue = Variables.GetValueOrDefault("_returnValue");
            Variables = originalVars;
            return returnValue ?? "";
        } 
        throw new AlgolVisitorExceptions.NonDeclaredMemberException(procName);
    }
    #endregion

    #region Proměnné

    public override object VisitVariable_declaration(Algol60Parser.Variable_declarationContext context)
    {
        var dataType = context.variable_type().GetText();
        var identifier = context.IDENTIFIER().GetText();
        object? value = null;
        
        if (context.expression() != null)
        {
            value = Visit(context.expression());
            if (!IsExpectedDataType(dataType, value))
            {
                throw new AlgolVisitorExceptions.IncorrectAssignmentException(identifier, value, dataType);
            }
        }

        Variables[identifier] = value;
        return identifier;
    }
    
    #endregion

    #region Cykly a podmínky
    
    public override object VisitIf_block(Algol60Parser.If_blockContext context)
    {
        var condition = (bool)Visit(context.expression());

        if (condition)
        {
            Visit(context.block()); // IF větev
        }
        else if (context.else_if_block() != null)
        {
            Visit(context.else_if_block()); // ELSE větev
        }
        return condition;
    }

    public override object VisitWhile_block(Algol60Parser.While_blockContext context)
    {
        while ((bool)Visit(context.expression()))
        {
            Visit(context.block());
        }
        return "";
    }

    #endregion
    
    #region Aritmetické operace
    
    public override object VisitAdditive_expression(Algol60Parser.Additive_expressionContext context)
    {
        var left = Visit(context.expression(0));
        var right = Visit(context.expression(1));

        var op = context.ADDITIVE_OPPERANDS().GetText();
        return op switch
        {
            "+" => Addition(left, right),
            "-" => Substraction(left, right),
            _ => throw new AlgolVisitorExceptions.UnsupportedAdditiveOpperatorException(op)
        };
    }

    public override object VisitMultiplicative_expression(Algol60Parser.Multiplicative_expressionContext context)
    {
        var left = Visit(context.expression(0));
        var right = Visit(context.expression(1));

        var op = context.MULTIPLICATIVE_OPPERANDS().GetText();
        return op switch
        {
            "*" => Multiplication(left, right),
            "/" or "%" => Division(left, right, op),
            _ => throw new AlgolVisitorExceptions.UnsupportedMultiplicativeOpperatorException(op)
        };

    }

    public override object VisitComparison_expression(Algol60Parser.Comparison_expressionContext context)
    {
        var left = Visit(context.expression(0));
        var right = Visit(context.expression(1));

        var op = context.COMPARISON_OPPERANDS().GetText();
        
        return op switch
        {
            "<" => LessThan(left, right),
            "<=" => LessThanEquals(left, right),
            ">" => GreaterThan(left, right),
            ">=" => GreaterThanEquals(left, right),
            "==" => EqualsEquals(left, right),
            "!=" => NotEquals(left, right),
            _ => throw new AlgolVisitorExceptions.UnsupportedComparisonOpperatorException(op)
        };
    }

    #region Porovnávací metody
        private static bool LessThan(object? left, object? right)
        {
            return left switch
            {
                int leftInt when right is int rightInt => leftInt < rightInt,
                float leftFloat when right is float rightFloat => leftFloat < rightFloat,
                int lInt when right is float rFloat => lInt < rFloat,
                float lFloat when right is int rInt => lFloat < rInt,
                _ => throw new  AlgolVisitorExceptions.UnsupportedSpecificComparisonOpperatorException("<")
            };
        }
        
        private static bool LessThanEquals(object? left, object? right)
        {
            return left switch
            {
                int leftInt when right is int rightInt => leftInt <= rightInt,
                float leftFloat when right is float rightFloat => leftFloat <= rightFloat,
                int lInt when right is float rFloat => lInt <= rFloat,
                float lFloat when right is int rInt => lFloat <= rInt,
                _ => throw new AlgolVisitorExceptions.UnsupportedSpecificComparisonOpperatorException("<=")
            };
        }

        private static bool GreaterThan(object? left, object? right)
        {
            return left switch
            {
                int leftInt when right is int rightInt => leftInt > rightInt,
                float leftFloat when right is float rightFloat => leftFloat > rightFloat,
                int lInt when right is float rFloat => lInt > rFloat,
                float lFloat when right is int rInt => lFloat > rInt,
                _ => throw new AlgolVisitorExceptions.UnsupportedSpecificComparisonOpperatorException(">")
            };
        }

        private static bool GreaterThanEquals(object? left, object? right)
        {
            return left switch
            {
                int leftInt when right is int rightInt => leftInt >= rightInt,
                float leftFloat when right is float rightFloat => leftFloat >= rightFloat,
                int lInt when right is float rFloat => lInt >= rFloat,
                float lFloat when right is int rInt => lFloat >= rInt,
                _ => throw new AlgolVisitorExceptions.UnsupportedSpecificComparisonOpperatorException(">=")
            };
        }
        
        private static bool EqualsEquals(object? left, object? right)
        {
            return left switch
            {
                int leftInt when right is int rightInt => leftInt == rightInt,
                float leftFloat when right is float rightFloat => leftFloat == rightFloat,
                int lInt when right is float rFloat => lInt == rFloat,
                float lFloat when right is int rInt => lFloat == rInt,
                _ => throw new AlgolVisitorExceptions.UnsupportedSpecificComparisonOpperatorException("==")
            };
        }
        
        private static bool NotEquals(object? left, object? right)
        {
            return left switch
            {
                int leftInt when right is int rightInt => leftInt != rightInt,
                float leftFloat when right is float rightFloat => leftFloat != rightFloat,
                int lInt when right is float rFloat => lInt != rFloat,
                float lFloat when right is int rInt => lFloat != rInt,
                _ => throw new AlgolVisitorExceptions.UnsupportedSpecificComparisonOpperatorException("!=")
            };
        }
    #endregion

    #region Matematické operace

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
            return $"{left}{right}";
        }
        throw new AlgolVisitorExceptions.UnsupportedDataTypeException($"{left.GetType()} nebo {right.GetType()}");
    }


    private static object Division(object left, object right, object op)
    {
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

    #endregion
    
    #endregion

    #region Pomocné metody

    // Výpis argumentů do konzole
    private static object? Print(object?[] args)
    {
        if (args.Length > 1)
        {
            foreach (var arg in args)
            {
                Console.Write(arg);
            }
            Console.WriteLine();
        }
        else
        {
            foreach (var arg in args)
            {
                Console.WriteLine(arg);
            } 
        }
        return null;
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

    #endregion
}