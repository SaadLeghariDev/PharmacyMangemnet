using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Admin;
using PharmacyManagement.Application.DTOs.Alerts;
using PharmacyManagement.Application.DTOs.Auth;
using PharmacyManagement.Application.DTOs.Cash;
using PharmacyManagement.Application.DTOs.Controlled;
using PharmacyManagement.Application.DTOs.Customers;
using PharmacyManagement.Application.DTOs.Expenses;
using PharmacyManagement.Application.DTOs.Finance;
using PharmacyManagement.Application.DTOs.Fiscal;
using PharmacyManagement.Application.DTOs.Hardware;
using PharmacyManagement.Application.DTOs.Inventory;
using PharmacyManagement.Application.DTOs.Organization;
using PharmacyManagement.Application.DTOs.Prescriptions;
using PharmacyManagement.Application.DTOs.Pricing;
using PharmacyManagement.Application.DTOs.Products;
using PharmacyManagement.Application.DTOs.Procurement;
using PharmacyManagement.Application.DTOs.Purchasing;
using PharmacyManagement.Application.DTOs.Sales;
using PharmacyManagement.Application.DTOs.Sync;
using PharmacyManagement.Application.DTOs.Tax;

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
    Task<PagedResult<SupplierLedgerEntryDto>> GetLedgerAsync(long supplierId, SupplierLedgerQuery query, CancellationToken ct = default);
}

public interface ISupplierPaymentService
{
    Task<PagedResult<SupplierPaymentDto>> SearchAsync(SupplierPaymentQuery query, CancellationToken ct = default);
    Task<SupplierPaymentDto?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<SupplierPaymentDto> CreateAsync(CreateSupplierPaymentRequest request, CancellationToken ct = default);
}

