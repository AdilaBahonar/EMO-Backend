using Microsoft.EntityFrameworkCore;
using EnergyMonitor.DTOs;

namespace EMO.Repositories.DashboardServicesRepo;

// Breadcrumb chain resolvers — partial class extension of DashboardService
public partial class DashboardService
{
    // Each method walks UP the FK tree and returns the full breadcrumb chain
    // in top-down order: Business → Facility → Building → … → target

    public async Task<List<BreadcrumbDto>> ResolveSensorChainAsync(Guid sensorId)
    {
        var s = await _db.tbl_sensor
            .Include(x => x.device)
                .ThenInclude(d => d.office)
                    .ThenInclude(o => o.section)
                        .ThenInclude(sec => sec.floor)
                            .ThenInclude(f => f.building)
                                .ThenInclude(b => b.facility)
                                    .ThenInclude(fac => fac.business)
            .FirstOrDefaultAsync(x => x.sensor_id == sensorId);

        if (s is null) return new();

        var o   = s.device.office;
        var sec = o.section;
        var fl  = sec.floor;
        var bld = fl.building;
        var fac = bld.facility;
        var bus = fac.business;

        return new List<BreadcrumbDto>
        {
            new() { Id = bus.business_id,   Name = bus.business_name,   Level = "business"  },
            new() { Id = fac.facility_id,   Name = fac.facility_name,   Level = "facility"  },
            new() { Id = bld.building_id,   Name = bld.building_name,   Level = "building"  },
            new() { Id = fl.floor_id,       Name = fl.floor_name,       Level = "floor"     },
            new() { Id = sec.section_id,    Name = sec.section_name,    Level = "section"   },
            new() { Id = o.office_id,       Name = o.office_name,       Level = "office"    },
            new() { Id = s.device.device_id, Name = s.device.device_name, Level = "device"  },
            new() { Id = s.sensor_id,       Name = s.sensor_name,       Level = "sensor"    },
        };
    }

    public async Task<List<BreadcrumbDto>> ResolveDeviceChainAsync(Guid deviceId)
    {
        var device = await _db.tbl_device
            .Include(x => x.office)
                .ThenInclude(o => o.section)
                    .ThenInclude(sec => sec.floor)
                        .ThenInclude(f => f.building)
                            .ThenInclude(b => b.facility)
                                .ThenInclude(fac => fac.business)
            .FirstOrDefaultAsync(x => x.device_id == deviceId && !x.is_deleted);

        if (device is null) return new();

        var office = device.office;
        var section = office.section;
        var floor = section.floor;
        var building = floor.building;
        var facility = building.facility;
        var business = facility.business;

        return new List<BreadcrumbDto>
        {
            new() { Id = business.business_id, Name = business.business_name, Level = "business" },
            new() { Id = facility.facility_id, Name = facility.facility_name, Level = "facility" },
            new() { Id = building.building_id, Name = building.building_name, Level = "building" },
            new() { Id = floor.floor_id, Name = floor.floor_name, Level = "floor" },
            new() { Id = section.section_id, Name = section.section_name, Level = "section" },
            new() { Id = office.office_id, Name = office.office_name, Level = "office" },
            new() { Id = device.device_id, Name = device.device_name, Level = "device" },
        };
    }

    public async Task<List<BreadcrumbDto>> ResolveOfficeChainAsync(Guid officeId)
    {
        var o = await _db.tbl_office
            .Include(x => x.section)
                .ThenInclude(s => s.floor)
                    .ThenInclude(f => f.building)
                        .ThenInclude(b => b.facility)
                            .ThenInclude(fac => fac.business)
            .FirstOrDefaultAsync(x => x.office_id == officeId);

        if (o is null) return new();

        var sec = o.section;
        var fl  = sec.floor;
        var bld = fl.building;
        var fac = bld.facility;
        var bus = fac.business;

        return new List<BreadcrumbDto>
        {
            new() { Id = bus.business_id, Name = bus.business_name, Level = "business" },
            new() { Id = fac.facility_id, Name = fac.facility_name, Level = "facility" },
            new() { Id = bld.building_id, Name = bld.building_name, Level = "building" },
            new() { Id = fl.floor_id,     Name = fl.floor_name,     Level = "floor"    },
            new() { Id = sec.section_id,  Name = sec.section_name,  Level = "section"  },
            new() { Id = o.office_id,     Name = o.office_name,     Level = "office"   },
        };
    }

    public async Task<List<BreadcrumbDto>> ResolveSectionChainAsync(Guid sectionId)
    {
        var sec = await _db.tbl_section
            .Include(x => x.floor)
                .ThenInclude(f => f.building)
                    .ThenInclude(b => b.facility)
                        .ThenInclude(fac => fac.business)
            .FirstOrDefaultAsync(x => x.section_id == sectionId);

        if (sec is null) return new();

        var fl  = sec.floor;
        var bld = fl.building;
        var fac = bld.facility;
        var bus = fac.business;

        return new List<BreadcrumbDto>
        {
            new() { Id = bus.business_id, Name = bus.business_name, Level = "business" },
            new() { Id = fac.facility_id, Name = fac.facility_name, Level = "facility" },
            new() { Id = bld.building_id, Name = bld.building_name, Level = "building" },
            new() { Id = fl.floor_id,     Name = fl.floor_name,     Level = "floor"    },
            new() { Id = sec.section_id,  Name = sec.section_name,  Level = "section"  },
        };
    }

    public async Task<List<BreadcrumbDto>> ResolveFloorChainAsync(Guid floorId)
    {
        var fl = await _db.tbl_floor
            .Include(x => x.building)
                .ThenInclude(b => b.facility)
                    .ThenInclude(fac => fac.business)
            .FirstOrDefaultAsync(x => x.floor_id == floorId);

        if (fl is null) return new();

        var bld = fl.building;
        var fac = bld.facility;
        var bus = fac.business;

        return new List<BreadcrumbDto>
        {
            new() { Id = bus.business_id, Name = bus.business_name, Level = "business" },
            new() { Id = fac.facility_id, Name = fac.facility_name, Level = "facility" },
            new() { Id = bld.building_id, Name = bld.building_name, Level = "building" },
            new() { Id = fl.floor_id,     Name = fl.floor_name,     Level = "floor"    },
        };
    }

    public async Task<List<BreadcrumbDto>> ResolveBuildingChainAsync(Guid buildingId)
    {
        var bld = await _db.tbl_building
            .Include(x => x.facility)
                .ThenInclude(fac => fac.business)
            .FirstOrDefaultAsync(x => x.building_id == buildingId);

        if (bld is null) return new();

        var fac = bld.facility;
        var bus = fac.business;

        return new List<BreadcrumbDto>
        {
            new() { Id = bus.business_id, Name = bus.business_name, Level = "business" },
            new() { Id = fac.facility_id, Name = fac.facility_name, Level = "facility" },
            new() { Id = bld.building_id, Name = bld.building_name, Level = "building" },
        };
    }

    public async Task<List<BreadcrumbDto>> ResolveFacilityChainAsync(Guid facilityId)
    {
        var fac = await _db.tbl_facility
            .Include(x => x.business)
            .FirstOrDefaultAsync(x => x.facility_id == facilityId);

        if (fac is null) return new();

        return new List<BreadcrumbDto>
        {
            new() { Id = fac.business.business_id, Name = fac.business.business_name, Level = "business" },
            new() { Id = fac.facility_id,           Name = fac.facility_name,          Level = "facility"  },
        };
    }
}
