/** Matches PharmacyManagement.Application.Common.ApiResponse<T> */
export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T | null;
  errors: string[] | null;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasNext: boolean;
  hasPrevious: boolean;
}

export interface LoginRequest {
  username: string;
  password: string;
  tenantId?: number | null;
}

export interface UserProfile {
  id: number;
  tenantId: number;
  username: string;
  fullName: string;
  email?: string | null;
  roles: string[];
  permissions: string[];
  branchIds: number[];
}

export interface LoginResponse {
  accessToken: string;
  expiresAtUtc: string;
  user: UserProfile;
}

export interface ProductDto {
  id: number;
  tenantId: number;
  categoryId: number;
  categoryName?: string | null;
  manufacturerId: number;
  manufacturerName?: string | null;
  brandId: number;
  brandName?: string | null;
  therapeuticClassId: number;
  therapeuticClassName?: string | null;
  sku: string;
  productCode?: string | null;
  name: string;
  genericName?: string | null;
  form?: string | null;
  strength?: string | null;
  strengthUnit?: string | null;
  packDescription?: string | null;
  prescriptionRequired: boolean;
  isControlled: boolean;
  isTemperatureSensitive: boolean;
  isRefrigerated: boolean;
  isReturnable: boolean;
  isSaleable: boolean;
  isActive: boolean;
  defaultSaleUnitId?: number | null;
  createdAt: string;
  updatedAt: string;
}

export interface BarcodeLookupDto {
  barcodeId: number;
  barcodeValue: string;
  productId: number;
  sku: string;
  productName: string;
  productUnitId: number;
  isPrimary: boolean;
}

export interface BranchDto {
  id: number;
  tenantId: number;
  code: string;
  name: string;
  branchType?: string | null;
  city?: string | null;
  province?: string | null;
  phone?: string | null;
  email?: string | null;
  address?: string | null;
  isActive: boolean;
}

export interface CreateBranchRequest {
  code: string;
  name: string;
  branchType?: string | null;
  city?: string | null;
  province?: string | null;
  phone?: string | null;
  email?: string | null;
  address?: string | null;
}

export interface UpdateBranchRequest {
  name: string;
  branchType?: string | null;
  city?: string | null;
  province?: string | null;
  phone?: string | null;
  email?: string | null;
  address?: string | null;
  isActive: boolean;
}

export interface UserAdminDto {
  id: number;
  tenantId: number;
  username: string;
  email?: string | null;
  fullName: string;
  phone?: string | null;
  employeeCode?: string | null;
  isActive: boolean;
  lastLoginAt?: string | null;
  createdAt: string;
  updatedAt: string;
  roleIds: number[];
  roleNames: string[];
  branchIds: number[];
}

export interface UserAdminSearchParams {
  page?: number;
  pageSize?: number;
  search?: string;
  isActive?: boolean | null;
}

export interface CreateUserRequest {
  username: string;
  password: string;
  fullName: string;
  email?: string | null;
  phone?: string | null;
  employeeCode?: string | null;
  isActive: boolean;
  roleIds?: number[];
  branchIds?: number[];
}

export interface UpdateUserRequest {
  fullName: string;
  email?: string | null;
  phone?: string | null;
  employeeCode?: string | null;
  isActive: boolean;
  password?: string | null;
}

export interface RoleDto {
  id: number;
  tenantId: number;
  name: string;
  description?: string | null;
  isSystemRole: boolean;
  permissionIds: number[];
  permissionCodes: string[];
}

export interface RoleSearchParams {
  page?: number;
  pageSize?: number;
  search?: string;
  isSystemRole?: boolean | null;
}

export interface CreateRoleRequest {
  name: string;
  description?: string | null;
  permissionIds?: number[];
}

export interface UpdateRoleRequest {
  name: string;
  description?: string | null;
}

export interface PermissionDto {
  id: number;
  code: string;
  name: string;
  module: string;
  description?: string | null;
}

export interface TenantSettingDto {
  id: number;
  tenantId: number;
  settingKey: string;
  settingValue?: string | null;
  isEncrypted: boolean;
}

export interface BranchSettingDto {
  id: number;
  branchId: number;
  settingKey: string;
  settingValue?: string | null;
  isEncrypted: boolean;
}

export interface UpsertSettingRequest {
  settingKey: string;
  settingValue?: string | null;
  isEncrypted: boolean;
}