public interface ISupplierReturnService
{
    Task<PagedResult<SupplierReturnDto>> SearchAsync(SupplierReturnQuery query, CancellationToken ct = default);
    Task<SupplierReturnDto?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<SupplierReturnDto> CreateDraftAsync(CreateSupplierReturnRequest request, CancellationToken ct = default);
    Task<SupplierReturnDto> PostAsync(long id, CancellationToken ct = default);
    Task<SupplierReturnDto> CancelAsync(long id, CancellationToken ct = default);
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

public interface IExpenseCategoryService
{
    Task<PagedResult<ExpenseCategoryDto>> SearchAsync(ExpenseCategoryQuery query, CancellationToken ct = default);
    Task<ExpenseCategoryDto?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<ExpenseCategoryDto> CreateAsync(CreateExpenseCategoryRequest request, CancellationToken ct = default);
    Task<ExpenseCategoryDto> UpdateAsync(long id, UpdateExpenseCategoryRequest request, CancellationToken ct = default);
}

public interface IExpenseService
{
    Task<PagedResult<ExpenseDto>> SearchAsync(ExpenseQuery query, CancellationToken ct = default);
    Task<ExpenseDto?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<ExpenseDto> CreateAsync(CreateExpenseRequest request, CancellationToken ct = default);
    Task<ExpenseDto> UpdateAsync(long id, UpdateExpenseRequest request, CancellationToken ct = default);
}

public interface IPriceListService
{
    Task<PagedResult<PriceListDto>> SearchAsync(PriceListQuery query, CancellationToken ct = default);
    Task<PriceListDto?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<PriceListDto> CreateAsync(CreatePriceListRequest request, CancellationToken ct = default);
    Task<PriceListDto> UpdateAsync(long id, UpdatePriceListRequest request, CancellationToken ct = default);
}

public interface IProductPriceService
{
    Task<PagedResult<ProductPriceDto>> SearchAsync(ProductPriceQuery query, CancellationToken ct = default);
    Task<ProductPriceDto?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<ProductPriceDto> CreateAsync(CreateProductPriceRequest request, CancellationToken ct = default);
    Task<ProductPriceDto> UpdateAsync(long id, UpdateProductPriceRequest request, CancellationToken ct = default);
}

public interface ITaxProfileService
{
    Task<PagedResult<TaxProfileDto>> SearchAsync(TaxProfileQuery query, CancellationToken ct = default);
    Task<TaxProfileDto?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<TaxProfileDto> CreateAsync(CreateTaxProfileRequest request, CancellationToken ct = default);
    Task<TaxProfileDto> UpdateAsync(long id, UpdateTaxProfileRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<TaxRateDto>> GetRatesAsync(long taxProfileId, CancellationToken ct = default);
    Task<TaxRateDto> AddRateAsync(long taxProfileId, CreateTaxRateRequest request, CancellationToken ct = default);
    Task<TaxRateDto> UpdateRateAsync(long taxProfileId, long rateId, UpdateTaxRateRequest request, CancellationToken ct = default);
    Task<IReadOnlyList<ProductTaxProfileDto>> GetProductTaxProfilesAsync(long productId, CancellationToken ct = default);
    Task<IReadOnlyList<ProductTaxProfileDto>> ReplaceProductTaxProfilesAsync(
        long productId, ReplaceProductTaxProfilesRequest request, CancellationToken ct = default);
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

public interface IReorderRuleService
{
    Task<PagedResult<ReorderRuleDto>> SearchAsync(ReorderRuleQuery query, CancellationToken ct = default);
    Task<ReorderRuleDto?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<ReorderRuleDto> CreateAsync(CreateReorderRuleRequest request, CancellationToken ct = default);
    Task<ReorderRuleDto> UpdateAsync(long id, UpdateReorderRuleRequest request, CancellationToken ct = default);
    Task<PagedResult<LowStockCandidateDto>> GetLowStockCandidatesAsync(LowStockCandidateQuery query, CancellationToken ct = default);
}

public interface IAlertRuleService
{
    Task<PagedResult<AlertRuleDto>> SearchAsync(AlertRuleQuery query, CancellationToken ct = default);
    Task<AlertRuleDto?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<AlertRuleDto> CreateAsync(CreateAlertRuleRequest request, CancellationToken ct = default);
    Task<AlertRuleDto> UpdateAsync(long id, UpdateAlertRuleRequest request, CancellationToken ct = default);
}

public interface IAlertService
{
    Task<PagedResult<AlertDto>> SearchAsync(AlertQuery query, CancellationToken ct = default);
    Task<AlertDto?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<AlertDto> AcknowledgeAsync(long id, CancellationToken ct = default);
    Task<AlertDto> ResolveAsync(long id, CancellationToken ct = default);
    Task<EvaluateAlertsResultDto> EvaluateAsync(CancellationToken ct = default);
}

public interface INotificationTemplateService
{
    Task<PagedResult<NotificationTemplateDto>> SearchAsync(NotificationTemplateQuery query, CancellationToken ct = default);
    Task<NotificationTemplateDto?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<NotificationTemplateDto> CreateAsync(CreateNotificationTemplateRequest request, CancellationToken ct = default);
    Task<NotificationTemplateDto> UpdateAsync(long id, UpdateNotificationTemplateRequest request, CancellationToken ct = default);
    Task<PagedResult<NotificationLogDto>> SearchLogsAsync(NotificationLogQuery query, CancellationToken ct = default);
}

public interface IUserAdminService
{
    Task<PagedResult<UserAdminDto>> SearchAsync(UserAdminQuery query, CancellationToken ct = default);
    Task<UserAdminDto?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<UserAdminDto> CreateAsync(CreateUserRequest request, CancellationToken ct = default);
    Task<UserAdminDto> UpdateAsync(long id, UpdateUserRequest request, CancellationToken ct = default);
    Task DeactivateAsync(long id, CancellationToken ct = default);
    Task<UserAdminDto> AssignRolesAsync(long id, AssignUserRolesRequest request, CancellationToken ct = default);
    Task<UserAdminDto> AssignBranchesAsync(long id, AssignUserBranchesRequest request, CancellationToken ct = default);
}

public interface IRoleAdminService
{
    Task<PagedResult<RoleDto>> SearchAsync(RoleQuery query, CancellationToken ct = default);
    Task<RoleDto?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<RoleDto> CreateAsync(CreateRoleRequest request, CancellationToken ct = default);
    Task<RoleDto> UpdateAsync(long id, UpdateRoleRequest request, CancellationToken ct = default);
    Task<RoleDto> AssignPermissionsAsync(long id, AssignRolePermissionsRequest request, CancellationToken ct = default);
    Task<PagedResult<PermissionDto>> SearchPermissionsAsync(PermissionQuery query, CancellationToken ct = default);
}

public interface ISettingsService
{
    Task<PagedResult<TenantSettingDto>> SearchTenantSettingsAsync(SettingQuery query, CancellationToken ct = default);
    Task<TenantSettingDto?> GetTenantSettingAsync(string key, CancellationToken ct = default);
    Task<TenantSettingDto> UpsertTenantSettingAsync(UpsertSettingRequest request, CancellationToken ct = default);

    Task<PagedResult<BranchSettingDto>> SearchBranchSettingsAsync(BranchSettingQuery query, CancellationToken ct = default);
    Task<BranchSettingDto?> GetBranchSettingAsync(long branchId, string key, CancellationToken ct = default);
    Task<BranchSettingDto> UpsertBranchSettingAsync(long branchId, UpsertSettingRequest request, CancellationToken ct = default);
}

public interface IReasonCodeService
{
    Task<PagedResult<ReasonCodeDto>> SearchAsync(ReasonCodeQuery query, CancellationToken ct = default);
    Task<ReasonCodeDto?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<ReasonCodeDto> CreateAsync(CreateReasonCodeRequest request, CancellationToken ct = default);
    Task<ReasonCodeDto> UpdateAsync(long id, UpdateReasonCodeRequest request, CancellationToken ct = default);
}

public interface IAuditLogService
{
    Task<PagedResult<AuditLogDto>> SearchAsync(AuditLogQuery query, CancellationToken ct = default);
    Task<AuditLogDto?> GetByIdAsync(long id, CancellationToken ct = default);
}

public interface IDeviceService
{
    Task<PagedResult<DeviceTypeDto>> SearchDeviceTypesAsync(DeviceTypeQuery query, CancellationToken ct = default);

    Task<PagedResult<DeviceDto>> SearchDevicesAsync(DeviceQuery query, CancellationToken ct = default);
    Task<DeviceDto?> GetDeviceByIdAsync(long id, CancellationToken ct = default);
    Task<DeviceDto> CreateDeviceAsync(CreateDeviceRequest request, CancellationToken ct = default);
    Task<DeviceDto> UpdateDeviceAsync(long id, UpdateDeviceRequest request, CancellationToken ct = default);

    Task<PagedResult<DeviceAssignmentDto>> SearchAssignmentsAsync(DeviceAssignmentQuery query, CancellationToken ct = default);
    Task<DeviceAssignmentDto> CreateAssignmentAsync(CreateDeviceAssignmentRequest request, CancellationToken ct = default);
    Task<DeviceAssignmentDto> EndAssignmentAsync(long id, CancellationToken ct = default);

    Task<IReadOnlyList<DeviceSettingDto>> GetSettingsAsync(long deviceId, string? key, CancellationToken ct = default);
    Task<DeviceSettingDto> UpsertSettingAsync(long deviceId, UpsertDeviceSettingRequest request, CancellationToken ct = default);

    Task<PagedResult<DeviceEventDto>> SearchEventsAsync(long deviceId, DeviceEventQuery query, CancellationToken ct = default);
    Task<DeviceEventDto> AppendEventAsync(long deviceId, CreateDeviceEventRequest request, CancellationToken ct = default);
}

public interface IPrintService
{
    Task<PagedResult<PrintTemplateDto>> SearchTemplatesAsync(PrintTemplateQuery query, CancellationToken ct = default);
    Task<PrintTemplateDto?> GetTemplateByIdAsync(long id, CancellationToken ct = default);
    Task<PrintTemplateDto> CreateTemplateAsync(CreatePrintTemplateRequest request, CancellationToken ct = default);
    Task<PrintTemplateDto> UpdateTemplateAsync(long id, UpdatePrintTemplateRequest request, CancellationToken ct = default);

    Task<PagedResult<BarcodePrintJobDto>> SearchJobsAsync(BarcodePrintJobQuery query, CancellationToken ct = default);
    Task<BarcodePrintJobDto?> GetJobByIdAsync(long id, CancellationToken ct = default);
    Task<BarcodePrintJobDto> CreateJobAsync(CreateBarcodePrintJobRequest request, CancellationToken ct = default);
    Task<BarcodePrintJobDto> UpdateJobStatusAsync(long id, UpdateBarcodePrintJobStatusRequest request, CancellationToken ct = default);
    Task<BarcodePrintJobDto> SimulateCompleteAsync(long id, SimulateBarcodePrintJobRequest request, CancellationToken ct = default);
}

public interface IAttachmentService
{
    Task<PagedResult<AttachmentDto>> SearchAsync(AttachmentQuery query, CancellationToken ct = default);
    Task<AttachmentDto?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<AttachmentDto> CreateAsync(CreateAttachmentRequest request, CancellationToken ct = default);

    Task<PagedResult<EntityAttachmentDto>> SearchLinksAsync(EntityAttachmentQuery query, CancellationToken ct = default);
    Task<EntityAttachmentDto> LinkAsync(CreateEntityAttachmentRequest request, CancellationToken ct = default);
}

public interface IAccountTypeService
{
    Task<PagedResult<AccountTypeDto>> SearchAsync(AccountTypeQuery query, CancellationToken ct = default);
}

public interface IChartOfAccountService
{
    Task<PagedResult<ChartOfAccountDto>> SearchAsync(ChartOfAccountQuery query, CancellationToken ct = default);
    Task<ChartOfAccountDto?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<ChartOfAccountDto> CreateAsync(CreateChartOfAccountRequest request, CancellationToken ct = default);
    Task<ChartOfAccountDto> UpdateAsync(long id, UpdateChartOfAccountRequest request, CancellationToken ct = default);
    Task<ChartOfAccountDto> DeactivateAsync(long id, CancellationToken ct = default);
}

public interface IJournalEntryService
{
    Task<PagedResult<JournalEntryDto>> SearchAsync(JournalEntryQuery query, CancellationToken ct = default);
    Task<JournalEntryDto?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<JournalEntryDto> CreateDraftAsync(CreateJournalEntryRequest request, CancellationToken ct = default);
    Task<JournalEntryDto> PostAsync(long id, CancellationToken ct = default);
    Task<JournalEntryDto> ReverseAsync(long id, CancellationToken ct = default);
}

public interface ISyncService
{
    Task<PagedResult<SyncNodeDto>> SearchNodesAsync(SyncNodeQuery query, CancellationToken ct = default);
    Task<SyncNodeDto?> GetNodeByIdAsync(long id, CancellationToken ct = default);
    Task<SyncNodeDto> RegisterNodeAsync(RegisterSyncNodeRequest request, CancellationToken ct = default);
    Task<SyncNodeDto> UpdateNodeAsync(long id, UpdateSyncNodeRequest request, CancellationToken ct = default);

    Task<PagedResult<SyncBatchDto>> SearchBatchesAsync(SyncBatchQuery query, CancellationToken ct = default);
    Task<SyncBatchDto?> GetBatchByIdAsync(long id, CancellationToken ct = default);
    Task<SyncBatchDto> CreateBatchAsync(CreateSyncBatchRequest request, CancellationToken ct = default);
    Task<SyncBatchDto> UpdateBatchStatusAsync(long id, UpdateSyncBatchStatusRequest request, CancellationToken ct = default);

    Task<PagedResult<SyncItemDto>> SearchItemsAsync(long batchId, SyncItemQuery query, CancellationToken ct = default);
    Task<IReadOnlyList<SyncItemDto>> AddItemsAsync(long batchId, CreateSyncItemsRequest request, CancellationToken ct = default);
    Task<SyncItemDto> UpdateItemStatusAsync(long id, UpdateSyncItemStatusRequest request, CancellationToken ct = default);

    Task<SyncPushPullResultDto> PushAsync(SyncPushRequest request, CancellationToken ct = default);
    Task<SyncPushPullResultDto> PullAsync(SyncPullRequest request, CancellationToken ct = default);

    Task<PagedResult<IdempotencyKeyDto>> SearchIdempotencyKeysAsync(IdempotencyKeyQuery query, CancellationToken ct = default);
}
