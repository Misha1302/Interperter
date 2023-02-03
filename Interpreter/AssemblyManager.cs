namespace Interpreter;

using System.Reflection;

public class AssemblyManager
{
    public delegate void CallingDelegate(VmRuntime.VmRuntime vmRuntime);

    private readonly List<CallingDelegate> _methods = new();

    public CallingDelegate GetMethodByIndex(int index)
    {
        return _methods[index];
    }

    public void ImportMethodFromAssembly(string dllPath, string methodName)
    {
        var assembly = Assembly.LoadFrom(Path.GetFullPath(dllPath));
        var @class = assembly.GetType("Library.Library");
        if (@class is null) throw new Exception("Class 'Library' not found");

        var method = @class.GetMethod(methodName) ?? throw new Exception($"Method '{methodName}' not found");
        _methods.Add(method.CreateDelegate<CallingDelegate>());
    }
}