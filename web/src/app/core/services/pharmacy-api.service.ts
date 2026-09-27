import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  ApiResponse,
  BarcodeLookupDto,
  BranchDto,
  CartLine,
  CounterDto,
  CreateExpenseCategoryRequest,
  CreateExpenseRequest,
  CreatePriceListRequest,
  CreateProductPriceRequest,
  CreateReorderRuleRequest,
  CreateSaleRequest,
  CreateSupplierPaymentRequest,
  CreateSupplierReturnRequest,
  CreateTaxProfileRequest,
  CreateTaxRateRequest,
  CreateAlertRuleRequest,
  EvaluateAlertsResultDto,
  ExpenseCategoryDto,
  ExpenseDto,
  ExpenseSearchParams,
  FefoCandidateDto,
  HeldSaleDto,
  HoldSaleRequest,
  LowStockCandidateDto,
  LowStockCandidateSearchParams,
  PagedResult,
  PosTerminalDto,
  PriceListDto,
  PriceListSearchParams,
  ProductDto,
  ProductPriceDto,
  ProductPriceSearchParams,
  ProductTaxProfileDto,
  ReplaceProductTaxProfilesRequest,
  ReorderRuleDto,
  ReorderRuleSearchParams,
  AlertDto,
  AlertRuleDto,
  AlertRuleSearchParams,
  AlertSearchParams,
  AuditLogDto,
  AuditLogSearchParams,
  BranchSettingDto,
  CreateBranchRequest,
  CreateReasonCodeRequest,
  CreateRoleRequest,
  CreateUserRequest,
  PermissionDto,
  ReasonCodeDto,
  ReasonCodeSearchParams,
  RoleDto,
  RoleSearchParams,
  SaleDto,
  SaleReceiptDto,
  SupplierDto,
  SupplierLedgerEntryDto,
  SupplierLedgerSearchParams,
  SupplierPaymentDto,
  SupplierPaymentSearchParams,
  SupplierReturnDto,
  SupplierReturnSearchParams,
  TaxProfileDto,
  TaxProfileSearchParams,
  TaxRateDto,
  TenantSettingDto,
  UpdateAlertRuleRequest,
  UpdateBranchRequest,
  UpdateExpenseCategoryRequest,
  UpdatePriceListRequest,
  UpdateProductPriceRequest,
  UpdateReasonCodeRequest,
  UpdateReorderRuleRequest,
  UpdateRoleRequest,
  UpdateTaxProfileRequest,
  UpdateTaxRateRequest,
  UpdateUserRequest,
  UpsertSettingRequest,
  UserAdminDto,
  UserAdminSearchParams,
  WarehouseDto,
} from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class PharmacyApiService {
  private readonly base = environment.apiBaseUrl;

  constructor(private readonly http: HttpClient) {}

  searchProducts(search: string, pageSize = 20): Observable<ProductDto[]> {
    let params = new HttpParams().set('pageSize', pageSize).set('isActive', true);
    if (search.trim()) params = params.set('search', search.trim());
    return this.http
      .get<ApiResponse<PagedResult<ProductDto>>>(`${this.base}/api/v1/products`, { params })
      .pipe(map((r) => this.unwrap(r).items));
  }

  getProductBySku(sku: string): Observable<ProductDto> {
    return this.http
      .get<ApiResponse<ProductDto>>(`${this.base}/api/v1/products/by-sku/${encodeURIComponent(sku)}`)
      .pipe(map((r) => this.unwrap(r)));
  }

  getProductByBarcode(barcode: string): Observable<BarcodeLookupDto> {
    return this.http
      .get<ApiResponse<BarcodeLookupDto>>(
        `${this.base}/api/v1/products/by-barcode/${encodeURIComponent(barcode)}`,
      )
      .pipe(map((r) => this.unwrap(r)));
  }

  listBranches(): Observable<BranchDto[]> {
    return this.http
      .get<ApiResponse<PagedResult<BranchDto>>>(`${this.base}/api/v1/branches`, {
        params: { pageSize: 50 },
      })
      .pipe(map((r) => this.unwrap(r).items));
  }

  listCounters(branchId?: number): Observable<CounterDto[]> {
    let params = new HttpParams().set('pageSize', 50);
    if (branchId != null) params = params.set('branchId', branchId);
    return this.http
      .get<ApiResponse<PagedResult<CounterDto>>>(`${this.base}/api/v1/counters`, { params })
      .pipe(map((r) => this.unwrap(r).items));
  }

  listWarehouses(branchId?: number): Observable<WarehouseDto[]> {
    let params = new HttpParams().set('pageSize', 50);
    if (branchId != null) params = params.set('branchId', branchId);
    return this.http
      .get<ApiResponse<PagedResult<WarehouseDto>>>(`${this.base}/api/v1/warehouses`, { params })
      .pipe(map((r) => this.unwrap(r).items));
  }

  listTerminals(branchId?: number): Observable<PosTerminalDto[]> {
    let params = new HttpParams().set('pageSize', 50);
    if (branchId != null) params = params.set('branchId', branchId);
    return this.http
      .get<ApiResponse<PagedResult<PosTerminalDto>>>(`${this.base}/api/v1/pos-terminals`, { params })
      .pipe(map((r) => this.unwrap(r).items));
  }

  getFefo(productId: number, warehouseId: number, requiredQuantity = 1): Observable<FefoCandidateDto[]> {
    const params = new HttpParams()
      .set('productId', productId)
      .set('warehouseId', warehouseId)
      .set('requiredQuantity', requiredQuantity);
    return this.http
      .get<ApiResponse<FefoCandidateDto[]>>(`${this.base}/api/v1/inventory/fefo`, { params })
      .pipe(map((r) => this.unwrap(r)));
  }

  createSale(request: CreateSaleRequest): Observable<SaleDto> {
    return this.http
      .post<ApiResponse<SaleDto>>(`${this.base}/api/v1/sales`, request)
      .pipe(map((r) => this.unwrap(r)));
  }

  getReceipt(saleId: number): Observable<SaleReceiptDto> {
    return this.http
      .get<ApiResponse<SaleReceiptDto>>(`${this.base}/api/v1/sales/${saleId}/receipt`)
      .pipe(map((r) => this.unwrap(r)));
  }

  listHeldSales(branchId?: number, terminalId?: number): Observable<HeldSaleDto[]> {
    let params = new HttpParams().set('pageSize', 50).set('status', 'Held');
    if (branchId != null) params = params.set('branchId', branchId);
    if (terminalId != null) params = params.set('terminalId', terminalId);
    return this.http
      .get<ApiResponse<PagedResult<HeldSaleDto>>>(`${this.base}/api/v1/held-sales`, { params })
      .pipe(map((r) => this.unwrap(r).items));
  }

  holdSale(request: HoldSaleRequest): Observable<HeldSaleDto> {
    return this.http
      .post<ApiResponse<HeldSaleDto>>(`${this.base}/api/v1/held-sales`, request)
      .pipe(map((r) => this.unwrap(r)));
  }

  resumeHeldSale(id: number): Observable<HeldSaleDto> {
    return this.http
      .post<ApiResponse<HeldSaleDto>>(`${this.base}/api/v1/held-sales/${id}/resume`, {})
      .pipe(map((r) => this.unwrap(r)));
  }

  discardHeldSale(id: number): Observable<void> {
    return this.http
      .post<ApiResponse<unknown>>(`${this.base}/api/v1/held-sales/${id}/discard`, {})
      .pipe(map(() => void 0));
  }

  searchExpenses(params: ExpenseSearchParams = {}): Observable<PagedResult<ExpenseDto>> {
    let httpParams = new HttpParams()
      .set('page', params.page ?? 1)
      .set('pageSize', params.pageSize ?? 20);
    if (params.search?.trim()) httpParams = httpParams.set('search', params.search.trim());
    if (params.branchId != null) httpParams = httpParams.set('branchId', params.branchId);
    if (params.categoryId != null) httpParams = httpParams.set('categoryId', params.categoryId);
    if (params.paymentMethodId != null)
      httpParams = httpParams.set('paymentMethodId', params.paymentMethodId);
    if (params.fromDate) httpParams = httpParams.set('fromDate', params.fromDate);
    if (params.toDate) httpParams = httpParams.set('toDate', params.toDate);
    return this.http
      .get<ApiResponse<PagedResult<ExpenseDto>>>(`${this.base}/api/v1/expenses`, {
        params: httpParams,
      })
      .pipe(map((r) => this.unwrap(r)));
  }

  createExpense(request: CreateExpenseRequest): Observable<ExpenseDto> {
    return this.http
      .post<ApiResponse<ExpenseDto>>(`${this.base}/api/v1/expenses`, request)
      .pipe(map((r) => this.unwrap(r)));
  }

  listExpenseCategories(search = '', pageSize = 100): Observable<ExpenseCategoryDto[]> {
    let params = new HttpParams().set('pageSize', pageSize);
    if (search.trim()) params = params.set('search', search.trim());
    return this.http
      .get<ApiResponse<PagedResult<ExpenseCategoryDto>>>(`${this.base}/api/v1/expense-categories`, {
        params,
      })
      .pipe(map((r) => this.unwrap(r).items));
  }

  createExpenseCategory(request: CreateExpenseCategoryRequest): Observable<ExpenseCategoryDto> {
    return this.http
      .post<ApiResponse<ExpenseCategoryDto>>(`${this.base}/api/v1/expense-categories`, request)
      .pipe(map((r) => this.unwrap(r)));
  }

  updateExpenseCategory(
    id: number,
    request: UpdateExpenseCategoryRequest,
  ): Observable<ExpenseCategoryDto> {
    return this.http
      .put<ApiResponse<ExpenseCategoryDto>>(`${this.base}/api/v1/expense-categories/${id}`, request)
      .pipe(map((r) => this.unwrap(r)));
  }

  searchSuppliers(search = '', pageSize = 100): Observable<SupplierDto[]> {
    let params = new HttpParams().set('pageSize', pageSize);
    if (search.trim()) params = params.set('search', search.trim());
    return this.http
      .get<ApiResponse<PagedResult<SupplierDto>>>(`${this.base}/api/v1/suppliers`, { params })
      .pipe(map((r) => this.unwrap(r).items));
  }

  getSupplier(id: number): Observable<SupplierDto> {
    return this.http
      .get<ApiResponse<SupplierDto>>(`${this.base}/api/v1/suppliers/${id}`)
      .pipe(map((r) => this.unwrap(r)));
  }

  getSupplierLedger(
    supplierId: number,
    params: SupplierLedgerSearchParams = {},
  ): Observable<PagedResult<SupplierLedgerEntryDto>> {
    let httpParams = new HttpParams()
      .set('page', params.page ?? 1)
      .set('pageSize', params.pageSize ?? 20);
    if (params.branchId != null) httpParams = httpParams.set('branchId', params.branchId);
    if (params.fromDate) httpParams = httpParams.set('fromDate', params.fromDate);
    if (params.toDate) httpParams = httpParams.set('toDate', params.toDate);
    return this.http
      .get<ApiResponse<PagedResult<SupplierLedgerEntryDto>>>(
        `${this.base}/api/v1/suppliers/${supplierId}/ledger`,
        { params: httpParams },
      )
      .pipe(map((r) => this.unwrap(r)));
  }

  searchSupplierPayments(
    params: SupplierPaymentSearchParams = {},
  ): Observable<PagedResult<SupplierPaymentDto>> {
    let httpParams = new HttpParams()
      .set('page', params.page ?? 1)
      .set('pageSize', params.pageSize ?? 20);
    if (params.search?.trim()) httpParams = httpParams.set('search', params.search.trim());
    if (params.supplierId != null) httpParams = httpParams.set('supplierId', params.supplierId);
    if (params.branchId != null) httpParams = httpParams.set('branchId', params.branchId);
    if (params.paymentMethodId != null)
      httpParams = httpParams.set('paymentMethodId', params.paymentMethodId);
    if (params.fromDate) httpParams = httpParams.set('fromDate', params.fromDate);
    if (params.toDate) httpParams = httpParams.set('toDate', params.toDate);
    return this.http
      .get<ApiResponse<PagedResult<SupplierPaymentDto>>>(`${this.base}/api/v1/supplier-payments`, {
        params: httpParams,
      })
      .pipe(map((r) => this.unwrap(r)));
  }

  createSupplierPayment(request: CreateSupplierPaymentRequest): Observable<SupplierPaymentDto> {
    return this.http
      .post<ApiResponse<SupplierPaymentDto>>(`${this.base}/api/v1/supplier-payments`, request)
      .pipe(map((r) => this.unwrap(r)));
  }

  searchSupplierReturns(
    params: SupplierReturnSearchParams = {},
  ): Observable<PagedResult<SupplierReturnDto>> {
    let httpParams = new HttpParams()
      .set('page', params.page ?? 1)
      .set('pageSize', params.pageSize ?? 20);
    if (params.search?.trim()) httpParams = httpParams.set('search', params.search.trim());
    if (params.supplierId != null) httpParams = httpParams.set('supplierId', params.supplierId);
    if (params.branchId != null) httpParams = httpParams.set('branchId', params.branchId);
    if (params.warehouseId != null) httpParams = httpParams.set('warehouseId', params.warehouseId);
    if (params.status) httpParams = httpParams.set('status', params.status);
    return this.http
      .get<ApiResponse<PagedResult<SupplierReturnDto>>>(`${this.base}/api/v1/supplier-returns`, {
        params: httpParams,
      })
      .pipe(map((r) => this.unwrap(r)));
  }

  getSupplierReturn(id: number): Observable<SupplierReturnDto> {
    return this.http
      .get<ApiResponse<SupplierReturnDto>>(`${this.base}/api/v1/supplier-returns/${id}`)
      .pipe(map((r) => this.unwrap(r)));
  }

  createSupplierReturn(request: CreateSupplierReturnRequest): Observable<SupplierReturnDto> {
    return this.http
      .post<ApiResponse<SupplierReturnDto>>(`${this.base}/api/v1/supplier-returns`, request)
      .pipe(map((r) => this.unwrap(r)));
  }

  postSupplierReturn(id: number): Observable<SupplierReturnDto> {
    return this.http
      .post<ApiResponse<SupplierReturnDto>>(`${this.base}/api/v1/supplier-returns/${id}/post`, {})
      .pipe(map((r) => this.unwrap(r)));
  }

  cancelSupplierReturn(id: number): Observable<SupplierReturnDto> {
    return this.http
      .post<ApiResponse<SupplierReturnDto>>(`${this.base}/api/v1/supplier-returns/${id}/cancel`, {})
      .pipe(map((r) => this.unwrap(r)));
  }

  searchPriceLists(params: PriceListSearchParams = {}): Observable<PagedResult<PriceListDto>> {
    let httpParams = new HttpParams()
      .set('page', params.page ?? 1)
      .set('pageSize', params.pageSize ?? 20);
    if (params.search?.trim()) httpParams = httpParams.set('search', params.search.trim());
    if (params.isActive != null) httpParams = httpParams.set('isActive', params.isActive);
    if (params.isDefault != null) httpParams = httpParams.set('isDefault', params.isDefault);
    return this.http
      .get<ApiResponse<PagedResult<PriceListDto>>>(`${this.base}/api/v1/price-lists`, { params: httpParams })
      .pipe(map((r) => this.unwrap(r)));
  }

  createPriceList(request: CreatePriceListRequest): Observable<PriceListDto> {
    return this.http
      .post<ApiResponse<PriceListDto>>(`${this.base}/api/v1/price-lists`, request)
      .pipe(map((r) => this.unwrap(r)));
  }

  updatePriceList(id: number, request: UpdatePriceListRequest): Observable<PriceListDto> {
    return this.http
      .put<ApiResponse<PriceListDto>>(`${this.base}/api/v1/price-lists/${id}`, request)
      .pipe(map((r) => this.unwrap(r)));
  }

  searchProductPrices(params: ProductPriceSearchParams = {}): Observable<PagedResult<ProductPriceDto>> {
    let httpParams = new HttpParams()
      .set('page', params.page ?? 1)
      .set('pageSize', params.pageSize ?? 20);
    if (params.search?.trim()) httpParams = httpParams.set('search', params.search.trim());
    if (params.productId != null) httpParams = httpParams.set('productId', params.productId);
    if (params.priceListId != null) httpParams = httpParams.set('priceListId', params.priceListId);
    if (params.productUnitId != null) httpParams = httpParams.set('productUnitId', params.productUnitId);
    if (params.activeOnly != null) httpParams = httpParams.set('activeOnly', params.activeOnly);
    return this.http
      .get<ApiResponse<PagedResult<ProductPriceDto>>>(`${this.base}/api/v1/product-prices`, {
        params: httpParams,
      })
      .pipe(map((r) => this.unwrap(r)));
  }

  createProductPrice(request: CreateProductPriceRequest): Observable<ProductPriceDto> {
    return this.http
      .post<ApiResponse<ProductPriceDto>>(`${this.base}/api/v1/product-prices`, request)
      .pipe(map((r) => this.unwrap(r)));
  }

  updateProductPrice(id: number, request: UpdateProductPriceRequest): Observable<ProductPriceDto> {
    return this.http
      .put<ApiResponse<ProductPriceDto>>(`${this.base}/api/v1/product-prices/${id}`, request)
      .pipe(map((r) => this.unwrap(r)));
  }

  searchTaxProfiles(params: TaxProfileSearchParams = {}): Observable<PagedResult<TaxProfileDto>> {
    let httpParams = new HttpParams()
      .set('page', params.page ?? 1)
      .set('pageSize', params.pageSize ?? 20);
    if (params.search?.trim()) httpParams = httpParams.set('search', params.search.trim());
    if (params.isActive != null) httpParams = httpParams.set('isActive', params.isActive);
    return this.http
      .get<ApiResponse<PagedResult<TaxProfileDto>>>(`${this.base}/api/v1/tax-profiles`, { params: httpParams })
      .pipe(map((r) => this.unwrap(r)));
  }

  getTaxProfile(id: number): Observable<TaxProfileDto> {
    return this.http
      .get<ApiResponse<TaxProfileDto>>(`${this.base}/api/v1/tax-profiles/${id}`)
      .pipe(map((r) => this.unwrap(r)));
  }

  createTaxProfile(request: CreateTaxProfileRequest): Observable<TaxProfileDto> {
    return this.http
      .post<ApiResponse<TaxProfileDto>>(`${this.base}/api/v1/tax-profiles`, request)
      .pipe(map((r) => this.unwrap(r)));
  }

  updateTaxProfile(id: number, request: UpdateTaxProfileRequest): Observable<TaxProfileDto> {
    return this.http
      .put<ApiResponse<TaxProfileDto>>(`${this.base}/api/v1/tax-profiles/${id}`, request)
      .pipe(map((r) => this.unwrap(r)));
  }

  listTaxRates(profileId: number): Observable<TaxRateDto[]> {
    return this.http
      .get<ApiResponse<TaxRateDto[]>>(`${this.base}/api/v1/tax-profiles/${profileId}/rates`)
      .pipe(map((r) => this.unwrap(r)));
  }

  addTaxRate(profileId: number, request: CreateTaxRateRequest): Observable<TaxRateDto> {
    return this.http
      .post<ApiResponse<TaxRateDto>>(`${this.base}/api/v1/tax-profiles/${profileId}/rates`, request)
      .pipe(map((r) => this.unwrap(r)));
  }

  updateTaxRate(
    profileId: number,
    rateId: number,
    request: UpdateTaxRateRequest,
  ): Observable<TaxRateDto> {
    return this.http
      .put<ApiResponse<TaxRateDto>>(`${this.base}/api/v1/tax-profiles/${profileId}/rates/${rateId}`, request)
      .pipe(map((r) => this.unwrap(r)));
  }

  getProductTaxProfiles(productId: number): Observable<ProductTaxProfileDto[]> {
    return this.http
      .get<ApiResponse<ProductTaxProfileDto[]>>(
        `${this.base}/api/v1/products/${productId}/tax-profiles`,
      )
      .pipe(map((r) => this.unwrap(r)));
  }

  replaceProductTaxProfiles(
    productId: number,
    request: ReplaceProductTaxProfilesRequest,
  ): Observable<ProductTaxProfileDto[]> {
    return this.http
      .put<ApiResponse<ProductTaxProfileDto[]>>(
        `${this.base}/api/v1/products/${productId}/tax-profiles`,
        request,
      )
      .pipe(map((r) => this.unwrap(r)));
  }

  searchReorderRules(params: ReorderRuleSearchParams = {}): Observable<PagedResult<ReorderRuleDto>> {
    let httpParams = new HttpParams()
      .set('page', params.page ?? 1)
      .set('pageSize', params.pageSize ?? 20);
    if (params.search?.trim()) httpParams = httpParams.set('search', params.search.trim());
    if (params.branchId != null) httpParams = httpParams.set('branchId', params.branchId);
    if (params.warehouseId != null) httpParams = httpParams.set('warehouseId', params.warehouseId);
    if (params.productId != null) httpParams = httpParams.set('productId', params.productId);
    if (params.isActive != null) httpParams = httpParams.set('isActive', params.isActive);
    return this.http
      .get<ApiResponse<PagedResult<ReorderRuleDto>>>(`${this.base}/api/v1/reorder-rules`, {
        params: httpParams,
      })
      .pipe(map((r) => this.unwrap(r)));
  }

  createReorderRule(request: CreateReorderRuleRequest): Observable<ReorderRuleDto> {
    return this.http
      .post<ApiResponse<ReorderRuleDto>>(`${this.base}/api/v1/reorder-rules`, request)
      .pipe(map((r) => this.unwrap(r)));
  }

  updateReorderRule(id: number, request: UpdateReorderRuleRequest): Observable<ReorderRuleDto> {
    return this.http
      .put<ApiResponse<ReorderRuleDto>>(`${this.base}/api/v1/reorder-rules/${id}`, request)
      .pipe(map((r) => this.unwrap(r)));
  }

  searchLowStockCandidates(
    params: LowStockCandidateSearchParams = {},
  ): Observable<PagedResult<LowStockCandidateDto>> {
    let httpParams = new HttpParams()
      .set('page', params.page ?? 1)
      .set('pageSize', params.pageSize ?? 20);
    if (params.branchId != null) httpParams = httpParams.set('branchId', params.branchId);
    if (params.warehouseId != null) httpParams = httpParams.set('warehouseId', params.warehouseId);
    if (params.productId != null) httpParams = httpParams.set('productId', params.productId);
    return this.http
      .get<ApiResponse<PagedResult<LowStockCandidateDto>>>(
        `${this.base}/api/v1/reorder-rules/low-stock`,
        { params: httpParams },
      )
      .pipe(map((r) => this.unwrap(r)));
  }

  searchAlertRules(params: AlertRuleSearchParams = {}): Observable<PagedResult<AlertRuleDto>> {
    let httpParams = new HttpParams()
      .set('page', params.page ?? 1)
      .set('pageSize', params.pageSize ?? 20);
    if (params.search?.trim()) httpParams = httpParams.set('search', params.search.trim());
    if (params.branchId != null) httpParams = httpParams.set('branchId', params.branchId);
    if (params.alertType) httpParams = httpParams.set('alertType', params.alertType);
    if (params.isActive != null) httpParams = httpParams.set('isActive', params.isActive);
    return this.http
      .get<ApiResponse<PagedResult<AlertRuleDto>>>(`${this.base}/api/v1/alert-rules`, {
        params: httpParams,
      })
      .pipe(map((r) => this.unwrap(r)));
  }

  createAlertRule(request: CreateAlertRuleRequest): Observable<AlertRuleDto> {
    return this.http
      .post<ApiResponse<AlertRuleDto>>(`${this.base}/api/v1/alert-rules`, request)
      .pipe(map((r) => this.unwrap(r)));
  }

  updateAlertRule(id: number, request: UpdateAlertRuleRequest): Observable<AlertRuleDto> {
    return this.http
      .put<ApiResponse<AlertRuleDto>>(`${this.base}/api/v1/alert-rules/${id}`, request)
      .pipe(map((r) => this.unwrap(r)));
  }

  searchAlerts(params: AlertSearchParams = {}): Observable<PagedResult<AlertDto>> {
    let httpParams = new HttpParams()
      .set('page', params.page ?? 1)
      .set('pageSize', params.pageSize ?? 20);
    if (params.search?.trim()) httpParams = httpParams.set('search', params.search.trim());
    if (params.branchId != null) httpParams = httpParams.set('branchId', params.branchId);
    if (params.productId != null) httpParams = httpParams.set('productId', params.productId);
    if (params.status) httpParams = httpParams.set('status', params.status);
    if (params.severity) httpParams = httpParams.set('severity', params.severity);
    if (params.alertType) httpParams = httpParams.set('alertType', params.alertType);
    return this.http
      .get<ApiResponse<PagedResult<AlertDto>>>(`${this.base}/api/v1/alerts`, { params: httpParams })
      .pipe(map((r) => this.unwrap(r)));
  }

  evaluateAlerts(): Observable<EvaluateAlertsResultDto> {
    return this.http
      .post<ApiResponse<EvaluateAlertsResultDto>>(`${this.base}/api/v1/alerts/evaluate`, {})
      .pipe(map((r) => this.unwrap(r)));
  }

  acknowledgeAlert(id: number): Observable<AlertDto> {
    return this.http
      .post<ApiResponse<AlertDto>>(`${this.base}/api/v1/alerts/${id}/acknowledge`, {})
      .pipe(map((r) => this.unwrap(r)));
  }

  resolveAlert(id: number): Observable<AlertDto> {
    return this.http
      .post<ApiResponse<AlertDto>>(`${this.base}/api/v1/alerts/${id}/resolve`, {})
      .pipe(map((r) => this.unwrap(r)));
  }

  serializeCart(lines: CartLine[]): string {
    return JSON.stringify(lines);
  }

  parseCart(cartData: string): CartLine[] {
    try {
      const parsed = JSON.parse(cartData) as CartLine[];
      return Array.isArray(parsed) ? parsed : [];
    } catch {
      return [];
    }
  }

  searchUsers(params: UserAdminSearchParams = {}): Observable<PagedResult<UserAdminDto>> {
    let httpParams = new HttpParams()
      .set('page', params.page ?? 1)
      .set('pageSize', params.pageSize ?? 20);
    if (params.search?.trim()) httpParams = httpParams.set('search', params.search.trim());
    if (params.isActive != null) httpParams = httpParams.set('isActive', params.isActive);
    return this.http
      .get<ApiResponse<PagedResult<UserAdminDto>>>(`${this.base}/api/v1/users`, { params: httpParams })
      .pipe(map((r) => this.unwrap(r)));
  }

  createUser(request: CreateUserRequest): Observable<UserAdminDto> {
    return this.http
      .post<ApiResponse<UserAdminDto>>(`${this.base}/api/v1/users`, request)
      .pipe(map((r) => this.unwrap(r)));
  }

  updateUser(id: number, request: UpdateUserRequest): Observable<UserAdminDto> {
    return this.http
      .put<ApiResponse<UserAdminDto>>(`${this.base}/api/v1/users/${id}`, request)
      .pipe(map((r) => this.unwrap(r)));
  }

  deactivateUser(id: number): Observable<void> {
    return this.http
      .post<ApiResponse<unknown>>(`${this.base}/api/v1/users/${id}/deactivate`, {})
      .pipe(map((r) => { this.unwrap(r); }));
  }

  assignUserRoles(id: number, roleIds: number[]): Observable<UserAdminDto> {
    return this.http
      .put<ApiResponse<UserAdminDto>>(`${this.base}/api/v1/users/${id}/roles`, { roleIds })
      .pipe(map((r) => this.unwrap(r)));
  }

  assignUserBranches(id: number, branchIds: number[]): Observable<UserAdminDto> {
    return this.http
      .put<ApiResponse<UserAdminDto>>(`${this.base}/api/v1/users/${id}/branches`, { branchIds })
      .pipe(map((r) => this.unwrap(r)));
  }

  searchRoles(params: RoleSearchParams = {}): Observable<PagedResult<RoleDto>> {
    let httpParams = new HttpParams()
      .set('page', params.page ?? 1)
      .set('pageSize', params.pageSize ?? 50);
    if (params.search?.trim()) httpParams = httpParams.set('search', params.search.trim());
    if (params.isSystemRole != null) httpParams = httpParams.set('isSystemRole', params.isSystemRole);
    return this.http
      .get<ApiResponse<PagedResult<RoleDto>>>(`${this.base}/api/v1/roles`, { params: httpParams })
      .pipe(map((r) => this.unwrap(r)));
  }

  createRole(request: CreateRoleRequest): Observable<RoleDto> {
    return this.http
      .post<ApiResponse<RoleDto>>(`${this.base}/api/v1/roles`, request)
      .pipe(map((r) => this.unwrap(r)));
  }

  updateRole(id: number, request: UpdateRoleRequest): Observable<RoleDto> {
    return this.http
      .put<ApiResponse<RoleDto>>(`${this.base}/api/v1/roles/${id}`, request)
      .pipe(map((r) => this.unwrap(r)));
  }

  assignRolePermissions(id: number, permissionIds: number[]): Observable<RoleDto> {
    return this.http
      .put<ApiResponse<RoleDto>>(`${this.base}/api/v1/roles/${id}/permissions`, { permissionIds })
      .pipe(map((r) => this.unwrap(r)));
  }

  listPermissions(pageSize = 100): Observable<PermissionDto[]> {
    return this.http
      .get<ApiResponse<PagedResult<PermissionDto>>>(`${this.base}/api/v1/permissions`, {
        params: { pageSize },
      })
      .pipe(map((r) => this.unwrap(r).items));
  }

  createBranch(request: CreateBranchRequest): Observable<BranchDto> {
    return this.http
      .post<ApiResponse<BranchDto>>(`${this.base}/api/v1/branches`, request)
      .pipe(map((r) => this.unwrap(r)));
  }

  updateBranch(id: number, request: UpdateBranchRequest): Observable<BranchDto> {
    return this.http
      .put<ApiResponse<BranchDto>>(`${this.base}/api/v1/branches/${id}`, request)
      .pipe(map((r) => this.unwrap(r)));
  }

  deactivateBranch(id: number): Observable<void> {
    return this.http
      .post<ApiResponse<unknown>>(`${this.base}/api/v1/branches/${id}/deactivate`, {})
      .pipe(map((r) => { this.unwrap(r); }));
  }

  searchTenantSettings(search?: string, page = 1, pageSize = 50): Observable<PagedResult<TenantSettingDto>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (search?.trim()) params = params.set('search', search.trim());
    return this.http
      .get<ApiResponse<PagedResult<TenantSettingDto>>>(`${this.base}/api/v1/tenant-settings`, { params })
      .pipe(map((r) => this.unwrap(r)));
  }

  upsertTenantSetting(request: UpsertSettingRequest): Observable<TenantSettingDto> {
    return this.http
      .put<ApiResponse<TenantSettingDto>>(`${this.base}/api/v1/tenant-settings`, request)
      .pipe(map((r) => this.unwrap(r)));
  }

  searchBranchSettings(branchId?: number, search?: string, page = 1, pageSize = 50): Observable<PagedResult<BranchSettingDto>> {
    let params = new HttpParams().set('page', page).set('pageSize', pageSize);
    if (branchId != null) params = params.set('branchId', branchId);
    if (search?.trim()) params = params.set('search', search.trim());
    return this.http
      .get<ApiResponse<PagedResult<BranchSettingDto>>>(`${this.base}/api/v1/branch-settings`, { params })
      .pipe(map((r) => this.unwrap(r)));
  }

  upsertBranchSetting(branchId: number, request: UpsertSettingRequest): Observable<BranchSettingDto> {
    return this.http
      .put<ApiResponse<BranchSettingDto>>(`${this.base}/api/v1/branch-settings/${branchId}`, request)
      .pipe(map((r) => this.unwrap(r)));
  }

  searchReasonCodes(params: ReasonCodeSearchParams = {}): Observable<PagedResult<ReasonCodeDto>> {
    let httpParams = new HttpParams()
      .set('page', params.page ?? 1)
      .set('pageSize', params.pageSize ?? 50);
    if (params.search?.trim()) httpParams = httpParams.set('search', params.search.trim());
    if (params.reasonType) httpParams = httpParams.set('reasonType', params.reasonType);
    if (params.isActive != null) httpParams = httpParams.set('isActive', params.isActive);
    return this.http
      .get<ApiResponse<PagedResult<ReasonCodeDto>>>(`${this.base}/api/v1/reason-codes`, { params: httpParams })
      .pipe(map((r) => this.unwrap(r)));
  }

  createReasonCode(request: CreateReasonCodeRequest): Observable<ReasonCodeDto> {
    return this.http
      .post<ApiResponse<ReasonCodeDto>>(`${this.base}/api/v1/reason-codes`, request)
      .pipe(map((r) => this.unwrap(r)));
  }

  updateReasonCode(id: number, request: UpdateReasonCodeRequest): Observable<ReasonCodeDto> {
    return this.http
      .put<ApiResponse<ReasonCodeDto>>(`${this.base}/api/v1/reason-codes/${id}`, request)
      .pipe(map((r) => this.unwrap(r)));
  }

  searchAuditLogs(params: AuditLogSearchParams = {}): Observable<PagedResult<AuditLogDto>> {
    let httpParams = new HttpParams()
      .set('page', params.page ?? 1)
      .set('pageSize', params.pageSize ?? 20);
    if (params.search?.trim()) httpParams = httpParams.set('search', params.search.trim());
    if (params.entityName) httpParams = httpParams.set('entityName', params.entityName);
    if (params.action) httpParams = httpParams.set('action', params.action);
    if (params.userId != null) httpParams = httpParams.set('userId', params.userId);
    if (params.branchId != null) httpParams = httpParams.set('branchId', params.branchId);
    return this.http
      .get<ApiResponse<PagedResult<AuditLogDto>>>(`${this.base}/api/v1/audit-logs`, { params: httpParams })
      .pipe(map((r) => this.unwrap(r)));
  }

  private unwrap<T>(res: ApiResponse<T>): T {
    if (!res.success || res.data === null || res.data === undefined) {
      const detail = res.errors?.length ? res.errors.join('; ') : res.message;
      throw new Error(detail || 'Request failed');
    }
    return res.data;
  }
}
