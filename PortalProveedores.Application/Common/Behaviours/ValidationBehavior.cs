using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using ValidationException = PortalProveedores.Application.Common.Exceptions.ValidationException; // (Excepción custom)

namespace PortalProveedores.Application.Common.Behaviours
{
    /// <summary>
    /// Este archivo es opcional pero muy recomendado. Es el "pegamento" entre MediatR y FluentValidation.
    /// Intercepta todos los comandos antes de que lleguen al Handler y ejecuta sus validaciones.
    /// </summary>
    public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
        {
            _validators = validators;
        }

        public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            if (_validators.Any())
            {
                var context = new ValidationContext<TRequest>(request);

                var validationResults = await Task.WhenAll(
                    _validators.Select(v =>
                        v.ValidateAsync(context, cancellationToken)));

                var failures = validationResults
                    .Where(r => r.Errors.Any())
                    .SelectMany(r => r.Errors)
                    .ToList();

                if (failures.Any())
                    throw new ValidationException(failures); // Lanzar excepción
            }
            return await next(); // Continuar al Handler si todo está OK
        }
    }
}
