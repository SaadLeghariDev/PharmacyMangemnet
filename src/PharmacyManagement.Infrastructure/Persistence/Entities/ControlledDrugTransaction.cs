using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class ControlledDrugTransaction
{
    public long Id { get; set; }

    public long RegisterId { get; set; }

    public string TransactionType { get; set; } = null!;

    public string? ReferenceType { get; set; }

    public long? ReferenceId { get; set; }

    public decimal Quantity { get; set; }

    public decimal BalanceBefore { get; set; }

    public decimal BalanceAfter { get; set; }

    public long? PrescriptionId { get; set; }

    public long? DoctorId { get; set; }

    public long? PerformedBy { get; set; }

    public long? WitnessedBy { get; set; }

    public DateTime TransactionDate { get; set; }

    public string? Remarks { get; set; }

    public virtual Doctor? Doctor { get; set; }

    public virtual Prescription? Prescription { get; set; }

    public virtual ControlledDrugRegister Register { get; set; } = null!;
}
