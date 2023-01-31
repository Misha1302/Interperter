using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Interpreter.VmRuntime;

public partial class VmRuntime
{
    public static readonly Number128 Fraction = 10_000_000_000_000_000_000;
    private static readonly Number256 _fraction256 = (Number256)Fraction;

    private string _errorString = string.Empty;

    private VmMemory _memory;

    private Dictionary<int, string> _pointersToInsertVariables = new(4096);
    private Dictionary<string, Number128> _variablesNumbers = new(64);

    public Action<string, VmRuntime>? OnProgramExit;
    public Action? OnProgramStart;

    public VmRuntime()
    {
        _memory = new VmMemory();

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

        // var operations = () => true;


        var ip = _memory.InstructionPointer;
        var operation = _memory.MemoryArray[ip];

        var instructions = new Dictionary<int, Func<bool>>();

        while (operation != (byte)InstructionName.Halt)
        {
            Func<bool> instr = operation switch
            {
                (byte)InstructionName.AddNumber => AddNumber,
                (byte)InstructionName.SubNumber => SubNumber,
                (byte)InstructionName.MultiplyNumber => MulNumber,
                (byte)InstructionName.DivideNumber => DivNumber,
                (byte)InstructionName.LoadConstNumber => LoadConst,
                (byte)InstructionName.LoadNumber => LoadNumber,
                (byte)InstructionName.SetNumber => SetNumber,
                (byte)InstructionName.EqualsNumber => EqualsNumber,
                (byte)InstructionName.NotNumber => NotNumber,
                (byte)InstructionName.JumpIfOneNumber => JumpIfOneNumber,
                (byte)InstructionName.JumpIfNotZeroNumber => JumpIfNotZeroNumber,
                (byte)InstructionName.SetVariableNumber => SetVariableNumber,
                (byte)InstructionName.LoadVariableNumber => LoadVariableNumber,
                _ => DefaultInstruction
            };
            instructions.Add(ip, instr);

            var attribute = instr.Method.GetCustomAttribute<VmInstruction>()
                            ?? throw new NullReferenceException(instr.Method.Name);
            ip++;
            ip += attribute.SizeOfArgs;

            operation = _memory.MemoryArray[ip];
        }

        instructions.Add(ip, () => false);

        OnProgramStart?.Invoke();

        Func<bool> currentInstruction;
        do
        {
            currentInstruction = instructions[_memory.InstructionPointer];
            _memory.InstructionPointer++;
        } while (currentInstruction.Invoke());

        Exit();
    }

    private void SetImage(VmImage image)
    {
        _pointersToInsertVariables = image.GetPointersToInsertVariables();
        _variablesNumbers = image.GetVariables();
        _memory = image.GetMemory();
    }

    public void PrintState()
    {
        if (_memory == null) throw new NullReferenceException();
        // _memory!!.MemoryArray;

        var stackInBytes = _memory.MemoryArray[_memory.StackStart.._memory.StackPointer];

        var stackInInt128 = new Number128[stackInBytes.Length / VmMemory.Step];
        for (var i = 0; i < stackInBytes.Length; i = VmMemory.MoveIntToForward(i))
            stackInInt128[i / VmMemory.Step] = _memory.ReadWord(i + _memory.StackStart);

        Console.WriteLine(
            $"SP={_memory.StackPointer - _memory.StackStart} IP={_memory.InstructionPointer}");
        Console.WriteLine(
            $"STACK_SIZE={stackInBytes.Length} STACK=[{string.Join(",", stackInBytes)}]");
        Console.WriteLine(
            $"STACK_INT128_SIZE={stackInInt128.Length} STACK_INT128=[{string.Join(",", stackInInt128.Select(DecimalToString))}]");
    }

    private static string DecimalToString(Number128 a)
    {
        var str = a.ToString();
        str = str.PadLeft(38, '0');
        str = str.Insert(19, ".");

        str = str.Trim('0').TrimEnd('.');
        if (string.IsNullOrEmpty(str)) str = "0";
        str = Regex.Replace(str, "^\\.", "0.");
        str = Regex.Replace(str, "^-\\.", "-0.");

        return str;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ReadTwoWords(out Number128 a, out Number128 b)
    {
        ReadWordFromStack(out b);
        ReadWordFromStack(out a);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ReadWordFromStack(out Number128 word)
    {
        if (_memory.StackStart > _memory.StackPointer)
        {
            _errorString = "attempt to read a word below the stack";
#if DEBUG
            throw new Exception(_errorString);
#endif
            Exit();
            word = default;
            return;
        }

        _memory.StackPointer = VmMemory.MoveIntToBackward(_memory.StackPointer);
        word = _memory.ReadWord(_memory.StackPointer);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void WriteWordToStack(Number128 word)
    {
        if (_memory.StackEnd <= _memory.StackPointer)
        {
            _errorString = $"stack overflow (sizeOfStack={_memory.StackEnd - _memory.StackStart})";
            Exit();
            return;
        }

        _memory.WriteWord(word, _memory.StackPointer);
        _memory.StackPointer = VmMemory.MoveIntToForward(_memory.StackPointer);
    }
}