using FluentValidation;
using MediatR;

namespace EmotionService.Infrastructure.Pipeline;

public class ValidationPipelineBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : class, IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationPipelineBehavior(IEnumerable<IValidator<TRequest>> validators)
        => _validators = validators;

    public Task<TResponse> Handle(
        TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken ct)
    {
        if (!_validators.Any())
            return next();

        var context = new ValidationContext<TRequest>(request);

        var errors = _validators
            .Select(v => v.Validate(context))
            .Where(r => r.Errors.Count != 0)
            .SelectMany(r => r.Errors)
            .DistinctBy(e => e.ErrorMessage)
            .ToList();

        if (errors.Count != 0)
            throw new ValidationException(errors);

        return next();
    }
}
