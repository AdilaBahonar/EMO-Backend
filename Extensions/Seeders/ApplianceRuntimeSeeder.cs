using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EMO.Models.DBModels;
using EMO.Models.DBModels.DBTables;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EMO.Extensions.Seeders
{
    /// <summary>
    /// Seeds realistic engineering baseline values for normal appliance operation.
    ///
    /// Rules used:
    /// 1. Min/Max power represent normal active operating states, not OFF or sleep.
    /// 2. Standby power represents sleep, ready, control-electronics, or compressor-off demand.
    /// 3. Sub-second motor inrush is intentionally excluded from the active-power range.
    /// 4. Current is calculated from active power using I = P / (V x PF).
    /// 5. Multi-stage appliances may still have wider ranges because heaters, compressors,
    ///    pumps, and fans do not operate at the same power throughout a full cycle.
    ///
    /// These values are defensible monitoring defaults. A measured device nameplate or
    /// manufacturer datasheet should override the default when the exact installed model is known.
    /// </summary>
    public static class ApplianceRuntimeSeeder
    {
        public static async Task SeedDefaultAppliancesAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DBUserManagementContext>();
            await SeedDefaultAppliancesAsync(db);
        }

        public static async Task SeedDefaultAppliancesAsync(DBUserManagementContext db)
        {
            var now = DateTime.Now;

            var hvacUtilityId = await GetOrCreateUtilityAsync(db, "HVAC");
            var miscellaneousUtilityId = await GetOrCreateUtilityAsync(db, "Miscellaneous");
            var computingUtilityId = await GetOrCreateUtilityAsync(db, "Computing");
            var lightingUtilityId = await GetOrCreateUtilityAsync(db, "Lighting");

            var utilityIds = new Dictionary<string, Guid>(StringComparer.OrdinalIgnoreCase)
            {
                ["HVAC"] = hvacUtilityId,
                ["Miscellaneous"] = miscellaneousUtilityId,
                ["Computing"] = computingUtilityId,
                ["Lighting"] = lightingUtilityId
            };

            var appliances = new List<ApplianceSeedItem>
            {
                new ApplianceSeedItem
                {
                    UtilityName = "HVAC",
                    ApplianceName = "Split Air Conditioner 1.5 Ton",
                    CompanyName = "GREE",
                    ModelNumber = "Pular 18K Inverter",
                    RatedVoltage = 230f,
                    MinCurrent = 2.059f,
                    MaxCurrent = 8.696f,
                    MinPower = 450f,
                    MaxPower = 1900f,
                    StandbyPower = 5f,
                    NormalPowerFactor = 0.95f,
                    Description = "Inverter split AC for office cooling load and after-hours wastage detection."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "HVAC",
                    ApplianceName = "Split Air Conditioner 1 Ton",
                    CompanyName = "Haier",
                    ModelNumber = "HSU-12 Inverter",
                    RatedVoltage = 230f,
                    MinCurrent = 1.602f,
                    MaxCurrent = 6.178f,
                    MinPower = 350f,
                    MaxPower = 1350f,
                    StandbyPower = 5f,
                    NormalPowerFactor = 0.95f,
                    Description = "Small inverter AC for rooms and offices."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "HVAC",
                    ApplianceName = "Split Air Conditioner 1.5 Ton",
                    CompanyName = "Dawlance",
                    ModelNumber = "Inverter 30 Series",
                    RatedVoltage = 230f,
                    MinCurrent = 2.059f,
                    MaxCurrent = 8.467f,
                    MinPower = 450f,
                    MaxPower = 1850f,
                    StandbyPower = 5f,
                    NormalPowerFactor = 0.95f,
                    Description = "Office split AC for HVAC consumption monitoring."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "HVAC",
                    ApplianceName = "Split Air Conditioner 1 Ton",
                    CompanyName = "Orient",
                    ModelNumber = "Ultron Classic 12G",
                    RatedVoltage = 230f,
                    MinCurrent = 1.602f,
                    MaxCurrent = 6.178f,
                    MinPower = 350f,
                    MaxPower = 1350f,
                    StandbyPower = 5f,
                    NormalPowerFactor = 0.95f,
                    Description = "Compact inverter AC for small office areas."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "HVAC",
                    ApplianceName = "Split Air Conditioner 1.5 Ton",
                    CompanyName = "Kenwood",
                    ModelNumber = "eSmart 18K",
                    RatedVoltage = 230f,
                    MinCurrent = 2.059f,
                    MaxCurrent = 8.467f,
                    MinPower = 450f,
                    MaxPower = 1850f,
                    StandbyPower = 5f,
                    NormalPowerFactor = 0.95f,
                    Description = "Energy efficient AC used for room cooling."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "HVAC",
                    ApplianceName = "WindFree Air Conditioner 1.5 Ton",
                    CompanyName = "Samsung",
                    ModelNumber = "AR18 WindFree",
                    RatedVoltage = 230f,
                    MinCurrent = 2.059f,
                    MaxCurrent = 8.696f,
                    MinPower = 450f,
                    MaxPower = 1900f,
                    StandbyPower = 5f,
                    NormalPowerFactor = 0.95f,
                    Description = "Office cooling appliance with variable compressor load."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "HVAC",
                    ApplianceName = "Dual Inverter Air Conditioner 1.5 Ton",
                    CompanyName = "LG",
                    ModelNumber = "DualCool 18K",
                    RatedVoltage = 230f,
                    MinCurrent = 2.059f,
                    MaxCurrent = 8.238f,
                    MinPower = 450f,
                    MaxPower = 1800f,
                    StandbyPower = 5f,
                    NormalPowerFactor = 0.95f,
                    Description = "Inverter air conditioner suitable for HVAC load analytics."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "HVAC",
                    ApplianceName = "Inverter Air Conditioner 1.5 Ton",
                    CompanyName = "Mitsubishi Electric",
                    ModelNumber = "MSY-GS18",
                    RatedVoltage = 230f,
                    MinCurrent = 2.288f,
                    MaxCurrent = 9.153f,
                    MinPower = 500f,
                    MaxPower = 2000f,
                    StandbyPower = 5f,
                    NormalPowerFactor = 0.95f,
                    Description = "Premium split AC for HVAC monitoring."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "HVAC",
                    ApplianceName = "Inverter Air Conditioner 1.5 Ton",
                    CompanyName = "Daikin",
                    ModelNumber = "FTXA50BV1H",
                    RatedVoltage = 230f,
                    MinCurrent = 2.059f,
                    MaxCurrent = 8.696f,
                    MinPower = 450f,
                    MaxPower = 1900f,
                    StandbyPower = 6f,
                    NormalPowerFactor = 0.95f,
                    Description = "High-capacity inverter AC for commercial cooling zones."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "HVAC",
                    ApplianceName = "Split Air Conditioner 2 Ton",
                    CompanyName = "Carrier",
                    ModelNumber = "XPower Gold 24K",
                    RatedVoltage = 230f,
                    MinCurrent = 3.204f,
                    MaxCurrent = 11.899f,
                    MinPower = 700f,
                    MaxPower = 2600f,
                    StandbyPower = 8f,
                    NormalPowerFactor = 0.95f,
                    Description = "Large room AC for higher HVAC load monitoring."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "HVAC",
                    ApplianceName = "Inverter Air Conditioner 1 Ton",
                    CompanyName = "Panasonic",
                    ModelNumber = "CS-PU12",
                    RatedVoltage = 230f,
                    MinCurrent = 1.602f,
                    MaxCurrent = 5.95f,
                    MinPower = 350f,
                    MaxPower = 1300f,
                    StandbyPower = 5f,
                    NormalPowerFactor = 0.95f,
                    Description = "Small office AC with moderate cooling load."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "HVAC",
                    ApplianceName = "Inverter Air Conditioner 1.5 Ton",
                    CompanyName = "TCL",
                    ModelNumber = "TAC-18HE",
                    RatedVoltage = 230f,
                    MinCurrent = 2.059f,
                    MaxCurrent = 8.238f,
                    MinPower = 450f,
                    MaxPower = 1800f,
                    StandbyPower = 5f,
                    NormalPowerFactor = 0.95f,
                    Description = "Smart inverter AC for office cooling."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "HVAC",
                    ApplianceName = "Inverter Air Conditioner 1.5 Ton",
                    CompanyName = "Midea",
                    ModelNumber = "XtremeSave 18K",
                    RatedVoltage = 230f,
                    MinCurrent = 2.059f,
                    MaxCurrent = 8.467f,
                    MinPower = 450f,
                    MaxPower = 1850f,
                    StandbyPower = 5f,
                    NormalPowerFactor = 0.95f,
                    Description = "Inverter split AC for HVAC optimization."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "HVAC",
                    ApplianceName = "Inverter Air Conditioner 1.5 Ton",
                    CompanyName = "Hisense",
                    ModelNumber = "Energy Pro 18K",
                    RatedVoltage = 230f,
                    MinCurrent = 2.059f,
                    MaxCurrent = 8.009f,
                    MinPower = 450f,
                    MaxPower = 1750f,
                    StandbyPower = 5f,
                    NormalPowerFactor = 0.95f,
                    Description = "Office split AC for cooling load monitoring."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "HVAC",
                    ApplianceName = "Ceiling Cassette Air Conditioner",
                    CompanyName = "Mitsubishi Heavy",
                    ModelNumber = "FDT Commercial Cassette",
                    RatedVoltage = 230f,
                    MinCurrent = 4.577f,
                    MaxCurrent = 14.645f,
                    MinPower = 1000f,
                    MaxPower = 3200f,
                    StandbyPower = 10f,
                    NormalPowerFactor = 0.95f,
                    Description = "Ceiling cassette AC for commercial HVAC zones."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "HVAC",
                    ApplianceName = "Fan Coil Unit",
                    CompanyName = "Carrier",
                    ModelNumber = "42CE FCU",
                    RatedVoltage = 230f,
                    MinCurrent = 0.512f,
                    MaxCurrent = 1.125f,
                    MinPower = 100f,
                    MaxPower = 220f,
                    StandbyPower = 3f,
                    NormalPowerFactor = 0.85f,
                    Description = "Fan coil unit used in centralized HVAC systems."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "HVAC",
                    ApplianceName = "Air Handling Unit",
                    CompanyName = "Trane",
                    ModelNumber = "Commercial AHU",
                    RatedVoltage = 230f,
                    MinCurrent = 5.797f,
                    MaxCurrent = 15.459f,
                    MinPower = 1200f,
                    MaxPower = 3200f,
                    StandbyPower = 10f,
                    NormalPowerFactor = 0.9f,
                    Description = "Commercial air handling unit for floor-level HVAC load."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "HVAC",
                    ApplianceName = "Exhaust Fan",
                    CompanyName = "Royal Fan",
                    ModelNumber = "Industrial Exhaust 18 Inch",
                    RatedVoltage = 230f,
                    MinCurrent = 0.38f,
                    MaxCurrent = 0.652f,
                    MinPower = 70f,
                    MaxPower = 120f,
                    StandbyPower = 0f,
                    NormalPowerFactor = 0.8f,
                    Description = "Ventilation fan for exhaust and air circulation."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "HVAC",
                    ApplianceName = "Ventilation Fan",
                    CompanyName = "KDK",
                    ModelNumber = "Wall Exhaust Fan",
                    RatedVoltage = 230f,
                    MinCurrent = 0.245f,
                    MaxCurrent = 0.462f,
                    MinPower = 45f,
                    MaxPower = 85f,
                    StandbyPower = 0f,
                    NormalPowerFactor = 0.8f,
                    Description = "Wall-mounted ventilation fan for airflow monitoring."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "HVAC",
                    ApplianceName = "Air Curtain",
                    CompanyName = "Midea",
                    ModelNumber = "FM-1209T2",
                    RatedVoltage = 230f,
                    MinCurrent = 0.921f,
                    MaxCurrent = 1.79f,
                    MinPower = 180f,
                    MaxPower = 350f,
                    StandbyPower = 5f,
                    NormalPowerFactor = 0.85f,
                    Description = "Air curtain used at entrances to reduce HVAC loss."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "HVAC",
                    ApplianceName = "Dehumidifier",
                    CompanyName = "Sharp",
                    ModelNumber = "DW-D20A",
                    RatedVoltage = 230f,
                    MinCurrent = 1.208f,
                    MaxCurrent = 2.029f,
                    MinPower = 250f,
                    MaxPower = 420f,
                    StandbyPower = 2f,
                    NormalPowerFactor = 0.9f,
                    Description = "Dehumidifier load for humidity control monitoring."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "HVAC",
                    ApplianceName = "Electric Room Heater",
                    CompanyName = "Philips",
                    ModelNumber = "Comfort Heater 2000W",
                    RatedVoltage = 230f,
                    MinCurrent = 4.392f,
                    MaxCurrent = 8.783f,
                    MinPower = 1000f,
                    MaxPower = 2000f,
                    StandbyPower = 0f,
                    NormalPowerFactor = 0.99f,
                    Description = "Two-stage resistive heater; the range represents its 1 kW and 2 kW operating settings."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "HVAC",
                    ApplianceName = "Oil Filled Radiator",
                    CompanyName = "DeLonghi",
                    ModelNumber = "TRRS0920",
                    RatedVoltage = 230f,
                    MinCurrent = 3.953f,
                    MaxCurrent = 8.783f,
                    MinPower = 900f,
                    MaxPower = 2000f,
                    StandbyPower = 0f,
                    NormalPowerFactor = 0.99f,
                    Description = "Multi-stage resistive heater; the range represents its normal selectable heating levels."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "HVAC",
                    ApplianceName = "Humidifier",
                    CompanyName = "Xiaomi",
                    ModelNumber = "Smart Humidifier",
                    RatedVoltage = 230f,
                    MinCurrent = 0.113f,
                    MaxCurrent = 0.23f,
                    MinPower = 22f,
                    MaxPower = 45f,
                    StandbyPower = 1f,
                    NormalPowerFactor = 0.85f,
                    Description = "Small HVAC-support appliance for indoor humidity."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "HVAC",
                    ApplianceName = "Portable Air Cooler",
                    CompanyName = "Symphony",
                    ModelNumber = "Diet 3D Cooler",
                    RatedVoltage = 230f,
                    MinCurrent = 0.563f,
                    MaxCurrent = 0.921f,
                    MinPower = 110f,
                    MaxPower = 180f,
                    StandbyPower = 0f,
                    NormalPowerFactor = 0.85f,
                    Description = "Portable evaporative cooler for small areas."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Computing",
                    ApplianceName = "Desktop Computer",
                    CompanyName = "Dell",
                    ModelNumber = "OptiPlex Desktop",
                    RatedVoltage = 230f,
                    MinCurrent = 0.206f,
                    MaxCurrent = 0.824f,
                    MinPower = 45f,
                    MaxPower = 180f,
                    StandbyPower = 3f,
                    NormalPowerFactor = 0.95f,
                    Description = "Office desktop computer used for computing load monitoring."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Computing",
                    ApplianceName = "Desktop Computer",
                    CompanyName = "HP",
                    ModelNumber = "ProDesk 400",
                    RatedVoltage = 230f,
                    MinCurrent = 0.206f,
                    MaxCurrent = 0.824f,
                    MinPower = 45f,
                    MaxPower = 180f,
                    StandbyPower = 3f,
                    NormalPowerFactor = 0.95f,
                    Description = "Office desktop PC for workstation monitoring."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Computing",
                    ApplianceName = "Desktop Computer",
                    CompanyName = "Lenovo",
                    ModelNumber = "ThinkCentre M Series",
                    RatedVoltage = 230f,
                    MinCurrent = 0.206f,
                    MaxCurrent = 0.778f,
                    MinPower = 45f,
                    MaxPower = 170f,
                    StandbyPower = 3f,
                    NormalPowerFactor = 0.95f,
                    Description = "Business desktop for computing load analytics."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Computing",
                    ApplianceName = "Mini Computer",
                    CompanyName = "Apple",
                    ModelNumber = "Mac mini",
                    RatedVoltage = 230f,
                    MinCurrent = 0.046f,
                    MaxCurrent = 0.412f,
                    MinPower = 10f,
                    MaxPower = 90f,
                    StandbyPower = 1f,
                    NormalPowerFactor = 0.95f,
                    Description = "Compact office computer with low idle load."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Computing",
                    ApplianceName = "Desktop Computer",
                    CompanyName = "Acer",
                    ModelNumber = "Veriton Business PC",
                    RatedVoltage = 230f,
                    MinCurrent = 0.183f,
                    MaxCurrent = 0.732f,
                    MinPower = 40f,
                    MaxPower = 160f,
                    StandbyPower = 3f,
                    NormalPowerFactor = 0.95f,
                    Description = "Business desktop used for office computing loads."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Computing",
                    ApplianceName = "Mini PC",
                    CompanyName = "ASUS",
                    ModelNumber = "ExpertCenter Mini",
                    RatedVoltage = 230f,
                    MinCurrent = 0.069f,
                    MaxCurrent = 0.412f,
                    MinPower = 15f,
                    MaxPower = 90f,
                    StandbyPower = 2f,
                    NormalPowerFactor = 0.95f,
                    Description = "Mini PC for low-power office computing."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Computing",
                    ApplianceName = "Workstation",
                    CompanyName = "HP",
                    ModelNumber = "Z2 Tower Workstation",
                    RatedVoltage = 230f,
                    MinCurrent = 0.549f,
                    MaxCurrent = 2.059f,
                    MinPower = 120f,
                    MaxPower = 450f,
                    StandbyPower = 5f,
                    NormalPowerFactor = 0.95f,
                    Description = "High-power workstation for technical office load."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Computing",
                    ApplianceName = "Rack Server",
                    CompanyName = "Dell",
                    ModelNumber = "PowerEdge R250",
                    RatedVoltage = 230f,
                    MinCurrent = 0.824f,
                    MaxCurrent = 2.059f,
                    MinPower = 180f,
                    MaxPower = 450f,
                    StandbyPower = 20f,
                    NormalPowerFactor = 0.95f,
                    Description = "Small rack server usually running continuously."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Computing",
                    ApplianceName = "Rack Server",
                    CompanyName = "HPE",
                    ModelNumber = "ProLiant DL360",
                    RatedVoltage = 230f,
                    MinCurrent = 1.144f,
                    MaxCurrent = 3.204f,
                    MinPower = 250f,
                    MaxPower = 700f,
                    StandbyPower = 25f,
                    NormalPowerFactor = 0.95f,
                    Description = "Enterprise server for server-room computing loads."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Computing",
                    ApplianceName = "Rack Server",
                    CompanyName = "Lenovo",
                    ModelNumber = "ThinkSystem SR250",
                    RatedVoltage = 230f,
                    MinCurrent = 0.824f,
                    MaxCurrent = 2.059f,
                    MinPower = 180f,
                    MaxPower = 450f,
                    StandbyPower = 20f,
                    NormalPowerFactor = 0.95f,
                    Description = "Business rack server for continuous operation."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Computing",
                    ApplianceName = "LED Monitor 24 Inch",
                    CompanyName = "Dell",
                    ModelNumber = "SE2422H",
                    RatedVoltage = 230f,
                    MinCurrent = 0.068f,
                    MaxCurrent = 0.116f,
                    MinPower = 14f,
                    MaxPower = 24f,
                    StandbyPower = 0.3f,
                    NormalPowerFactor = 0.9f,
                    Description = "Office display monitor for workstation energy monitoring."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Computing",
                    ApplianceName = "LED Monitor 24 Inch",
                    CompanyName = "HP",
                    ModelNumber = "P24 G5",
                    RatedVoltage = 230f,
                    MinCurrent = 0.072f,
                    MaxCurrent = 0.145f,
                    MinPower = 15f,
                    MaxPower = 30f,
                    StandbyPower = 0.5f,
                    NormalPowerFactor = 0.9f,
                    Description = "Office monitor with low standby load."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Computing",
                    ApplianceName = "LED Monitor 27 Inch",
                    CompanyName = "Samsung",
                    ModelNumber = "S27R350",
                    RatedVoltage = 230f,
                    MinCurrent = 0.097f,
                    MaxCurrent = 0.169f,
                    MinPower = 20f,
                    MaxPower = 35f,
                    StandbyPower = 0.5f,
                    NormalPowerFactor = 0.9f,
                    Description = "Larger display monitor for office computing category."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Computing",
                    ApplianceName = "Projector",
                    CompanyName = "Epson",
                    ModelNumber = "EB-X49",
                    RatedVoltage = 230f,
                    MinCurrent = 1.353f,
                    MaxCurrent = 1.691f,
                    MinPower = 280f,
                    MaxPower = 350f,
                    StandbyPower = 0.5f,
                    NormalPowerFactor = 0.9f,
                    Description = "Meeting-room projector with high short-duration load."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Computing",
                    ApplianceName = "Projector",
                    CompanyName = "BenQ",
                    ModelNumber = "MW560",
                    RatedVoltage = 230f,
                    MinCurrent = 1.256f,
                    MaxCurrent = 1.594f,
                    MinPower = 260f,
                    MaxPower = 330f,
                    StandbyPower = 0.5f,
                    NormalPowerFactor = 0.9f,
                    Description = "Office projector for meeting rooms and classrooms."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Computing",
                    ApplianceName = "Laser Printer",
                    CompanyName = "HP",
                    ModelNumber = "Color LaserJet Pro M255dw",
                    RatedVoltage = 230f,
                    MinCurrent = 1.449f,
                    MaxCurrent = 1.739f,
                    MinPower = 300f,
                    MaxPower = 360f,
                    StandbyPower = 0.8f,
                    NormalPowerFactor = 0.9f,
                    Description = "Active printing range; sleep power is stored separately as standby power."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Computing",
                    ApplianceName = "Laser Printer",
                    CompanyName = "Canon",
                    ModelNumber = "imageCLASS LBP6030",
                    RatedVoltage = 230f,
                    MinCurrent = 1.449f,
                    MaxCurrent = 4.203f,
                    MinPower = 300f,
                    MaxPower = 870f,
                    StandbyPower = 0.8f,
                    NormalPowerFactor = 0.9f,
                    Description = "Approx. 330 W during printing; the maximum includes the manufacturer's normal short warm-up peak."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Computing",
                    ApplianceName = "Inkjet Printer",
                    CompanyName = "Epson",
                    ModelNumber = "EcoTank L3250",
                    RatedVoltage = 230f,
                    MinCurrent = 0.054f,
                    MaxCurrent = 0.082f,
                    MinPower = 10f,
                    MaxPower = 15f,
                    StandbyPower = 0.5f,
                    NormalPowerFactor = 0.8f,
                    Description = "Low-power inkjet printer for office printing."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Computing",
                    ApplianceName = "Laser Printer",
                    CompanyName = "Brother",
                    ModelNumber = "HL-L2350DW",
                    RatedVoltage = 230f,
                    MinCurrent = 1.932f,
                    MaxCurrent = 2.415f,
                    MinPower = 400f,
                    MaxPower = 500f,
                    StandbyPower = 0.8f,
                    NormalPowerFactor = 0.9f,
                    Description = "Active printing range; sleep power is stored separately as standby power."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Computing",
                    ApplianceName = "Wi-Fi Router",
                    CompanyName = "TP-Link",
                    ModelNumber = "Archer C6",
                    RatedVoltage = 230f,
                    MinCurrent = 0.036f,
                    MaxCurrent = 0.061f,
                    MinPower = 7f,
                    MaxPower = 12f,
                    StandbyPower = 5f,
                    NormalPowerFactor = 0.85f,
                    Description = "Network router usually running continuously."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Computing",
                    ApplianceName = "Network Switch",
                    CompanyName = "Cisco",
                    ModelNumber = "CBS350 24-Port",
                    RatedVoltage = 230f,
                    MinCurrent = 0.121f,
                    MaxCurrent = 0.29f,
                    MinPower = 25f,
                    MaxPower = 60f,
                    StandbyPower = 18f,
                    NormalPowerFactor = 0.9f,
                    Description = "Office network switch normally active 24/7."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Computing",
                    ApplianceName = "Wireless Access Point",
                    CompanyName = "Ubiquiti",
                    ModelNumber = "UniFi U6 Lite",
                    RatedVoltage = 230f,
                    MinCurrent = 0.024f,
                    MaxCurrent = 0.058f,
                    MinPower = 5f,
                    MaxPower = 12f,
                    StandbyPower = 4f,
                    NormalPowerFactor = 0.9f,
                    Description = "PoE access point for office network monitoring."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Computing",
                    ApplianceName = "CCTV Camera",
                    CompanyName = "Hikvision",
                    ModelNumber = "2MP Bullet Camera",
                    RatedVoltage = 230f,
                    MinCurrent = 0.018f,
                    MaxCurrent = 0.033f,
                    MinPower = 3.5f,
                    MaxPower = 6.5f,
                    StandbyPower = 2f,
                    NormalPowerFactor = 0.85f,
                    Description = "Security camera load usually active 24/7."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Computing",
                    ApplianceName = "NAS Storage",
                    CompanyName = "Synology",
                    ModelNumber = "DiskStation DS220+",
                    RatedVoltage = 230f,
                    MinCurrent = 0.068f,
                    MaxCurrent = 0.121f,
                    MinPower = 14f,
                    MaxPower = 25f,
                    StandbyPower = 5f,
                    NormalPowerFactor = 0.9f,
                    Description = "Active disk-access range with low-power disk hibernation represented by standby power."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Computing",
                    ApplianceName = "UPS",
                    CompanyName = "APC",
                    ModelNumber = "Back-UPS 1200VA",
                    RatedVoltage = 230f,
                    MinCurrent = 0.072f,
                    MaxCurrent = 0.58f,
                    MinPower = 15f,
                    MaxPower = 120f,
                    StandbyPower = 10f,
                    NormalPowerFactor = 0.9f,
                    Description = "UPS self-consumption and battery-charging overhead only; attached equipment load should be monitored separately."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Lighting",
                    ApplianceName = "LED Tube Light",
                    CompanyName = "Philips",
                    ModelNumber = "Ecofit T8 18W",
                    RatedVoltage = 230f,
                    MinCurrent = 0.077f,
                    MaxCurrent = 0.092f,
                    MinPower = 16f,
                    MaxPower = 19f,
                    StandbyPower = 0f,
                    NormalPowerFactor = 0.9f,
                    Description = "LED tube light for office and corridor lighting."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Lighting",
                    ApplianceName = "LED Bulb",
                    CompanyName = "Philips",
                    ModelNumber = "Essential LED 12W",
                    RatedVoltage = 230f,
                    MinCurrent = 0.051f,
                    MaxCurrent = 0.066f,
                    MinPower = 10f,
                    MaxPower = 13f,
                    StandbyPower = 0f,
                    NormalPowerFactor = 0.85f,
                    Description = "General LED bulb for small rooms."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Lighting",
                    ApplianceName = "LED Panel Light",
                    CompanyName = "Philips",
                    ModelNumber = "SmartBright Panel 36W",
                    RatedVoltage = 230f,
                    MinCurrent = 0.155f,
                    MaxCurrent = 0.193f,
                    MinPower = 32f,
                    MaxPower = 40f,
                    StandbyPower = 0f,
                    NormalPowerFactor = 0.9f,
                    Description = "Ceiling panel light for office rooms."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Lighting",
                    ApplianceName = "LED Panel Light",
                    CompanyName = "Osram",
                    ModelNumber = "BackLED Panel 36W",
                    RatedVoltage = 230f,
                    MinCurrent = 0.155f,
                    MaxCurrent = 0.193f,
                    MinPower = 32f,
                    MaxPower = 40f,
                    StandbyPower = 0f,
                    NormalPowerFactor = 0.9f,
                    Description = "Office ceiling light panel for lighting analytics."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Lighting",
                    ApplianceName = "LED Flood Light",
                    CompanyName = "LEDVANCE",
                    ModelNumber = "Floodlight LED 50W",
                    RatedVoltage = 230f,
                    MinCurrent = 0.217f,
                    MaxCurrent = 0.266f,
                    MinPower = 45f,
                    MaxPower = 55f,
                    StandbyPower = 0f,
                    NormalPowerFactor = 0.9f,
                    Description = "Outdoor flood light for parking or exterior areas."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Lighting",
                    ApplianceName = "LED Bulb",
                    CompanyName = "GE Lighting",
                    ModelNumber = "A19 10W",
                    RatedVoltage = 230f,
                    MinCurrent = 0.046f,
                    MaxCurrent = 0.056f,
                    MinPower = 9f,
                    MaxPower = 11f,
                    StandbyPower = 0f,
                    NormalPowerFactor = 0.85f,
                    Description = "Small bulb used in offices and support rooms."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Lighting",
                    ApplianceName = "LED Bulb",
                    CompanyName = "Panasonic",
                    ModelNumber = "LED Bulb 15W",
                    RatedVoltage = 230f,
                    MinCurrent = 0.066f,
                    MaxCurrent = 0.082f,
                    MinPower = 13f,
                    MaxPower = 16f,
                    StandbyPower = 0f,
                    NormalPowerFactor = 0.85f,
                    Description = "Energy efficient bulb for indoor lighting."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Lighting",
                    ApplianceName = "LED Panel Light",
                    CompanyName = "OPPLE",
                    ModelNumber = "Office Panel 48W",
                    RatedVoltage = 230f,
                    MinCurrent = 0.208f,
                    MaxCurrent = 0.251f,
                    MinPower = 43f,
                    MaxPower = 52f,
                    StandbyPower = 0f,
                    NormalPowerFactor = 0.9f,
                    Description = "Commercial LED panel for office ceilings."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Lighting",
                    ApplianceName = "LED Tube Light",
                    CompanyName = "Paklite",
                    ModelNumber = "T8 LED Tube 20W",
                    RatedVoltage = 230f,
                    MinCurrent = 0.087f,
                    MaxCurrent = 0.106f,
                    MinPower = 18f,
                    MaxPower = 22f,
                    StandbyPower = 0f,
                    NormalPowerFactor = 0.9f,
                    Description = "LED tube light for local office lighting."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Lighting",
                    ApplianceName = "LED Downlight",
                    CompanyName = "FSL",
                    ModelNumber = "Downlight 12W",
                    RatedVoltage = 230f,
                    MinCurrent = 0.051f,
                    MaxCurrent = 0.066f,
                    MinPower = 10f,
                    MaxPower = 13f,
                    StandbyPower = 0f,
                    NormalPowerFactor = 0.85f,
                    Description = "Recessed LED downlight for corridors and offices."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Lighting",
                    ApplianceName = "LED Downlight",
                    CompanyName = "NVC",
                    ModelNumber = "Downlight 18W",
                    RatedVoltage = 230f,
                    MinCurrent = 0.077f,
                    MaxCurrent = 0.097f,
                    MinPower = 16f,
                    MaxPower = 20f,
                    StandbyPower = 0f,
                    NormalPowerFactor = 0.9f,
                    Description = "Ceiling downlight for lighting zones."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Lighting",
                    ApplianceName = "LED Tube Light",
                    CompanyName = "Sylvania",
                    ModelNumber = "T8 Tube 18W",
                    RatedVoltage = 230f,
                    MinCurrent = 0.077f,
                    MaxCurrent = 0.097f,
                    MinPower = 16f,
                    MaxPower = 20f,
                    StandbyPower = 0f,
                    NormalPowerFactor = 0.9f,
                    Description = "Tube light for general office lighting."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Lighting",
                    ApplianceName = "LED High Bay Light",
                    CompanyName = "Cree Lighting",
                    ModelNumber = "High Bay 100W",
                    RatedVoltage = 230f,
                    MinCurrent = 0.412f,
                    MaxCurrent = 0.503f,
                    MinPower = 90f,
                    MaxPower = 110f,
                    StandbyPower = 0f,
                    NormalPowerFactor = 0.95f,
                    Description = "Warehouse or lobby high bay light."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Lighting",
                    ApplianceName = "LED Street Light",
                    CompanyName = "Philips",
                    ModelNumber = "SmartBright Street 70W",
                    RatedVoltage = 230f,
                    MinCurrent = 0.288f,
                    MaxCurrent = 0.352f,
                    MinPower = 63f,
                    MaxPower = 77f,
                    StandbyPower = 0f,
                    NormalPowerFactor = 0.95f,
                    Description = "Street or campus area light."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Lighting",
                    ApplianceName = "LED Street Light",
                    CompanyName = "LEDVANCE",
                    ModelNumber = "Streetlight 100W",
                    RatedVoltage = 230f,
                    MinCurrent = 0.412f,
                    MaxCurrent = 0.503f,
                    MinPower = 90f,
                    MaxPower = 110f,
                    StandbyPower = 0f,
                    NormalPowerFactor = 0.95f,
                    Description = "Outdoor road or parking lighting load."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Lighting",
                    ApplianceName = "Emergency Light",
                    CompanyName = "Schneider Electric",
                    ModelNumber = "Emergency LED Light",
                    RatedVoltage = 230f,
                    MinCurrent = 0.022f,
                    MaxCurrent = 0.043f,
                    MinPower = 4f,
                    MaxPower = 8f,
                    StandbyPower = 1f,
                    NormalPowerFactor = 0.8f,
                    Description = "Emergency lighting with low standby battery charging load."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Lighting",
                    ApplianceName = "Exit Sign Light",
                    CompanyName = "Legrand",
                    ModelNumber = "LED Exit Sign",
                    RatedVoltage = 230f,
                    MinCurrent = 0.016f,
                    MaxCurrent = 0.033f,
                    MinPower = 3f,
                    MaxPower = 6f,
                    StandbyPower = 1f,
                    NormalPowerFactor = 0.8f,
                    Description = "Exit sign light normally active for safety."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Lighting",
                    ApplianceName = "Pendant Light",
                    CompanyName = "IKEA",
                    ModelNumber = "LED Pendant 20W",
                    RatedVoltage = 230f,
                    MinCurrent = 0.092f,
                    MaxCurrent = 0.113f,
                    MinPower = 18f,
                    MaxPower = 22f,
                    StandbyPower = 0f,
                    NormalPowerFactor = 0.85f,
                    Description = "Decorative office lighting load."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Lighting",
                    ApplianceName = "Track Light",
                    CompanyName = "Panasonic",
                    ModelNumber = "LED Track 30W",
                    RatedVoltage = 230f,
                    MinCurrent = 0.13f,
                    MaxCurrent = 0.159f,
                    MinPower = 27f,
                    MaxPower = 33f,
                    StandbyPower = 0f,
                    NormalPowerFactor = 0.9f,
                    Description = "Focused lighting for reception or display areas."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Lighting",
                    ApplianceName = "LED Strip Light",
                    CompanyName = "Osram",
                    ModelNumber = "Flexible LED Strip 24W",
                    RatedVoltage = 230f,
                    MinCurrent = 0.102f,
                    MaxCurrent = 0.133f,
                    MinPower = 20f,
                    MaxPower = 26f,
                    StandbyPower = 0f,
                    NormalPowerFactor = 0.85f,
                    Description = "Strip light used for decorative lighting zones."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Lighting",
                    ApplianceName = "Motion Sensor Corridor Light",
                    CompanyName = "Philips",
                    ModelNumber = "Sensor LED 18W",
                    RatedVoltage = 230f,
                    MinCurrent = 0.082f,
                    MaxCurrent = 0.102f,
                    MinPower = 16f,
                    MaxPower = 20f,
                    StandbyPower = 0.5f,
                    NormalPowerFactor = 0.85f,
                    Description = "Corridor light with sensor standby power."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Lighting",
                    ApplianceName = "Parking Flood Light",
                    CompanyName = "Havells",
                    ModelNumber = "LED Flood 100W",
                    RatedVoltage = 230f,
                    MinCurrent = 0.435f,
                    MaxCurrent = 0.531f,
                    MinPower = 90f,
                    MaxPower = 110f,
                    StandbyPower = 0f,
                    NormalPowerFactor = 0.9f,
                    Description = "Parking area lighting with high evening load."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Lighting",
                    ApplianceName = "Warehouse High Bay",
                    CompanyName = "Havells",
                    ModelNumber = "LED High Bay 150W",
                    RatedVoltage = 230f,
                    MinCurrent = 0.618f,
                    MaxCurrent = 0.755f,
                    MinPower = 135f,
                    MaxPower = 165f,
                    StandbyPower = 0f,
                    NormalPowerFactor = 0.95f,
                    Description = "Warehouse high bay lighting load."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Lighting",
                    ApplianceName = "Recessed Spot Light",
                    CompanyName = "Wipro",
                    ModelNumber = "LED Spot 9W",
                    RatedVoltage = 230f,
                    MinCurrent = 0.041f,
                    MaxCurrent = 0.051f,
                    MinPower = 8f,
                    MaxPower = 10f,
                    StandbyPower = 0f,
                    NormalPowerFactor = 0.85f,
                    Description = "Recessed spot light for offices."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Lighting",
                    ApplianceName = "Smart LED Bulb",
                    CompanyName = "Yeelight",
                    ModelNumber = "Smart Bulb 10W",
                    RatedVoltage = 230f,
                    MinCurrent = 0.041f,
                    MaxCurrent = 0.051f,
                    MinPower = 8f,
                    MaxPower = 10f,
                    StandbyPower = 0.5f,
                    NormalPowerFactor = 0.85f,
                    Description = "Smart bulb with small standby load for Wi-Fi electronics."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Miscellaneous",
                    ApplianceName = "Refrigerator",
                    CompanyName = "Dawlance",
                    ModelNumber = "Double Door Refrigerator",
                    RatedVoltage = 230f,
                    MinCurrent = 0.46f,
                    MaxCurrent = 1.125f,
                    MinPower = 90f,
                    MaxPower = 220f,
                    StandbyPower = 5f,
                    NormalPowerFactor = 0.85f,
                    Description = "Compressor-running active range; standby represents electronics and controls while the compressor is off."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Miscellaneous",
                    ApplianceName = "Microwave Oven",
                    CompanyName = "Samsung",
                    ModelNumber = "Solo Microwave 23L",
                    RatedVoltage = 230f,
                    MinCurrent = 4.805f,
                    MaxCurrent = 5.721f,
                    MinPower = 1050f,
                    MaxPower = 1250f,
                    StandbyPower = 2f,
                    NormalPowerFactor = 0.95f,
                    Description = "Configured values represent electrical input power, not the lower microwave output rating."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Miscellaneous",
                    ApplianceName = "Water Dispenser",
                    CompanyName = "Orient",
                    ModelNumber = "Crystal 3 Taps",
                    RatedVoltage = 230f,
                    MinCurrent = 0.435f,
                    MaxCurrent = 2.415f,
                    MinPower = 90f,
                    MaxPower = 500f,
                    StandbyPower = 5f,
                    NormalPowerFactor = 0.9f,
                    Description = "Cooling compressor and hot-water heater operate in separate stages, so active power is cycle-dependent."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Miscellaneous",
                    ApplianceName = "Electric Kettle",
                    CompanyName = "Anex",
                    ModelNumber = "Office Electric Kettle",
                    RatedVoltage = 230f,
                    MinCurrent = 7.905f,
                    MaxCurrent = 8.783f,
                    MinPower = 1800f,
                    MaxPower = 2000f,
                    StandbyPower = 0f,
                    NormalPowerFactor = 0.99f,
                    Description = "Short-duration high heating load."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Miscellaneous",
                    ApplianceName = "Ceiling Fan",
                    CompanyName = "GFC",
                    ModelNumber = "Office Ceiling Fan",
                    RatedVoltage = 230f,
                    MinCurrent = 0.256f,
                    MaxCurrent = 0.435f,
                    MinPower = 50f,
                    MaxPower = 85f,
                    StandbyPower = 0f,
                    NormalPowerFactor = 0.85f,
                    Description = "General room fan used for office comfort."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Miscellaneous",
                    ApplianceName = "Ceiling Fan",
                    CompanyName = "Pak Fan",
                    ModelNumber = "Deluxe Ceiling Fan",
                    RatedVoltage = 230f,
                    MinCurrent = 0.281f,
                    MaxCurrent = 0.435f,
                    MinPower = 55f,
                    MaxPower = 85f,
                    StandbyPower = 0f,
                    NormalPowerFactor = 0.85f,
                    Description = "Office ceiling fan for room load monitoring."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Miscellaneous",
                    ApplianceName = "Microwave Oven",
                    CompanyName = "Panasonic",
                    ModelNumber = "NN-ST34",
                    RatedVoltage = 230f,
                    MinCurrent = 5.034f,
                    MaxCurrent = 5.95f,
                    MinPower = 1100f,
                    MaxPower = 1300f,
                    StandbyPower = 2f,
                    NormalPowerFactor = 0.95f,
                    Description = "Configured values represent electrical input power, not the lower microwave output rating."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Miscellaneous",
                    ApplianceName = "Coffee Maker",
                    CompanyName = "Kenwood",
                    ModelNumber = "Drip Coffee Maker",
                    RatedVoltage = 230f,
                    MinCurrent = 3.513f,
                    MaxCurrent = 4.392f,
                    MinPower = 800f,
                    MaxPower = 1000f,
                    StandbyPower = 1f,
                    NormalPowerFactor = 0.99f,
                    Description = "Office coffee machine with heating load."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Miscellaneous",
                    ApplianceName = "Mini Refrigerator",
                    CompanyName = "Haier",
                    ModelNumber = "Mini Bar Refrigerator",
                    RatedVoltage = 230f,
                    MinCurrent = 0.326f,
                    MaxCurrent = 0.652f,
                    MinPower = 60f,
                    MaxPower = 120f,
                    StandbyPower = 4f,
                    NormalPowerFactor = 0.8f,
                    Description = "Compressor-running active range; standby represents controls while the compressor is off."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Miscellaneous",
                    ApplianceName = "Refrigerator",
                    CompanyName = "LG",
                    ModelNumber = "Inverter Refrigerator",
                    RatedVoltage = 230f,
                    MinCurrent = 0.358f,
                    MaxCurrent = 0.921f,
                    MinPower = 70f,
                    MaxPower = 180f,
                    StandbyPower = 5f,
                    NormalPowerFactor = 0.85f,
                    Description = "Compressor-running active range; standby represents electronics and controls while the compressor is off."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Miscellaneous",
                    ApplianceName = "Deep Freezer",
                    CompanyName = "Dawlance",
                    ModelNumber = "Chest Freezer",
                    RatedVoltage = 230f,
                    MinCurrent = 0.614f,
                    MaxCurrent = 1.535f,
                    MinPower = 120f,
                    MaxPower = 300f,
                    StandbyPower = 6f,
                    NormalPowerFactor = 0.85f,
                    Description = "Compressor-running active range; standby represents controls while the compressor is off."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Miscellaneous",
                    ApplianceName = "Washing Machine",
                    CompanyName = "Samsung",
                    ModelNumber = "Front Load Washer",
                    RatedVoltage = 230f,
                    MinCurrent = 1.449f,
                    MaxCurrent = 10.628f,
                    MinPower = 300f,
                    MaxPower = 2200f,
                    StandbyPower = 2f,
                    NormalPowerFactor = 0.9f,
                    Description = "Cycle-dependent motor and heater load; the upper value applies while the internal heater is active."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Miscellaneous",
                    ApplianceName = "Washing Machine",
                    CompanyName = "Haier",
                    ModelNumber = "Top Load Washer",
                    RatedVoltage = 230f,
                    MinCurrent = 1.279f,
                    MaxCurrent = 3.581f,
                    MinPower = 250f,
                    MaxPower = 700f,
                    StandbyPower = 2f,
                    NormalPowerFactor = 0.85f,
                    Description = "Cycle-dependent motor and spin load; this top-load benchmark excludes a high-power water heater."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Miscellaneous",
                    ApplianceName = "Vacuum Cleaner",
                    CompanyName = "Philips",
                    ModelNumber = "PowerPro Compact",
                    RatedVoltage = 230f,
                    MinCurrent = 5.797f,
                    MaxCurrent = 7.729f,
                    MinPower = 1200f,
                    MaxPower = 1600f,
                    StandbyPower = 0f,
                    NormalPowerFactor = 0.9f,
                    Description = "Cleaning equipment with high motor load."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Miscellaneous",
                    ApplianceName = "Steam Iron",
                    CompanyName = "Philips",
                    ModelNumber = "EasySpeed Iron",
                    RatedVoltage = 230f,
                    MinCurrent = 7.905f,
                    MaxCurrent = 9.662f,
                    MinPower = 1800f,
                    MaxPower = 2200f,
                    StandbyPower = 0f,
                    NormalPowerFactor = 0.99f,
                    Description = "Short-duration heating appliance."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Miscellaneous",
                    ApplianceName = "Toaster",
                    CompanyName = "Kenwood",
                    ModelNumber = "2 Slice Toaster",
                    RatedVoltage = 230f,
                    MinCurrent = 3.294f,
                    MaxCurrent = 3.953f,
                    MinPower = 750f,
                    MaxPower = 900f,
                    StandbyPower = 0f,
                    NormalPowerFactor = 0.99f,
                    Description = "Pantry heating load for office kitchen."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Miscellaneous",
                    ApplianceName = "Photocopier",
                    CompanyName = "Ricoh",
                    ModelNumber = "MP 2014D",
                    RatedVoltage = 230f,
                    MinCurrent = 1.932f,
                    MaxCurrent = 4.831f,
                    MinPower = 400f,
                    MaxPower = 1000f,
                    StandbyPower = 5f,
                    NormalPowerFactor = 0.9f,
                    Description = "Copying and warm-up range; ready/sleep consumption is stored separately as standby power."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Miscellaneous",
                    ApplianceName = "Water Pump",
                    CompanyName = "Grundfos",
                    ModelNumber = "Small Booster Pump",
                    RatedVoltage = 230f,
                    MinCurrent = 2.558f,
                    MaxCurrent = 3.836f,
                    MinPower = 500f,
                    MaxPower = 750f,
                    StandbyPower = 0f,
                    NormalPowerFactor = 0.85f,
                    Description = "Water pressure pump for building services."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Miscellaneous",
                    ApplianceName = "Pedestal Fan",
                    CompanyName = "Super Asia",
                    ModelNumber = "Pedestal Fan",
                    RatedVoltage = 230f,
                    MinCurrent = 0.256f,
                    MaxCurrent = 0.46f,
                    MinPower = 50f,
                    MaxPower = 90f,
                    StandbyPower = 0f,
                    NormalPowerFactor = 0.85f,
                    Description = "Movable fan used in office areas."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Miscellaneous",
                    ApplianceName = "Motorized Projector Screen",
                    CompanyName = "Elite Screens",
                    ModelNumber = "Electric Screen Motor",
                    RatedVoltage = 230f,
                    MinCurrent = 0.435f,
                    MaxCurrent = 0.815f,
                    MinPower = 80f,
                    MaxPower = 150f,
                    StandbyPower = 0f,
                    NormalPowerFactor = 0.8f,
                    Description = "Meeting room motorized screen load."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Miscellaneous",
                    ApplianceName = "Elevator Auxiliary Load",
                    CompanyName = "Otis",
                    ModelNumber = "Lift Cabin Fan and Light",
                    RatedVoltage = 230f,
                    MinCurrent = 0.58f,
                    MaxCurrent = 1.691f,
                    MinPower = 120f,
                    MaxPower = 350f,
                    StandbyPower = 20f,
                    NormalPowerFactor = 0.9f,
                    Description = "Auxiliary elevator load such as cabin fan and lighting."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Miscellaneous",
                    ApplianceName = "Vending Machine",
                    CompanyName = "Necta",
                    ModelNumber = "Snack Vending Machine",
                    RatedVoltage = 230f,
                    MinCurrent = 0.767f,
                    MaxCurrent = 2.302f,
                    MinPower = 150f,
                    MaxPower = 450f,
                    StandbyPower = 20f,
                    NormalPowerFactor = 0.85f,
                    Description = "Cycle-dependent load covering refrigeration, lighting, and dispensing operation."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Miscellaneous",
                    ApplianceName = "Coffee Vending Machine",
                    CompanyName = "Nestlé",
                    ModelNumber = "Office Coffee Vending",
                    RatedVoltage = 230f,
                    MinCurrent = 3.382f,
                    MaxCurrent = 7.246f,
                    MinPower = 700f,
                    MaxPower = 1500f,
                    StandbyPower = 20f,
                    NormalPowerFactor = 0.9f,
                    Description = "Cycle-dependent load covering dispensing, water heating, and temperature maintenance."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Miscellaneous",
                    ApplianceName = "Dishwasher",
                    CompanyName = "Bosch",
                    ModelNumber = "Series 4 Dishwasher",
                    RatedVoltage = 230f,
                    MinCurrent = 0.483f,
                    MaxCurrent = 11.594f,
                    MinPower = 100f,
                    MaxPower = 2400f,
                    StandbyPower = 2f,
                    NormalPowerFactor = 0.9f,
                    Description = "Cycle-dependent appliance: pump-only wash phases are low, while water-heating phases can reach the configured maximum."
                },
                new ApplianceSeedItem
                {
                    UtilityName = "Miscellaneous",
                    ApplianceName = "Hand Dryer",
                    CompanyName = "Dyson",
                    ModelNumber = "Airblade Hand Dryer",
                    RatedVoltage = 230f,
                    MinCurrent = 4.437f,
                    MaxCurrent = 7.098f,
                    MinPower = 1000f,
                    MaxPower = 1600f,
                    StandbyPower = 1f,
                    NormalPowerFactor = 0.98f,
                    Description = "Washroom hand dryer with high short-duration load."
                }
            };

            foreach (var item in appliances)
            {
                var utilityId = utilityIds[item.UtilityName];

                var existing = await db.tbl_appliance.FirstOrDefaultAsync(x =>
                    x.fk_utility == utilityId &&
                    x.appliance_name.ToLower() == item.ApplianceName.ToLower() &&
                    x.company_name.ToLower() == item.CompanyName.ToLower() &&
                    x.model_number.ToLower() == item.ModelNumber.ToLower() &&
                    !x.is_deleted);

                if (existing == null)
                {
                    db.tbl_appliance.Add(new tbl_appliance
                    {
                        appliance_id = Guid.NewGuid(),
                        appliance_name = item.ApplianceName,
                        company_name = item.CompanyName,
                        model_number = item.ModelNumber,
                        rated_voltage = item.RatedVoltage,
                        min_current = item.MinCurrent,
                        max_current = item.MaxCurrent,
                        min_power = item.MinPower,
                        max_power = item.MaxPower,
                        standby_power = item.StandbyPower,
                        normal_power_factor = item.NormalPowerFactor,
                        description = item.Description,
                        priority_level = "Normal",
                        is_critical = false,
                        is_default = true,
                        is_custom = false,
                        created_at = now,
                        updated_at = now,
                        is_deleted = false,
                        is_active = true,
                        fk_utility = utilityId
                    });

                    continue;
                }

                // Idempotent update: replacing this file also corrects values already seeded.
                existing.rated_voltage = item.RatedVoltage;
                existing.min_current = item.MinCurrent;
                existing.max_current = item.MaxCurrent;
                existing.min_power = item.MinPower;
                existing.max_power = item.MaxPower;
                existing.standby_power = item.StandbyPower;
                existing.normal_power_factor = item.NormalPowerFactor;
                existing.description = item.Description;
                existing.is_default = true;
                existing.is_custom = false;
                existing.is_active = true;
                existing.updated_at = now;
            }

            await db.SaveChangesAsync();
            await SeedBusinessApplianceCopiesAsync(db);
        }

        private static async Task SeedBusinessApplianceCopiesAsync(DBUserManagementContext db)
        {
            var now = DateTime.Now;

            var businessIds = await db.tbl_business
                .Where(x => !x.is_deleted)
                .Select(x => x.business_id)
                .ToListAsync();

            if (!businessIds.Any())
                return;

            var defaultAppliances = await db.tbl_appliance
                .Where(x => x.is_default && x.is_active && !x.is_deleted)
                .ToListAsync();

            if (!defaultAppliances.Any())
                return;

            var existingCopies = await db.tbl_business_appliance
                .Where(x => x.fk_appliance.HasValue && !x.is_deleted)
                .ToListAsync();

            var existingByKey = existingCopies
                .GroupBy(x => $"{x.fk_business:N}:{x.fk_appliance!.Value:N}")
                .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);

            foreach (var businessId in businessIds)
            {
                foreach (var defaultAppliance in defaultAppliances)
                {
                    var key = $"{businessId:N}:{defaultAppliance.appliance_id:N}";

                    if (existingByKey.TryGetValue(key, out var existingCopy))
                    {
                        // Do not overwrite a business-owned/customized appliance.
                        if (!existingCopy.is_default || existingCopy.is_custom)
                            continue;

                        CopyDefaultValues(existingCopy, defaultAppliance, now);
                        continue;
                    }

                    db.tbl_business_appliance.Add(new tbl_business_appliance
                    {
                        business_appliance_id = Guid.NewGuid(),
                        fk_business = businessId,
                        fk_appliance = defaultAppliance.appliance_id,
                        appliance_name = defaultAppliance.appliance_name,
                        company_name = defaultAppliance.company_name,
                        model_number = defaultAppliance.model_number,
                        rated_voltage = defaultAppliance.rated_voltage,
                        min_current = defaultAppliance.min_current,
                        max_current = defaultAppliance.max_current,
                        min_power = defaultAppliance.min_power,
                        max_power = defaultAppliance.max_power,
                        standby_power = defaultAppliance.standby_power,
                        normal_power_factor = defaultAppliance.normal_power_factor,
                        description = defaultAppliance.description,
                        priority_level = defaultAppliance.priority_level,
                        is_critical = defaultAppliance.is_critical,
                        is_default = true,
                        is_custom = false,
                        created_at = now,
                        updated_at = now,
                        is_deleted = false,
                        is_active = defaultAppliance.is_active,
                        fk_utility = defaultAppliance.fk_utility
                    });
                }
            }

            await db.SaveChangesAsync();
        }

        private static void CopyDefaultValues(
            tbl_business_appliance target,
            tbl_appliance source,
            DateTime updatedAt)
        {
            target.appliance_name = source.appliance_name;
            target.company_name = source.company_name;
            target.model_number = source.model_number;
            target.rated_voltage = source.rated_voltage;
            target.min_current = source.min_current;
            target.max_current = source.max_current;
            target.min_power = source.min_power;
            target.max_power = source.max_power;
            target.standby_power = source.standby_power;
            target.normal_power_factor = source.normal_power_factor;
            target.description = source.description;
            target.priority_level = source.priority_level;
            target.is_critical = source.is_critical;
            target.is_default = true;
            target.is_custom = false;
            target.is_active = source.is_active;
            target.fk_utility = source.fk_utility;
            target.updated_at = updatedAt;
        }

        private static async Task<Guid> GetOrCreateUtilityAsync(
            DBUserManagementContext db,
            string utilityName)
        {
            var existingUtility = await db.tbl_utility.FirstOrDefaultAsync(x =>
                x.utility_name.ToLower() == utilityName.ToLower() &&
                !x.is_deleted);

            if (existingUtility != null)
                return existingUtility.utility_id;

            var newUtility = new tbl_utility
            {
                utility_id = Guid.NewGuid(),
                utility_name = utilityName,
                is_active = true,
                is_deleted = false
            };

            await db.tbl_utility.AddAsync(newUtility);
            await db.SaveChangesAsync();

            return newUtility.utility_id;
        }

        private sealed class ApplianceSeedItem
        {
            public string UtilityName { get; set; } = string.Empty;
            public string ApplianceName { get; set; } = string.Empty;
            public string CompanyName { get; set; } = string.Empty;
            public string ModelNumber { get; set; } = string.Empty;
            public float RatedVoltage { get; set; }
            public float MinCurrent { get; set; }
            public float MaxCurrent { get; set; }
            public float MinPower { get; set; }
            public float MaxPower { get; set; }
            public float StandbyPower { get; set; }
            public float NormalPowerFactor { get; set; }
            public string Description { get; set; } = string.Empty;
        }
    }
}