export interface ReasonCodeDto {
  id: number;
  tenantId: number;
  reasonType: string;
  code: string;
  name: string;
  description?: string | null;
  isActive: boolean;
}

export interface ReasonCodeSearchParams {
  page?: number;
  pageSize?: number;
  search?: string;
  reasonType?: string | null;
  isActive?: boolean | null;
}

export interface CreateReasonCodeRequest {
  reasonType: string;
  code: string;
  name: string;
  description?: string | null;
  isActive: boolean;
}

export interface UpdateReasonCodeRequest {
  name: string;
  description?: string | null;
  isActive: boolean;
}

export interface AuditLogDto {
  id: number;
  tenantId?: number | null;
  branchId?: number | null;
  userId?: number | null;
  entityName: string;
  entityId?: number | null;
  action: string;
  oldValues?: string | null;
  newValues?: string | null;
  ipAddress?: string | null;
  terminalId?: number | null;
  createdAt: string;
}

export interface AuditLogSearchParams {
  page?: number;
  pageSize?: number;
  search?: string;
  entityName?: string | null;
  action?: string | null;
  userId?: number | null;
  branchId?: number | null;
}

export interface CounterDto {
  id: number;
  branchId: number;
  code: string;
  name: string;
  counterType?: string | null;
  isActive: boolean;
}

export interface WarehouseDto {
  id: number;
  branchId: number;
  code: string;
  name: string;
  warehouseType?: string | null;
  isMain: boolean;
  temperatureControlled: boolean;
  isActive: boolean;
}

export interface PosTerminalDto {
  id: number;
  branchId: number;
  counterId: number;
  terminalCode: string;
  computerName?: string | null;
  isPrimary: boolean;
  isOnline: boolean;
  isActive: boolean;
}

export interface FefoCandidateDto {
  batchId: number;
  productId: number;
  batchNumber: string;
  expiryDate: string;
  warehouseLocationId: number;
  availableQuantity: number;
  purchaseCost: number;
  salePrice: number;
}

export interface CreateSaleLineRequest {
  productId: number;
  productUnitId: number;
  quantity: number;
  unitPrice?: number | null;
  discountAmount: number;
  taxAmount: number;
  prescriptionItemId?: number | null;
}

export interface CreateSalePaymentRequest {
  paymentMethodId: number;
  amount: number;
  referenceNumber?: string | null;
}

export interface CreateSaleRequest {
  branchId: number;
  counterId: number;
  terminalId: number;
  warehouseId: number;
  customerId?: number | null;
  saleType: string;
  currencyCode: string;
  roundOff: number;
  idempotencyKey?: string | null;
  lines: CreateSaleLineRequest[];
  payments: CreateSalePaymentRequest[];
}

export interface SaleLineBatchDto {
  id: number;
  batchId: number;
  batchNumber?: string | null;
  expiryDate?: string | null;
  quantity: number;
  baseQuantity: number;
  unitCost: number;
}

export interface SaleLineDto {
  id: number;
  productId: number;
  sku?: string | null;
  productName?: string | null;
  productUnitId: number;
  quantity: number;
  baseQuantity: number;
  conversionFactor: number;
  unitPrice: number;
  mrp: number;
  discountAmount: number;
  taxAmount: number;
  netAmount: number;
  prescriptionItemId?: number | null;
  batches: SaleLineBatchDto[];
}

export interface SalePaymentDto {
  id: number;
  paymentMethodId: number;
  paymentMethodCode?: string | null;
  paymentMethodName?: string | null;
  amount: number;
  referenceNumber?: string | null;
  paymentDate: string;
  status: string;
  transactionType: string;
}

export interface SaleDto {
  id: number;
  branchId: number;
  counterId: number;
  terminalId: number;
  userId: number;
  customerId?: number | null;
  invoiceNumber: string;
  saleDate: string;
  saleType: string;
  status: string;
  currencyCode: string;
  subtotal: number;
  discountAmount: number;
  taxAmount: number;
  roundOff: number;
  netAmount: number;
  paidAmount: number;
  dueAmount: number;
  changeAmount: number;
  paymentStatus: string;
  fbrStatus?: string | null;
  createdAt: string;
  lines: SaleLineDto[];
  payments: SalePaymentDto[];
}

export interface SaleReceiptDto {
  sale: SaleDto;
  customerName?: string | null;
  customerCode?: string | null;
  branchName?: string | null;
  counterCode?: string | null;
  terminalCode?: string | null;
  cashierName?: string | null;
}

