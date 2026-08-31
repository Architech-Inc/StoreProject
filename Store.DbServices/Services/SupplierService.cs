using Microsoft.EntityFrameworkCore;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Procurement;
using Store.Models.Entities;
using Store.Models.Entities.Contacts;
using Store.Models.Enums;
using Store.Models.Interfaces;
using Store.Models.Interfaces.Services;

namespace Store.DbServices.Services;

public class SupplierService : ISupplierService
{
    private readonly IUnitOfWork _uow;

    public SupplierService(IUnitOfWork uow) => _uow = uow;

    public async Task<List<SupplierDto>> GetAllAsync(string? search = null, string? city = null, string? country = null, string? sortBy = null)
    {
        var query = BuildBaseSupplierQuery();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(x =>
                x.Name.ToLower().Contains(s) ||
                (x.RegistrationNumber != null && x.RegistrationNumber.ToLower().Contains(s)) ||
                x.Emails.Any(e => e.Email != null && e.Email.Address.ToLower().Contains(s)) ||
                x.Phones.Any(p => p.Phone != null && p.Phone.Number.ToLower().Contains(s))
            );
        }

        if (!string.IsNullOrWhiteSpace(city))
        {
            var c = city.Trim().ToLower();
            query = query.Where(x => x.Locations.Any(l => l.Location != null && l.Location.City != null && l.Location.City.Name.ToLower().Contains(c)));
        }

        if (!string.IsNullOrWhiteSpace(country))
        {
            var cnt = country.Trim().ToLower();
            query = query.Where(x => x.Locations.Any(l => l.Location != null && l.Location.City != null && l.Location.City.Region != null && l.Location.City.Region.Country != null && l.Location.City.Region.Country.Name.ToLower().Contains(cnt)));
        }

        query = sortBy switch
        {
            "name_desc" => query.OrderByDescending(s => s.Name),
            "date_asc" => query.OrderBy(s => s.DateCreated),
            "date_desc" => query.OrderByDescending(s => s.DateCreated),
            _ => query.OrderBy(s => s.Name)
        };

