namespace Interpreter.VmRuntime;

internal static class IdManager
{
    private static int _maxId;

    public static int GetNewId()
    {
        return _maxId++;
    }
}

public record VmVariable(string Name, object? Value = default!)
{
    public readonly int Id = IdManager.GetNewId();
    public readonly string Name = Name;
    public object? Value = Value;

    public void ChangeValue(object? value)
    {
        Value = value;
    }
}