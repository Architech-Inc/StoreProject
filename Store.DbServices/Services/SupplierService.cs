using Microsoft.EntityFrameworkCore;
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

    public async Task<List<SupplierDto>> GetAllAsync()
    {
        var suppliers = await _uow.Repository<Supplier>().Query()
            .AsNoTracking()
            .Include(s => s.Emails)
            .Include(s => s.Phones)
            .Include(s => s.Locations)
            .OrderBy(s => s.Name)
            .ToListAsync();

        return suppliers.Select(MapToDto).ToList();
    }

    public async Task<SupplierDto?> GetByIdAsync(Guid id)
    {
        var supplier = await _uow.Repository<Supplier>().Query()
            .AsNoTracking()
            .Include(s => s.Emails)
            .Include(s => s.Phones)
            .Include(s => s.Locations)
            .FirstOrDefaultAsync(s => s.SupplierId == id);

        return supplier is null ? null : MapToDto(supplier);
    }

    public async Task<SupplierDto> CreateAsync(CreateSupplierRequest request, Guid createdByUserId)
    {
        var supplier = new Supplier
        {
            SupplierId = Guid.NewGuid(),
            Name = request.Name.Trim(),
            RegistrationNumber = request.RegistrationNumber?.Trim(),
            Notes = request.Notes?.Trim(),
            ImagePath = request.ImagePath?.Trim()
        };

        // Create emails
        if (request.Emails is not null)
        {
            foreach (var emailReq in request.Emails)
            {
                var email = await GetOrCreateEmailAsync(emailReq.Email, emailReq.EmailType);
                supplier.Emails.Add(new SupplierEmail
                {
                    EmailId = email.EmailId,
                    IsPrimary = emailReq.IsPrimary
                });
            }
        }

        // Create phones
        if (request.Phones is not null)
        {
            foreach (var phoneReq in request.Phones)
            {
                var phone = await GetOrCreatePhoneAsync(phoneReq.PhoneNumber, phoneReq.PhoneType);
                supplier.Phones.Add(new SupplierPhone
                {
                    PhoneId = phone.PhoneId,
                    IsPrimary = phoneReq.IsPrimary
                });
            }
        }

        // Create locations
        if (request.Locations is not null)
        {
            foreach (var locReq in request.Locations)
            {
                var location = await GetOrCreateLocationAsync(locReq);
                supplier.Locations.Add(new SupplierLocation
                {
                    LocationId = location.LocationId,
                    IsPrimary = locReq.IsPrimary
                });
            }
        }

        await _uow.Repository<Supplier>().AddAsync(supplier);
        await _uow.SaveChangesAsync();
        return MapToDto(supplier);
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

        if (request.Emails is not null)
        {
            supplier.Emails.Clear();
            foreach (var emailReq in request.Emails)
            {
                var email = await GetOrCreateEmailAsync(emailReq.Email, emailReq.EmailType);
                supplier.Emails.Add(new SupplierEmail
                {
                    EmailId = email.EmailId,
                    IsPrimary = emailReq.IsPrimary
                });
            }
        }

        if (request.Phones is not null)
        {
            supplier.Phones.Clear();
            foreach (var phoneReq in request.Phones)
            {
                var phone = await GetOrCreatePhoneAsync(phoneReq.PhoneNumber, phoneReq.PhoneType);
                supplier.Phones.Add(new SupplierPhone
                {
                    PhoneId = phone.PhoneId,
                    IsPrimary = phoneReq.IsPrimary
                });
            }
        }

        if (request.Locations is not null)
        {
            supplier.Locations.Clear();
            foreach (var locReq in request.Locations)
            {
                var location = await GetOrCreateLocationAsync(locReq);
                supplier.Locations.Add(new SupplierLocation
                {
                    LocationId = location.LocationId,
                    IsPrimary = locReq.IsPrimary
                });
            }
        }

        _uow.Repository<Supplier>().Update(supplier);
        await _uow.SaveChangesAsync();
        return MapToDto(supplier);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var supplier = await _uow.Repository<Supplier>().GetByIdAsync(id);
        if (supplier is null) return false;

        // Check for related orders
        var hasOrders = await _uow.Repository<ItemsOrder>().ExistsAsync(o => o.SupplierId == id);
        if (hasOrders) return false;

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
            ImagePath = supplier.ImagePath,
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
        var email = await _uow.Repository<Email>().Query()
            .FirstOrDefaultAsync(e => e.Address == address.Trim());

        if (email is null)
        {
            email = new Email
            {
                Address = address.Trim(),
                Type = type,
                IsVerified = false
            };
            await _uow.Repository<Email>().AddAsync(email);
        }

        return email;
    }

    private async Task<Phone> GetOrCreatePhoneAsync(string number, PhoneType type)
    {
        var phone = await _uow.Repository<Phone>().Query()
            .FirstOrDefaultAsync(p => p.Number == number.Trim());

        if (phone is null)
        {
            phone = new Phone
            {
                Number = number.Trim(),
                Type = type
            };
            await _uow.Repository<Phone>().AddAsync(phone);
        }

        return phone;
    }

    private async Task<Location> GetOrCreateLocationAsync(CreateSupplierLocationRequest locReq)
    {
        // Find or create city
        var city = await _uow.Repository<City>().Query()
            .FirstOrDefaultAsync(c => c.Name == locReq.City.Trim());

        if (city is null)
        {
            // For simplicity, create a default country/region if none exists.
            var country = await _uow.Repository<Country>().Query().FirstOrDefaultAsync(c => c.Name == "Default");
            if (country is null)
            {
                country = new Country { Name = "Default" };
                await _uow.Repository<Country>().AddAsync(country);
            }

            var region = await _uow.Repository<Region>().Query().FirstOrDefaultAsync(r => r.Name == "Default" && r.CountryId == country.CountryId);
            if (region is null)
            {
                region = new Region { Name = "Default", CountryId = country.CountryId };
                await _uow.Repository<Region>().AddAsync(region);
            }

            city = new City { Name = locReq.City.Trim(), RegionId = region.RegionId };
            await _uow.Repository<City>().AddAsync(city);
        }

        var location = await _uow.Repository<Location>().Query()
            .FirstOrDefaultAsync(l =>
                l.StreetAddress == locReq.AddressLine1.Trim() &&
                l.CityId == city.CityId);

        if (location is null)
        {
            location = new Location
            {
                StreetAddress = locReq.AddressLine1.Trim(),
                PostalCode = locReq.PostalCode?.Trim(),
                CityId = city.CityId
            };
            await _uow.Repository<Location>().AddAsync(location);
        }

        return location;
    }
}