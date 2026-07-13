using EMO.Models.DBModels;
using Microsoft.EntityFrameworkCore;

namespace EMO.Repositories.AgreementServicesRepo;

/// <summary>
/// Keeps agreement status and office occupancy synchronized once at startup and
/// at local midnight. It never deletes agreements or historical assignments.
/// </summary>
public sealed class AgreementStatusWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AgreementStatusWorker> _logger;

    public AgreementStatusWorker(IServiceScopeFactory scopeFactory, ILogger<AgreementStatusWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await SynchronizeAsync(stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.Now;
            var nextMidnight = now.Date.AddDays(1);
            await Task.Delay(nextMidnight - now, stoppingToken);
            await SynchronizeAsync(stoppingToken);
        }
    }

    private async Task SynchronizeAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DBUserManagementContext>();
            var today = DateTime.Today;
            var now = DateTime.Now;

            var expired = await db.tbl_agreement
                .Where(x => !x.is_deleted && x.is_active && x.agreement_end_date.Date < today)
                .ToListAsync(cancellationToken);
            foreach (var agreement in expired)
            {
                agreement.is_active = false;
                agreement.updated_at = now;
            }

            var occupiedOfficeIds = await (
                from agreement in db.tbl_agreement.AsNoTracking()
                join officeAgreement in db.tbl_office_agreement.AsNoTracking()
                    on agreement.agreement_id equals officeAgreement.fk_agreement
                where agreement.is_active
                      && !agreement.is_deleted
                      && agreement.agreement_start_date.Date <= today
                      && agreement.agreement_end_date.Date >= today
                      && !officeAgreement.is_deleted
                select officeAgreement.fk_office)
                .Distinct()
                .ToListAsync(cancellationToken);

            var occupiedSet = occupiedOfficeIds.ToHashSet();
            var offices = await db.tbl_office.Where(x => !x.is_deleted).ToListAsync(cancellationToken);
            foreach (var office in offices)
            {
                var shouldBeOccupied = occupiedSet.Contains(office.office_id);
                if (office.is_occupied != shouldBeOccupied)
                {
                    office.is_occupied = shouldBeOccupied;
                    office.updated_at = now;
                }
            }

            await db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Agreement status synchronized. Expired: {ExpiredCount}, occupied offices: {OccupiedCount}",
                expired.Count, occupiedOfficeIds.Count);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Agreement status synchronization failed");
        }
    }
}
