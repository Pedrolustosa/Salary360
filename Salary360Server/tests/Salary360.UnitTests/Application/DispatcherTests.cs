using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Salary360.Application.Common;
using Salary360.Application.Interfaces.Messaging;

namespace Salary360.UnitTests.Application;

public sealed class DispatcherTests
{
    public sealed record EchoCommand(string Text) : ICommand<string>;

    public sealed class EchoValidator : AbstractValidator<EchoCommand>
    {
        public EchoValidator() => RuleFor(x => x.Text).NotEmpty();
    }

    public sealed class EchoHandler : ICommandHandler<EchoCommand, string>
    {
        public int Calls { get; private set; }

        public Task<string> HandleAsync(EchoCommand request, CancellationToken cancellationToken = default)
        {
            Calls++;
            return Task.FromResult(request.Text);
        }
    }

    private static (Dispatcher Dispatcher, EchoHandler Handler) Create()
    {
        var handler = new EchoHandler();
        var services = new ServiceCollection();
        services.AddSingleton<IRequestHandler<EchoCommand, string>>(handler);
        services.AddSingleton<IValidator<EchoCommand>, EchoValidator>();
        var provider = services.BuildServiceProvider();

        return (new Dispatcher(provider, NullLogger<Dispatcher>.Instance), handler);
    }

    [Fact]
    public async Task SendAsync_ValidRequest_RunsHandler()
    {
        var (dispatcher, handler) = Create();

        var result = await dispatcher.SendAsync(new EchoCommand("hi"));

        Assert.Equal("hi", result);
        Assert.Equal(1, handler.Calls);
    }

    [Fact]
    public async Task SendAsync_InvalidRequest_ThrowsBeforeHandler()
    {
        var (dispatcher, handler) = Create();

        await Assert.ThrowsAsync<ValidationException>(() => dispatcher.SendAsync(new EchoCommand("")));

        Assert.Equal(0, handler.Calls);
    }
}