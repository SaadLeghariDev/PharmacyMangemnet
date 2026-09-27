using FluentAssertions;
using PharmacyManagement.Application.DTOs.Cash;
using PharmacyManagement.Application.DTOs.Customers;
using PharmacyManagement.Application.Validators.Cash;
using PharmacyManagement.Application.Validators.Customers;

namespace PharmacyManagement.Application.Tests.Validators;

public class Phase2DValidatorsTests
{
    [Fact]
    public void OpenCashShiftRequest_valid()
    {
        var v = new OpenCashShiftRequestValidator();
        var result = v.Validate(new OpenCashShiftRequest
        {
            BranchId = 1,
            CounterId = 1,
            TerminalId = 1,
            OpeningAmount = 1000
        });
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void OpenCashShiftRequest_rejects_negative_opening()
    {
        var v = new OpenCashShiftRequestValidator();
        var result = v.Validate(new OpenCashShiftRequest
        {
            BranchId = 1,
            CounterId = 1,
            TerminalId = 1,
            OpeningAmount = -1
        });
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void CashDrawerMovementRequest_requires_positive_amount()
    {
        var v = new CashDrawerMovementRequestValidator();
        var result = v.Validate(new CashDrawerMovementRequest { Amount = 0 });
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void CloseCashShiftRequest_valid()
    {
        var v = new CloseCashShiftRequestValidator();
        var result = v.Validate(new CloseCashShiftRequest { ClosingAmount = 0 });
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void CreateCustomerRequest_requires_name()
    {
        var v = new CreateCustomerRequestValidator();
        var result = v.Validate(new CreateCustomerRequest { Name = "", CreditLimit = 0 });
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void RecordCustomerPaymentRequest_requires_positive_amount()
    {
        var v = new RecordCustomerPaymentRequestValidator();
        var result = v.Validate(new RecordCustomerPaymentRequest
        {
            BranchId = 1,
            PaymentMethodId = 1,
            Amount = 0
        });
        result.IsValid.Should().BeFalse();
    }
}
