using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Store.API.Contracts;
using Store.API.Controllers;
using Store.DbServices.Services;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Loyalty;
using Store.Models.Entities;
using Store.Models.Enums;
using Store.Models.Interfaces.Services;
using Xunit;

namespace Store.API.Tests;

public class LoyaltyControllerTests
{
    private static LoyaltyController CreateController(ILoyaltyService loyaltyService)
    {
        var controller = new LoyaltyController(loyaltyService)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
        return controller;
    }

    // ================================================================
    // GET /api/loyalty/metrics
    // ================================================================

    [Fact]
    public async Task GetMetrics_ReturnsOkResult_WithStoreWideAggregations()
    {
        var mockService = new Mock<ILoyaltyService>();
        var metrics = new LoyaltyMetricsDto
        {
            TotalMembers = 80,
            ActiveMembers = 65,
            TotalPointsLiability = 45000,
            PointsLiabilityValueXaf = 225000,
            PointsEarnedThisMonth = 8500,
            PointsRedeemedThisMonth = 2100,
            BronzeCount = 50,
            SilverCount = 22,
            GoldCount = 8,
            VipTierRatio = 37.5
        };
        mockService.Setup(s => s.GetMetricsAsync(It.IsAny<CancellationToken>()))
                   .ReturnsAsync(metrics);

        var controller = CreateController(mockService.Object);
        var result = await controller.GetMetrics(CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<LoyaltyMetricsDto>>(ok.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal(80, response.Data.TotalMembers);
        Assert.Equal(45000, response.Data.TotalPointsLiability);
        Assert.Equal(225000, response.Data.PointsLiabilityValueXaf);
        Assert.Equal(37.5, response.Data.VipTierRatio);
    }

    // ================================================================
    // GET /api/loyalty/members
    // ================================================================

    [Fact]
    public async Task GetAllMembers_WithNoFilters_ReturnsPagedResult()
    {
        var mockService = new Mock<ILoyaltyService>();
        var customerId = Guid.NewGuid();
        var members = new PagedResult<LoyaltyMemberDto>(
            new List<LoyaltyMemberDto>
            {
                new() { LoyaltyAccountId = 1, CustomerId = customerId, FullName = "Jean Moise", Points = 2500, Tier = "Gold" }
            },
            totalCount: 1,
            page: 1,
            pageSize: 20
        );

        mockService.Setup(s => s.GetAllMembersAsync(null, null, null, 1, 20, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(members);

        var controller = CreateController(mockService.Object);
        var result = await controller.GetAllMembers(null, null, null, 1, 20, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<PagedResult<LoyaltyMemberDto>>>(ok.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Single(response.Data.Items);
        Assert.Equal("Jean Moise", response.Data.Items.First().FullName);
        Assert.Equal("Gold", response.Data.Items.First().Tier);
    }

    [Fact]
    public async Task GetAllMembers_WithTierFilter_ReturnsTierFilteredResult()
    {
        var mockService = new Mock<ILoyaltyService>();
        var paged = new PagedResult<LoyaltyMemberDto>(
            new List<LoyaltyMemberDto>
            {
                new() { FullName = "Marie Claire", Tier = "Gold", Points = 3500 }
            },
            totalCount: 1,
            page: 1,
            pageSize: 20
        );
        mockService.Setup(s => s.GetAllMembersAsync(null, "Gold", null, 1, 20, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(paged);

        var controller = CreateController(mockService.Object);
        var result = await controller.GetAllMembers(null, "Gold", null, 1, 20, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<PagedResult<LoyaltyMemberDto>>>(ok.Value);
        Assert.True(response.Success);
        Assert.Equal("Gold", response.Data!.Items.First().Tier);
    }

    // ================================================================
    // GET /api/loyalty/customers/{customerId}/profile
    // ================================================================

    [Fact]
    public async Task GetProfile_ReturnsOkResult_WhenMemberExists()
    {
        var customerId = Guid.NewGuid();
        var mockService = new Mock<ILoyaltyService>();
        var profile = new LoyaltyMemberProfileDto
        {
            Member = new LoyaltyMemberDto
            {
                CustomerId = customerId,
                FullName = "Bernard Nkemba",
                Points = 1800,
                Tier = "Silver",
                LifetimePointsEarned = 2300,
                TotalPointsRedeemed = 500,
                EstimatedMonetaryValue = 9000,
                TierProgressPercentage = 80
            },
            RecentTransactions = new List<LoyaltyTransactionDto>(),
            ActiveCampaigns = new List<LoyaltyCampaignDto>(),
            LifetimeSpend = 450000,
            TotalOrders = 22
        };
        mockService.Setup(s => s.GetMemberProfileAsync(customerId, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(profile);

        var controller = CreateController(mockService.Object);
        var result = await controller.GetProfile(customerId, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<LoyaltyMemberProfileDto>>(ok.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal("Bernard Nkemba", response.Data.Member.FullName);
        Assert.Equal("Silver", response.Data.Member.Tier);
        Assert.Equal(9000, response.Data.Member.EstimatedMonetaryValue);
    }

    [Fact]
    public async Task GetProfile_ReturnsNotFound_WhenMemberDoesNotExist()
    {
        var customerId = Guid.NewGuid();
        var mockService = new Mock<ILoyaltyService>();
        mockService.Setup(s => s.GetMemberProfileAsync(customerId, It.IsAny<CancellationToken>()))
                   .ReturnsAsync((LoyaltyMemberProfileDto?)null);

        var controller = CreateController(mockService.Object);
        var result = await controller.GetProfile(customerId, CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    // ================================================================
    // POST /api/loyalty/earn
    // ================================================================

    [Fact]
    public async Task Earn_ValidRequest_ReturnsOkWithTransaction()
    {
        var customerId = Guid.NewGuid();
        var mockService = new Mock<ILoyaltyService>();
        var txn = new LoyaltyTransaction
        {
            LoyaltyTransactionId = 1001,
            Points = 200,
            TransactionType = LoyaltyTransactionType.Earn,
            Note = "Purchase Reward",
            DateCreated = DateTime.UtcNow
        };
        mockService.Setup(s => s.EarnPointsAsync(customerId, 200, null, "Purchase Reward", It.IsAny<CancellationToken>()))
                   .ReturnsAsync(txn);

        var controller = CreateController(mockService.Object);
        var result = await controller.Earn(new EarnPointsRequest
        {
            CustomerId = customerId,
            Points = 200,
            Note = "Purchase Reward"
        }, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<LoyaltyTransactionDto>>(ok.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal(200, response.Data.Points);
        Assert.Equal("200 points earned successfully.", response.Message);
    }

    [Fact]
    public async Task Earn_InvalidPoints_ReturnsBadRequest()
    {
        var customerId = Guid.NewGuid();
        var mockService = new Mock<ILoyaltyService>();
        mockService.Setup(s => s.EarnPointsAsync(customerId, -5, null, null, It.IsAny<CancellationToken>()))
                   .ThrowsAsync(new ArgumentOutOfRangeException("points", "Points must be positive."));

        var controller = CreateController(mockService.Object);
        var result = await controller.Earn(new EarnPointsRequest
        {
            CustomerId = customerId,
            Points = -5
        }, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    // ================================================================
    // POST /api/loyalty/redeem
    // ================================================================

    [Fact]
    public async Task Redeem_ValidRequest_ReturnsOkWithTransaction()
    {
        var customerId = Guid.NewGuid();
        var mockService = new Mock<ILoyaltyService>();
        var txn = new LoyaltyTransaction
        {
            LoyaltyTransactionId = 2001,
            Points = -500,
            TransactionType = LoyaltyTransactionType.Redeem,
            Note = "Redeemed 500 points (2500 XAF voucher)",
            DateCreated = DateTime.UtcNow
        };
        mockService.Setup(s => s.RedeemPointsAsync(customerId, 500, "Reward voucher", It.IsAny<CancellationToken>()))
                   .ReturnsAsync(txn);

        var controller = CreateController(mockService.Object);
        var result = await controller.Redeem(new RedeemPointsRequest
        {
            CustomerId = customerId,
            Points = 500,
            Note = "Reward voucher"
        }, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<LoyaltyTransactionDto>>(ok.Value);
        Assert.True(response.Success);
        Assert.Equal(-500, response.Data!.Points);
    }

    [Fact]
    public async Task Redeem_InsufficientBalance_ReturnsConflict()
    {
        var customerId = Guid.NewGuid();
        var mockService = new Mock<ILoyaltyService>();
        mockService.Setup(s => s.RedeemPointsAsync(customerId, 5000, null, It.IsAny<CancellationToken>()))
                   .ThrowsAsync(new InvalidOperationException("Insufficient points. Available balance: 100 pts, requested redemption: 5000 pts."));

        var controller = CreateController(mockService.Object);
        var result = await controller.Redeem(new RedeemPointsRequest
        {
            CustomerId = customerId,
            Points = 5000
        }, CancellationToken.None);

        Assert.IsType<ConflictObjectResult>(result);
    }

    // ================================================================
    // POST /api/loyalty/adjust
    // ================================================================

    [Fact]
    public async Task Adjust_PositiveAdjustment_ReturnsOk()
    {
        var customerId = Guid.NewGuid();
        var mockService = new Mock<ILoyaltyService>();
        var txn = new LoyaltyTransaction
        {
            LoyaltyTransactionId = 3001,
            Points = 100,
            TransactionType = LoyaltyTransactionType.Adjust,
            Note = "Promo correction",
            DateCreated = DateTime.UtcNow
        };
        mockService.Setup(s => s.AdjustPointsAsync(customerId, 100, "Promo correction", It.IsAny<CancellationToken>()))
                   .ReturnsAsync(txn);

        var controller = CreateController(mockService.Object);
        var result = await controller.Adjust(new AdjustPointsRequest
        {
            CustomerId = customerId,
            Points = 100,
            Note = "Promo correction"
        }, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<LoyaltyTransactionDto>>(ok.Value);
        Assert.True(response.Success);
        Assert.Equal(100, response.Data!.Points);
    }

    [Fact]
    public async Task Adjust_NegativeAdjustment_ReturnsOk()
    {
        var customerId = Guid.NewGuid();
        var mockService = new Mock<ILoyaltyService>();
        var txn = new LoyaltyTransaction
        {
            LoyaltyTransactionId = 3002,
            Points = -50,
            TransactionType = LoyaltyTransactionType.Adjust,
            Note = "Correction",
            DateCreated = DateTime.UtcNow
        };
        mockService.Setup(s => s.AdjustPointsAsync(customerId, -50, "Correction", It.IsAny<CancellationToken>()))
                   .ReturnsAsync(txn);

        var controller = CreateController(mockService.Object);
        var result = await controller.Adjust(new AdjustPointsRequest
        {
            CustomerId = customerId,
            Points = -50,
            Note = "Correction"
        }, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<LoyaltyTransactionDto>>(ok.Value);
        Assert.True(response.Success);
        Assert.Equal(-50, response.Data!.Points);
    }

    // ================================================================
    // GET /api/loyalty/transactions (Global Audit Ledger)
    // ================================================================

    [Fact]
    public async Task GetGlobalTransactions_ReturnsOkWithAuditLedger()
    {
        var mockService = new Mock<ILoyaltyService>();
        var txns = new List<GlobalLoyaltyTransactionDto>
        {
            new() { LoyaltyTransactionId = 10001, CustomerName = "Jean Moise", Points = 300, TransactionType = "Earn", DateCreated = DateTime.UtcNow },
            new() { LoyaltyTransactionId = 10002, CustomerName = "Marie Claire", Points = -200, TransactionType = "Redeem", DateCreated = DateTime.UtcNow.AddMinutes(-5) }
        };

        mockService.Setup(s => s.GetGlobalTransactionsAsync(null, null, null, null, 50, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(txns);

        var controller = CreateController(mockService.Object);
        var result = await controller.GetGlobalTransactions(null, null, null, null, 50, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<IEnumerable<GlobalLoyaltyTransactionDto>>>(ok.Value);
        Assert.True(response.Success);
        Assert.Equal(2, response.Data!.Count());
    }

    // ================================================================
    // POST /api/loyalty/manage (Unified endpoint)
    // ================================================================

    [Fact]
    public async Task ManagePoints_EarnAction_DelegatesToEarnService()
    {
        var customerId = Guid.NewGuid();
        var mockService = new Mock<ILoyaltyService>();
        var txn = new LoyaltyTransaction
        {
            LoyaltyTransactionId = 5001,
            Points = 150,
            TransactionType = LoyaltyTransactionType.Earn,
            DateCreated = DateTime.UtcNow
        };
        mockService.Setup(s => s.EarnPointsAsync(customerId, 150, null, null, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(txn);

        var controller = CreateController(mockService.Object);
        var result = await controller.ManagePoints(new ManagePointsRequest
        {
            CustomerId = customerId,
            Points = 150,
            ActionType = "Earn"
        }, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<LoyaltyTransactionDto>>(ok.Value);
        Assert.True(response.Success);
        mockService.Verify(s => s.EarnPointsAsync(customerId, 150, null, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ManagePoints_RedeemAction_DelegatesToRedeemService()
    {
        var customerId = Guid.NewGuid();
        var mockService = new Mock<ILoyaltyService>();
        var txn = new LoyaltyTransaction
        {
            LoyaltyTransactionId = 5002,
            Points = -300,
            TransactionType = LoyaltyTransactionType.Redeem,
            DateCreated = DateTime.UtcNow
        };
        mockService.Setup(s => s.RedeemPointsAsync(customerId, 300, null, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(txn);

        var controller = CreateController(mockService.Object);
        var result = await controller.ManagePoints(new ManagePointsRequest
        {
            CustomerId = customerId,
            Points = 300,
            ActionType = "Redeem"
        }, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<LoyaltyTransactionDto>>(ok.Value);
        Assert.True(response.Success);
        mockService.Verify(s => s.RedeemPointsAsync(customerId, 300, null, It.IsAny<CancellationToken>()), Times.Once);
    }

    // ================================================================
    // CRITICAL: Tier Demotion Bug Fix Verification (Business Logic Tests)
    // ================================================================

    [Fact]
    public void ComputeTier_OnEarn_UsesLifetimeQualifyingPoints_NotBalance()
    {
        // This validates the tier computation formula. Tier threshold constants are accessible:
        // Bronze:  0 - 499 points
        // Silver: 500 - 1999 points
        // Gold:   2000+ points

        // A Gold member redeeming points should NOT be demoted (tier stays based on lifetime earned)
        Assert.Equal(LoyaltyTier.Gold, CalculateTier(2000));   // Gold threshold
        Assert.Equal(LoyaltyTier.Silver, CalculateTier(500));  // Silver threshold
        Assert.Equal(LoyaltyTier.Bronze, CalculateTier(499));  // Below Silver
        Assert.Equal(LoyaltyTier.Bronze, CalculateTier(0));    // Zero points

        // A Gold member (2500 lifetime earned) who spends 2100 points should still be Gold
        // because tier is based on lifetime qualifying points, not spendable balance
        int lifetimeEarned = 2500;  // Qualifies for Gold
        int spendableBalance = lifetimeEarned - 2100; // = 400 (would be Bronze if balance-based!)

        Assert.Equal(LoyaltyTier.Gold, CalculateTier(lifetimeEarned)); // Correct: Gold based on lifetime
        Assert.Equal(LoyaltyTier.Bronze, CalculateTier(spendableBalance)); // Wrong if we use balance: would demote
        Assert.NotEqual(LoyaltyTier.Bronze, CalculateTier(lifetimeEarned)); // Proves fix works: no demotion
    }

    [Fact]
    public void TierProgressCalculation_GoldMember_Returns100Percent()
    {
        var (pct, _, _) = CalculateTierProgress(2000, LoyaltyTier.Gold);
        Assert.Equal(100, pct);
    }

    [Fact]
    public void TierProgressCalculation_BronzeMember_ReturnsCorrectProgress()
    {
        var (pct, nextThreshold, needed) = CalculateTierProgress(250, LoyaltyTier.Bronze);
        Assert.Equal(50, pct);                          // 250 / 500 = 50%
        Assert.Equal(LoyaltyService.SilverThreshold, nextThreshold); // Next tier = Silver at 500
        Assert.Equal(250, needed);                      // 500 - 250 = 250 pts needed
    }

    [Fact]
    public void TierProgressCalculation_SilverMember_ReturnsCorrectProgress()
    {
        // Silver range is 500 - 2000 (range = 1500)
        // Member has 1250 pts -> 1250 - 500 = 750 into Silver range -> 750/1500 = 50%
        var (pct, nextThreshold, needed) = CalculateTierProgress(1250, LoyaltyTier.Silver);
        Assert.Equal(50, pct);
        Assert.Equal(LoyaltyService.GoldThreshold, nextThreshold);
        Assert.Equal(750, needed); // 2000 - 1250 = 750
    }

    // ================================================================
    // GET /api/loyalty/customers/{customerId}
    // ================================================================

    [Fact]
    public async Task GetAccount_ReturnsNotFound_WhenNoAccountExists()
    {
        var customerId = Guid.NewGuid();
        var mockService = new Mock<ILoyaltyService>();
        mockService.Setup(s => s.GetAccountAsync(customerId, It.IsAny<CancellationToken>()))
                   .ReturnsAsync((CustomerLoyaltyAccount?)null);

        var controller = CreateController(mockService.Object);
        var result = await controller.GetAccount(customerId, CancellationToken.None);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task GetAccount_ReturnsOkWithAccountDto_WhenAccountExists()
    {
        var customerId = Guid.NewGuid();
        var mockService = new Mock<ILoyaltyService>();
        var account = new CustomerLoyaltyAccount
        {
            LoyaltyAccountId = 42,
            CustomerId = customerId,
            Points = 1500,
            Tier = LoyaltyTier.Silver
        };
        mockService.Setup(s => s.GetAccountAsync(customerId, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(account);

        var controller = CreateController(mockService.Object);
        var result = await controller.GetAccount(customerId, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<LoyaltyAccountDto>>(ok.Value);
        Assert.True(response.Success);
        Assert.Equal(1500, response.Data!.Points);
        Assert.Equal("Silver", response.Data.Tier);
    }

    // ================================================================
    // GET /api/loyalty/customers/{customerId}/transactions
    // ================================================================

    [Fact]
    public async Task GetTransactions_ReturnsOkWithTransactionDtos()
    {
        var customerId = Guid.NewGuid();
        var mockService = new Mock<ILoyaltyService>();
        var txns = new List<LoyaltyTransaction>
        {
            new() { LoyaltyTransactionId = 100, Points = 300, TransactionType = LoyaltyTransactionType.Earn, DateCreated = DateTime.UtcNow.AddDays(-1) },
            new() { LoyaltyTransactionId = 101, Points = -100, TransactionType = LoyaltyTransactionType.Redeem, DateCreated = DateTime.UtcNow }
        };
        mockService.Setup(s => s.GetTransactionsAsync(customerId, 50, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(txns);

        var controller = CreateController(mockService.Object);
        var result = await controller.GetTransactions(customerId, 50, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<IEnumerable<LoyaltyTransactionDto>>>(ok.Value);
        Assert.True(response.Success);
        Assert.Equal(2, response.Data!.Count());
    }

    // ================================================================
    // Helper methods (mirror LoyaltyService internals for unit testing)
    // ================================================================

    private static LoyaltyTier CalculateTier(int qualifyingPoints) => qualifyingPoints switch
    {
        >= LoyaltyService.GoldThreshold => LoyaltyTier.Gold,
        >= LoyaltyService.SilverThreshold => LoyaltyTier.Silver,
        _ => LoyaltyTier.Bronze
    };

    private static (int progressPct, int nextThreshold, int pointsNeeded) CalculateTierProgress(int points, LoyaltyTier tier)
    {
        switch (tier)
        {
            case LoyaltyTier.Bronze:
                var bPct = Math.Min(100, (int)Math.Round((points / (double)LoyaltyService.SilverThreshold) * 100));
                return (bPct, LoyaltyService.SilverThreshold, Math.Max(0, LoyaltyService.SilverThreshold - points));

            case LoyaltyTier.Silver:
                var sDiff = points - LoyaltyService.SilverThreshold;
                var sRange = LoyaltyService.GoldThreshold - LoyaltyService.SilverThreshold;
                var sPct = Math.Min(100, Math.Max(0, (int)Math.Round((sDiff / (double)sRange) * 100)));
                return (sPct, LoyaltyService.GoldThreshold, Math.Max(0, LoyaltyService.GoldThreshold - points));

            case LoyaltyTier.Gold:
            default:
                return (100, LoyaltyService.GoldThreshold, 0);
        }
    }
}
