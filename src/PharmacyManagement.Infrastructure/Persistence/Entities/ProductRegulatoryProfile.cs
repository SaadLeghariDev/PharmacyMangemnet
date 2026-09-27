using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class ProductRegulatoryProfile
{
    public long Id { get; set; }

    public long ProductId { get; set; }

    public string? DrugRegistrationNo { get; set; }

    public string? DrugLicenseCategory { get; set; }

    public string? ScheduleCode { get; set; }

    public string? ControlledDrugClass { get; set; }

    public bool RequiresPrescription { get; set; }

    public bool RequiresSpecialRecord { get; set; }

    public string? StorageCondition { get; set; }

    public decimal? TemperatureMin { get; set; }

    public decimal? TemperatureMax { get; set; }

    public DateOnly? EffectiveFrom { get; set; }

    public DateOnly? EffectiveTo { get; set; }

    public bool IsActive { get; set; }

    public virtual Product Product { get; set; } = null!;
}
