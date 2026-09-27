using FluentAssertions;
using PharmacyManagement.Application.DTOs.Controlled;
using PharmacyManagement.Application.DTOs.Fiscal;
using PharmacyManagement.Application.DTOs.Prescriptions;
using PharmacyManagement.Application.Validators.Controlled;
using PharmacyManagement.Application.Validators.Fiscal;
using PharmacyManagement.Application.Validators.Prescriptions;

namespace PharmacyManagement.Application.Tests.Validators;

public class Phase2EValidatorsTests
{
    [Fact]
    public void CreatePrescriptionRequest_requires_items()
    {
        var v = new CreatePrescriptionRequestValidator();
        var result = v.Validate(new CreatePrescriptionRequest
        {
            CustomerId = 1,
            DoctorId = 1,
            Items = []
        });
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void CreatePrescriptionRequest_valid()
    {
        var v = new CreatePrescriptionRequestValidator();
        var result = v.Validate(new CreatePrescriptionRequest
        {
            CustomerId = 1,
            DoctorId = 1,
            Items =
            [
                new CreatePrescriptionItemRequest { ProductId = 1, Quantity = 10 }
            ]
        });
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void DispensePrescriptionRequest_requires_sale()
    {
        var v = new DispensePrescriptionRequestValidator();
        var result = v.Validate(new DispensePrescriptionRequest { SaleId = 0 });
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void CreateDoctorRequest_requires_name()
    {
        var v = new CreateDoctorRequestValidator();
        var result = v.Validate(new CreateDoctorRequest { Name = "" });
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void OpenControlledRegisterRequest_rejects_negative_opening()
    {
        var v = new OpenControlledRegisterRequestValidator();
        var result = v.Validate(new OpenControlledRegisterRequest
        {
            BranchId = 1,
            ProductId = 1,
            OpeningBalance = -1
        });
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void PostControlledTransactionRequest_rejects_dispense_type()
    {
        var v = new PostControlledTransactionRequestValidator();
        var result = v.Validate(new PostControlledTransactionRequest
        {
            TransactionType = "Dispense",
            Quantity = 1
        });
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void PostControlledTransactionRequest_receipt_valid()
    {
        var v = new PostControlledTransactionRequestValidator();
        var result = v.Validate(new PostControlledTransactionRequest
        {
            TransactionType = "Receipt",
            Quantity = 5
        });
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void CreateFiscalDocumentRequest_requires_sale()
    {
        var v = new CreateFiscalDocumentRequestValidator();
        var result = v.Validate(new CreateFiscalDocumentRequest { SaleId = 0 });
        result.IsValid.Should().BeFalse();
    }
}
