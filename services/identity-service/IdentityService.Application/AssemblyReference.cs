using System.Reflection;

// Assembly marker — used for MediatR and FluentValidation scanning
// Without this, MediatR can't find handlers in Application assembly

namespace IdentityService.Application;
public static class AssemblyReference
{
    public static readonly Assembly Assembly = typeof(AssemblyReference).Assembly;
}