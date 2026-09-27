using FluentValidation;
using PharmacyManagement.Application.DTOs.Prescriptions;

namespace PharmacyManagement.Application.Validators.Prescriptions;

public sealed class CreateDoctorRequestValidator : AbstractValidator<CreateDoctorRequest>
{
    public CreateDoctorRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PmdcNumber).MaximumLength(100).When(x => x.PmdcNumber is not null);
        RuleFor(x => x.Specialization).MaximumLength(150).When(x => x.Specialization is not null);
        RuleFor(x => x.Phone).MaximumLength(50).When(x => x.Phone is not null);
        RuleFor(x => x.Email).MaximumLength(200).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.ClinicName).MaximumLength(200).When(x => x.ClinicName is not null);
        RuleFor(x => x.Address).MaximumLength(500).When(x => x.Address is not null);
    }
}

public sealed class UpdateDoctorRequestValidator : AbstractValidator<UpdateDoctorRequest>
{
    public UpdateDoctorRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.PmdcNumber).MaximumLength(100).When(x => x.PmdcNumber is not null);
        RuleFor(x => x.Specialization).MaximumLength(150).When(x => x.Specialization is not null);
        RuleFor(x => x.Phone).MaximumLength(50).When(x => x.Phone is not null);
        RuleFor(x => x.Email).MaximumLength(200).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.ClinicName).MaximumLength(200).When(x => x.ClinicName is not null);
        RuleFor(x => x.Address).MaximumLength(500).When(x => x.Address is not null);
    }
}

public sealed class CreatePrescriptionItemRequestValidator : AbstractValidator<CreatePrescriptionItemRequest>
{
    public CreatePrescriptionItemRequestValidator()
    {
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.RefillCount).GreaterThanOrEqualTo(0);
        RuleFor(x => x.DosageUnit).MaximumLength(50).When(x => x.DosageUnit is not null);
        RuleFor(x => x.FrequencyCode).MaximumLength(50).When(x => x.FrequencyCode is not null);
        RuleFor(x => x.Route).MaximumLength(50).When(x => x.Route is not null);
        RuleFor(x => x.DurationUnit).MaximumLength(30).When(x => x.DurationUnit is not null);
        RuleFor(x => x.Instructions).MaximumLength(1000).When(x => x.Instructions is not null);
    }
}

public sealed class CreatePrescriptionRequestValidator : AbstractValidator<CreatePrescriptionRequest>
{
    public CreatePrescriptionRequestValidator()
    {
        RuleFor(x => x.CustomerId).GreaterThan(0);
        RuleFor(x => x.DoctorId).GreaterThan(0);
        RuleFor(x => x.DiagnosisNotes).MaximumLength(2000).When(x => x.DiagnosisNotes is not null);
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).SetValidator(new CreatePrescriptionItemRequestValidator());
    }
}

public sealed class DispensePrescriptionRequestValidator : AbstractValidator<DispensePrescriptionRequest>
{
    public DispensePrescriptionRequestValidator()
    {
        RuleFor(x => x.SaleId).GreaterThan(0);
    }
}
