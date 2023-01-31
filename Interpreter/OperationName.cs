namespace Interpreter;

public enum InstructionName : byte
{
    LoadConstNumber = 1,
    LoadNumber, // load from memory
    SetNumber, // set to memory

    Jump,
    JumpIfLessThanZeroNumber,
    JumpIfZeroNumber,
    JumpIfMoreThanZeroNumber,

    JumpIfOneNumber,
    JumpIfNotZeroNumber,
    JumpIfNotOneNumber,

    NotNumber,
    CompareNumber,
    EqualsNumber,
    NotEqualsNumber,
    AndNumber,
    OrNumber,

    AddNumber,
    SubNumber,
    MultiplyNumber,
    DivideNumber,

    Halt,
    SetVariableNumber,
    LoadVariableNumber
}