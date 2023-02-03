namespace Interpreter;

public record VmMemory
{
    public readonly Dictionary<int, object?> Constants = new();
    public byte[] MemoryArray = Array.Empty<byte>();

    
    public object? ARegister;
    public object? BRegister;

    public int InstructionPointer;
}