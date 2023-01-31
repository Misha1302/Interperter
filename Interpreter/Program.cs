using System.Diagnostics;

namespace Interpreter;

public static class Program
{
    private static Stopwatch? _stopwatch;

    public static void Main()
    {
        var vmImage = new VmImage();

        vmImage.VmRuntime.OnProgramStart += OnProgramStart;
        vmImage.VmRuntime.OnProgramExit += OnProgramExit;


        vmImage.CreateVariableNumber("counter");

        vmImage.WriteNextOperation(InstructionName.LoadConstNumber);
        vmImage.WriteNextNumber("100_000_000");
        vmImage.SetVariableNumber("counter");

        vmImage.SetLabel("label");
        
        vmImage.LoadVariableNumber("counter");
        vmImage.WriteNextOperation(InstructionName.LoadConstNumber);
        vmImage.WriteNextNumber("1");
        vmImage.WriteNextOperation(InstructionName.SubNumber);
        vmImage.SetVariableNumber("counter");
        
        // vmImage.LoadVariableNumber("counter");
        
        vmImage.LoadVariableNumber("counter");
        vmImage.GotoIfNotZeroNumber("label");


        vmImage.VmRuntime.Run(vmImage);
        // Environment.Exit(0);
        Console.Read();
    }

    private static void OnProgramStart()
    {
        _stopwatch = Stopwatch.StartNew();
    }

    private static void OnProgramExit(string errorString, VmRuntime.VmRuntime memory)
    {
        var ms = -1L;
        if (_stopwatch != null)
        {
            _stopwatch.Stop();
            ms = _stopwatch.ElapsedMilliseconds;
        }

        Console.WriteLine($"Program completed in {ms} ms");


        if (errorString == string.Empty)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Program executed without errors");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Error.WriteLine($"Program executed with error [{errorString}]");
        }

        Console.ResetColor();

        memory.PrintState();
    }
}