using System.Runtime.CompilerServices;

namespace Interpreter;

public unsafe class VmMemory
{
    public static readonly int Step = sizeof(Int128);
    public byte[] MemoryArray = Array.Empty<byte>();

    public int StackStart;
    public int StackEnd;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void WriteWord(Number128 word, Number128 position)
    {
        var pos = position.Low;
        fixed (byte* w = &MemoryArray[pos])
        {
            Unsafe.Write(w, word);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Number128 ReadWord(Number128 position)
    {
        var pos = position.Low;
        fixed (byte* w = &MemoryArray[pos])
        {
            var word = Unsafe.Read<Number128>(w);
            return word;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int MoveIntToForward(int number)
    {
        return number + Step;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int MoveIntToBackward(int number)
    {
        return number - Step;
    }

    #region registers

    public int StackPointer;
    public int InstructionPointer;
    public int DataPointer;

    #endregion
}