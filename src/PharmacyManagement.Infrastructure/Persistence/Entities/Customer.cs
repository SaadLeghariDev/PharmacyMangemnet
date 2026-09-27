using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class Customer
{
    public long Id { get; set; }

    public long TenantId { get; set; }

    public string CustomerCode { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string? Cnic { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public string? Address { get; set; }

    public DateOnly? DateOfBirth { get; set; }

    public string? Gender { get; set; }

    public decimal CreditLimit { get; set; }

    public bool IsPatient { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<CustomerLedger> CustomerLedgers { get; set; } = new List<CustomerLedger>();

    public virtual ICollection<CustomerPayment> CustomerPayments { get; set; } = new List<CustomerPayment>();

    public virtual ICollection<DispensingRecord> DispensingRecords { get; set; } = new List<DispensingRecord>();

    public virtual ICollection<HeldSale> HeldSales { get; set; } = new List<HeldSale>();

    public virtual ICollection<Prescription> Prescriptions { get; set; } = new List<Prescription>();

    public virtual ICollection<Sale> Sales { get; set; } = new List<Sale>();

    public virtual Tenant Tenant { get; set; } = null!;
}
