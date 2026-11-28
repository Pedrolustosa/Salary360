using NSubstitute;
using Salary360.Application.Auth;
using Salary360.Application.Auth.Commands;
using Salary360.Application.Interfaces.Identity;
using Salary360.Domain.Common;
using Salary360.Domain.Enums;

namespace Salary360.UnitTests.Application;

public sealed class SetUserStatusCommandHandlerTests
{
    private readonly IIdentityService identity = Substitute.For<IIdentityService>();
    private readonly ICurrentUser currentUser = Substitute.For<ICurrentUser>();

    [Fact]
    public async Task HandleAsync_DeactivatingYourself_ThrowsDomainException()
    {
        var userId = Guid.NewGuid();
        currentUser.GetUserId().Returns(userId);
        var handler = new SetUserStatusCommandHandler(identity, currentUser);

        var exception = await Assert.ThrowsAsync<DomainException>(
            () => handler.HandleAsync(new SetUserStatusCommand(userId, UserStatus.Inactive)));

        Assert.Equal("cannot_deactivate_self", exception.Code);
        await identity.DidNotReceiveWithAnyArgs().SetStatusAsync(default, default, default);
    }

    [Fact]
    public async Task HandleAsync_DeactivatingAnotherUser_CallsIdentityService()
    {
        currentUser.GetUserId().Returns(Guid.NewGuid());
        var target = Guid.NewGuid();
        var handler = new SetUserStatusCommandHandler(identity, currentUser);

        await handler.HandleAsync(new SetUserStatusCommand(target, UserStatus.Inactive));

        await identity.Received(1).SetStatusAsync(target, UserStatus.Inactive, Arg.Any<CancellationToken>());
    }

    [Fact]
    public void Validator_UnknownStatus_Fails()
        => Assert.False(new SetUserStatusCommandValidator().Validate(new SetUserStatusCommand(Guid.NewGuid(), (UserStatus)99)).IsValid);

    [Fact]
    public void Validator_EmptyUserId_Fails()
        => Assert.False(new SetUserStatusCommandValidator().Validate(new SetUserStatusCommand(Guid.Empty, UserStatus.Active)).IsValid);
}