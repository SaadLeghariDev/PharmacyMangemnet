using FluentAssertions;
using PharmacyManagement.Application.DTOs.Purchasing;
using PharmacyManagement.Application.Validators.Purchasing;

namespace PharmacyManagement.Application.Tests.Validators;

public class PurchasingValidatorsTests
{
    [Fact]
    public async Task CreateSupplier_requires_code_and_name()
    {
        var validator = new CreateSupplierRequestValidator();
        var result = await validator.ValidateAsync(new CreateSupplierRequest());
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateSupplierRequest.Code));
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateSupplierRequest.Name));
    }

    [Fact]
    public async Task CreatePurchaseOrder_requires_lines()
    {
        var validator = new CreatePurchaseOrderRequestValidator();
        var result = await validator.ValidateAsync(new CreatePurchaseOrderRequest
        {
            BranchId = 1,
            WarehouseId = 1,
            SupplierId = 1
        });
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task CreateGoodsReceipt_requires_batches()
    {
        var validator = new CreateGoodsReceiptRequestValidator();
        var result = await validator.ValidateAsync(new CreateGoodsReceiptRequest
        {
            BranchId = 1,
            WarehouseId = 1,
            SupplierId = 1,
            Lines =
            [
                new GoodsReceiptLineRequest
                {
                    ProductId = 1,
                    ProductUnitId = 1,
                    ReceivedQuantity = 1,
                    UnitCost = 10,
                    Batches = []
                }
            ]
        });
        result.IsValid.Should().BeFalse();
    }
}
