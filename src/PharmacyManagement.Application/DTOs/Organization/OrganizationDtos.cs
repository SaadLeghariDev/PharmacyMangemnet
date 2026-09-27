namespace PharmacyManagement.Application.DTOs.Organization;

public sealed class TenantDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? Ntn { get; set; }
    public string? Strn { get; set; }
    public string? LicenseNo { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; }
}

public sealed class BranchDto
{
    public long Id { get; set; }
    public long TenantId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? BranchType { get; set; }
    public string? City { get; set; }
    public string? Province { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; }
}

public sealed class CreateBranchRequest
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? BranchType { get; set; }
    public string? City { get; set; }
    public string? Province { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
}

public sealed class UpdateBranchRequest
{
    public string Name { get; set; } = string.Empty;
    public string? BranchType { get; set; }
    public string? City { get; set; }
    public string? Province { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class WarehouseDto
{
    public long Id { get; set; }
    public long BranchId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? WarehouseType { get; set; }
    public bool IsMain { get; set; }
    public bool TemperatureControlled { get; set; }
    public bool IsActive { get; set; }
}

public sealed class CreateWarehouseRequest
{
    public long BranchId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? WarehouseType { get; set; }
    public bool IsMain { get; set; }
    public bool TemperatureControlled { get; set; }
}

public sealed class UpdateWarehouseRequest
{
    public string Name { get; set; } = string.Empty;
    public string? WarehouseType { get; set; }
    public bool IsMain { get; set; }
    public bool TemperatureControlled { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class WarehouseLocationDto
{
    public long Id { get; set; }
    public long WarehouseId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? RackNo { get; set; }
    public string? ShelfNo { get; set; }
    public string? BinNo { get; set; }
    public string? LocationType { get; set; }
    public bool IsActive { get; set; }
}

public sealed class CreateWarehouseLocationRequest
{
    public long WarehouseId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? RackNo { get; set; }
    public string? ShelfNo { get; set; }
    public string? BinNo { get; set; }
    public string? LocationType { get; set; }
}

public sealed class UpdateWarehouseLocationRequest
{
    public string Name { get; set; } = string.Empty;
    public string? RackNo { get; set; }
    public string? ShelfNo { get; set; }
    public string? BinNo { get; set; }
    public string? LocationType { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class CounterDto
{
    public long Id { get; set; }
    public long BranchId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? CounterType { get; set; }
    public bool IsActive { get; set; }
}

public sealed class CreateCounterRequest
{
    public long BranchId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? CounterType { get; set; }
}

public sealed class UpdateCounterRequest
{
    public string Name { get; set; } = string.Empty;
    public string? CounterType { get; set; }
    public bool IsActive { get; set; } = true;
}

public sealed class PosTerminalDto
{
    public long Id { get; set; }
    public long BranchId { get; set; }
    public long CounterId { get; set; }
    public string TerminalCode { get; set; } = string.Empty;
    public string? ComputerName { get; set; }
    public string? MacAddress { get; set; }
    public string? IpAddress { get; set; }
    public string? SerialNumber { get; set; }
    public bool IsPrimary { get; set; }
    public bool IsOnline { get; set; }
    public bool IsActive { get; set; }
}

public sealed class CreatePosTerminalRequest
{
    public long BranchId { get; set; }
    public long CounterId { get; set; }
    public string TerminalCode { get; set; } = string.Empty;
    public string? ComputerName { get; set; }
    public string? MacAddress { get; set; }
    public string? IpAddress { get; set; }
    public string? SerialNumber { get; set; }
    public bool IsPrimary { get; set; }
}

public sealed class UpdatePosTerminalRequest
{
    public long CounterId { get; set; }
    public string? ComputerName { get; set; }
    public string? MacAddress { get; set; }
    public string? IpAddress { get; set; }
    public string? SerialNumber { get; set; }
    public bool IsPrimary { get; set; }
    public bool IsActive { get; set; } = true;
}