export interface HeldSaleDto {
  id: number;
  branchId: number;
  terminalId: number;
  userId: number;
  customerId?: number | null;
  cartData: string;
  totalAmount: number;
  heldAt: string;
  expiresAt?: string | null;
  status: string;
}

export interface HoldSaleRequest {
  branchId: number;
  terminalId: number;
  customerId?: number | null;
  cartData: string;
  totalAmount: number;
  expiresAt?: string | null;
}

/** Client-side cart line (serialized into held-sale cartData). */
export interface CartLine {
  productId: number;
  productUnitId: number;
  sku: string;
  productName: string;
  quantity: number;
  unitPrice: number;
  discountAmount: number;
  taxAmount: number;
  batchNumber?: string;
  expiryDate?: string;
}

export interface ExpenseCategoryDto {
  id: number;
  name: string;
  code: string;
}

export interface CreateExpenseCategoryRequest {
  name: string;
  code: string;
}

export interface UpdateExpenseCategoryRequest {
  name: string;
  code: string;
}

export interface ExpenseDto {
  id: number;
  branchId: number;
  branchName?: string | null;
  categoryId: number;
  categoryName?: string | null;
  categoryCode?: string | null;
  expenseNumber: string;
  expenseDate: string;
  amount: number;
  paymentMethodId: number;
  paymentMethodCode?: string | null;
  paymentMethodName?: string | null;
  paymentMethodType?: string | null;
  description?: string | null;
  createdBy?: number | null;
  approvedBy?: number | null;
}

export interface CreateExpenseRequest {
  branchId: number;
  categoryId: number;
  expenseDate: string;
  amount: number;
  paymentMethodId: number;
  description?: string | null;
  terminalId?: number | null;
}

export interface ExpenseSearchParams {
  page?: number;
  pageSize?: number;
  search?: string;
  branchId?: number | null;
  categoryId?: number | null;
  paymentMethodId?: number | null;
  fromDate?: string | null;
  toDate?: string | null;
}

export interface SupplierDto {
  id: number;
  tenantId: number;
  code: string;
  name: string;
  companyName?: string | null;
  phone?: string | null;
  email?: string | null;
  creditLimit: number;
  paymentTermsDays: number;
  isActive: boolean;
}

export interface SupplierPaymentDto {
  id: number;
  supplierId: number;
  supplierCode?: string | null;
  supplierName?: string | null;
  branchId: number;
  branchName?: string | null;
  paymentMethodId: number;
  paymentMethodCode?: string | null;
  paymentMethodName?: string | null;
  paymentMethodType?: string | null;
  amount: number;
  referenceNumber?: string | null;
  paymentDate: string;
  remarks?: string | null;
  paidBy?: number | null;
}

export interface CreateSupplierPaymentRequest {
  supplierId: number;
  branchId: number;
  paymentMethodId: number;
  amount: number;
  referenceNumber?: string | null;
  paymentDate: string;
  remarks?: string | null;
  terminalId?: number | null;
}

export interface SupplierPaymentSearchParams {
  page?: number;
  pageSize?: number;
  search?: string;
  supplierId?: number | null;
  branchId?: number | null;
  paymentMethodId?: number | null;
  fromDate?: string | null;
  toDate?: string | null;
}

export interface SupplierReturnLineDto {
  id: number;
  productId: number;
  batchId: number;
  goodsReceiptLineId?: number | null;
  productUnitId: number;
  quantity: number;
  unitCost: number;
  lineAmount: number;
}

export interface SupplierReturnDto {
  id: number;
  supplierId: number;
  supplierCode?: string | null;
  supplierName?: string | null;
  branchId: number;
  branchName?: string | null;
  warehouseId: number;
  warehouseName?: string | null;
  returnNumber: string;
  returnDate: string;
  reason?: string | null;
  status: string;
  totalAmount: number;
  lines: SupplierReturnLineDto[];
}

export interface CreateSupplierReturnLineRequest {
  productId: number;
  batchId: number;
  productUnitId: number;
  quantity: number;
  unitCost: number;
  goodsReceiptLineId?: number | null;
  warehouseLocationId?: number | null;
}

export interface CreateSupplierReturnRequest {
  supplierId: number;
  branchId: number;
  warehouseId: number;
  returnDate?: string | null;
  reason?: string | null;
  lines: CreateSupplierReturnLineRequest[];
}

