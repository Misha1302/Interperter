namespace Interpreter;

using System.Diagnostics;

public static class Program
{
    private static Stopwatch? _stopwatch;

    public static void Main()
    {
        var vmImage = new VmImage();

        vmImage.VmRuntime.OnProgramStart += OnProgramStart;
        vmImage.VmRuntime.OnProgramExit += OnProgramExit;

        const string dllPath = @"D:\DRIVE\Programming\ProgrammingLanguages\CSharp\Interpreter\Interpreter\MainLibrary\bin\Debug\net7.0\MainLibrary.dll";
        vmImage.ImportMethodFromAssembly(dllPath, "Print");
        vmImage.ImportMethodFromAssembly(dllPath, "Input");
        vmImage.ImportMethodFromAssembly(dllPath, "ToNumber");
        
        vmImage.CreateVariable("i");
        
        

        vmImage.WriteNextOperation(InstructionName.LoadConstNumberToA);
        vmImage.WriteNextConstant(1_000_000);
        
        vmImage.SetVariable("i");
        
        
        vmImage.SetLabel("loopLabel");

        vmImage.LoadVariable("i");
        vmImage.WriteNextOperation(InstructionName.CallMethod);
        vmImage.WriteNextConstant(vmImage.ImportedMethodsIndexes["Print"]);
        
        vmImage.WriteNextOperation(InstructionName.LoadConstNumberToB);
        vmImage.WriteNextConstant(12.22345m);
        vmImage.WriteNextOperation(InstructionName.SubNumber);
        vmImage.SetVariable("i");
        
        vmImage.WriteNextOperation(InstructionName.LoadConstNumberToB);
        vmImage.WriteNextConstant(0);
        vmImage.WriteNextOperation(InstructionName.LessThan);
        vmImage.Goto("loopLabel", InstructionName.JumpIfZero);



        vmImage.VmRuntime.Run(vmImage);
        // Environment.Exit(0);
        Console.Read();
    }

    private static void OnProgramStart()
    {
        _stopwatch = Stopwatch.StartNew();
    }

    private static void OnProgramExit(VmRuntime.VmRuntime vmRuntime, int code, Exception? error)
    {
        var ms = -1L;
        if (_stopwatch != null)
        {
            _stopwatch.Stop();
            ms = _stopwatch.ElapsedMilliseconds;
        }

        Console.WriteLine($"Program completed in {ms} ms");


        if (code == 0)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Program executed without errors");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine($"Program executed with error [{error}]");
        }

        Console.ResetColor();

        vmRuntime.PrintState();
    }
}