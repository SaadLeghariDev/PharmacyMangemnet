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
  isActive: boolean;
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
