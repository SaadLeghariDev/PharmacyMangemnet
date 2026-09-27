using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Auth;
using PharmacyManagement.Application.DTOs.Cash;
using PharmacyManagement.Application.DTOs.Controlled;
using PharmacyManagement.Application.DTOs.Customers;
using PharmacyManagement.Application.DTOs.Fiscal;
using PharmacyManagement.Application.DTOs.Inventory;
using PharmacyManagement.Application.DTOs.Organization;
using PharmacyManagement.Application.DTOs.Prescriptions;
using PharmacyManagement.Application.DTOs.Products;
using PharmacyManagement.Application.DTOs.Purchasing;
using PharmacyManagement.Application.DTOs.Sales;

namespace PharmacyManagement.Application.Interfaces;

public interface ICurrentUserService
{
    long? UserId { get; }
    long? TenantId { get; }
    bool IsAuthenticated { get; }
    IReadOnlyCollection<string> Permissions { get; }
    IReadOnlyCollection<long> BranchIds { get; }
}

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<UserProfileDto> GetCurrentUserAsync(CancellationToken ct = default);
}

public interface IOrganizationService
{
    Task<IReadOnlyList<TenantDto>> GetTenantsAsync(CancellationToken ct = default);
    Task<TenantDto?> GetTenantAsync(long id, CancellationToken ct = default);

    Task<PagedResult<BranchDto>> GetBranchesAsync(PaginationQuery query, CancellationToken ct = default);
    Task<BranchDto?> GetBranchAsync(long id, CancellationToken ct = default);
    Task<BranchDto> CreateBranchAsync(CreateBranchRequest request, CancellationToken ct = default);
    Task<BranchDto> UpdateBranchAsync(long id, UpdateBranchRequest request, CancellationToken ct = default);
    Task DeactivateBranchAsync(long id, CancellationToken ct = default);

    Task<PagedResult<WarehouseDto>> GetWarehousesAsync(long? branchId, PaginationQuery query, CancellationToken ct = default);
    Task<WarehouseDto?> GetWarehouseAsync(long id, CancellationToken ct = default);
    Task<WarehouseDto> CreateWarehouseAsync(CreateWarehouseRequest request, CancellationToken ct = default);
    Task<WarehouseDto> UpdateWarehouseAsync(long id, UpdateWarehouseRequest request, CancellationToken ct = default);
    Task DeactivateWarehouseAsync(long id, CancellationToken ct = default);

    Task<PagedResult<WarehouseLocationDto>> GetWarehouseLocationsAsync(long? warehouseId, PaginationQuery query, CancellationToken ct = default);
    Task<WarehouseLocationDto?> GetWarehouseLocationAsync(long id, CancellationToken ct = default);
    Task<WarehouseLocationDto> CreateWarehouseLocationAsync(CreateWarehouseLocationRequest request, CancellationToken ct = default);
    Task<WarehouseLocationDto> UpdateWarehouseLocationAsync(long id, UpdateWarehouseLocationRequest request, CancellationToken ct = default);
    Task DeactivateWarehouseLocationAsync(long id, CancellationToken ct = default);

    Task<PagedResult<CounterDto>> GetCountersAsync(long? branchId, PaginationQuery query, CancellationToken ct = default);
    Task<CounterDto?> GetCounterAsync(long id, CancellationToken ct = default);
    Task<CounterDto> CreateCounterAsync(CreateCounterRequest request, CancellationToken ct = default);
    Task<CounterDto> UpdateCounterAsync(long id, UpdateCounterRequest request, CancellationToken ct = default);
    Task DeactivateCounterAsync(long id, CancellationToken ct = default);

    Task<PagedResult<PosTerminalDto>> GetPosTerminalsAsync(long? branchId, PaginationQuery query, CancellationToken ct = default);
    Task<PosTerminalDto?> GetPosTerminalAsync(long id, CancellationToken ct = default);
    Task<PosTerminalDto> CreatePosTerminalAsync(CreatePosTerminalRequest request, CancellationToken ct = default);
    Task<PosTerminalDto> UpdatePosTerminalAsync(long id, UpdatePosTerminalRequest request, CancellationToken ct = default);
    Task DeactivatePosTerminalAsync(long id, CancellationToken ct = default);
}

public interface IProductService
{
    Task<PagedResult<ProductDto>> SearchAsync(ProductQuery query, CancellationToken ct = default);
    Task<ProductDto?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<ProductDto?> GetBySkuAsync(string sku, CancellationToken ct = default);
    Task<BarcodeLookupDto?> GetByBarcodeAsync(string barcode, CancellationToken ct = default);
    Task<ProductDto> CreateAsync(CreateProductRequest request, CancellationToken ct = default);
    Task<ProductDto> UpdateAsync(long id, UpdateProductRequest request, CancellationToken ct = default);
    Task DeactivateAsync(long id, CancellationToken ct = default);
}

