using DRT.Application.Abstractions;
using DRT.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DRT.Infrastructure.Persistence;

/// <summary>
/// Repository implementation for Core ASK aggregate entities.
/// </summary>
public sealed class CoreAskRepository : ICoreAskRepository
{
    private readonly ApplicationDbContext _dbContext;

    public CoreAskRepository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAskAsync(Ask ask, CancellationToken cancellationToken = default)
        => await _dbContext.Asks.AddAsync(ask, cancellationToken);

    public async Task AddAskVersionAsync(AskVersion askVersion, CancellationToken cancellationToken = default)
        => await _dbContext.AskVersions.AddAsync(askVersion, cancellationToken);

    public async Task AddCoreAskDetailAsync(CoreAskDetail detail, CancellationToken cancellationToken = default)
        => await _dbContext.CoreAskDetails.AddAsync(detail, cancellationToken);

    public async Task AddAskCommentAsync(AskComment comment, CancellationToken cancellationToken = default)
        => await _dbContext.AskComments.AddAsync(comment, cancellationToken);

    public async Task AddAskAttachmentLinkAsync(AskAttachmentLink link, CancellationToken cancellationToken = default)
        => await _dbContext.AskAttachmentLinks.AddAsync(link, cancellationToken);

    public async Task AddWorkflowTaskAsync(WorkflowTask task, CancellationToken cancellationToken = default)
        => await _dbContext.WorkflowTasks.AddAsync(task, cancellationToken);
}
