namespace Interpreter;

public static class Extension
{
    public static bool IsEquals(this decimal a, decimal b)
    {
        return decimal.Round(a, 25, MidpointRounding.ToZero) == decimal.Round(b, 25, MidpointRounding.ToZero);
    }
}