export interface SupplierReturnSearchParams {
  page?: number;
  pageSize?: number;
  search?: string;
  supplierId?: number | null;
  branchId?: number | null;
  warehouseId?: number | null;
  status?: string | null;
}

export interface SupplierLedgerEntryDto {
  id: number;
  supplierId: number;
  branchId: number;
  transactionDate: string;
  transactionType: string;
  referenceType?: string | null;
  referenceId?: number | null;
  debit: number;
  credit: number;
  sequenceNo: number;
  remarks?: string | null;
  runningBalance: number;
}

export interface SupplierLedgerSearchParams {
  page?: number;
  pageSize?: number;
  branchId?: number | null;
  fromDate?: string | null;
  toDate?: string | null;
}

export interface PriceListDto {
  id: number;
  tenantId: number;
  name: string;
  priceType?: string | null;
  currencyCode: string;
  isDefault: boolean;
  isActive: boolean;
}

export interface CreatePriceListRequest {
  name: string;
  priceType?: string | null;
  currencyCode: string;
  isDefault: boolean;
  isActive: boolean;
}

export interface UpdatePriceListRequest {
  name: string;
  priceType?: string | null;
  currencyCode: string;
  isDefault: boolean;
  isActive: boolean;
}

export interface PriceListSearchParams {
  page?: number;
  pageSize?: number;
  search?: string;
  isActive?: boolean | null;
  isDefault?: boolean | null;
}

export interface ProductPriceDto {
  id: number;
  priceListId: number;
  priceListName?: string | null;
  priceListIsDefault: boolean;
  productId: number;
  productSku?: string | null;
  productName?: string | null;
  productUnitId: number;
  unitName?: string | null;
  purchasePrice: number;
  salePrice: number;
  mrp: number;
  discountPercent: number;
  effectiveFrom: string;
  effectiveTo?: string | null;
}

export interface CreateProductPriceRequest {
  priceListId: number;
  productId: number;
  productUnitId: number;
  purchasePrice: number;
  salePrice: number;
  mrp: number;
  discountPercent: number;
  effectiveFrom: string;
  effectiveTo?: string | null;
}

export interface UpdateProductPriceRequest {
  purchasePrice: number;
  salePrice: number;
  mrp: number;
  discountPercent: number;
  effectiveFrom: string;
  effectiveTo?: string | null;
}

export interface ProductPriceSearchParams {
  page?: number;
  pageSize?: number;
  search?: string;
  productId?: number | null;
  priceListId?: number | null;
  productUnitId?: number | null;
  activeOnly?: boolean | null;
}

export interface TaxRateDto {
  id: number;
  taxProfileId: number;
  rate: number;
  effectiveFrom: string;
  effectiveTo?: string | null;
  isActive: boolean;
}

export interface TaxProfileDto {
  id: number;
  name: string;
  taxType?: string | null;
  description?: string | null;
  isActive: boolean;
  rates: TaxRateDto[];
}

export interface CreateTaxProfileRequest {
  name: string;
  taxType?: string | null;
  description?: string | null;
  isActive: boolean;
}

export interface UpdateTaxProfileRequest {
  name: string;
  taxType?: string | null;
  description?: string | null;
  isActive: boolean;
}

export interface CreateTaxRateRequest {
  rate: number;
  effectiveFrom: string;
  effectiveTo?: string | null;
  isActive: boolean;
}

export interface UpdateTaxRateRequest {
  rate: number;
  effectiveFrom: string;
  effectiveTo?: string | null;
  isActive: boolean;
}

export interface TaxProfileSearchParams {
  page?: number;
  pageSize?: number;
  search?: string;
  isActive?: boolean | null;
}

export interface ProductTaxProfileDto {
  taxProfileId: number;
  name: string;
  taxType?: string | null;
  isActive: boolean;
}

export interface ReplaceProductTaxProfilesRequest {
  taxProfileIds: number[];
}

export interface ReorderRuleDto {
  id: number;
  branchId: number;
  branchCode?: string | null;
  warehouseId: number;
  warehouseCode?: string | null;
  productId: number;
  productSku?: string | null;
  productName?: string | null;
  minimumStock: number;
  maximumStock: number;
  reorderPoint: number;
  reorderQuantity: number;
  preferredSupplierId: number;
  preferredSupplierName?: string | null;
  isActive: boolean;
}

