using Microsoft.EntityFrameworkCore;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Customers;
using Store.Models.Entities;
using Store.Models.Entities.Contacts;
using Store.Models.Enums;
using Store.Models.Interfaces;
using Store.Models.Interfaces.Services;

namespace Store.DbServices.Services;

public class CustomerService : ICustomerService
{
    private readonly IUnitOfWork _uow;

    public CustomerService(IUnitOfWork uow) => _uow = uow;

    public async Task<CustomerDto?> GetByIdAsync(Guid customerId, CancellationToken ct = default)
    {
        var customer = await _uow.Repository<Customer>().Query()
            .AsNoTracking()
            .Include(c => c.Phones).ThenInclude(cp => cp.Phone)
            .Include(c => c.Emails).ThenInclude(ce => ce.Email)
            .Include(c => c.LoyaltyAccount)
            .Include(c => c.Invoices)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId, ct);

        return customer is null ? null : MapToDto(customer);
    }

    public async Task<PagedResult<CustomerDto>> GetAllAsync(PagedRequest request, CancellationToken ct = default)
    {
        var query = _uow.Repository<Customer>().Query().AsNoTracking()
            .Include(c => c.Phones).ThenInclude(cp => cp.Phone)
            .Include(c => c.Emails).ThenInclude(ce => ce.Email)
            .Include(c => c.LoyaltyAccount)
            .Include(c => c.Invoices)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            var search = request.SearchTerm.Trim();
            var isGuid = Guid.TryParse(search, out var searchGuid);

            query = query.Where(c =>
                c.FirstName.Contains(search) ||
                c.LastName.Contains(search) ||
                (c.MiddleName != null && c.MiddleName.Contains(search)) ||
                c.Phones.Any(p => p.Phone.Number.Contains(search)) ||
                c.Emails.Any(e => e.Email.Address.Contains(search)) ||
                (isGuid && c.CustomerId == searchGuid));
        }

        var total = await query.CountAsync(ct);
        var customers = await query
            .OrderBy(c => c.LastName).ThenBy(c => c.FirstName)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(ct);

        var items = customers.Select(MapToDto).ToList();

        return new PagedResult<CustomerDto>(items, total, request.Page, request.PageSize);
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerRequest request, CancellationToken ct = default)
    {
        var customer = new Customer
        {
            CustomerId = Guid.NewGuid(),
            FirstName = request.FirstName.Trim(),
            MiddleName = request.MiddleName?.Trim(),
            LastName = request.LastName.Trim(),
            Gender = request.Gender,
            DateOfBirth = request.DateOfBirth,
            Notes = request.Notes?.Trim(),
            Segment = request.Segment,
            ThumbnailUrl = request.ThumbnailUrl?.Trim(),
            FullImageUrl = request.FullImageUrl?.Trim()
        };

        if (!string.IsNullOrWhiteSpace(request.Phone))
        {
            var phone = await GetOrCreatePhoneAsync(request.Phone.Trim(), PhoneType.Mobile, ct);
            customer.Phones.Add(new CustomerPhone
            {
                CustomerId = customer.CustomerId,
                PhoneId = phone.PhoneId,
                IsPrimary = true
            });
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            var email = await GetOrCreateEmailAsync(request.Email.Trim(), EmailType.Personal, ct);
            customer.Emails.Add(new CustomerEmail
            {
                CustomerId = customer.CustomerId,
                EmailId = email.EmailId,
                IsPrimary = true
            });
        }

        // Auto-create Loyalty Account
        customer.LoyaltyAccount = new CustomerLoyaltyAccount
        {
            CustomerId = customer.CustomerId,
            Points = 0,
            Tier = LoyaltyTier.Bronze
        };

        await _uow.Repository<Customer>().AddAsync(customer, ct);
        await _uow.SaveChangesAsync(ct);

        return MapToDto(customer);
    }

    public async Task<CustomerDto?> UpdateAsync(Guid customerId, UpdateCustomerRequest request, CancellationToken ct = default)
    {
        var customer = await _uow.Repository<Customer>().Query()
            .Include(c => c.Phones).ThenInclude(cp => cp.Phone)
            .Include(c => c.Emails).ThenInclude(ce => ce.Email)
            .Include(c => c.LoyaltyAccount)
            .Include(c => c.Invoices)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId, ct);

        if (customer is null) return null;

        if (!string.IsNullOrWhiteSpace(request.FirstName)) customer.FirstName = request.FirstName.Trim();
        if (!string.IsNullOrWhiteSpace(request.LastName)) customer.LastName = request.LastName.Trim();
        if (request.MiddleName is not null) customer.MiddleName = request.MiddleName.Trim();
        if (request.Gender.HasValue) customer.Gender = request.Gender.Value;
        if (request.DateOfBirth.HasValue) customer.DateOfBirth = request.DateOfBirth;
        if (request.Notes is not null) customer.Notes = request.Notes.Trim();
        if (request.Segment.HasValue) customer.Segment = request.Segment.Value;
        if (request.ThumbnailUrl != null) customer.ThumbnailUrl = request.ThumbnailUrl.Trim();
        if (request.FullImageUrl != null) customer.FullImageUrl = request.FullImageUrl.Trim();

        // Update Phone
        if (request.Phone is not null)
        {
            var trimmedPhone = request.Phone.Trim();
            if (string.IsNullOrEmpty(trimmedPhone))
            {
                customer.Phones.Clear();
            }
            else
            {
                var phone = await GetOrCreatePhoneAsync(trimmedPhone, PhoneType.Mobile, ct);
                var primaryPhone = customer.Phones.FirstOrDefault(p => p.IsPrimary) ?? customer.Phones.FirstOrDefault();
                if (primaryPhone != null)
                {
                    primaryPhone.PhoneId = phone.PhoneId;
                    primaryPhone.IsPrimary = true;
                }
                else
                {
                    customer.Phones.Add(new CustomerPhone
                    {
                        CustomerId = customer.CustomerId,
                        PhoneId = phone.PhoneId,
                        IsPrimary = true
                    });
                }
            }
        }

        // Update Email
        if (request.Email is not null)
        {
            var trimmedEmail = request.Email.Trim();
            if (string.IsNullOrEmpty(trimmedEmail))
            {
                customer.Emails.Clear();
            }
            else
            {
                var email = await GetOrCreateEmailAsync(trimmedEmail, EmailType.Personal, ct);
                var primaryEmail = customer.Emails.FirstOrDefault(e => e.IsPrimary) ?? customer.Emails.FirstOrDefault();
                if (primaryEmail != null)
                {
                    primaryEmail.EmailId = email.EmailId;
                    primaryEmail.IsPrimary = true;
                }
                else
                {
                    customer.Emails.Add(new CustomerEmail
                    {
                        CustomerId = customer.CustomerId,
                        EmailId = email.EmailId,
                        IsPrimary = true
                    });
                }
            }
        }

        _uow.Repository<Customer>().Update(customer);
        await _uow.SaveChangesAsync(ct);

        return MapToDto(customer);
    }

    public async Task<bool> DeleteAsync(Guid customerId, CancellationToken ct = default)
    {
        var customer = await _uow.Repository<Customer>().Query()
            .Include(c => c.Phones)
            .Include(c => c.Emails)
            .Include(c => c.LoyaltyAccount)
            .FirstOrDefaultAsync(c => c.CustomerId == customerId, ct);

        if (customer is null) return false;

        _uow.Repository<Customer>().Remove(customer);
        await _uow.SaveChangesAsync(ct);
        return true;
    }

    private async Task<Phone> GetOrCreatePhoneAsync(string number, PhoneType type, CancellationToken ct)
    {
        var phone = await _uow.Repository<Phone>().Query()
            .FirstOrDefaultAsync(p => p.Number == number.Trim(), ct);

        if (phone is null)
        {
            phone = new Phone
            {
                Number = number.Trim(),
                Type = type
            };
            await _uow.Repository<Phone>().AddAsync(phone, ct);
            await _uow.SaveChangesAsync(ct);
        }

        return phone;
    }

    private async Task<Email> GetOrCreateEmailAsync(string address, EmailType type, CancellationToken ct)
    {
        var email = await _uow.Repository<Email>().Query()
            .FirstOrDefaultAsync(e => e.Address == address.Trim(), ct);

        if (email is null)
        {
            email = new Email
            {
                Address = address.Trim(),
                Type = type,
                IsVerified = false
            };
            await _uow.Repository<Email>().AddAsync(email, ct);
            await _uow.SaveChangesAsync(ct);
        }

        return email;
    }

    private static CustomerDto MapToDto(Customer c)
    {
        var primaryPhone = c.Phones?.FirstOrDefault(p => p.IsPrimary)?.Phone?.Number
                           ?? c.Phones?.FirstOrDefault()?.Phone?.Number;

        var primaryEmail = c.Emails?.FirstOrDefault(e => e.IsPrimary)?.Email?.Address
                           ?? c.Emails?.FirstOrDefault()?.Email?.Address;

        var paidInvoices = c.Invoices?.Where(i => i.IsPaid).ToList() ?? new List<Invoice>();
        var unpaidInvoices = c.Invoices?.Where(i => !i.IsPaid).ToList() ?? new List<Invoice>();

        var totalSpend = paidInvoices.Sum(i => i.TotalAmount);
        var outstandingDebt = unpaidInvoices.Sum(i => i.TotalAmount);
        var totalOrders = c.Invoices?.Count ?? 0;
        var lastOrder = c.Invoices?.OrderByDescending(i => i.DateCreated).FirstOrDefault()?.DateCreated;

        return new CustomerDto
        {
            CustomerId = c.CustomerId,
            FirstName = c.FirstName,
            MiddleName = c.MiddleName,
            LastName = c.LastName,
            Gender = c.Gender,
            DateOfBirth = c.DateOfBirth,
            PrimaryPhone = primaryPhone,
            PrimaryEmail = primaryEmail,
            Notes = c.Notes,
            ThumbnailUrl = c.ThumbnailUrl,
            FullImageUrl = c.FullImageUrl,
            Segment = c.Segment,
            LoyaltyTier = c.LoyaltyAccount?.Tier ?? LoyaltyTier.Bronze,
            LoyaltyPoints = c.LoyaltyAccount?.Points ?? 0,
            LifetimeValue = totalSpend,
            TotalOrders = totalOrders,
            OutstandingBalance = outstandingDebt,
            LastOrderDate = lastOrder,
            DateCreated = c.DateCreated
        };
    }
}
