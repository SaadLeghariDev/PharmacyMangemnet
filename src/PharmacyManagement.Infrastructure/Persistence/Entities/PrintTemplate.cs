using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class PrintTemplate
{
    public long Id { get; set; }

    public long TenantId { get; set; }

    public string TemplateType { get; set; } = null!;

    public string Name { get; set; } = null!;

    public string TemplateContent { get; set; } = null!;

    public decimal? PaperWidth { get; set; }

    public bool IsDefault { get; set; }

    public bool IsActive { get; set; }

    public virtual ICollection<BarcodePrintJob> BarcodePrintJobs { get; set; } = new List<BarcodePrintJob>();

    public virtual Tenant Tenant { get; set; } = null!;
}
