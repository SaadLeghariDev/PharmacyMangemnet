using FluentAssertions;
using PharmacyManagement.Application.DTOs.Sales;
using PharmacyManagement.Application.Validators.Sales;

namespace PharmacyManagement.Application.Tests.Validators;

public class SalesValidatorsTests
{
    [Fact]
    public void CreateSaleRequest_valid()
    {
        var v = new CreateSaleRequestValidator();
        var result = v.Validate(new CreateSaleRequest
        {
            BranchId = 1,
            CounterId = 1,
            TerminalId = 1,
            WarehouseId = 1,
            SaleType = "Retail",
            Lines =
            [
                new CreateSaleLineRequest { ProductId = 1, ProductUnitId = 1, Quantity = 1, UnitPrice = 5 }
            ],
            Payments =
            [
                new CreateSalePaymentRequest { PaymentMethodId = 1, Amount = 5 }
            ]
        });
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void CreateSaleRequest_credit_requires_customer()
    {
        var v = new CreateSaleRequestValidator();
        var result = v.Validate(new CreateSaleRequest
        {
            BranchId = 1,
            CounterId = 1,
            TerminalId = 1,
            WarehouseId = 1,
            SaleType = "Credit",
            Lines =
            [
                new CreateSaleLineRequest { ProductId = 1, ProductUnitId = 1, Quantity = 1 }
            ]
        });
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CustomerId");
    }

    [Fact]
    public void HoldSaleRequest_requires_cart()
    {
        var v = new HoldSaleRequestValidator();
        var result = v.Validate(new HoldSaleRequest { BranchId = 1, TerminalId = 1, CartData = "" });
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void CreateSaleReturnRequest_rejects_bad_condition()
    {
        var v = new CreateSaleReturnRequestValidator();
        var result = v.Validate(new CreateSaleReturnRequest
        {
            SaleId = 1,
            Lines =
            [
                new CreateSaleReturnLineRequest
                {
                    SaleLineId = 1,
                    BatchId = 1,
                    ProductUnitId = 1,
                    Quantity = 1,
                    Condition = "Weird"
                }
            ]
        });
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void VoidSaleRequest_accepts_null_reason()
    {
        var v = new VoidSaleRequestValidator();
        var result = v.Validate(new VoidSaleRequest());
        result.IsValid.Should().BeTrue();
    }
}

