namespace Interpreter.VmRuntime;

using System.Runtime.CompilerServices;

public partial class VmRuntime
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void JumpIfOneNumber()
    {
        ReadTwoNumbers(out var a, out var b);
        if (a.IsEquals(1)) Memory.InstructionPointer = (int)b;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void NotNumber()
    {
        ReadNumber(out var a);
        Memory.ARegister = a.IsEquals(0); // a == 0 ? 1 : 0
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void EqualsNumber()
    {
        ReadTwoNumbers(out var a, out var b);

        Memory.ARegister = a.IsEquals(b);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void LoadConstToA()
    {
        var value = Memory.Constants[Memory.InstructionPointer];

        Memory.ARegister = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void LoadConstToB()
    {
        var value = Memory.Constants[Memory.InstructionPointer];

        Memory.BRegister = value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void DivNumber()
    {
        ReadTwoNumbers(out var a, out var b);

        if (a == 0) throw new DivideByZeroException($"a={a}; b={b}");

        Memory.ARegister = a / b;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void MulNumber()
    {
        ReadTwoNumbers(out var a, out var b);
        Memory.ARegister = a * b;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void SubNumber()
    {
        ReadTwoNumbers(out var a, out var b);

        Memory.ARegister = a - b;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddNumber()
    {
        ReadTwoNumbers(out var a, out var b);
        Memory.ARegister = a + b;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Exit(int code, Exception? error)
    {
        OnProgramExit?.Invoke(this, code, error);
    }

    private void SetVariable()
    {
        var varId = _pointersToInsertVariables[Memory.InstructionPointer];
        _variables[varId].ChangeValue(Memory.ARegister);
    }

    private void LoadVariableToA()
    {
        var value = _variables[_pointersToInsertVariables[Memory.InstructionPointer]].Value;
        Memory.ARegister = value;
    }

    private void LoadVariableToB()
    {
        var value = _variables[_pointersToInsertVariables[Memory.InstructionPointer]].Value;
        Memory.BRegister = value;
    }

    private void CallMethod()
    {
        var obj = Memory.Constants[Memory.InstructionPointer] ?? throw new InvalidOperationException();

        var index = obj switch
        {
            decimal d => (int)d,
            int i => i,
            _ => throw new InvalidCastException(obj.GetType().ToString())
        };

        _assemblyManager.GetMethodByIndex(index).Invoke(this);
    }

    private void JumpIfNotZero()
    {
        var obj = Memory.ARegister ?? throw new InvalidOperationException();
        switch (obj)
        {
            case decimal d when !d.IsEquals(0):
            case true:
                Memory.InstructionPointer = (int)(Memory.BRegister ?? throw new InvalidOperationException());
                break;
        }
    }

    private void DuplicateAToB()
    {
        Memory.BRegister = Memory.ARegister;
    }

    private void JumpIfZero()
    {
        var obj = Memory.ARegister ?? throw new InvalidOperationException();
        switch (obj)
        {
            case decimal d when d.IsEquals(0):
            case false:
                Memory.InstructionPointer = (int)(Memory.BRegister ?? throw new InvalidOperationException());
                break;
        }
    }

    private void LessThan()
    {
        ReadTwoNumbers(out var a, out var b);
        Memory.ARegister = a < b;
    }
}