public interface INumberSequenceService
{
    Task<string> AllocateNextAsync(
        long tenantId,
        string documentType,
        long? branchId,
        long? terminalId = null,
        string? defaultPrefix = null,
        CancellationToken ct = default);
}

public interface ISupplierService
{
    Task<PagedResult<SupplierDto>> SearchAsync(PaginationQuery query, CancellationToken ct = default);
    Task<SupplierDto?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<SupplierDto> CreateAsync(CreateSupplierRequest request, CancellationToken ct = default);
    Task<SupplierDto> UpdateAsync(long id, UpdateSupplierRequest request, CancellationToken ct = default);
    Task DeactivateAsync(long id, CancellationToken ct = default);
}

public interface IPurchaseOrderService
{
    Task<PagedResult<PurchaseOrderDto>> SearchAsync(PurchaseOrderQuery query, CancellationToken ct = default);
    Task<PurchaseOrderDto?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<PurchaseOrderDto> CreateAsync(CreatePurchaseOrderRequest request, CancellationToken ct = default);
    Task<PurchaseOrderDto> UpdateAsync(long id, UpdatePurchaseOrderRequest request, CancellationToken ct = default);
    Task<PurchaseOrderDto> SubmitAsync(long id, CancellationToken ct = default);
    Task<PurchaseOrderDto> ApproveAsync(long id, CancellationToken ct = default);
}

public interface IGoodsReceiptService
{
    Task<PagedResult<GoodsReceiptDto>> SearchAsync(GoodsReceiptQuery query, CancellationToken ct = default);
    Task<GoodsReceiptDto?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<GoodsReceiptDto> CreateDraftAsync(CreateGoodsReceiptRequest request, CancellationToken ct = default);
    Task<GoodsReceiptDto> PostAsync(long id, CancellationToken ct = default);
}

public interface IInventoryQueryService
{
    Task<PagedResult<StockBalanceDto>> GetStockAsync(StockQuery query, CancellationToken ct = default);
    Task<IReadOnlyList<FefoCandidateDto>> GetFefoCandidatesAsync(FefoQuery query, CancellationToken ct = default);
    Task<PagedResult<StockBalanceDto>> GetNearExpiryAsync(NearExpiryQuery query, CancellationToken ct = default);
}

public interface IStockTransferService
{
    Task<StockTransferDto?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<StockTransferDto> CreateAsync(CreateStockTransferRequest request, CancellationToken ct = default);
    Task<StockTransferDto> CompleteAsync(long id, CancellationToken ct = default);
}

public interface IStockAdjustmentService
{
    Task<StockAdjustmentDto?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<StockAdjustmentDto> CreateAsync(CreateStockAdjustmentRequest request, CancellationToken ct = default);
    Task<StockAdjustmentDto> ApproveAsync(long id, CancellationToken ct = default);
    Task<StockAdjustmentDto> PostAsync(long id, CancellationToken ct = default);
}

public interface IStockCountService
{
    Task<StockCountDto?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<StockCountDto> CreateAsync(CreateStockCountRequest request, CancellationToken ct = default);
    Task<StockCountDto> CompleteAsync(long id, CancellationToken ct = default);
}

public interface ISaleService
{
    Task<PagedResult<SaleDto>> SearchAsync(SaleQuery query, CancellationToken ct = default);
    Task<SaleDto?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<SaleReceiptDto?> GetReceiptAsync(long id, CancellationToken ct = default);
    Task<SaleDto> CreateAsync(CreateSaleRequest request, CancellationToken ct = default);
    Task<SaleDto> RecordPaymentAsync(long saleId, RecordSalePaymentRequest request, CancellationToken ct = default);
    Task<SaleDto> VoidAsync(long saleId, VoidSaleRequest request, CancellationToken ct = default);
}

public interface ICashShiftService
{
    Task<PagedResult<CashShiftDto>> SearchAsync(CashShiftQuery query, CancellationToken ct = default);
    Task<CashShiftDto?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<CashShiftDto?> GetCurrentAsync(long terminalId, CancellationToken ct = default);
    Task<CashShiftDto> OpenAsync(OpenCashShiftRequest request, CancellationToken ct = default);
    Task<CashShiftDto> PayInAsync(long id, CashDrawerMovementRequest request, CancellationToken ct = default);
    Task<CashShiftDto> PayOutAsync(long id, CashDrawerMovementRequest request, CancellationToken ct = default);
    Task<CashShiftDto> CloseAsync(long id, CloseCashShiftRequest request, CancellationToken ct = default);
}