export interface CreateReorderRuleRequest {
  branchId: number;
  warehouseId: number;
  productId: number;
  minimumStock: number;
  maximumStock: number;
  reorderPoint: number;
  reorderQuantity: number;
  preferredSupplierId: number;
  isActive: boolean;
}

export interface UpdateReorderRuleRequest {
  minimumStock: number;
  maximumStock: number;
  reorderPoint: number;
  reorderQuantity: number;
  preferredSupplierId: number;
  isActive: boolean;
}

export interface ReorderRuleSearchParams {
  page?: number;
  pageSize?: number;
  search?: string;
  branchId?: number | null;
  warehouseId?: number | null;
  productId?: number | null;
  isActive?: boolean | null;
}

export interface LowStockCandidateDto {
  reorderRuleId: number;
  branchId: number;
  branchCode?: string | null;
  warehouseId: number;
  warehouseCode?: string | null;
  productId: number;
  productSku?: string | null;
  productName?: string | null;
  minimumStock: number;
  reorderPoint: number;
  reorderQuantity: number;
  preferredSupplierId: number;
  preferredSupplierName?: string | null;
  onHandQuantity: number;
  availableQuantity: number;
  shortageQuantity: number;
}

export interface LowStockCandidateSearchParams {
  page?: number;
  pageSize?: number;
  branchId?: number | null;
  warehouseId?: number | null;
  productId?: number | null;
}

export interface AlertRuleDto {
  id: number;
  tenantId: number;
  branchId?: number | null;
  branchCode?: string | null;
  alertType: string;
  threshold?: number | null;
  daysBeforeExpiry?: number | null;
  isActive: boolean;
}

export interface CreateAlertRuleRequest {
  branchId?: number | null;
  alertType: string;
  threshold?: number | null;
  daysBeforeExpiry?: number | null;
  isActive: boolean;
}

export interface UpdateAlertRuleRequest {
  branchId?: number | null;
  alertType: string;
  threshold?: number | null;
  daysBeforeExpiry?: number | null;
  isActive: boolean;
}

export interface AlertRuleSearchParams {
  page?: number;
  pageSize?: number;
  search?: string;
  branchId?: number | null;
  alertType?: string | null;
  isActive?: boolean | null;
}

export interface AlertDto {
  id: number;
  alertRuleId: number;
  alertType?: string | null;
  branchId: number;
  branchCode?: string | null;
  productId?: number | null;
  productSku?: string | null;
  productName?: string | null;
  batchId?: number | null;
  batchNumber?: string | null;
  severity: string;
  title: string;
  message?: string | null;
  status: string;
  createdAt: string;
  resolvedAt?: string | null;
  resolvedBy?: number | null;
}

export interface AlertSearchParams {
  page?: number;
  pageSize?: number;
  search?: string;
  branchId?: number | null;
  productId?: number | null;
  status?: string | null;
  severity?: string | null;
  alertType?: string | null;
}

export interface EvaluateAlertsResultDto {
  rulesScanned: number;
  alertsCreated: number;
  notificationLogsCreated: number;
  createdAlerts: AlertDto[];
}

export interface DeviceTypeDto {
  id: number;
  code: string;
  name: string;
}

export interface DeviceDto {
  id: number;
  branchId: number;
  branchName?: string | null;
  counterId?: number | null;
  deviceTypeId: number;
  deviceTypeCode?: string | null;
  deviceTypeName?: string | null;
  name: string;
  manufacturer?: string | null;
  model?: string | null;
  serialNumber?: string | null;
  connectionType?: string | null;
  ip?: string | null;
  port?: number | null;
  comPort?: string | null;
  macAddress?: string | null;
  driverName?: string | null;
  driverVersion?: string | null;
  isDefault: boolean;
  isActive: boolean;
  lastSeenAt?: string | null;
}

export interface DeviceSearchParams {
  page?: number;
  pageSize?: number;
  search?: string;
  branchId?: number | null;
  deviceTypeId?: number | null;
  isActive?: boolean | null;
}

export interface CreateDeviceRequest {
  branchId: number;
  counterId?: number | null;
  deviceTypeId: number;
  name: string;
  manufacturer?: string | null;
  model?: string | null;
  serialNumber?: string | null;
  connectionType?: string | null;
  ip?: string | null;
  port?: number | null;
  comPort?: string | null;
  macAddress?: string | null;
  driverName?: string | null;
  driverVersion?: string | null;
  isDefault: boolean;
  isActive: boolean;
}