        var suppliers = await query.ToListAsync();
        return suppliers.Select(MapToDto).ToList();
    }

    public async Task<PagedResult<SupplierDto>> GetPagedAsync(PagedRequest request, CancellationToken ct = default)
    {
        var query = BuildBaseSupplierQuery();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var s = request.SearchTerm.Trim().ToLower();
            query = query.Where(x =>
                x.Name.ToLower().Contains(s) ||
                (x.RegistrationNumber != null && x.RegistrationNumber.ToLower().Contains(s)) ||
                x.Emails.Any(e => e.Email != null && e.Email.Address.ToLower().Contains(s)) ||
                x.Phones.Any(p => p.Phone != null && p.Phone.Number.ToLower().Contains(s))
            );
        }

        var totalCount = await query.CountAsync(ct);

        query = request.SortBy?.ToLowerInvariant() switch
        {
            "name_desc" => query.OrderByDescending(s => s.Name),
            "date_asc" => query.OrderBy(s => s.DateCreated),
            "date_desc" => query.OrderByDescending(s => s.DateCreated),
            _ => query.OrderBy(s => s.Name)
        };

        var items = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        return new PagedResult<SupplierDto>
        {
            Items = items.Select(MapToDto).ToList(),
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize
        };
    }

    public async Task<SupplierDto?> GetByCodeOrNameAsync(string codeOrName, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(codeOrName)) return null;

        var target = codeOrName.Trim().ToLower();
        var query = BuildBaseSupplierQuery();

        var supplier = await query.FirstOrDefaultAsync(x =>
            (x.RegistrationNumber != null && x.RegistrationNumber.ToLower() == target) ||
            x.Name.ToLower() == target, ct);

        return supplier is null ? null : MapToDto(supplier);
    }

    private IQueryable<Supplier> BuildBaseSupplierQuery()
    {
        return _uow.Repository<Supplier>().Query()
            .AsNoTracking()
            .Include(s => s.Emails).ThenInclude(se => se.Email)
            .Include(s => s.Phones).ThenInclude(sp => sp.Phone)
            .Include(s => s.Locations).ThenInclude(sl => sl.Location).ThenInclude(l => l.City).ThenInclude(c => c.Region).ThenInclude(r => r.Country)
            .AsQueryable();
    }

    public async Task<SupplierDto?> GetByIdAsync(Guid id)
    {
        var supplier = await _uow.Repository<Supplier>().Query()
            .AsNoTracking()
            .Include(s => s.Emails).ThenInclude(se => se.Email)
            .Include(s => s.Phones).ThenInclude(sp => sp.Phone)
            .Include(s => s.Locations).ThenInclude(sl => sl.Location).ThenInclude(l => l.City).ThenInclude(c => c.Region).ThenInclude(r => r.Country)
            .FirstOrDefaultAsync(s => s.SupplierId == id);

        return supplier is null ? null : MapToDto(supplier);
    }

    public async Task<SupplierProfileDto?> GetProfileAsync(Guid id)
    {
        var supplier = await _uow.Repository<Supplier>().Query()
            .AsNoTracking()
            .Include(s => s.Emails).ThenInclude(se => se.Email)
            .Include(s => s.Phones).ThenInclude(sp => sp.Phone)
            .Include(s => s.Locations).ThenInclude(sl => sl.Location).ThenInclude(l => l.City).ThenInclude(c => c.Region).ThenInclude(r => r.Country)
            .FirstOrDefaultAsync(s => s.SupplierId == id);

        if (supplier is null) return null;

        var baseDto = MapToDto(supplier);

        // Query purchase orders for this supplier
        var purchaseOrders = await _uow.Repository<PurchaseOrder>().Query()
            .AsNoTracking()
            .Include(p => p.Items).ThenInclude(i => i.Item)
            .Where(p => p.SupplierId == id)
            .OrderByDescending(p => p.DateCreated)
            .ToListAsync();

        var recentOrders = purchaseOrders.Select(p => new SupplierPurchaseOrderSummaryDto
        {
            PurchaseOrderId = p.PurchaseOrderId,
            ReferenceNumber = p.ReferenceNumber,
            Status = p.Status.ToString(),
            TotalAmount = p.Items.Sum(i => i.OrderedQuantity * i.UnitCost),
            ItemsCount = p.Items.Count,
            DateCreated = p.DateCreated,
            ExpectedDeliveryDate = p.ExpectedDeliveryDate
        }).ToList();

        var totalSpend = purchaseOrders
            .Where(p => p.Status == PurchaseOrderStatus.Approved || p.Status == PurchaseOrderStatus.PartiallyReceived || p.Status == PurchaseOrderStatus.Received)
            .SelectMany(p => p.Items)
            .Sum(i => (i.ReceivedQuantity > 0 ? i.ReceivedQuantity : i.OrderedQuantity) * i.UnitCost);

        var openOrdersCount = purchaseOrders
            .Count(p => p.Status == PurchaseOrderStatus.Draft || p.Status == PurchaseOrderStatus.Submitted || p.Status == PurchaseOrderStatus.Approved || p.Status == PurchaseOrderStatus.PartiallyReceived);

        var lastOrderDate = purchaseOrders.FirstOrDefault()?.DateCreated;

        // Group supplied items from PO items
        var suppliedItems = purchaseOrders
            .SelectMany(p => p.Items)
            .Where(i => i.Item != null)
            .GroupBy(i => i.ItemId)
            .Select(g =>
            {
                var latest = g.OrderByDescending(x => x.PurchaseOrder?.DateCreated ?? DateTime.MinValue).First();
                return new SupplierItemSummaryDto
                {
                    ItemId = g.Key,
                    ItemName = latest.Item?.Name ?? "Item",
                    Barcode = latest.Item?.Barcode,
                    LastUnitCost = latest.UnitCost,
                    TotalQuantityReceived = g.Sum(x => x.ReceivedQuantity),
                    LastReceivedDate = latest.PurchaseOrder?.ReceivedAt ?? latest.PurchaseOrder?.DateCreated
                };
            })
            .OrderByDescending(i => i.TotalQuantityReceived)
            .ToList();

        return new SupplierProfileDto
        {
            SupplierId = baseDto.SupplierId,
            Name = baseDto.Name,
            RegistrationNumber = baseDto.RegistrationNumber,
            Notes = baseDto.Notes,
            ThumbnailUrl = baseDto.ThumbnailUrl,
            FullImageUrl = baseDto.FullImageUrl,
            DateCreated = baseDto.DateCreated,
            Emails = baseDto.Emails,
            Phones = baseDto.Phones,
            Locations = baseDto.Locations,
            TotalSpend = totalSpend,
            TotalPurchaseOrdersCount = purchaseOrders.Count,
            OpenOrdersCount = openOrdersCount,
            LastOrderDate = lastOrderDate,
            RecentOrders = recentOrders,
            SuppliedItems = suppliedItems
        };
    }

    public async Task<SupplierMetricsDto> GetMetricsAsync()
    {
        var totalSuppliers = await _uow.Repository<Supplier>().Query().CountAsync();

        var purchaseOrders = await _uow.Repository<PurchaseOrder>().Query()
            .AsNoTracking()
            .Include(p => p.Items)
            .ToListAsync();

        var totalSpend = purchaseOrders
            .Where(p => p.Status == PurchaseOrderStatus.Approved || p.Status == PurchaseOrderStatus.PartiallyReceived || p.Status == PurchaseOrderStatus.Received)
            .SelectMany(p => p.Items)
            .Sum(i => (i.ReceivedQuantity > 0 ? i.ReceivedQuantity : i.OrderedQuantity) * i.UnitCost);

        var openOrdersCount = purchaseOrders
            .Count(p => p.Status == PurchaseOrderStatus.Draft || p.Status == PurchaseOrderStatus.Submitted || p.Status == PurchaseOrderStatus.Approved || p.Status == PurchaseOrderStatus.PartiallyReceived);

        var pendingDeliveriesCount = purchaseOrders
            .Count(p => (p.Status == PurchaseOrderStatus.Approved || p.Status == PurchaseOrderStatus.PartiallyReceived) &&
                        p.ExpectedDeliveryDate.HasValue &&
                        p.ExpectedDeliveryDate.Value.Date <= DateTime.UtcNow.Date.AddDays(3));

        var activeSuppliers = await _uow.Repository<PurchaseOrder>().Query()
            .Where(p => p.DateCreated >= DateTime.UtcNow.AddMonths(-6))
            .Select(p => p.SupplierId)
            .Distinct()
            .CountAsync();

        if (activeSuppliers == 0 && totalSuppliers > 0)
            activeSuppliers = totalSuppliers;

        return new SupplierMetricsDto
        {
            TotalSuppliers = totalSuppliers,
            ActiveSuppliers = activeSuppliers,
            TotalProcurementSpend = totalSpend,
            OpenPurchaseOrdersCount = openOrdersCount,
            PendingDeliveriesCount = pendingDeliveriesCount
        };
    }

    public async Task<SupplierDto> CreateAsync(CreateSupplierRequest request, Guid createdByUserId)
    {
        var supplier = new Supplier
        {
            SupplierId = Guid.NewGuid(),
            Name = request.Name.Trim(),
            RegistrationNumber = request.RegistrationNumber?.Trim(),
            Notes = request.Notes?.Trim(),
            ThumbnailUrl = request.ThumbnailUrl?.Trim(),
            FullImageUrl = request.FullImageUrl?.Trim()
        };

        // Create emails
        if (request.Emails is not null)
        {
            foreach (var emailReq in request.Emails)
            {
                if (!string.IsNullOrWhiteSpace(emailReq.Email))
                {
                    var email = await GetOrCreateEmailAsync(emailReq.Email, emailReq.EmailType);
                    supplier.Emails.Add(new SupplierEmail
                    {
                        SupplierId = supplier.SupplierId,
                        EmailId = email.EmailId,
                        IsPrimary = emailReq.IsPrimary
                    });
                }
            }
        }

        // Create phones
        if (request.Phones is not null)
        {
            foreach (var phoneReq in request.Phones)
            {
                if (!string.IsNullOrWhiteSpace(phoneReq.PhoneNumber))
                {
                    var phone = await GetOrCreatePhoneAsync(phoneReq.PhoneNumber, phoneReq.PhoneType);
                    supplier.Phones.Add(new SupplierPhone
                    {
                        SupplierId = supplier.SupplierId,
                        PhoneId = phone.PhoneId,
                        IsPrimary = phoneReq.IsPrimary
                    });
                }
            }
        }

        // Create locations
        if (request.Locations is not null)
        {
            foreach (var locReq in request.Locations)
            {
                if (!string.IsNullOrWhiteSpace(locReq.AddressLine1) || !string.IsNullOrWhiteSpace(locReq.City))
                {
                    var location = await GetOrCreateLocationAsync(locReq);
                    supplier.Locations.Add(new SupplierLocation
                    {
                        SupplierId = supplier.SupplierId,
                        LocationId = location.LocationId,
                        IsPrimary = locReq.IsPrimary
                    });
                }
            }
        }

        await _uow.Repository<Supplier>().AddAsync(supplier);
        await _uow.SaveChangesAsync();
        return (await GetByIdAsync(supplier.SupplierId)) ?? MapToDto(supplier);
    }

    public async Task<SupplierDto?> UpdateAsync(Guid id, UpdateSupplierRequest request)
    {
        var supplier = await _uow.Repository<Supplier>().Query()
            .Include(s => s.Emails)
            .Include(s => s.Phones)
            .Include(s => s.Locations)
            .ThenInclude(sl => sl.Location)
            .ThenInclude(l => l.City)
            .FirstOrDefaultAsync(s => s.SupplierId == id);

        if (supplier is null) return null;

        supplier.Name = request.Name.Trim();
        supplier.RegistrationNumber = request.RegistrationNumber?.Trim();
        supplier.Notes = request.Notes?.Trim();
        if (request.ThumbnailUrl != null) supplier.ThumbnailUrl = request.ThumbnailUrl;
        if (request.FullImageUrl != null) supplier.FullImageUrl = request.FullImageUrl;

        if (request.Emails is not null)
        {
            supplier.Emails.Clear();
            foreach (var emailReq in request.Emails)
            {
                if (!string.IsNullOrWhiteSpace(emailReq.Email))
                {
                    var email = await GetOrCreateEmailAsync(emailReq.Email, emailReq.EmailType);
                    supplier.Emails.Add(new SupplierEmail
                    {
                        SupplierId = supplier.SupplierId,
                        EmailId = email.EmailId,
                        IsPrimary = emailReq.IsPrimary
                    });
                }
            }
        }

        if (request.Phones is not null)
        {
            supplier.Phones.Clear();
            foreach (var phoneReq in request.Phones)
            {
                if (!string.IsNullOrWhiteSpace(phoneReq.PhoneNumber))
                {
                    var phone = await GetOrCreatePhoneAsync(phoneReq.PhoneNumber, phoneReq.PhoneType);
                    supplier.Phones.Add(new SupplierPhone
                    {
                        SupplierId = supplier.SupplierId,
                        PhoneId = phone.PhoneId,
                        IsPrimary = phoneReq.IsPrimary
                    });
                }
            }
        }

        if (request.Locations is not null)
        {
            supplier.Locations.Clear();
            foreach (var locReq in request.Locations)
            {
                if (!string.IsNullOrWhiteSpace(locReq.AddressLine1) || !string.IsNullOrWhiteSpace(locReq.City))
                {
                    var location = await GetOrCreateLocationAsync(locReq);
                    supplier.Locations.Add(new SupplierLocation
                    {
                        SupplierId = supplier.SupplierId,
                        LocationId = location.LocationId,
                        IsPrimary = locReq.IsPrimary
                    });
                }
            }
        }

        _uow.Repository<Supplier>().Update(supplier);
        await _uow.SaveChangesAsync();
        return (await GetByIdAsync(supplier.SupplierId)) ?? MapToDto(supplier);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var supplier = await _uow.Repository<Supplier>().GetByIdAsync(id);
        if (supplier is null) return false;

        // Check for related items orders and purchase orders
        var hasItemsOrders = await _uow.Repository<ItemsOrder>().ExistsAsync(o => o.SupplierId == id);
        if (hasItemsOrders) return false;

        var hasPurchaseOrders = await _uow.Repository<PurchaseOrder>().ExistsAsync(p => p.SupplierId == id);
        if (hasPurchaseOrders) return false;

        _uow.Repository<Supplier>().Remove(supplier);
        await _uow.SaveChangesAsync();
        return true;
    }

    private static SupplierDto MapToDto(Supplier supplier)
    {
        return new SupplierDto
        {
            SupplierId = supplier.SupplierId,
            Name = supplier.Name,
            RegistrationNumber = supplier.RegistrationNumber,
            Notes = supplier.Notes,
            ThumbnailUrl = supplier.ThumbnailUrl,
            FullImageUrl = supplier.FullImageUrl,
            DateCreated = supplier.DateCreated,
            Emails = supplier.Emails.Select(se => new SupplierEmailDto
            {
                Email = se.Email?.Address ?? string.Empty,
                EmailType = se.Email?.Type ?? default,
                IsPrimary = se.IsPrimary
            }).ToList(),
            Phones = supplier.Phones.Select(sp => new SupplierPhoneDto
            {
                PhoneNumber = sp.Phone?.Number ?? string.Empty,
                PhoneType = sp.Phone?.Type ?? default,
                IsPrimary = sp.IsPrimary
            }).ToList(),
            Locations = supplier.Locations.Select(sl => new SupplierLocationDto
            {
                AddressLine1 = sl.Location?.StreetAddress ?? string.Empty,
                AddressLine2 = null,
                City = sl.Location?.City?.Name ?? string.Empty,
                State = sl.Location?.City?.Region?.Name ?? null,
                PostalCode = sl.Location?.PostalCode,
                Country = sl.Location?.City?.Region?.Country?.Name ?? string.Empty,
                IsPrimary = sl.IsPrimary
            }).ToList()
        };
    }

    private async Task<Email> GetOrCreateEmailAsync(string address, EmailType type)
    {
        var trimmed = address.Trim();
        var email = await _uow.Repository<Email>().Query()
            .FirstOrDefaultAsync(e => e.Address == trimmed);

        if (email is null)
        {
            email = new Email
            {
                Address = trimmed,
                Type = type,
                IsVerified = false
            };
            await _uow.Repository<Email>().AddAsync(email);
            await _uow.SaveChangesAsync();
        }

        return email;
    }

    private async Task<Phone> GetOrCreatePhoneAsync(string number, PhoneType type)
    {
        var trimmed = number.Trim();
        var phone = await _uow.Repository<Phone>().Query()
            .FirstOrDefaultAsync(p => p.Number == trimmed);

        if (phone is null)
        {
            phone = new Phone
            {
                Number = trimmed,
                Type = type
            };
            await _uow.Repository<Phone>().AddAsync(phone);
            await _uow.SaveChangesAsync();
        }

        return phone;
    }

    private async Task<Location> GetOrCreateLocationAsync(CreateSupplierLocationRequest locReq)
    {
        var cityName = string.IsNullOrWhiteSpace(locReq.City) ? "Default" : locReq.City.Trim();
        var city = await _uow.Repository<City>().Query()
            .FirstOrDefaultAsync(c => c.Name == cityName);

        if (city is null)
        {
            var country = await _uow.Repository<Country>().Query().FirstOrDefaultAsync(c => c.Name == "Default");
            if (country is null)
            {
                country = new Country { Name = "Default" };
                await _uow.Repository<Country>().AddAsync(country);
                await _uow.SaveChangesAsync();
            }

            var region = await _uow.Repository<Region>().Query().FirstOrDefaultAsync(r => r.Name == "Default" && r.CountryId == country.CountryId);
            if (region is null)
            {
                region = new Region { Name = "Default", CountryId = country.CountryId };
                await _uow.Repository<Region>().AddAsync(region);
                await _uow.SaveChangesAsync();
            }

            city = new City { Name = cityName, RegionId = region.RegionId };
            await _uow.Repository<City>().AddAsync(city);
            await _uow.SaveChangesAsync();
        }

        var street = string.IsNullOrWhiteSpace(locReq.AddressLine1) ? "Default" : locReq.AddressLine1.Trim();
        var location = await _uow.Repository<Location>().Query()
            .FirstOrDefaultAsync(l =>
                l.StreetAddress == street &&
                l.CityId == city.CityId);

        if (location is null)
        {
            location = new Location
            {
                StreetAddress = street,
                PostalCode = locReq.PostalCode?.Trim(),
                CityId = city.CityId
            };
            await _uow.Repository<Location>().AddAsync(location);
            await _uow.SaveChangesAsync();
        }

        return location;
    }
}