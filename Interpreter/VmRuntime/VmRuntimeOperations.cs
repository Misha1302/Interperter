using System.Runtime.CompilerServices;

namespace Interpreter.VmRuntime;

public partial class VmRuntime
{
    [VmInstruction(0)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool JumpIfOneNumber()
    {
        ReadTwoWords(out var a, out var b);
        if (a == 1) _memory.InstructionPointer = (int)b;
        return true;
    }

    [VmInstruction(0)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool NotNumber()
    {
        ReadWordFromStack(out var a);
        WriteWordToStack(a == 1 ? 0 : 1);
        return true;
    }

    [VmInstruction(0)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool EqualsNumber()
    {
        ReadTwoWords(out var a, out var b);

        WriteWordToStack(a == b);
        return true;
    }

    [VmInstruction(0)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool SetNumber()
    {
        // a - value, b - position
        ReadTwoWords(out var a, out var b);
        // b /= Fraction;

        _memory.WriteWord(a, b);
        return true;
    }

    [VmInstruction(0)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool LoadNumber()
    {
        ReadWordFromStack(out var a);

        // a /= Fraction;
        var readWord = _memory.ReadWord(a);
        _memory.WriteWord(readWord, _memory.StackPointer);

        _memory.StackPointer = VmMemory.MoveIntToForward(_memory.StackPointer);
        return true;
    }

    [VmInstruction(16)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool LoadConst()
    {
        var value = _memory.ReadWord(_memory.InstructionPointer);
        _memory.InstructionPointer = VmMemory.MoveIntToForward(_memory.InstructionPointer);

        WriteWordToStack(value);

        return true;
    }

    [VmInstruction(0)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool DivNumber()
    {
        ReadTwoWords(out var a, out var b);
        var a2 = (Number256)a * _fraction256;
        var b2 = (Number256)b;

        if (b2 == 0)
        {
            _errorString = "division by zero";
            return false;
        }

        _memory.WriteWord((Number128)(a2 / b2), _memory.StackPointer);

        _memory.StackPointer = VmMemory.MoveIntToForward(_memory.StackPointer);
        return true;
    }

    [VmInstruction(0)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool MulNumber()
    {
        ReadTwoWords(out var a, out var b);
        var a2 = (Number256)a;
        var b2 = (Number256)b;

        WriteWordToStack((Number128)(a2 * b2 / _fraction256));
        return true;
    }

    [VmInstruction(0)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool SubNumber()
    {
        ReadTwoWords(out var a, out var b);
        WriteWordToStack(a - b);
        return true;
    }

    [VmInstruction(0)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool AddNumber()
    {
        ReadTwoWords(out var a, out var b);
        WriteWordToStack(a + b);
        return true;
    }

    [VmInstruction(-1)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool DefaultInstruction()
    {
        var operation = _memory.MemoryArray[_memory.InstructionPointer];
        _errorString = $"unknown operation - {(InstructionName)operation}";

#if DEBUG
        Exit();
        throw new ArgumentOutOfRangeException(_errorString);
#endif
        return false;
    }

    [VmInstruction(0)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Exit()
    {
        OnProgramExit?.Invoke(_errorString, this);
    }

    [VmInstruction(0)]
    private bool SetVariableNumber()
    {
        if (!_pointersToInsertVariables.TryGetValue(_memory.InstructionPointer, out var currentVarName))
        {
            _errorString = $"variable at position {_memory.InstructionPointer} not found";
            return false;
        }

        ReadWordFromStack(out var word);
        _variablesNumbers[currentVarName] = word;

        return true;
    }

    [VmInstruction(0)]
    private bool LoadVariableNumber()
    {
        if (!_pointersToInsertVariables.TryGetValue(_memory.InstructionPointer, out var currentVarName))
        {
            _errorString = $"variable at position {_memory.InstructionPointer} not found";
            return false;
        }

        WriteWordToStack(_variablesNumbers[currentVarName]);

        return true;
    }

    [VmInstruction(0)]
    private bool JumpIfNotZeroNumber()
    {
        ReadTwoWords(out var a, out var b);
        if (a != 0) _memory.InstructionPointer = (int)b;
        return true;
    }
}