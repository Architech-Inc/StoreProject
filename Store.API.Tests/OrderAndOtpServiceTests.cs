using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Store.DbServices.Context;
using Store.DbServices.Services;
using Store.DbServices.UnitOfWork;
using Store.Models.DTOs.Orders;
using Store.Models.Entities;
using Store.Models.Enums;
using Store.Models.Interfaces;
using Xunit;

namespace Store.API.Tests;

/// <summary>
/// Wave 33 — Tests for <see cref="OrderService"/> and <see cref="OtpService"/> (GAP-13).
/// Verifies order creation, line cost calculation, inventory increment on receipt,
/// order cancellation, cryptographic OTP generation, and constant-time OTP validation.
/// </summary>
public class OrderAndOtpServiceTests : IDisposable
{
    private readonly StoreDbContext _context;
    private readonly IUnitOfWork _uow;
    private readonly OrderService _orderService;
    private readonly OtpService _otpService;

    public OrderAndOtpServiceTests()
    {
        var options = new DbContextOptionsBuilder<StoreDbContext>()
            .UseInMemoryDatabase(databaseName: $"OrderOtpTests_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        _context = new StoreDbContext(options);
        _uow = new UnitOfWork(_context);
        _orderService = new OrderService(_uow);

        var pepperOptions = Options.Create(new OtpPepperOptions
        {
            OtpPepper = "UNIT-TEST-PEPPER-AT-LEAST-32-BYTES-SECRET!!"
        });
        _otpService = new OtpService(_uow, pepperOptions);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    // ─── OrderService Tests ───────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_CalculatesLineTotalsAndCreatesPendingOrder()
    {
        // Arrange
        var supplier = new Supplier { SupplierId = Guid.NewGuid(), Name = "Best Beverage Co" };
        var item1 = new Item { ItemId = Guid.NewGuid(), Name = "Cola Can 330ml", CostPrice = 0.50m, InStock = 100, IsActive = true };
        var item2 = new Item { ItemId = Guid.NewGuid(), Name = "Orange Juice 1L", CostPrice = 1.20m, InStock = 50, IsActive = true };

        _context.Suppliers.Add(supplier);
        _context.Items.AddRange(item1, item2);
        await _context.SaveChangesAsync();

        var request = new CreateOrderRequest
        {
            SupplierId = supplier.SupplierId,
            Notes = "Urgent weekend stock",
            Lines = new List<CreateOrderLineRequest>
            {
                new() { ItemId = item1.ItemId, QuantityOrdered = 100 },
                new() { ItemId = item2.ItemId, QuantityOrdered = 50 }
            }
        };

        var actingUserId = Guid.NewGuid();

        // Act
        var order = await _orderService.CreateAsync(request, actingUserId);

        // Assert
        Assert.NotNull(order);
        Assert.Equal(OrderStatus.Pending, order.Status);
        Assert.Equal(supplier.SupplierId, order.SupplierId);
        Assert.Equal(110m, order.TotalAmount); // (100 * 0.50) + (50 * 1.20) = 50 + 60 = 110
        Assert.Equal(2, order.Lines.Count);
        Assert.Contains(order.Lines, l => l.ItemId == item1.ItemId && l.QuantityOrdered == 100 && l.LineTotal == 50m);
        Assert.Contains(order.Lines, l => l.ItemId == item2.ItemId && l.QuantityOrdered == 50 && l.LineTotal == 60m);
    }

    [Fact]
    public async Task ReceiveOrderAsync_IncrementsStockAndMarksStatusReceived()
    {
        // Arrange
        var supplier = new Supplier { SupplierId = Guid.NewGuid(), Name = "Bakery Supplies" };
        var item = new Item { ItemId = Guid.NewGuid(), Name = "Flour 25kg", CostPrice = 20m, InStock = 5, IsActive = true };

        var order = new ItemsOrder
        {
            ItemsOrderId = Guid.NewGuid(),
            OrderNumber = "PO-2026-TEST",
            SupplierId = supplier.SupplierId,
            Status = OrderStatus.Pending,
            TotalAmount = 200m,
            DateCreated = DateTime.UtcNow
        };

        var line = new OrderItem
        {
            OrderItemId = 1,
            ItemsOrderId = order.ItemsOrderId,
            ItemId = item.ItemId,
            ItemName = item.Name,
            QuantityOrdered = 10,
            QuantityReceived = 0,
            UnitCost = 20m,
            LineTotal = 200m
        };

        _context.Suppliers.Add(supplier);
        _context.Items.Add(item);
        _context.ItemsOrders.Add(order);
        _context.OrderItems.Add(line);
        await _context.SaveChangesAsync();

        // Act
        var success = await _orderService.ReceiveOrderAsync(order.ItemsOrderId);

        // Assert
        Assert.True(success);

        var updatedOrder = await _context.ItemsOrders.FindAsync(order.ItemsOrderId);
        Assert.NotNull(updatedOrder);
        Assert.Equal(OrderStatus.Received, updatedOrder.Status);

        var updatedItem = await _context.Items.FindAsync(item.ItemId);
        Assert.NotNull(updatedItem);
        Assert.Equal(15, updatedItem.InStock); // 5 + 10 = 15

        var updatedLine = await _context.OrderItems.FindAsync(line.OrderItemId);
        Assert.NotNull(updatedLine);
        Assert.Equal(10, updatedLine.QuantityReceived);
    }

    [Fact]
    public async Task CancelOrderAsync_MarksCancelled_WhenNotReceived()
    {
        // Arrange
        var order = new ItemsOrder
        {
            ItemsOrderId = Guid.NewGuid(),
            OrderNumber = "PO-CANCEL-01",
            Status = OrderStatus.Pending,
            DateCreated = DateTime.UtcNow
        };

        _context.ItemsOrders.Add(order);
        await _context.SaveChangesAsync();

        // Act
        var success = await _orderService.CancelOrderAsync(order.ItemsOrderId);

        // Assert
        Assert.True(success);
        var updated = await _context.ItemsOrders.FindAsync(order.ItemsOrderId);
        Assert.NotNull(updated);
        Assert.Equal(OrderStatus.Cancelled, updated.Status);
    }

    [Fact]
    public async Task CancelOrderAsync_ReturnsFalse_WhenOrderAlreadyReceived()
    {
        // Arrange
        var order = new ItemsOrder
        {
            ItemsOrderId = Guid.NewGuid(),
            OrderNumber = "PO-RECEIVED-01",
            Status = OrderStatus.Received,
            DateCreated = DateTime.UtcNow
        };

        _context.ItemsOrders.Add(order);
        await _context.SaveChangesAsync();

        // Act
        var success = await _orderService.CancelOrderAsync(order.ItemsOrderId);

        // Assert
        Assert.False(success);
        var updated = await _context.ItemsOrders.FindAsync(order.ItemsOrderId);
        Assert.NotNull(updated);
        Assert.Equal(OrderStatus.Received, updated.Status);
    }

    // ─── OtpService Tests ─────────────────────────────────────────────────────

    [Fact]
    public async Task GenerateAsync_CreatesHashedOtp_AndInvalidatesPriorActiveOtps()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // First generation
        var code1 = await _otpService.GenerateAsync(userId, OtpPurpose.TwoFactorLogin);
        Assert.NotNull(code1);
        Assert.Equal(6, code1.Length);

        // Second generation for same purpose
        var code2 = await _otpService.GenerateAsync(userId, OtpPurpose.TwoFactorLogin);
        Assert.NotNull(code2);
        Assert.Equal(6, code2.Length);

        // Assert
        var otps = await _context.Otps.Where(o => o.UserId == userId).ToListAsync();
        Assert.Equal(2, otps.Count);

        var firstOtp = otps.First();
        var secondOtp = otps.Last();

        Assert.True(firstOtp.IsUsed); // invalidated
        Assert.False(secondOtp.IsUsed); // active
        Assert.NotEqual(code2, secondOtp.CodeHash); // securely hashed, not plaintext
    }

    [Fact]
    public async Task ValidateAsync_ReturnsTrue_ForValidCode_AndMarksOtpUsed()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var code = await _otpService.GenerateAsync(userId, OtpPurpose.PasswordReset);

        // Act
        var isValid = await _otpService.ValidateAsync(userId, code, OtpPurpose.PasswordReset);

        // Assert
        Assert.True(isValid);

        var otp = await _context.Otps.FirstAsync(o => o.UserId == userId);
        Assert.True(otp.IsUsed);

        // Second validation must fail because it was marked used
        var isSecondValid = await _otpService.ValidateAsync(userId, code, OtpPurpose.PasswordReset);
        Assert.False(isSecondValid);
    }

    [Fact]
    public async Task ValidateAsync_ReturnsFalse_ForWrongCode()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await _otpService.GenerateAsync(userId, OtpPurpose.EmailVerification);

        // Act
        var isValid = await _otpService.ValidateAsync(userId, "000000", OtpPurpose.EmailVerification);

        // Assert
        Assert.False(isValid);
    }
}
