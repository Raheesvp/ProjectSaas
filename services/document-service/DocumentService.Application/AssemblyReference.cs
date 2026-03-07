using System.Reflection;

namespace DocumentService.Application;

// Assembly marker — MediatR and FluentValidation use this
// to scan for all handlers and validators in this project
// Same pattern used in Identity Service
public static class AssemblyReference
{
    public static readonly Assembly Assembly =
        typeof(AssemblyReference).Assembly;
}