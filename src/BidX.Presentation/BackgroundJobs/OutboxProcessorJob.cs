using System.Reflection;
using System.Text.Json;
using BidX.DataAccess;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Quartz;
using Serilog;

namespace BidX.Presentation.BackgroundJobs;

[DisallowConcurrentExecution]
public class OutboxProcessorJob : IJob
{
    private readonly IDbContextFactory<AppDbContext> dbContextFactory;
    private readonly Assembly assembly;
    private readonly IMediator mediator;

    // Use DbContextFactory instead of injecting AppDbContext directly
    // to avoid sharing the same AppDbContext instance with MediatR handlers and prevent EF Core change tracker conflicts.
    public OutboxProcessorJob(IDbContextFactory<AppDbContext> dbContextFactory, Assembly assembly, IMediator mediator)
    {
        this.dbContextFactory = dbContextFactory;
        this.assembly = assembly;
        this.mediator = mediator;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        // fresh DbContext instance for this job execution (isolated from handlers)
        using var appDbContext = await dbContextFactory.CreateDbContextAsync(context.CancellationToken);

        var messages = await appDbContext.OutboxMessages
            .Where(m => m.ProcessedAt == null)
            .OrderBy(m => m.CreatedAt)
            .Take(10)
            .ToListAsync(context.CancellationToken);

        foreach (var message in messages)
        {
            try
            {
                var messageType = assembly.GetType(message.Type)
                    ?? throw new InvalidOperationException(
                        $"Type '{message.Type}' was not found in assembly '{assembly.FullName}'.");

                var deserializedMessage = JsonSerializer.Deserialize(message.Content, messageType)
                    ?? throw new InvalidOperationException(
                        $"Failed to deserialize message of type '{message.Type}'.");

                await mediator.Publish(deserializedMessage, context.CancellationToken);

                message.ProcessedAt = DateTimeOffset.UtcNow;
            }
            catch (Exception ex)
            {
                Log.Error(ex.ToString());
                message.Error = ex.Message;
            }
        }

        await appDbContext.SaveChangesAsync(context.CancellationToken);
    }

}
