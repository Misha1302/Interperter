namespace Interpreter.VmRuntime;

public class VmInstruction : Attribute
{
    public readonly int SizeOfArgs;

    public VmInstruction(int sizeOfArgs)
    {
        SizeOfArgs = sizeOfArgs;
    }
}