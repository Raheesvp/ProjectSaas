using System.Reflection;

namespace DocumentService.Infrastructure;

public static class AssemblyReference
{
    public static readonly Assembly Assembly =
        typeof(AssemblyReference).Assembly;
}