export interface UpdateDeviceRequest {
  counterId?: number | null;
  deviceTypeId: number;
  name: string;
  manufacturer?: string | null;
  model?: string | null;
  serialNumber?: string | null;
  connectionType?: string | null;
  ip?: string | null;
  port?: number | null;
  comPort?: string | null;
  macAddress?: string | null;
  driverName?: string | null;
  driverVersion?: string | null;
  isDefault: boolean;
  isActive: boolean;
  lastSeenAt?: string | null;
}

export interface DeviceAssignmentDto {
  id: number;
  deviceId: number;
  deviceName?: string | null;
  terminalId: number;
  terminalCode?: string | null;
  assignedFrom: string;
  assignedTo?: string | null;
  isActive: boolean;
}

export interface CreateDeviceAssignmentRequest {
  deviceId: number;
  terminalId: number;
  assignedFrom?: string | null;
}

export interface DeviceSettingDto {
  id: number;
  deviceId: number;
  settingKey: string;
  settingValue?: string | null;
  isEncrypted: boolean;
}

export interface UpsertDeviceSettingRequest {
  settingKey: string;
  settingValue?: string | null;
  isEncrypted: boolean;
}

export interface DeviceEventDto {
  id: number;
  deviceId: number;
  eventType: string;
  payload?: string | null;
  status: string;
  errorMessage?: string | null;
  createdAt: string;
}

export interface CreateDeviceEventRequest {
  eventType: string;
  payload?: string | null;
  status: string;
  errorMessage?: string | null;
}

export interface PrintTemplateDto {
  id: number;
  tenantId: number;
  templateType: string;
  name: string;
  templateContent: string;
  paperWidth?: number | null;
  isDefault: boolean;
  isActive: boolean;
}

export interface PrintTemplateSearchParams {
  page?: number;
  pageSize?: number;
  search?: string;
  templateType?: string | null;
  isActive?: boolean | null;
}

export interface CreatePrintTemplateRequest {
  templateType: string;
  name: string;
  templateContent: string;
  paperWidth?: number | null;
  isDefault: boolean;
  isActive: boolean;
}

export interface UpdatePrintTemplateRequest {
  templateType: string;
  name: string;
  templateContent: string;
  paperWidth?: number | null;
  isDefault: boolean;
  isActive: boolean;
}

export interface BarcodePrintJobDto {
  id: number;
  branchId: number;
  branchName?: string | null;
  printerDeviceId: number;
  printerDeviceName?: string | null;
  productId: number;
  productSku?: string | null;
  productName?: string | null;
  batchId?: number | null;
  quantity: number;
  templateId: number;
  templateName?: string | null;
  status: string;
  createdBy?: number | null;
  createdAt: string;
  printedAt?: string | null;
}

export interface BarcodePrintJobSearchParams {
  page?: number;
  pageSize?: number;
  branchId?: number | null;
  status?: string | null;
  productId?: number | null;
}

export interface CreateBarcodePrintJobRequest {
  branchId: number;
  printerDeviceId: number;
  productId: number;
  batchId?: number | null;
  quantity: number;
  templateId: number;
}

export interface UpdateBarcodePrintJobStatusRequest {
  status: string;
}

export interface SimulateBarcodePrintJobRequest {
  fail?: boolean;
}

export interface AttachmentDto {
  id: number;
  tenantId: number;
  fileName: string;
  storagePath: string;
  contentType?: string | null;
  fileSize: number;
  hash?: string | null;
  createdAt: string;
}

export interface CreateAttachmentRequest {
  fileName: string;
  storagePath: string;
  contentType?: string | null;
  fileSize: number;
  hash?: string | null;
}

export interface EntityAttachmentDto {
  id: number;
  attachmentId: number;
  entityName: string;
  entityId: number;
  fileName?: string | null;
  storagePath?: string | null;
  contentType?: string | null;
}

export interface CreateEntityAttachmentRequest {
  attachmentId: number;
  entityName: string;
  entityId: number;
}

export interface EntityAttachmentSearchParams {
  page?: number;
  pageSize?: number;
  entityName?: string | null;
  entityId?: number | null;
  attachmentId?: number | null;
}

export interface AccountTypeDto {
  id: number;
  name: string;
}

