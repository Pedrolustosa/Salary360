namespace Salary360.Domain.Interfaces;

public interface IDomainEvent
{
    DateTime OccurredOnUtc { get; }
}
