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
  CreateSaleRequest,
  FefoCandidateDto,
  HeldSaleDto,
  HoldSaleRequest,
  PagedResult,
  PosTerminalDto,
  ProductDto,
  SaleDto,
  SaleReceiptDto,
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
