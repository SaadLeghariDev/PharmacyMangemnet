using PharmacyManagement.Application.Common;

namespace PharmacyManagement.Application.DTOs.Prescriptions;

public sealed class DoctorQuery : PaginationQuery
{
    public bool? IsActive { get; set; }
}

public sealed class DoctorDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? PmdcNumber { get; set; }
    public string? Specialization { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? ClinicName { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; }
}

public sealed class CreateDoctorRequest
{
    public string Name { get; set; } = string.Empty;
    public string? PmdcNumber { get; set; }
    public string? Specialization { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? ClinicName { get; set; }
    public string? Address { get; set; }
}

public sealed class UpdateDoctorRequest
{
    public string Name { get; set; } = string.Empty;
    public string? PmdcNumber { get; set; }
    public string? Specialization { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? ClinicName { get; set; }
    public string? Address { get; set; }
}

public sealed class PrescriptionQuery : PaginationQuery
{
    public long? CustomerId { get; set; }
    public long? DoctorId { get; set; }
    public string? Status { get; set; }
}

public sealed class PrescriptionItemDto
{
    public long Id { get; set; }
    public long ProductId { get; set; }
    public string? Sku { get; set; }
    public string? ProductName { get; set; }
    public decimal? DosageAmount { get; set; }
    public string? DosageUnit { get; set; }
    public string? FrequencyCode { get; set; }
    public string? Route { get; set; }
    public decimal? DurationValue { get; set; }
    public string? DurationUnit { get; set; }
    public decimal Quantity { get; set; }
    public string? Instructions { get; set; }
    public bool RefillAllowed { get; set; }
    public int RefillCount { get; set; }
    public decimal DispensedQuantity { get; set; }
    public decimal RemainingQuantity { get; set; }
}

public sealed class DispensingItemDto
{
    public long Id { get; set; }
    public long PrescriptionItemId { get; set; }
    public long ProductId { get; set; }
    public long BatchId { get; set; }
    public string? BatchNumber { get; set; }
    public decimal Quantity { get; set; }
}

public sealed class DispensingRecordDto
{
    public long Id { get; set; }
    public long PrescriptionId { get; set; }
    public long SaleId { get; set; }
    public long CustomerId { get; set; }
    public long? DispensedBy { get; set; }
    public DateTime DispensedAt { get; set; }
    public IReadOnlyList<DispensingItemDto> Items { get; set; } = Array.Empty<DispensingItemDto>();
}

public sealed class PrescriptionDto
{
    public long Id { get; set; }
    public long CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public long DoctorId { get; set; }
    public string? DoctorName { get; set; }
    public string PrescriptionNumber { get; set; } = string.Empty;
    public DateOnly PrescriptionDate { get; set; }
    public string? DiagnosisNotes { get; set; }
    public string Status { get; set; } = string.Empty;
    public long? CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public IReadOnlyList<PrescriptionItemDto> Items { get; set; } = Array.Empty<PrescriptionItemDto>();
    public IReadOnlyList<DispensingRecordDto> DispensingRecords { get; set; } = Array.Empty<DispensingRecordDto>();
}

public sealed class CreatePrescriptionItemRequest
{
    public long ProductId { get; set; }
    public decimal? DosageAmount { get; set; }
    public string? DosageUnit { get; set; }
    public string? FrequencyCode { get; set; }
    public string? Route { get; set; }
    public decimal? DurationValue { get; set; }
    public string? DurationUnit { get; set; }
    public decimal Quantity { get; set; }
    public string? Instructions { get; set; }
    public bool RefillAllowed { get; set; }
    public int RefillCount { get; set; }
}

public sealed class CreatePrescriptionRequest
{
    public long CustomerId { get; set; }
    public long DoctorId { get; set; }
    public DateOnly? PrescriptionDate { get; set; }
    public string? DiagnosisNotes { get; set; }
    public IReadOnlyList<CreatePrescriptionItemRequest> Items { get; set; } = Array.Empty<CreatePrescriptionItemRequest>();
}

public sealed class DispensePrescriptionRequest
{
    public long SaleId { get; set; }
    public long? WitnessedBy { get; set; }
}
