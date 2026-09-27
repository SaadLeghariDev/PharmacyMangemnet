using Microsoft.EntityFrameworkCore;
using PharmacyManagement.Application.Common;
using PharmacyManagement.Application.DTOs.Prescriptions;
using PharmacyManagement.Application.Exceptions;
using PharmacyManagement.Application.Interfaces;
using PharmacyManagement.Infrastructure.Persistence;
using PharmacyManagement.Infrastructure.Persistence.Entities;

namespace PharmacyManagement.Infrastructure.Services;

public sealed class DoctorService(PharmacyManagementDbContext db) : IDoctorService
{
    public async Task<PagedResult<DoctorDto>> SearchAsync(DoctorQuery query, CancellationToken ct = default)
    {
        var q = db.Doctors.AsNoTracking().AsQueryable();
        if (query.IsActive is bool active) q = q.Where(d => d.IsActive == active);
        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var s = query.Search.Trim();
            q = q.Where(d =>
                d.Name.Contains(s) ||
                (d.Pmdcnumber != null && d.Pmdcnumber.Contains(s)) ||
                (d.Specialization != null && d.Specialization.Contains(s)));
        }

        q = q.OrderBy(d => d.Name);
        var total = await q.CountAsync(ct);
        var items = await q.Skip((query.Page - 1) * query.PageSize).Take(query.PageSize).ToListAsync(ct);
        return new PagedResult<DoctorDto>
        {
            Items = items.Select(Map).ToList(),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = total
        };
    }

    public async Task<DoctorDto?> GetByIdAsync(long id, CancellationToken ct = default)
    {
        var entity = await db.Doctors.AsNoTracking().FirstOrDefaultAsync(d => d.Id == id, ct);
        return entity is null ? null : Map(entity);
    }

    public async Task<DoctorDto> CreateAsync(CreateDoctorRequest request, CancellationToken ct = default)
    {
        var entity = new Doctor
        {
            Name = request.Name.Trim(),
            Pmdcnumber = request.PmdcNumber,
            Specialization = request.Specialization,
            Phone = request.Phone,
            Email = request.Email,
            ClinicName = request.ClinicName,
            Address = request.Address,
            IsActive = true
        };
        db.Doctors.Add(entity);
        await db.SaveChangesAsync(ct);
        return Map(entity);
    }

    public async Task<DoctorDto> UpdateAsync(long id, UpdateDoctorRequest request, CancellationToken ct = default)
    {
        var entity = await db.Doctors.FirstOrDefaultAsync(d => d.Id == id, ct)
            ?? throw new NotFoundException($"Doctor {id} not found.");

        entity.Name = request.Name.Trim();
        entity.Pmdcnumber = request.PmdcNumber;
        entity.Specialization = request.Specialization;
        entity.Phone = request.Phone;
        entity.Email = request.Email;
        entity.ClinicName = request.ClinicName;
        entity.Address = request.Address;
        await db.SaveChangesAsync(ct);
        return Map(entity);
    }

    public async Task DeactivateAsync(long id, CancellationToken ct = default)
    {
        var entity = await db.Doctors.FirstOrDefaultAsync(d => d.Id == id, ct)
            ?? throw new NotFoundException($"Doctor {id} not found.");
        entity.IsActive = false;
        await db.SaveChangesAsync(ct);
    }

    private static DoctorDto Map(Doctor d) => new()
    {
        Id = d.Id,
        Name = d.Name,
        PmdcNumber = d.Pmdcnumber,
        Specialization = d.Specialization,
        Phone = d.Phone,
        Email = d.Email,
        ClinicName = d.ClinicName,
        Address = d.Address,
        IsActive = d.IsActive
    };
}
