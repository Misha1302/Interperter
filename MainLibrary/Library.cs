// ReSharper disable once CheckNamespace

namespace Library;

using Interpreter.VmRuntime;

public static class Library
{
    public static void Print(VmRuntime vmRuntime)
    {
        Console.WriteLine(vmRuntime.Memory.ARegister);
    }

    public static void Input(VmRuntime vmRuntime)
    {
        vmRuntime.Memory.ARegister = Console.ReadLine();
    }

    public static void ToNumber(VmRuntime vmRuntime)
    {
        vmRuntime.Memory.ARegister =
            Convert.ToDecimal(vmRuntime.Memory.ARegister ?? throw new NullReferenceException());
    }
}