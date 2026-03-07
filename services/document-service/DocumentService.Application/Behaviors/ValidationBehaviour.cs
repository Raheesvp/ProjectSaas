using FluentValidation;
using MediatR;
using Shared.Domain.Common;

namespace DocumentService.Application.Behaviors;

// MediatR pipeline behavior — identical to Identity Service
// Runs FluentValidation BEFORE every command handler
//
// Flow:
// Controller → MediatR.Send(command)
//           → ValidationBehavior (this class)
//           → If invalid → return Result.Failure immediately
//           → If valid   → CommandHandler.Handle() runs
//
// Zero validation code inside any handler — clean separation
public sealed class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(
        IEnumerable<IValidator<TRequest>> validators)
        => _validators = validators;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // No validators registered — skip validation
        if (!_validators.Any())
            return await next();

        var context = new ValidationContext<TRequest>(request);

        var failures = _validators
            .Select(v => v.Validate(context))
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        // All validators passed — continue to handler
        if (failures.Count == 0)
            return await next();

        // Return first validation failure as Result
        var error = new Error(
            failures[0].PropertyName,
            failures[0].ErrorMessage);

        return (TResponse)typeof(Result)
            .GetMethod(nameof(Result.Failure))!
            .MakeGenericMethod(
                typeof(TResponse).GenericTypeArguments
                .FirstOrDefault() ?? typeof(object))
            .Invoke(null, new object[] { error })!;
    }
}