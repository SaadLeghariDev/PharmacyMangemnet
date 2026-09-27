using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class Barcode
{
    public long Id { get; set; }

    public long ProductId { get; set; }

    public long ProductUnitId { get; set; }

    public string BarcodeValue { get; set; } = null!;

    public string? BarcodeType { get; set; }

    public string? Gtin { get; set; }

    public string? SerialNumber { get; set; }

    public bool IsPrimary { get; set; }

    public bool IsActive { get; set; }

    public virtual Product Product { get; set; } = null!;

    public virtual ProductUnit ProductUnit { get; set; } = null!;
}
