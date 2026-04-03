using Mediator;

namespace Application.Features.RebuildAccountsProjections;

public record RebuildAccountsProjectionsCommand(IEnumerable<Guid> AccountIds) : ICommand;
