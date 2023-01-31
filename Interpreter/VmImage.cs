using System.Runtime.CompilerServices;

namespace Interpreter;

public class VmImage
{
    private const int StackSize = 16_000;
    private const int DataSize = 16_000;
    private const int ProgramSize = 16_000;

    private readonly Dictionary<string, int> _goto;
    private readonly Dictionary<string, int> _labels;
    private readonly VmMemory _memory;
    private readonly Dictionary<int, string> _pointersToInsertVariables;
    private readonly Dictionary<string, Number128> _variablesNumber;

    public readonly VmRuntime.VmRuntime VmRuntime;

    public VmImage()
    {
        VmRuntime = new VmRuntime.VmRuntime();
        _variablesNumber = new Dictionary<string, Number128>();
        _pointersToInsertVariables = new Dictionary<int, string>();
        _labels = new Dictionary<string, int>();
        _goto = new Dictionary<string, int>();

        const int fullSize = ProgramSize + StackSize + DataSize + 16_000;
        _memory = new VmMemory
        {
            MemoryArray = GC.AllocateArray<byte>(fullSize, pinned: true),// new byte[fullSize],
            InstructionPointer = 0,
            StackPointer = int.MinValue,
            DataPointer = StackSize
        };
    }

    public void WriteNextOperation(InstructionName operation)
    {
        _memory.MemoryArray[_memory.InstructionPointer] = (byte)operation;
        _memory.InstructionPointer++;
    }


    private unsafe void WriteNextNumber(Number128 word, int? position = null)
    {
        var pos = position ?? _memory.InstructionPointer;

        fixed (byte* w = &_memory.MemoryArray[pos])
        {
            Unsafe.Write(w, word);
        }

        _memory.InstructionPointer = VmMemory.MoveIntToForward(_memory.InstructionPointer);
    }

    public void WriteNextNumber(string s, bool multiply = true)
    {
        s = s.Replace("_", "");
        var div = 0;
        if (s.Contains('.'))
        {
            div = s.Split(".")[^1].Length;
            s = s.Replace(".", "");
        }

        Number128 i = 1;
        if (s[0] == '-')
        {
            i = -1;
            s = s[1..];
        }

        if (multiply) i *= Interpreter.VmRuntime.VmRuntime.Fraction;

        i *= Number128.Parse(s);
        i /= Number128.Parse('1' + new string('0', div));

        WriteNextNumber(i);
    }

    public VmMemory GetMemory()
    {
        WriteNextOperation(InstructionName.Halt);

        ReplaceGoto();

        _memory.InstructionPointer = 0;
        _memory.StackPointer = _memory.MemoryArray.Length >> 1;
        _memory.StackStart = _memory.StackPointer;
        _memory.StackEnd = _memory.StackPointer + StackSize;

        return _memory;
    }

    private void ReplaceGoto()
    {
        foreach (var (key, value) in _goto)
        {
            var position = _labels[key];
            WriteNextNumber(position, value);
        }
    }

    public void CreateVariableNumber(string varName)
    {
        _variablesNumber.Add(varName, _memory.DataPointer);
        _memory.DataPointer = VmMemory.MoveIntToForward(_memory.DataPointer);
    }

    public void SetVariableNumber(string varName)
    {
        WriteNextOperation(InstructionName.SetVariableNumber);
        var keyValuePair = _variablesNumber.First(x => x.Key == varName);
        _pointersToInsertVariables.Add(_memory.InstructionPointer, keyValuePair.Key);
    }

    public void LoadVariableNumber(string varName)
    {
        WriteNextOperation(InstructionName.LoadVariableNumber);

        var keyValuePair = _variablesNumber.First(x => x.Key == varName);
        _pointersToInsertVariables.Add(_memory.InstructionPointer, keyValuePair.Key);
    }

    public void SetLabel(string label)
    {
        _labels.Add(label, _memory.InstructionPointer);
    }

    public void GotoIfNotZeroNumber(string label)
    {
        WriteNextOperation(InstructionName.LoadConstNumber);
        // when calling the GetMemory method, this integer will be replaced by the number you need to jump to
        _goto.Add(label, _memory.InstructionPointer);
        WriteNextNumber("-100");

        WriteNextOperation(InstructionName.JumpIfNotZeroNumber);
    }

    public Dictionary<int, string> GetPointersToInsertVariables()
    {
        return _pointersToInsertVariables;
    }

    public Dictionary<string, Number128> GetVariables()
    {
        return _variablesNumber;
    }
}