export interface ChartOfAccountDto {
  id: number;
  tenantId: number;
  parentAccountId?: number | null;
  parentAccountCode?: string | null;
  parentAccountName?: string | null;
  code: string;
  name: string;
  accountTypeId: number;
  accountTypeName?: string | null;
  isSystemAccount: boolean;
  isActive: boolean;
}

export interface ChartOfAccountSearchParams {
  page?: number;
  pageSize?: number;
  search?: string;
  accountTypeId?: number | null;
  parentAccountId?: number | null;
  isActive?: boolean | null;
}

export interface CreateChartOfAccountRequest {
  code: string;
  name: string;
  accountTypeId: number;
  parentAccountId?: number | null;
  isActive: boolean;
}

export interface UpdateChartOfAccountRequest {
  name: string;
  accountTypeId: number;
  parentAccountId?: number | null;
  isActive: boolean;
}

export interface JournalLineDto {
  id: number;
  lineNo: number;
  accountId: number;
  accountCode?: string | null;
  accountName?: string | null;
  debit: number;
  credit: number;
  description?: string | null;
}

export interface JournalEntryDto {
  id: number;
  tenantId: number;
  branchId: number;
  branchName?: string | null;
  entryNumber: string;
  entryDate: string;
  referenceType?: string | null;
  referenceId?: number | null;
  description?: string | null;
  status: string;
  postedBy?: number | null;
  postedAt?: string | null;
  reversalOfEntryId?: number | null;
  reversalOfEntryNumber?: string | null;
  totalDebit: number;
  totalCredit: number;
  lines: JournalLineDto[];
}

export interface JournalEntrySearchParams {
  page?: number;
  pageSize?: number;
  search?: string;
  branchId?: number | null;
  status?: string | null;
  fromDate?: string | null;
  toDate?: string | null;
}

export interface CreateJournalLineRequest {
  accountId: number;
  debit: number;
  credit: number;
  description?: string | null;
}

export interface CreateJournalEntryRequest {
  branchId: number;
  entryDate: string;
  referenceType?: string | null;
  referenceId?: number | null;
  description?: string | null;
  lines: CreateJournalLineRequest[];
}

export interface SyncNodeDto {
  id: number;
  tenantId: number;
  branchId: number;
  branchName?: string | null;
  terminalId: number;
  terminalCode?: string | null;
  nodeCode: string;
  lastSyncAt?: string | null;
  lastSequence: number;
  isActive: boolean;
}

export interface SyncNodeSearchParams {
  page?: number;
  pageSize?: number;
  search?: string;
  branchId?: number | null;
  terminalId?: number | null;
  isActive?: boolean | null;
}

export interface RegisterSyncNodeRequest {
  branchId: number;
  terminalId: number;
  nodeCode: string;
}

export interface UpdateSyncNodeRequest {
  isActive: boolean;
}

export interface SyncItemDto {
  id: number;
  syncBatchId: number;
  entityName: string;
  entityId: number;
  operation: string;
  payload?: string | null;
  version: number;
  processedAt?: string | null;
  status: string;
  errorMessage?: string | null;
}

export interface SyncBatchDto {
  id: number;
  syncNodeId: number;
  nodeCode?: string | null;
  batchNumber: string;
  startedAt: string;
  completedAt?: string | null;
  status: string;
  itemCount: number;
  pendingCount: number;
  processedCount: number;
  failedCount: number;
  skippedCount: number;
  items: SyncItemDto[];
}

export interface SyncBatchSearchParams {
  page?: number;
  pageSize?: number;
  search?: string;
  syncNodeId?: number | null;
  status?: string | null;
}

export interface CreateSyncItemRequest {
  entityName: string;
  entityId: number;
  operation: string;
  payload?: string | null;
  version: number;
}

export interface SyncPushRequest {
  syncNodeId: number;
  items: CreateSyncItemRequest[];
  autoComplete?: boolean;
  simulateFailures?: boolean;
}

export interface SyncPullRequest {
  syncNodeId: number;
  sinceSequence?: number | null;
  limit?: number;
}

export interface SyncPushPullResultDto {
  batch: SyncBatchDto;
  node: SyncNodeDto;
}

export interface IdempotencyKeyDto {
  id: number;
  terminalId: number;
  terminalCode?: string | null;
  key: string;
  entityType: string;
  entityId?: number | null;
  createdAt: string;
}

export interface IdempotencyKeySearchParams {
  page?: number;
  pageSize?: number;
  search?: string;
  terminalId?: number | null;
  entityType?: string | null;
}
