﻿using FluentValidation;
using MediatR;
using FluentValidation.Results;
using IdentityUser.src.Application.Common.Exceptions;

namespace IdentityUser.src.Application.Common.Behaviors
{
    public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators) => _validators = validators;

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var context = new ValidationContext<TRequest>(request);
            var validationFailures = new List<ValidationFailure>();

            foreach (var validator in _validators)
            {
                var validationResult = await validator.ValidateAsync(context, cancellationToken);
                if (!validationResult.IsValid)
                {
                    validationFailures.AddRange(validationResult.Errors);
                }
            }

            if (validationFailures.Any())
            {
                var errorMessages = validationFailures.Select(e => e.ErrorMessage).ToArray();
                throw new BadRequestException(errorMessages);
            }

            return await next();
        }

    }
}
