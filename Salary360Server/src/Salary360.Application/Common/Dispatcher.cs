using System.Collections.Concurrent;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Salary360.Application.Interfaces.Messaging;

namespace Salary360.Application.Common;

public sealed class Dispatcher(IServiceProvider serviceProvider, ILogger<Dispatcher> logger) : IDispatcher
{
    private static readonly ConcurrentDictionary<Type, object> Wrappers = new();

    public Task<TResponse> SendAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var wrapper = (RequestWrapper<TResponse>)Wrappers.GetOrAdd(
            request.GetType(),
            static requestType => Activator.CreateInstance(
                typeof(RequestWrapper<,>).MakeGenericType(requestType, typeof(TResponse)))!);

        return wrapper.HandleAsync(request, serviceProvider, logger, cancellationToken);
    }

    private abstract class RequestWrapper<TResponse>
    {
        public abstract Task<TResponse> HandleAsync(
            IRequest<TResponse> request, IServiceProvider provider, ILogger logger, CancellationToken cancellationToken);
    }

    private sealed class RequestWrapper<TRequest, TResponse> : RequestWrapper<TResponse>
        where TRequest : IRequest<TResponse>
    {
        public override async Task<TResponse> HandleAsync(
            IRequest<TResponse> request, IServiceProvider provider, ILogger logger, CancellationToken cancellationToken)
        {
            var typedRequest = (TRequest)request;

            await ValidateAsync(typedRequest, provider, cancellationToken);

            var handler = provider.GetRequiredService<IRequestHandler<TRequest, TResponse>>();

            logger.LogInformation("Handling {RequestName}", typeof(TRequest).Name);
            var response = await handler.HandleAsync(typedRequest, cancellationToken);
            logger.LogInformation("Handled {RequestName}", typeof(TRequest).Name);

            return response;
        }

        private static async Task ValidateAsync(TRequest request, IServiceProvider provider, CancellationToken cancellationToken)
        {
            var validators = provider.GetServices<IValidator<TRequest>>().ToList();
            if (validators.Count == 0)
                return;

            var context = new ValidationContext<TRequest>(request);
            var failures = new List<FluentValidation.Results.ValidationFailure>();

            foreach (var validator in validators)
            {
                var result = await validator.ValidateAsync(context, cancellationToken);
                failures.AddRange(result.Errors);
            }

            if (failures.Count > 0)
                throw new ValidationException(failures);
        }
    }
}