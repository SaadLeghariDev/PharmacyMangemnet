using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Auth;
using PharmacyManagement.Application.DTOs.Organization;
using PharmacyManagement.Application.DTOs.Products;

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
