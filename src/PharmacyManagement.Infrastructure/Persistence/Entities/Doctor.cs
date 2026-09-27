using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class Doctor
{
    public long Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Pmdcnumber { get; set; }

    public string? Specialization { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? ClinicName { get; set; }

    public string? Address { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<ControlledDrugTransaction> ControlledDrugTransactions { get; set; } = new List<ControlledDrugTransaction>();

    public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();
}
