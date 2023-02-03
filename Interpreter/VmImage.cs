namespace Interpreter;

using Interpreter.VmRuntime;

public class VmImage
{
    private const int BaseProgramSize = 512;

    private readonly Dictionary<string, int> _goto;
    private readonly Dictionary<string, int> _labels;
    private readonly VmMemory _memory;
    private readonly Dictionary<int, int> _pointersToInsertVariables;
    private readonly List<VmVariable> _variables;
    public readonly AssemblyManager AssemblyManager;

    public readonly VmRuntime.VmRuntime VmRuntime;
    public readonly Dictionary<string, int> ImportedMethodsIndexes;
    private int _index;

    public VmImage()
    {
        ImportedMethodsIndexes = new Dictionary<string, int>();
        AssemblyManager = new AssemblyManager();
        VmRuntime = new VmRuntime.VmRuntime();
        _variables = new List<VmVariable>();
        _pointersToInsertVariables = new Dictionary<int, int>();
        _labels = new Dictionary<string, int>();
        _goto = new Dictionary<string, int>();

        _memory = new VmMemory
        {
            MemoryArray = GC.AllocateArray<byte>(BaseProgramSize, true),
            InstructionPointer = 0
        };
    }

    public void WriteNextOperation(InstructionName operation)
    {
        _memory.MemoryArray[_memory.InstructionPointer] = (byte)operation;
        _memory.InstructionPointer++;

        IncreaseMemArrayIfItNeed();
    }

    private void IncreaseMemArrayIfItNeed()
    {
        var len = _memory.MemoryArray.Length;
        if (_memory.InstructionPointer < len) return;
        Array.Resize(ref _memory.MemoryArray, len << 1);
    }


    private void WriteNextConstant(object? word, int? position = null)
    {
        var pos = position ?? _memory.InstructionPointer - 1;
        _memory.Constants.Add(pos, word);
    }

    public void WriteNextConstant(decimal number)
    {
        WriteNextConstant(number, null);
    }

    public VmMemory GetMemory()
    {
        WriteNextOperation(InstructionName.Halt);

        ReplaceGoto();

        _memory.InstructionPointer = 0;

        return _memory;
    }

    private void ReplaceGoto()
    {
        foreach (var (key, position) in _goto)
        {
            var value = _labels[key];
            WriteNextConstant(value, position);
        }
    }

    public void CreateVariable(string varName)
    {
        _variables.Add(new VmVariable(varName));
    }

    public void SetVariable(string varName)
    {
        WriteNextOperation(InstructionName.SetVariable);

        var keyValuePair = _variables.First(x => x.Name == varName);
        _pointersToInsertVariables.Add(_memory.InstructionPointer - 1, keyValuePair.Id);
    }

    public void LoadVariable(string varName, bool toA = true)
    {
        WriteNextOperation(toA ? InstructionName.LoadVariableToA : InstructionName.LoadVariableToB);

        var keyValuePair = _variables.First(x => x.Name == varName);
        _pointersToInsertVariables.Add(_memory.InstructionPointer - 1, keyValuePair.Id);
    }

    public void SetLabel(string label)
    {
        _labels.Add(label, _memory.InstructionPointer - 1);
    }

    public void GotoIfNotZeroNumber(string label)
    {
        Goto(label, InstructionName.JumpIfNotZero);
    }

    public void Goto(string label, InstructionName jumpInstruction)
    {
        _goto.Add(label, _memory.InstructionPointer);
        WriteNextOperation(InstructionName.LoadConstNumberToB);
        WriteNextOperation(jumpInstruction);
    }

    public Dictionary<int, int> GetPointersToInsertVariables()
    {
        return _pointersToInsertVariables;
    }

    public IEnumerable<VmVariable> GetVariables()
    {
        return _variables;
    }

    public void ImportMethodFromAssembly(string dllPath, string methodName)
    {
        AssemblyManager.ImportMethodFromAssembly(dllPath, methodName);
        ImportedMethodsIndexes.Add(methodName, _index);
        _index++;
    }

    public void CreateFunction(string name)
    {
        WriteNextOperation(InstructionName.Halt);
        
        SetLabel(name);
    }
}