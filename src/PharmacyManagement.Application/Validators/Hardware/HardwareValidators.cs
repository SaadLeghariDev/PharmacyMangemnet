using FluentValidation;
using PharmacyManagement.Application.DTOs.Hardware;
using PharmacyManagement.Domain.Constants;

namespace PharmacyManagement.Application.Validators.Hardware;

public sealed class CreateDeviceRequestValidator : AbstractValidator<CreateDeviceRequest>
{
    public CreateDeviceRequestValidator()
    {
        RuleFor(x => x.BranchId).GreaterThan(0);
        RuleFor(x => x.DeviceTypeId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Manufacturer).MaximumLength(150);
        RuleFor(x => x.Model).MaximumLength(150);
        RuleFor(x => x.SerialNumber).MaximumLength(150);
        RuleFor(x => x.ConnectionType).MaximumLength(50);
        RuleFor(x => x.Ip).MaximumLength(50);
        RuleFor(x => x.ComPort).MaximumLength(50);
        RuleFor(x => x.MacAddress).MaximumLength(100);
        RuleFor(x => x.DriverName).MaximumLength(150);
        RuleFor(x => x.DriverVersion).MaximumLength(50);
    }
}

public sealed class UpdateDeviceRequestValidator : AbstractValidator<UpdateDeviceRequest>
{
    public UpdateDeviceRequestValidator()
    {
        RuleFor(x => x.DeviceTypeId).GreaterThan(0);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Manufacturer).MaximumLength(150);
        RuleFor(x => x.Model).MaximumLength(150);
        RuleFor(x => x.SerialNumber).MaximumLength(150);
        RuleFor(x => x.ConnectionType).MaximumLength(50);
        RuleFor(x => x.Ip).MaximumLength(50);
        RuleFor(x => x.ComPort).MaximumLength(50);
        RuleFor(x => x.MacAddress).MaximumLength(100);
        RuleFor(x => x.DriverName).MaximumLength(150);
        RuleFor(x => x.DriverVersion).MaximumLength(50);
    }
}

public sealed class CreateDeviceAssignmentRequestValidator : AbstractValidator<CreateDeviceAssignmentRequest>
{
    public CreateDeviceAssignmentRequestValidator()
    {
        RuleFor(x => x.DeviceId).GreaterThan(0);
        RuleFor(x => x.TerminalId).GreaterThan(0);
    }
}

public sealed class UpsertDeviceSettingRequestValidator : AbstractValidator<UpsertDeviceSettingRequest>
{
    public UpsertDeviceSettingRequestValidator()
    {
        RuleFor(x => x.SettingKey).NotEmpty().MaximumLength(100);
    }
}

public sealed class CreateDeviceEventRequestValidator : AbstractValidator<CreateDeviceEventRequest>
{
    public CreateDeviceEventRequestValidator()
    {
        RuleFor(x => x.EventType).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Status).NotEmpty().MaximumLength(50);
        RuleFor(x => x.ErrorMessage).MaximumLength(2000);
    }
}

public sealed class CreatePrintTemplateRequestValidator : AbstractValidator<CreatePrintTemplateRequest>
{
    public CreatePrintTemplateRequestValidator()
    {
        RuleFor(x => x.TemplateType).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.TemplateContent).NotEmpty();
        RuleFor(x => x.PaperWidth).GreaterThan(0).When(x => x.PaperWidth.HasValue);
    }
}

public sealed class UpdatePrintTemplateRequestValidator : AbstractValidator<UpdatePrintTemplateRequest>
{
    public UpdatePrintTemplateRequestValidator()
    {
        RuleFor(x => x.TemplateType).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.TemplateContent).NotEmpty();
        RuleFor(x => x.PaperWidth).GreaterThan(0).When(x => x.PaperWidth.HasValue);
    }
}

public sealed class CreateBarcodePrintJobRequestValidator : AbstractValidator<CreateBarcodePrintJobRequest>
{
    public CreateBarcodePrintJobRequestValidator()
    {
        RuleFor(x => x.BranchId).GreaterThan(0);
        RuleFor(x => x.PrinterDeviceId).GreaterThan(0);
        RuleFor(x => x.ProductId).GreaterThan(0);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.TemplateId).GreaterThan(0);
    }
}

public sealed class UpdateBarcodePrintJobStatusRequestValidator : AbstractValidator<UpdateBarcodePrintJobStatusRequest>
{
    public UpdateBarcodePrintJobStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty()
            .Must(BarcodePrintJobStatuses.IsKnown)
            .WithMessage($"Status must be one of: {BarcodePrintJobStatuses.Queued}, {BarcodePrintJobStatuses.Printing}, {BarcodePrintJobStatuses.Printed}, {BarcodePrintJobStatuses.Failed}, {BarcodePrintJobStatuses.Cancelled}.");
    }
}

public sealed class CreateAttachmentRequestValidator : AbstractValidator<CreateAttachmentRequest>
{
    public CreateAttachmentRequestValidator()
    {
        RuleFor(x => x.FileName).NotEmpty().MaximumLength(250);
        RuleFor(x => x.StoragePath).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.ContentType).MaximumLength(150);
        RuleFor(x => x.FileSize).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Hash).MaximumLength(128);
    }
}

public sealed class CreateEntityAttachmentRequestValidator : AbstractValidator<CreateEntityAttachmentRequest>
{
    public CreateEntityAttachmentRequestValidator()
    {
        RuleFor(x => x.AttachmentId).GreaterThan(0);
        RuleFor(x => x.EntityName).NotEmpty().MaximumLength(150);
        RuleFor(x => x.EntityId).GreaterThan(0);
    }
}