public interface ICustomerService
{
    Task<PagedResult<CustomerDto>> SearchAsync(CustomerQuery query, CancellationToken ct = default);
    Task<CustomerDto?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<CustomerDto> CreateAsync(CreateCustomerRequest request, CancellationToken ct = default);
    Task<CustomerDto> UpdateAsync(long id, UpdateCustomerRequest request, CancellationToken ct = default);
    Task DeactivateAsync(long id, CancellationToken ct = default);
    Task<PagedResult<CustomerLedgerEntryDto>> GetLedgerAsync(long customerId, CustomerLedgerQuery query, CancellationToken ct = default);
    Task<CustomerPaymentDto> RecordPaymentAsync(long customerId, RecordCustomerPaymentRequest request, CancellationToken ct = default);
}

public interface IHeldSaleService
{
    Task<PagedResult<HeldSaleDto>> SearchAsync(HeldSaleQuery query, CancellationToken ct = default);
    Task<HeldSaleDto?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<HeldSaleDto> HoldAsync(HoldSaleRequest request, CancellationToken ct = default);
    Task<HeldSaleDto> ResumeAsync(long id, CancellationToken ct = default);
    Task DiscardAsync(long id, CancellationToken ct = default);
}

public interface ISaleReturnService
{
    Task<PagedResult<SaleReturnDto>> SearchAsync(SaleReturnQuery query, CancellationToken ct = default);
    Task<SaleReturnDto?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<SaleReturnDto> CreateAsync(CreateSaleReturnRequest request, CancellationToken ct = default);
    Task<SaleReturnDto> PostAsync(long id, CancellationToken ct = default);
}

public interface IDoctorService
{
    Task<PagedResult<DoctorDto>> SearchAsync(DoctorQuery query, CancellationToken ct = default);
    Task<DoctorDto?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<DoctorDto> CreateAsync(CreateDoctorRequest request, CancellationToken ct = default);
    Task<DoctorDto> UpdateAsync(long id, UpdateDoctorRequest request, CancellationToken ct = default);
    Task DeactivateAsync(long id, CancellationToken ct = default);
}

public interface IPrescriptionService
{
    Task<PagedResult<PrescriptionDto>> SearchAsync(PrescriptionQuery query, CancellationToken ct = default);
    Task<PrescriptionDto?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<PrescriptionDto> CreateAsync(CreatePrescriptionRequest request, CancellationToken ct = default);
    Task<PrescriptionDto> CancelAsync(long id, CancellationToken ct = default);
    Task<PrescriptionDto> DispenseAsync(long id, DispensePrescriptionRequest request, CancellationToken ct = default);
}

public interface IControlledDrugService
{
    Task<PagedResult<ControlledRegisterDto>> SearchAsync(ControlledRegisterQuery query, CancellationToken ct = default);
    Task<ControlledRegisterDto?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<ControlledRegisterDto> OpenAsync(OpenControlledRegisterRequest request, CancellationToken ct = default);
    Task<PagedResult<ControlledTransactionDto>> GetTransactionsAsync(long registerId, ControlledTransactionQuery query, CancellationToken ct = default);
    Task<ControlledTransactionDto> PostTransactionAsync(long registerId, PostControlledTransactionRequest request, CancellationToken ct = default);
}

public interface IFiscalGateway
{
    Task<FiscalGatewayResult> SubmitAsync(FiscalGatewayRequest request, CancellationToken ct = default);
}

public sealed class FiscalGatewayRequest
{
    public string Provider { get; set; } = string.Empty;
    public string InternalInvoiceNumber { get; set; } = string.Empty;
    public decimal NetAmount { get; set; }
    public decimal TaxAmount { get; set; }
    public bool ForceFailure { get; set; }
}

public sealed class FiscalGatewayResult
{
    public bool Success { get; set; }
    public string Status { get; set; } = string.Empty;
    public int HttpStatusCode { get; set; }
    public string? FbrInvoiceNumber { get; set; }
    public string? QrData { get; set; }
    public string? VerificationUrl { get; set; }
    public string? ErrorCode { get; set; }
    public string? ErrorMessage { get; set; }
    public string RequestPayload { get; set; } = string.Empty;
    public string ResponsePayload { get; set; } = string.Empty;
}

public interface IFiscalService
{
    Task<PagedResult<FiscalDocumentDto>> SearchAsync(FiscalDocumentQuery query, CancellationToken ct = default);
    Task<FiscalDocumentDto?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<FiscalDocumentDto> CreateAsync(CreateFiscalDocumentRequest request, CancellationToken ct = default);
    Task<FiscalDocumentDto> SubmitAsync(long id, FiscalSubmitRequest request, CancellationToken ct = default);
    Task<FiscalDocumentDto> RetryAsync(long id, FiscalSubmitRequest request, CancellationToken ct = default);
}
