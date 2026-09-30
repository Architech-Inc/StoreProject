using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Store.API.Controllers;
using Store.Models.DTOs.Common;
using Store.Models.DTOs.Operations;
using Store.Models.Interfaces.Services;
using Xunit;

namespace Store.API.Tests;

public class AdminRoleMatrixControllerTests
{
    private static AdminRoleMatrixController CreateController(IStoreOperationsService opsService)
    {
        var controller = new AdminRoleMatrixController(opsService)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
        return controller;
    }

    [Fact]
    public async Task UpdatePermission_ReturnsBadRequest_WhenRequestIsNull()
    {
        var mockOps = new Mock<IStoreOperationsService>();
        var controller = CreateController(mockOps.Object);

        var result = await controller.UpdatePermission(null!, CancellationToken.None);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<ApiErrorResponse>(badRequestResult.Value);
        Assert.Equal(ErrorCode.InvalidRequest, response.Code);
    }

    [Theory]
    [InlineData(0, "inventory.read")]
    [InlineData(-1, "inventory.read")]
    [InlineData(1, "")]
    [InlineData(1, "   ")]
    public async Task UpdatePermission_ReturnsBadRequest_WhenParametersAreInvalid(int roleId, string permissionKey)
    {
        var mockOps = new Mock<IStoreOperationsService>();
        var controller = CreateController(mockOps.Object);

        var req = new UpdateRolePermissionRequest
        {
            RoleId = roleId,
            PermissionKey = permissionKey,
            IsAllowed = true
        };

        var result = await controller.UpdatePermission(req, CancellationToken.None);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<ApiErrorResponse>(badRequestResult.Value);
        Assert.Equal(ErrorCode.InvalidRequest, response.Code);
    }

    [Fact]
    public async Task UpdatePermission_ReturnsOk_WhenValid()
    {
        var mockOps = new Mock<IStoreOperationsService>();
        var expectedDto = new RolePermissionDto
        {
            RoleId = 2,
            RoleName = "Cashier",
            PermissionKey = "pos.checkout",
            IsAllowed = true
        };
        mockOps.Setup(o => o.UpdateRolePermissionAsync(It.IsAny<UpdateRolePermissionRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        var controller = CreateController(mockOps.Object);
        var req = new UpdateRolePermissionRequest
        {
            RoleId = 2,
            PermissionKey = "pos.checkout",
            IsAllowed = true
        };

        var result = await controller.UpdatePermission(req, CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<RolePermissionDto>>(okResult.Value);
        Assert.True(response.Success);
        Assert.NotNull(response.Data);
        Assert.Equal("pos.checkout", response.Data.PermissionKey);
    }

    [Fact]
    public void UpdateRolePermissionRequest_ValidationAttributes_RejectInvalidModel()
    {
        var invalidReq = new UpdateRolePermissionRequest
        {
            RoleId = 0,
            PermissionKey = "",
            IsAllowed = true
        };

        var validationContext = new ValidationContext(invalidReq);
        var validationResults = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(invalidReq, validationContext, validationResults, validateAllProperties: true);

        Assert.False(isValid);
        Assert.Contains(validationResults, v => v.MemberNames.Contains(nameof(UpdateRolePermissionRequest.RoleId)));
        Assert.Contains(validationResults, v => v.MemberNames.Contains(nameof(UpdateRolePermissionRequest.PermissionKey)));
    }
}
