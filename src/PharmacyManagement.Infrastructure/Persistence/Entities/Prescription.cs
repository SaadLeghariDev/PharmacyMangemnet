using System;
using System.Collections.Generic;

namespace PharmacyManagement.Infrastructure.Persistence.Entities;

public partial class Prescription
{
    public long Id { get; set; }

    public long CustomerId { get; set; }

    public long DoctorId { get; set; }

    public string PrescriptionNumber { get; set; } = null!;

    public DateOnly PrescriptionDate { get; set; }

    public string? DiagnosisNotes { get; set; }

    public long? PrescriptionAttachmentId { get; set; }

    public string Status { get; set; } = null!;

    public long? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<ControlledDrugTransaction> ControlledDrugTransactions { get; set; } = new List<ControlledDrugTransaction>();

    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<DispensingRecord> DispensingRecords { get; set; } = new List<DispensingRecord>();

    public virtual Doctor Doctor { get; set; } = null!;

    public virtual Attachment? PrescriptionAttachment { get; set; }

    public virtual ICollection<PrescriptionItem> PrescriptionItems { get; set; } = new List<PrescriptionItem>();
}
