namespace Interpreter.VmRuntime;

using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;

public partial class VmRuntime
{
    private AssemblyManager _assemblyManager;

    private Dictionary<int, int> _pointersToInsertVariables = new(64);
    private Dictionary<int, VmVariable> _variables = new(64);
    public VmMemory Memory;

    public Action<VmRuntime, int, Exception?>? OnProgramExit;
    public Action? OnProgramStart;

    public VmRuntime()
    {
        Memory = new VmMemory();
        _assemblyManager = new AssemblyManager();

        PrepareInstructionsForExecution();
    }

    private static void PrepareInstructionsForExecution()
    {
        var handles = typeof(VmRuntime).GetMethods(
            BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

        foreach (var handle in handles)
            RuntimeHelpers.PrepareMethod(handle.MethodHandle);
    }

    public void Run(VmImage image)
    {
        SetImage(image);

        var instructions = GenerateDictToExecute();

        Execute(instructions);
    }

    private void Execute(IReadOnlyList<Instruction> instructions)
    {
        OnProgramStart?.Invoke();

#if !DEBUG
        try
#endif
        {
            var instructionsCount = instructions.Count - 1;
            Memory.InstructionPointer = -1;
            do
            {
                Memory.InstructionPointer++;
                instructions[Memory.InstructionPointer]();
            } while (Memory.InstructionPointer < instructionsCount);
        }
#if !DEBUG
        catch (Exception ex)
        {
            Exit(1, ex);
            return;
        }
#endif

        Exit(0, null);
    }

    private List<Instruction> GenerateDictToExecute()
    {
        var ip = Memory.InstructionPointer;
        var operation = Memory.MemoryArray[ip];

        var instructions = new List<Instruction>();

        while (operation != (byte)InstructionName.Halt)
        {
            Instruction instr = operation switch
            {
                (byte)InstructionName.AddNumber => AddNumber,
                (byte)InstructionName.SubNumber => SubNumber,
                (byte)InstructionName.MultiplyNumber => MulNumber,
                (byte)InstructionName.DivideNumber => DivNumber,
                (byte)InstructionName.LoadConstNumberToA => LoadConstToA,
                (byte)InstructionName.LoadConstNumberToB => LoadConstToB,
                (byte)InstructionName.EqualsNumber => EqualsNumber,
                (byte)InstructionName.NotNumber => NotNumber,
                (byte)InstructionName.JumpIfZero => JumpIfZero,
                (byte)InstructionName.JumpIfNotZero => JumpIfNotZero,
                (byte)InstructionName.SetVariable => SetVariable,
                (byte)InstructionName.LoadVariableToA => LoadVariableToA,
                (byte)InstructionName.LoadVariableToB => LoadVariableToB,
                (byte)InstructionName.CallMethod => CallMethod,
                (byte)InstructionName.DuplicateAToB => DuplicateAToB,
                (byte)InstructionName.LessThan => LessThan,
                _ => throw new InvalidOperationException($"unknown instruction - {(InstructionName)operation}")
            };
            instructions.Add(instr);

            ip++;

            operation = Memory.MemoryArray[ip];
        }

        return instructions;
    }

    private void SetImage(VmImage image)
    {
        _pointersToInsertVariables = image.GetPointersToInsertVariables();
        _variables = image.GetVariables().ToDictionary(x => x.Id);
        Memory = image.GetMemory();
        _assemblyManager = image.AssemblyManager;
    }

    public void PrintState()
    {
        if (Memory == null) throw new NullReferenceException();

        foreach (var variable in _variables)
            Console.WriteLine($"{variable.Value.Name}={variable.Value.Value}");

        string a, b;
        if (Memory.ARegister is decimal decA) a = decA.ToString(CultureInfo.InvariantCulture);
        else a = Memory.ARegister?.ToString() ?? string.Empty;
        if (Memory.BRegister is decimal decB) b = decB.ToString(CultureInfo.InvariantCulture);
        else b = Memory.BRegister?.ToString() ?? string.Empty;

        Console.WriteLine($"rA={a}; rB={b}");
    }

    private void ReadTwoNumbers(out decimal a, out decimal b)
    {
        a = (decimal)(Memory.ARegister ?? throw new InvalidOperationException());
        b = (decimal)(Memory.BRegister ?? throw new InvalidOperationException());
    }

    private void ReadNumber(out decimal a)
    {
        a = (decimal)(Memory.ARegister ?? throw new InvalidOperationException());
    }

    private delegate void Instruction();
}