using System.Data;
using EMO.Models.DBModels;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;

namespace EMO.Repositories.DeepDiveRepo;

public sealed class SensorEnergyBucketRow
{
    public Guid SensorId { get; set; }
    public string UtilityName { get; set; } = "Unknown";
    public DateTime At { get; set; }
    public double EnergyKwh { get; set; }
    public double ReactiveEnergyKvarh { get; set; }
    public double ActivePower { get; set; }
    public double MaxActivePower { get; set; }
    public double Voltage { get; set; }
    public double Current { get; set; }
    public double ReactivePower { get; set; }
    public double ApparentPower { get; set; }
    public double PowerFactor { get; set; }
    public double Frequency { get; set; }
    public int SampleCount { get; set; }
    public int PfExcellentCount { get; set; }
    public int PfGoodCount { get; set; }
    public int PfAcceptableCount { get; set; }
    public int PfPoorCount { get; set; }
    public int AlertSampleCount { get; set; }
    public DateTime FirstReadingAt { get; set; }
    public DateTime LastReadingAt { get; set; }
    public int ResetCount { get; set; }
    public int IgnoredSpikeCount { get; set; }
}


public sealed class SensorLatestReadingRow
{
    public Guid SensorId { get; set; }
    public DateTime At { get; set; }
    public int PacketId { get; set; }
    public double Voltage { get; set; }
    public double Current { get; set; }
    public double ActivePower { get; set; }
    public double PowerFactor { get; set; }
}

public interface ISensorEnergyAggregateStore
{
    Task<bool> RefreshAsync(CancellationToken cancellationToken = default);

    Task<List<SensorEnergyBucketRow>> LoadAsync(
        IReadOnlyCollection<Guid> sensorIds,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default);

