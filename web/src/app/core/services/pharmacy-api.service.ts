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
  CreateSaleRequest,
  ExpenseCategoryDto,
  ExpenseDto,
  ExpenseSearchParams,
  FefoCandidateDto,
  HeldSaleDto,
  HoldSaleRequest,
  PagedResult,
  PosTerminalDto,
  ProductDto,
  SaleDto,
  SaleReceiptDto,
  UpdateExpenseCategoryRequest,
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

  private unwrap<T>(res: ApiResponse<T>): T {
    if (!res.success || res.data === null || res.data === undefined) {
      const detail = res.errors?.length ? res.errors.join('; ') : res.message;
      throw new Error(detail || 'Request failed');
    }
    return res.data;
  }
}
