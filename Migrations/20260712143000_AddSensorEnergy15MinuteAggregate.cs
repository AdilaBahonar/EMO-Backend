using EMO.Models.DBModels;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EMO.Migrations
{
    /// <summary>
    /// Additive migration only. Raw sensor readings are not changed or deleted.
    /// The new table is a rebuildable performance layer over existing data.
    /// </summary>
    [DbContext(typeof(DBUserManagementContext))]
    [Migration("20260712143000_AddSensorEnergy15MinuteAggregate")]
    public partial class AddSensorEnergy15MinuteAggregate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE tbl_dashboard_chart_aggregate
                    ADD COLUMN IF NOT EXISTS scope_id uuid NULL;
                ALTER TABLE tbl_dashboard_chart_aggregate
                    ADD COLUMN IF NOT EXISTS scope_level text NULL;

                CREATE TABLE IF NOT EXISTS tbl_sensor_energy_15min
                (
                    fk_sensor uuid NOT NULL,
                    bucket_start timestamp with time zone NOT NULL,
                    energy_kwh double precision NOT NULL DEFAULT 0,
                    reactive_energy_kvarh double precision NOT NULL DEFAULT 0,
                    avg_active_power_w double precision NOT NULL DEFAULT 0,
                    max_active_power_w double precision NOT NULL DEFAULT 0,
                    avg_voltage_v double precision NOT NULL DEFAULT 0,
                    avg_current_a double precision NOT NULL DEFAULT 0,
                    avg_reactive_power_var double precision NOT NULL DEFAULT 0,
                    avg_apparent_power_va double precision NOT NULL DEFAULT 0,
                    avg_power_factor double precision NOT NULL DEFAULT 0,
                    avg_frequency_hz double precision NOT NULL DEFAULT 0,
                    sample_count integer NOT NULL DEFAULT 0,
                    pf_excellent_count integer NOT NULL DEFAULT 0,
                    pf_good_count integer NOT NULL DEFAULT 0,
                    pf_acceptable_count integer NOT NULL DEFAULT 0,
                    pf_poor_count integer NOT NULL DEFAULT 0,
                    alert_sample_count integer NOT NULL DEFAULT 0,
                    first_reading_at timestamp with time zone NOT NULL,
                    last_reading_at timestamp with time zone NOT NULL,
                    reset_count integer NOT NULL DEFAULT 0,
                    ignored_spike_count integer NOT NULL DEFAULT 0,
                    updated_at timestamp with time zone NOT NULL DEFAULT NOW(),
                    CONSTRAINT pk_sensor_energy_15min PRIMARY KEY (fk_sensor, bucket_start)
                );

                -- These ALTER statements make the migration safe even if an earlier
                -- test version of the aggregate table was created manually.
                ALTER TABLE tbl_sensor_energy_15min
                    ADD COLUMN IF NOT EXISTS reactive_energy_kvarh double precision NOT NULL DEFAULT 0,
                    ADD COLUMN IF NOT EXISTS avg_current_a double precision NOT NULL DEFAULT 0,
                    ADD COLUMN IF NOT EXISTS avg_reactive_power_var double precision NOT NULL DEFAULT 0,
                    ADD COLUMN IF NOT EXISTS avg_apparent_power_va double precision NOT NULL DEFAULT 0,
                    ADD COLUMN IF NOT EXISTS avg_frequency_hz double precision NOT NULL DEFAULT 0,
                    ADD COLUMN IF NOT EXISTS pf_excellent_count integer NOT NULL DEFAULT 0,
                    ADD COLUMN IF NOT EXISTS pf_good_count integer NOT NULL DEFAULT 0,
                    ADD COLUMN IF NOT EXISTS pf_acceptable_count integer NOT NULL DEFAULT 0,
                    ADD COLUMN IF NOT EXISTS pf_poor_count integer NOT NULL DEFAULT 0,
                    ADD COLUMN IF NOT EXISTS alert_sample_count integer NOT NULL DEFAULT 0;

                CREATE INDEX IF NOT EXISTS ix_sensor_energy_15min_bucket
                    ON tbl_sensor_energy_15min (bucket_start, fk_sensor);

                -- Remove duplicate cached payloads created by concurrent older workers.
                -- This affects only rebuildable dashboard cache rows, never meter data.
                DELETE FROM tbl_dashboard_chart_aggregate older
                USING tbl_dashboard_chart_aggregate newer
                WHERE older.dashboard_chart_aggregate_id <> newer.dashboard_chart_aggregate_id
                  AND older.scope_level IS NOT DISTINCT FROM newer.scope_level
                  AND older.scope_id IS NOT DISTINCT FROM newer.scope_id
                  AND older.chart_type = newer.chart_type
                  AND older.range_key = newer.range_key
                  AND (
                        older.updated_at < newer.updated_at
                        OR (older.updated_at = newer.updated_at
                            AND older.dashboard_chart_aggregate_id::text < newer.dashboard_chart_aggregate_id::text)
                  );

                DROP INDEX IF EXISTS "IX_dashboard_chart_scope";
                CREATE UNIQUE INDEX IF NOT EXISTS "UX_dashboard_chart_scope"
                    ON tbl_dashboard_chart_aggregate
                    (scope_level, scope_id, chart_type, range_key);
                """);

            // This index can be large on an existing minute-data table. CONCURRENTLY
            // avoids blocking normal inserts while PostgreSQL builds it.
            migrationBuilder.Sql("""
                CREATE INDEX CONCURRENTLY IF NOT EXISTS "IX_signal_phase_sensor_created_at"
                    ON tbl_singal_phase_data (fk_sensor, created_at);
                """, suppressTransaction: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DROP INDEX IF EXISTS "UX_dashboard_chart_scope";
                CREATE INDEX IF NOT EXISTS "IX_dashboard_chart_scope"
                    ON tbl_dashboard_chart_aggregate
                    (scope_level, scope_id, chart_type, range_key);

                DROP TABLE IF EXISTS tbl_sensor_energy_15min;
                """);
        }
    }
}