    Task<Dictionary<Guid, SensorLatestReadingRow>> LoadLatestAsync(
        IReadOnlyCollection<Guid> sensorIds,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Maintains and reads reset-aware 15-minute sensor aggregates. Raw meter rows are
/// never updated or deleted. The aggregate table is an additive performance layer.
/// </summary>
public sealed class SensorEnergyAggregateStore : ISensorEnergyAggregateStore
{
    private const int BucketMinutes = 15;
    private const long AdvisoryLockKey = 704_220_261;
    private static readonly TimeSpan RefreshOverlap = TimeSpan.FromDays(7);
    private static readonly TimeSpan MaximumInitialHistory = TimeSpan.FromDays(1100);

    private readonly DBUserManagementContext _db;
    private readonly ILogger<SensorEnergyAggregateStore> _logger;

    public SensorEnergyAggregateStore(
        DBUserManagementContext db,
        ILogger<SensorEnergyAggregateStore> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<bool> RefreshAsync(CancellationToken cancellationToken = default)
    {
        var connection = _db.Database.GetDbConnection() as NpgsqlConnection
            ?? throw new InvalidOperationException(
                "Sensor energy aggregates require the PostgreSQL/Npgsql provider.");

        var openedHere = connection.State != ConnectionState.Open;
        if (openedHere)
            await connection.OpenAsync(cancellationToken);

        var lockAcquired = false;
        try
        {
            await using (var lockCommand = new NpgsqlCommand(
                "SELECT pg_try_advisory_lock(@lock_key);", connection))
            {
                lockCommand.Parameters.AddWithValue("lock_key", AdvisoryLockKey);
                lockAcquired = Convert.ToBoolean(
                    await lockCommand.ExecuteScalarAsync(cancellationToken) ?? false);
            }

            if (!lockAcquired)
            {
                _logger.LogInformation(
                    "Sensor aggregate refresh skipped because another API instance owns the worker lock.");
                return false;
            }

            var sensorIds = new List<Guid>();
            await using (var sensorCommand = new NpgsqlCommand(
                "SELECT sensor_id FROM tbl_sensor WHERE is_deleted = false;", connection))
            await using (var reader = await sensorCommand.ExecuteReaderAsync(cancellationToken))
            {
                while (await reader.ReadAsync(cancellationToken))
                    sensorIds.Add(reader.GetGuid(0));
            }

            // Only closed buckets are persisted. The API calculates an unfinished
            // boundary bucket directly from raw rows, so live data is never hidden.
            var refreshTo = FloorToBucket(DateTime.UtcNow);
            foreach (var sensorId in sensorIds)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    var refreshFrom = await ResolveRefreshStartAsync(
                        connection, sensorId, refreshTo, cancellationToken);
                    if (!refreshFrom.HasValue || refreshFrom.Value >= refreshTo)
                        continue;

                    await RefreshSensorAsync(
                        connection, sensorId, refreshFrom.Value, refreshTo, cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex,
                        "Sensor aggregate refresh failed for sensor {SensorId}; other sensors will continue.",
                        sensorId);
                }
            }

            return true;
        }
        finally
        {
            if (lockAcquired)
            {
                try
                {
                    await using var unlockCommand = new NpgsqlCommand(
                        "SELECT pg_advisory_unlock(@lock_key);", connection);
                    unlockCommand.Parameters.AddWithValue("lock_key", AdvisoryLockKey);
                    await unlockCommand.ExecuteScalarAsync(CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not release the sensor aggregate advisory lock cleanly.");
                }
            }

            if (openedHere)
                await connection.CloseAsync();
        }
    }

    public async Task<List<SensorEnergyBucketRow>> LoadAsync(
        IReadOnlyCollection<Guid> sensorIds,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken = default)
    {
        if (sensorIds.Count == 0 || to <= from)
            return new List<SensorEnergyBucketRow>();

        from = EnsureUtc(from);
        to = EnsureUtc(to);

        var connection = _db.Database.GetDbConnection() as NpgsqlConnection
            ?? throw new InvalidOperationException(
                "Sensor energy aggregates require the PostgreSQL/Npgsql provider.");
        var openedHere = connection.State != ConnectionState.Open;
        if (openedHere)
            await connection.OpenAsync(cancellationToken);

        try
        {
            try
            {
                // Use persisted aggregates for every available complete bucket and
                // calculate only missing boundary/stale-tail portions from raw rows.
                // A delayed worker therefore never forces a complete selected range
                // back through the raw minute table.
                var innerFrom = CeilToBucket(from);
                var innerTo = FloorToBucket(to);

                if (innerFrom < innerTo)
                {
                    // Read whatever persisted coverage is available, then calculate
                    // only the missing leading/trailing portions from raw data. The
                    // previous all-or-nothing coverage check recalculated a complete
                    // 24h/7d/30d/90d range whenever the newest 15-minute bucket had
                    // not yet been written by the worker.
                    var aggregateRows = await LoadAggregateRowsAsync(
                        connection, sensorIds, innerFrom, innerTo, cancellationToken);
                    var aggregateBySensor = aggregateRows
                        .GroupBy(x => x.SensorId)
                        .ToDictionary(x => x.Key, x => x.OrderBy(y => y.At).ToList());
                    var result = new List<SensorEnergyBucketRow>(aggregateRows);
                    var rawSegments = new Dictionary<
                        (DateTime From, DateTime To),
                        List<Guid>>();

                    void AddRawSegment(Guid sensorId, DateTime segmentFrom, DateTime segmentTo)
                    {
                        segmentFrom = EnsureUtc(segmentFrom);
                        segmentTo = EnsureUtc(segmentTo);
                        if (segmentTo <= segmentFrom)
                            return;

                        var key = (segmentFrom, segmentTo);
                        if (!rawSegments.TryGetValue(key, out var ids))
                        {
                            ids = new List<Guid>();
                            rawSegments[key] = ids;
                        }

                        ids.Add(sensorId);
                    }

                    foreach (var sensorId in sensorIds)
                    {
                        if (!aggregateBySensor.TryGetValue(sensorId, out var rows) ||
                            rows.Count == 0)
                        {
                            AddRawSegment(sensorId, from, to);
                            continue;
                        }

                        var firstAggregateAt = rows[0].At;
                        var aggregateCoveredTo = rows[^1].At.AddMinutes(BucketMinutes);

                        // Preserve an exact custom-range boundary before the first
                        // complete stored 15-minute bucket.
                        AddRawSegment(sensorId, from, firstAggregateAt);

                        // This is normally only the unfinished/stale live edge. Even
                        // when the worker is one hour behind, only that hour is read
                        // from the raw table rather than the complete selected range.
                        AddRawSegment(sensorId, aggregateCoveredTo, to);
                    }

                    foreach (var segment in rawSegments)
                    {
                        result.AddRange(await LoadRawCompactRowsAsync(
                            connection,
                            segment.Value.Distinct().ToArray(),
                            segment.Key.From,
                            segment.Key.To,
                            cancellationToken));
                    }

                    return result
                        .OrderBy(x => x.SensorId)
                        .ThenBy(x => x.At)
                        .ToList();
                }
            }
            catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UndefinedTable)
            {
                _logger.LogWarning(
                    "Sensor aggregate table is not available yet; using compact database-side calculation.");
            }

            // Safe compatibility fallback before the additive migration/backfill runs.
            // Calculation still happens inside PostgreSQL and returns only 15-minute rows.
            return await LoadRawCompactRowsAsync(
                connection, sensorIds, from, to, cancellationToken);
        }
        finally
        {
            if (openedHere)
                await connection.CloseAsync();
        }
    }

    public async Task<Dictionary<Guid, SensorLatestReadingRow>> LoadLatestAsync(
        IReadOnlyCollection<Guid> sensorIds,
        CancellationToken cancellationToken = default)
    {
        var result = new Dictionary<Guid, SensorLatestReadingRow>();
        if (sensorIds.Count == 0)
            return result;

        var connection = _db.Database.GetDbConnection() as NpgsqlConnection
            ?? throw new InvalidOperationException(
                "Sensor latest-reading lookup requires the PostgreSQL/Npgsql provider.");
        var openedHere = connection.State != ConnectionState.Open;
        if (openedHere)
            await connection.OpenAsync(cancellationToken);

        try
        {
            const string sql = """
                WITH requested AS
                (
                    SELECT UNNEST(@sensor_ids::uuid[]) AS sensor_id
                )
                SELECT
                    requested.sensor_id,
                    latest.created_at,
                    latest.packet_id,
                    latest.volt::double precision,
                    latest.current::double precision,
                    latest.active_power::double precision,
                    latest.power_factor::double precision
                FROM requested
                INNER JOIN LATERAL
                (
                    SELECT
                        d.created_at,
                        d.packet_id,
                        d.volt,
                        d.current,
                        d.active_power,
                        d.power_factor
                    FROM tbl_singal_phase_data d
                    WHERE d.fk_sensor = requested.sensor_id
                      AND d.is_deleted = false
                    ORDER BY d.created_at DESC, d.packet_id DESC,
                             d.singal_phase_data_id DESC
                    LIMIT 1
                ) latest ON true;
                """;

            await using var command = new NpgsqlCommand(sql, connection)
            {
                CommandTimeout = 120
            };
            AddSensorArrayParameter(command, sensorIds);

            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            while (await reader.ReadAsync(cancellationToken))
            {
                var row = new SensorLatestReadingRow
                {
                    SensorId = reader.GetGuid(0),
                    At = EnsureUtc(reader.GetDateTime(1)),
                    PacketId = reader.GetInt32(2),
                    Voltage = reader.GetDouble(3),
                    Current = reader.GetDouble(4),
                    ActivePower = reader.GetDouble(5),
                    PowerFactor = reader.GetDouble(6)
                };
                result[row.SensorId] = row;
            }

            return result;
        }
        finally
        {
            if (openedHere)
                await connection.CloseAsync();
        }
    }

    private static async Task<DateTime?> ResolveRefreshStartAsync(
        NpgsqlConnection connection,
        Guid sensorId,
        DateTime refreshTo,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                (SELECT MAX(bucket_start)
                 FROM tbl_sensor_energy_15min
                 WHERE fk_sensor = @sensor_id) AS last_bucket,
                (SELECT MIN(created_at)
                 FROM tbl_singal_phase_data
                 WHERE fk_sensor = @sensor_id AND is_deleted = false) AS first_raw;
            """;

        await using var command = new NpgsqlCommand(sql, connection)
        {
            CommandTimeout = 120
        };
        command.Parameters.AddWithValue("sensor_id", sensorId);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
            return null;

        var lastBucket = reader.IsDBNull(0) ? (DateTime?)null : EnsureUtc(reader.GetDateTime(0));
        var firstRaw = reader.IsDBNull(1) ? (DateTime?)null : EnsureUtc(reader.GetDateTime(1));
        if (!firstRaw.HasValue)
            return null;

        if (lastBucket.HasValue)
            return FloorToBucket(lastBucket.Value - RefreshOverlap);

        var earliestAllowed = refreshTo - MaximumInitialHistory;
        return FloorToBucket(firstRaw.Value < earliestAllowed ? earliestAllowed : firstRaw.Value);
    }

    private static async Task RefreshSensorAsync(
        NpgsqlConnection connection,
        Guid sensorId,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);
        try
        {
            await using (var deleteCommand = new NpgsqlCommand("""
                DELETE FROM tbl_sensor_energy_15min
                WHERE fk_sensor = @sensor_id
                  AND bucket_start >= @from
                  AND bucket_start < @to;
                """, connection, transaction))
            {
                deleteCommand.Parameters.AddWithValue("sensor_id", sensorId);
                deleteCommand.Parameters.AddWithValue("from", from);
                deleteCommand.Parameters.AddWithValue("to", to);
                deleteCommand.CommandTimeout = 300;
                await deleteCommand.ExecuteNonQueryAsync(cancellationToken);
            }

            await using var insertCommand = new NpgsqlCommand(
                BuildAggregationSql(insertIntoAggregate: true), connection, transaction)
            {
                CommandTimeout = 600
            };
            AddSingleSensorParameters(insertCommand, sensorId, from, to);
            await insertCommand.ExecuteNonQueryAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }

    private static async Task<HashSet<Guid>> FindCoveredSensorIdsAsync(
        NpgsqlConnection connection,
        IReadOnlyCollection<Guid> sensorIds,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        // The worker writes a contiguous range per sensor in a transaction. Comparing
        // the latest raw bucket with the latest aggregate bucket is therefore enough to
        // detect an unfinished/stale backfill without scanning millions of raw rows.
        const string sql = """
            WITH requested AS
            (
                SELECT UNNEST(@sensor_ids::uuid[]) AS sensor_id
            ),
            coverage AS
            (
                SELECT
                    requested.sensor_id,
                    raw_first.first_reading,
                    raw_last.last_reading,
                    aggregate_first.first_bucket,
                    aggregate_last.last_bucket
                FROM requested
                LEFT JOIN LATERAL
                (
                    SELECT d.created_at AS first_reading
                    FROM tbl_singal_phase_data d
                    WHERE d.fk_sensor = requested.sensor_id
                      AND d.is_deleted = false
                      AND d.created_at >= @from
                      AND d.created_at < @to
                    ORDER BY d.created_at ASC
                    LIMIT 1
                ) raw_first ON true
                LEFT JOIN LATERAL
                (
                    SELECT d.created_at AS last_reading
                    FROM tbl_singal_phase_data d
                    WHERE d.fk_sensor = requested.sensor_id
                      AND d.is_deleted = false
                      AND d.created_at >= @from
                      AND d.created_at < @to
                    ORDER BY d.created_at DESC
                    LIMIT 1
                ) raw_last ON true
                LEFT JOIN LATERAL
                (
                    SELECT a.bucket_start AS first_bucket
                    FROM tbl_sensor_energy_15min a
                    WHERE a.fk_sensor = requested.sensor_id
                      AND a.bucket_start >= @from
                      AND a.bucket_start < @to
                    ORDER BY a.bucket_start ASC
                    LIMIT 1
                ) aggregate_first ON true
                LEFT JOIN LATERAL
                (
                    SELECT a.bucket_start AS last_bucket
                    FROM tbl_sensor_energy_15min a
                    WHERE a.fk_sensor = requested.sensor_id
                      AND a.bucket_start >= @from
                      AND a.bucket_start < @to
                    ORDER BY a.bucket_start DESC
                    LIMIT 1
                ) aggregate_last ON true
            )
            SELECT sensor_id, first_reading, last_reading, first_bucket, last_bucket
            FROM coverage
            ORDER BY sensor_id;
            """;

        await using var command = new NpgsqlCommand(sql, connection)
        {
            CommandTimeout = 120
        };
        AddSensorArrayParameter(command, sensorIds);
        command.Parameters.AddWithValue("from", from);
        command.Parameters.AddWithValue("to", to);

        var covered = new HashSet<Guid>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var sensorId = reader.GetGuid(0);
            var hasRaw = !reader.IsDBNull(1) && !reader.IsDBNull(2);
            if (!hasRaw)
            {
                covered.Add(sensorId);
                continue;
            }

            if (reader.IsDBNull(3) || reader.IsDBNull(4))
                continue;

            var firstReading = EnsureUtc(reader.GetDateTime(1));
            var lastReading = EnsureUtc(reader.GetDateTime(2));
            var firstBucket = EnsureUtc(reader.GetDateTime(3));
            var lastBucket = EnsureUtc(reader.GetDateTime(4));
            var expectedFirstBucket = FloorToBucket(firstReading);
            var expectedLastBucket = FloorToBucket(lastReading);
            if (firstBucket <= expectedFirstBucket && lastBucket >= expectedLastBucket)
                covered.Add(sensorId);
        }

        return covered;
    }

    private static async Task<List<SensorEnergyBucketRow>> LoadAggregateRowsAsync(
        NpgsqlConnection connection,
        IReadOnlyCollection<Guid> sensorIds,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        const string sql = """
            SELECT
                a.fk_sensor,
                COALESCE(u.utility_name, 'Unknown') AS utility_name,
                a.bucket_start,
                a.energy_kwh,
                a.reactive_energy_kvarh,
                a.avg_active_power_w,
                a.max_active_power_w,
                a.avg_voltage_v,
                a.avg_current_a,
                a.avg_reactive_power_var,
                a.avg_apparent_power_va,
                a.avg_power_factor,
                a.avg_frequency_hz,
                a.sample_count,
                a.pf_excellent_count,
                a.pf_good_count,
                a.pf_acceptable_count,
                a.pf_poor_count,
                a.alert_sample_count,
                a.first_reading_at,
                a.last_reading_at,
                a.reset_count,
                a.ignored_spike_count
            FROM tbl_sensor_energy_15min a
            INNER JOIN tbl_sensor s ON s.sensor_id = a.fk_sensor
            LEFT JOIN tbl_utility u ON u.utility_id = s.fk_utility
            WHERE a.fk_sensor = ANY(@sensor_ids)
              AND a.bucket_start >= @from
              AND a.bucket_start < @to
            ORDER BY a.fk_sensor, a.bucket_start;
            """;

        await using var command = new NpgsqlCommand(sql, connection)
        {
            CommandTimeout = 180
        };
        AddSensorArrayParameter(command, sensorIds);
        command.Parameters.AddWithValue("from", from);
        command.Parameters.AddWithValue("to", to);
        return await ReadRowsAsync(command, cancellationToken);
    }

    private static async Task<List<SensorEnergyBucketRow>> LoadRawCompactRowsAsync(
        NpgsqlConnection connection,
        IReadOnlyCollection<Guid> sensorIds,
        DateTime from,
        DateTime to,
        CancellationToken cancellationToken)
    {
        if (to <= from)
            return new List<SensorEnergyBucketRow>();

        await using var command = new NpgsqlCommand(
            BuildAggregationSql(insertIntoAggregate: false), connection)
        {
            CommandTimeout = 600
        };
        AddSensorArrayParameter(command, sensorIds);
        command.Parameters.AddWithValue("from", from);
        command.Parameters.AddWithValue("to", to);
        return await ReadRowsAsync(command, cancellationToken);
    }

    private static async Task<List<SensorEnergyBucketRow>> ReadRowsAsync(
        NpgsqlCommand command,
        CancellationToken cancellationToken)
    {
        var rows = new List<SensorEnergyBucketRow>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            var at = EnsureUtc(reader.GetDateTime(2));
            rows.Add(new SensorEnergyBucketRow
            {
                SensorId = reader.GetGuid(0),
                UtilityName = reader.IsDBNull(1) ? "Unknown" : reader.GetString(1),
                At = at,
                EnergyKwh = reader.IsDBNull(3) ? 0 : reader.GetDouble(3),
                ReactiveEnergyKvarh = reader.IsDBNull(4) ? 0 : reader.GetDouble(4),
                ActivePower = reader.IsDBNull(5) ? 0 : reader.GetDouble(5),
                MaxActivePower = reader.IsDBNull(6) ? 0 : reader.GetDouble(6),
                Voltage = reader.IsDBNull(7) ? 0 : reader.GetDouble(7),
                Current = reader.IsDBNull(8) ? 0 : reader.GetDouble(8),
                ReactivePower = reader.IsDBNull(9) ? 0 : reader.GetDouble(9),
                ApparentPower = reader.IsDBNull(10) ? 0 : reader.GetDouble(10),
                PowerFactor = reader.IsDBNull(11) ? 0 : reader.GetDouble(11),
                Frequency = reader.IsDBNull(12) ? 0 : reader.GetDouble(12),
                SampleCount = reader.IsDBNull(13) ? 0 : reader.GetInt32(13),
                PfExcellentCount = reader.IsDBNull(14) ? 0 : reader.GetInt32(14),
                PfGoodCount = reader.IsDBNull(15) ? 0 : reader.GetInt32(15),
                PfAcceptableCount = reader.IsDBNull(16) ? 0 : reader.GetInt32(16),
                PfPoorCount = reader.IsDBNull(17) ? 0 : reader.GetInt32(17),
                AlertSampleCount = reader.IsDBNull(18) ? 0 : reader.GetInt32(18),
                FirstReadingAt = reader.IsDBNull(19) ? at : EnsureUtc(reader.GetDateTime(19)),
                LastReadingAt = reader.IsDBNull(20) ? at : EnsureUtc(reader.GetDateTime(20)),
                ResetCount = reader.IsDBNull(21) ? 0 : reader.GetInt32(21),
                IgnoredSpikeCount = reader.IsDBNull(22) ? 0 : reader.GetInt32(22)
            });
        }

        return rows;
    }

    private static void AddSingleSensorParameters(
        NpgsqlCommand command,
        Guid sensorId,
        DateTime from,
        DateTime to)
    {
        command.Parameters.AddWithValue("sensor_id", sensorId);
        command.Parameters.AddWithValue("from", from);
        command.Parameters.AddWithValue("to", to);
    }

    private static void AddSensorArrayParameter(
        NpgsqlCommand command,
        IReadOnlyCollection<Guid> sensorIds)
    {
        command.Parameters.Add(new NpgsqlParameter(
            "sensor_ids",
            NpgsqlDbType.Array | NpgsqlDbType.Uuid)
        {
            Value = sensorIds.ToArray()
        });
    }

    private static bool IsBucketAligned(DateTime value) =>
        value.Second == 0 && value.Millisecond == 0 && value.Minute % BucketMinutes == 0;

    private static DateTime FloorToBucket(DateTime value)
    {
        value = EnsureUtc(value);
        return new DateTime(
            value.Year,
            value.Month,
            value.Day,
            value.Hour,
            value.Minute - value.Minute % BucketMinutes,
            0,
            DateTimeKind.Utc);
    }

    private static DateTime CeilToBucket(DateTime value)
    {
        value = EnsureUtc(value);
        var floor = FloorToBucket(value);
        return IsBucketAligned(value) ? floor : floor.AddMinutes(BucketMinutes);
    }

    private static DateTime EnsureUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };

    private static string BuildAggregationSql(bool insertIntoAggregate)
    {
        var scopeCte = insertIntoAggregate
            ? """
              sensor_scope AS (
                  SELECT
                      s.sensor_id,
                      COALESCE(u.utility_name, 'Unknown') AS utility_name,
                      COALESCE(SUM(CASE
                          WHEN sa.is_active = true AND sa.is_deleted = false
                               AND a.is_active = true AND a.is_deleted = false
                          THEN a.max_power
                          ELSE 0
                      END), 0)::double precision AS max_power_w
                  FROM tbl_sensor s
                  LEFT JOIN tbl_utility u ON u.utility_id = s.fk_utility
                  LEFT JOIN tbl_sensor_appliance sa ON sa.fk_sensor = s.sensor_id
                  LEFT JOIN tbl_appliance a ON a.appliance_id = sa.fk_appliance
                  WHERE s.sensor_id = @sensor_id AND s.is_deleted = false
                  GROUP BY s.sensor_id, u.utility_name
              ),
              """
            : """
              sensor_scope AS (
                  SELECT
                      s.sensor_id,
                      COALESCE(u.utility_name, 'Unknown') AS utility_name,
                      COALESCE(SUM(CASE
                          WHEN sa.is_active = true AND sa.is_deleted = false
                               AND a.is_active = true AND a.is_deleted = false
                          THEN a.max_power
                          ELSE 0
                      END), 0)::double precision AS max_power_w
                  FROM tbl_sensor s
                  LEFT JOIN tbl_utility u ON u.utility_id = s.fk_utility
                  LEFT JOIN tbl_sensor_appliance sa ON sa.fk_sensor = s.sensor_id
                  LEFT JOIN tbl_appliance a ON a.appliance_id = sa.fk_appliance
                  WHERE s.sensor_id = ANY(@sensor_ids) AND s.is_deleted = false
                  GROUP BY s.sensor_id, u.utility_name
              ),
              """;

        var finalSelect = """
            SELECT
                sensor_id,
                utility_name,
                bucket_start,
                SUM(active_energy_delta)::double precision AS energy_kwh,
                SUM(reactive_energy_delta)::double precision AS reactive_energy_kvarh,
                AVG(active_power)::double precision AS avg_active_power_w,
                MAX(active_power)::double precision AS max_active_power_w,
                AVG(voltage)::double precision AS avg_voltage_v,
                AVG(current_a)::double precision AS avg_current_a,
                AVG(reactive_power)::double precision AS avg_reactive_power_var,
                AVG(apparent_power)::double precision AS avg_apparent_power_va,
                AVG(power_factor)::double precision AS avg_power_factor,
                AVG(frequency)::double precision AS avg_frequency_hz,
                COUNT(*)::integer AS sample_count,
                COUNT(*) FILTER (WHERE power_factor >= 0.95)::integer AS pf_excellent_count,
                COUNT(*) FILTER (WHERE power_factor >= 0.90 AND power_factor < 0.95)::integer AS pf_good_count,
                COUNT(*) FILTER (WHERE power_factor >= 0.85 AND power_factor < 0.90)::integer AS pf_acceptable_count,
                COUNT(*) FILTER (WHERE power_factor < 0.85)::integer AS pf_poor_count,
                COUNT(*) FILTER (WHERE power_factor < 0.85 OR voltage < 210 OR voltage > 250)::integer AS alert_sample_count,
                MIN(reading_at) AS first_reading_at,
                MAX(reading_at) AS last_reading_at,
                SUM(reset_count)::integer AS reset_count,
                SUM(ignored_spike_count)::integer AS ignored_spike_count
            FROM bucket_source
            GROUP BY sensor_id, utility_name, bucket_start
            ORDER BY sensor_id, bucket_start
            """;

        var aggregateInsert = """
            INSERT INTO tbl_sensor_energy_15min
            (
                fk_sensor,
                bucket_start,
                energy_kwh,
                reactive_energy_kvarh,
                avg_active_power_w,
                max_active_power_w,
                avg_voltage_v,
                avg_current_a,
                avg_reactive_power_var,
                avg_apparent_power_va,
                avg_power_factor,
                avg_frequency_hz,
                sample_count,
                pf_excellent_count,
                pf_good_count,
                pf_acceptable_count,
                pf_poor_count,
                alert_sample_count,
                first_reading_at,
                last_reading_at,
                reset_count,
                ignored_spike_count,
                updated_at
            )
            SELECT
                sensor_id,
                bucket_start,
                SUM(active_energy_delta)::double precision,
                SUM(reactive_energy_delta)::double precision,
                AVG(active_power)::double precision,
                MAX(active_power)::double precision,
                AVG(voltage)::double precision,
                AVG(current_a)::double precision,
                AVG(reactive_power)::double precision,
                AVG(apparent_power)::double precision,
                AVG(power_factor)::double precision,
                AVG(frequency)::double precision,
                COUNT(*)::integer,
                COUNT(*) FILTER (WHERE power_factor >= 0.95)::integer,
                COUNT(*) FILTER (WHERE power_factor >= 0.90 AND power_factor < 0.95)::integer,
                COUNT(*) FILTER (WHERE power_factor >= 0.85 AND power_factor < 0.90)::integer,
                COUNT(*) FILTER (WHERE power_factor < 0.85)::integer,
                COUNT(*) FILTER (WHERE power_factor < 0.85 OR voltage < 210 OR voltage > 250)::integer,
                MIN(reading_at),
                MAX(reading_at),
                SUM(reset_count)::integer,
                SUM(ignored_spike_count)::integer,
                NOW()
            FROM bucket_source
            GROUP BY sensor_id, bucket_start
            ON CONFLICT (fk_sensor, bucket_start)
            DO UPDATE SET
                energy_kwh = EXCLUDED.energy_kwh,
                reactive_energy_kvarh = EXCLUDED.reactive_energy_kvarh,
                avg_active_power_w = EXCLUDED.avg_active_power_w,
                max_active_power_w = EXCLUDED.max_active_power_w,
                avg_voltage_v = EXCLUDED.avg_voltage_v,
                avg_current_a = EXCLUDED.avg_current_a,
                avg_reactive_power_var = EXCLUDED.avg_reactive_power_var,
                avg_apparent_power_va = EXCLUDED.avg_apparent_power_va,
                avg_power_factor = EXCLUDED.avg_power_factor,
                avg_frequency_hz = EXCLUDED.avg_frequency_hz,
                sample_count = EXCLUDED.sample_count,
                pf_excellent_count = EXCLUDED.pf_excellent_count,
                pf_good_count = EXCLUDED.pf_good_count,
                pf_acceptable_count = EXCLUDED.pf_acceptable_count,
                pf_poor_count = EXCLUDED.pf_poor_count,
                alert_sample_count = EXCLUDED.alert_sample_count,
                first_reading_at = EXCLUDED.first_reading_at,
                last_reading_at = EXCLUDED.last_reading_at,
                reset_count = EXCLUDED.reset_count,
                ignored_spike_count = EXCLUDED.ignored_spike_count,
                updated_at = NOW();
            """;

        return $$"""
            WITH
            {{scopeCte}}
            source AS (
                SELECT
                    ss.sensor_id,
                    ss.utility_name,
                    ss.max_power_w,
                    raw.singal_phase_data_id AS row_id,
                    raw.packet_id,
                    raw.created_at AS reading_at,
                    raw.active_energy::double precision AS active_energy,
                    raw.reactive_energy::double precision AS reactive_energy,
                    raw.active_power::double precision AS active_power,
                    raw.reactive_power::double precision AS reactive_power,
                    raw.apperent_power::double precision AS apparent_power,
                    raw.volt::double precision AS voltage,
                    raw.current::double precision AS current_a,
                    raw.power_factor::double precision AS power_factor,
                    raw.frequency::double precision AS frequency
                FROM sensor_scope ss
                CROSS JOIN LATERAL (
                    (SELECT d.singal_phase_data_id, d.packet_id, d.created_at,
                            d.active_energy, d.reactive_energy, d.active_power,
                            d.reactive_power, d.apperent_power, d.volt, d.current,
                            d.power_factor, d.frequency
                     FROM tbl_singal_phase_data d
                     WHERE d.fk_sensor = ss.sensor_id
                       AND d.is_deleted = false
                       AND d.created_at < @from
                     ORDER BY d.created_at DESC, d.packet_id DESC, d.singal_phase_data_id DESC
                     LIMIT 2)
                    UNION ALL
                    (SELECT d.singal_phase_data_id, d.packet_id, d.created_at,
                            d.active_energy, d.reactive_energy, d.active_power,
                            d.reactive_power, d.apperent_power, d.volt, d.current,
                            d.power_factor, d.frequency
                     FROM tbl_singal_phase_data d
                     WHERE d.fk_sensor = ss.sensor_id
                       AND d.is_deleted = false
                       AND d.created_at >= @from
                       AND d.created_at < @to)
                ) raw
            ),
            ordered AS (
                SELECT
                    source.*,
                    LAG(active_energy, 1) OVER sensor_window AS previous_active_energy,
                    LAG(active_energy, 2) OVER sensor_window AS previous2_active_energy,
                    LEAD(active_energy, 1) OVER sensor_window AS next_active_energy,
                    LAG(reactive_energy, 1) OVER sensor_window AS previous_reactive_energy,
                    LAG(reactive_energy, 2) OVER sensor_window AS previous2_reactive_energy,
                    LEAD(reactive_energy, 1) OVER sensor_window AS next_reactive_energy,
                    LAG(reading_at, 1) OVER sensor_window AS previous_at,
                    LAG(reading_at, 2) OVER sensor_window AS previous2_at
                FROM source
                WINDOW sensor_window AS
                (
                    PARTITION BY sensor_id
                    ORDER BY reading_at, packet_id, row_id
                )
            ),
            candidates AS (
                SELECT
                    ordered.*,
                    CASE
                        WHEN reading_at < @from OR previous_active_energy IS NULL OR active_energy < 0 THEN 0
                        WHEN previous2_active_energy IS NOT NULL
                             AND previous_active_energy < previous2_active_energy
                             AND active_energy >= previous2_active_energy
                            THEN active_energy - previous2_active_energy
                        WHEN active_energy >= previous_active_energy
                            THEN active_energy - previous_active_energy
                        WHEN active_energy < previous_active_energy
                             AND (next_active_energy IS NULL OR next_active_energy < previous_active_energy)
                            THEN active_energy
                        ELSE 0
                    END AS active_candidate_delta,
                    CASE
                        WHEN reading_at < @from OR previous_reactive_energy IS NULL OR reactive_energy < 0 THEN 0
                        WHEN previous2_reactive_energy IS NOT NULL
                             AND previous_reactive_energy < previous2_reactive_energy
                             AND reactive_energy >= previous2_reactive_energy
                            THEN reactive_energy - previous2_reactive_energy
                        WHEN reactive_energy >= previous_reactive_energy
                            THEN reactive_energy - previous_reactive_energy
                        WHEN reactive_energy < previous_reactive_energy
                             AND (next_reactive_energy IS NULL OR next_reactive_energy < previous_reactive_energy)
                            THEN reactive_energy
                        ELSE 0
                    END AS reactive_candidate_delta,
                    CASE
                        WHEN previous2_active_energy IS NOT NULL
                             AND previous_active_energy < previous2_active_energy
                             AND active_energy >= previous2_active_energy
                            THEN EXTRACT(EPOCH FROM (reading_at - previous2_at))
                        ELSE EXTRACT(EPOCH FROM (reading_at - previous_at))
                    END AS active_elapsed_seconds,
                    CASE
                        WHEN previous2_reactive_energy IS NOT NULL
                             AND previous_reactive_energy < previous2_reactive_energy
                             AND reactive_energy >= previous2_reactive_energy
                            THEN EXTRACT(EPOCH FROM (reading_at - previous2_at))
                        ELSE EXTRACT(EPOCH FROM (reading_at - previous_at))
                    END AS reactive_elapsed_seconds,
                    CASE
                        WHEN reading_at >= @from AND
                        (
                            (active_energy < previous_active_energy
                             AND (next_active_energy IS NULL OR next_active_energy < previous_active_energy))
                            OR
                            (reactive_energy < previous_reactive_energy
                             AND (next_reactive_energy IS NULL OR next_reactive_energy < previous_reactive_energy))
                        ) THEN 1 ELSE 0
                    END AS reset_candidate,
                    CASE
                        WHEN reading_at >= @from AND
                        (
                            (active_energy < previous_active_energy
                             AND next_active_energy >= previous_active_energy)
                            OR
                            (reactive_energy < previous_reactive_energy
                             AND next_reactive_energy >= previous_reactive_energy)
                        ) THEN 1 ELSE 0
                    END AS isolated_low_candidate
                FROM ordered
            ),
            validated AS (
                SELECT
                    candidates.*,
                    GREATEST(
                        ((CASE WHEN max_power_w > 0 THEN max_power_w ELSE 100000 END) / 1000.0)
                        * (GREATEST(COALESCE(active_elapsed_seconds, 60), 60) / 3600.0)
                        * 3.0,
                        0.05
                    ) AS maximum_allowed_active_delta,
                    GREATEST(
                        ((CASE WHEN max_power_w > 0 THEN max_power_w ELSE 100000 END) / 1000.0)
                        * (GREATEST(COALESCE(reactive_elapsed_seconds, 60), 60) / 3600.0)
                        * 3.0,
                        0.05
                    ) AS maximum_allowed_reactive_delta
                FROM candidates
            ),
            bucket_source AS (
                SELECT
                    sensor_id,
                    utility_name,
                    (
                        date_trunc('hour', reading_at AT TIME ZONE 'UTC')
                        + FLOOR(EXTRACT(MINUTE FROM reading_at AT TIME ZONE 'UTC') / 15)
                          * INTERVAL '15 minutes'
                    ) AT TIME ZONE 'UTC' AS bucket_start,
                    CASE
                        WHEN active_candidate_delta > 0
                             AND active_candidate_delta <= maximum_allowed_active_delta
                            THEN active_candidate_delta
                        ELSE 0
                    END AS active_energy_delta,
                    CASE
                        WHEN reactive_candidate_delta > 0
                             AND reactive_candidate_delta <= maximum_allowed_reactive_delta
                            THEN reactive_candidate_delta
                        ELSE 0
                    END AS reactive_energy_delta,
                    active_power,
                    reactive_power,
                    apparent_power,
                    voltage,
                    current_a,
                    power_factor,
                    frequency,
                    reading_at,
                    CASE
                        WHEN reset_candidate = 1
                             AND active_candidate_delta <= maximum_allowed_active_delta
                             AND reactive_candidate_delta <= maximum_allowed_reactive_delta
                            THEN 1 ELSE 0
                    END AS reset_count,
                    CASE
                        WHEN isolated_low_candidate = 1
                             OR active_candidate_delta > maximum_allowed_active_delta
                             OR reactive_candidate_delta > maximum_allowed_reactive_delta
                            THEN 1 ELSE 0
                    END AS ignored_spike_count
                FROM validated
                WHERE reading_at >= @from AND reading_at < @to
            )
            {{(insertIntoAggregate ? aggregateInsert : finalSelect)}}
            """;
    }
}
