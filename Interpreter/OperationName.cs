namespace Interpreter;

public enum InstructionName : byte
{
    LoadConstNumberToA = 1,
    LoadConstNumberToB,
    
    JumpIfZero,

    NotNumber,
    EqualsNumber,

    AddNumber,
    SubNumber,
    MultiplyNumber,
    DivideNumber,

    Halt,
    SetVariable,
    LoadVariableToA,
    LoadVariableToB,
    CallMethod,
    DuplicateAToB,
    LessThan,
    JumpIfNotZero
}