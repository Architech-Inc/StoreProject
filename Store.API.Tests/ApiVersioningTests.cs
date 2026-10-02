using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Store.API.Controllers;
using Store.API.Infrastructure.Conventions;
using Xunit;

namespace Store.API.Tests;

public class ApiVersioningTests
{
    [Fact]
    public void Apply_DuplicatesApiRoutesToV1()
    {
        // Arrange
        var convention = new ApiVersioningRouteConvention("v1");
        var appModel = new ApplicationModel();

        var controllerType = typeof(ItemController);
        var controllerModel = new ControllerModel(controllerType.GetTypeInfo(), new List<object>())
        {
            ControllerName = "Item"
        };
        controllerModel.Selectors.Add(new SelectorModel
        {
            AttributeRouteModel = new AttributeRouteModel(new RouteAttribute("api/item"))
        });
        controllerModel.Selectors.Add(new SelectorModel
        {
            AttributeRouteModel = new AttributeRouteModel(new RouteAttribute("api/items"))
        });
        appModel.Controllers.Add(controllerModel);

        // Act
        convention.Apply(appModel);

        // Assert
        var templates = controllerModel.Selectors
            .Select(s => s.AttributeRouteModel?.Template)
            .Where(t => t != null)
            .ToList();

        Assert.Contains("api/item", templates);
        Assert.Contains("api/items", templates);
        Assert.Contains("api/v1/item", templates);
        Assert.Contains("api/v1/items", templates);
        Assert.Equal(4, templates.Count);
    }

    [Fact]
    public void Apply_DoesNotDuplicate_AlreadyVersionedRoutes()
    {
        // Arrange
        var convention = new ApiVersioningRouteConvention("v1");
        var appModel = new ApplicationModel();

        var controllerModel = new ControllerModel(typeof(ItemController).GetTypeInfo(), new List<object>())
        {
            ControllerName = "Item"
        };
        controllerModel.Selectors.Add(new SelectorModel
        {
            AttributeRouteModel = new AttributeRouteModel(new RouteAttribute("api/v1/items"))
        });
        controllerModel.Selectors.Add(new SelectorModel
        {
            AttributeRouteModel = new AttributeRouteModel(new RouteAttribute("api/v{version:apiVersion}/items"))
        });
        appModel.Controllers.Add(controllerModel);

        // Act
        convention.Apply(appModel);

        // Assert
        Assert.Equal(2, controllerModel.Selectors.Count);
    }

    [Fact]
    public void Apply_Ignores_NonApiRoutes()
    {
        // Arrange
        var convention = new ApiVersioningRouteConvention("v1");
        var appModel = new ApplicationModel();

        var controllerModel = new ControllerModel(typeof(ItemController).GetTypeInfo(), new List<object>())
        {
            ControllerName = "Health"
        };
        controllerModel.Selectors.Add(new SelectorModel
        {
            AttributeRouteModel = new AttributeRouteModel(new RouteAttribute("health"))
        });
        appModel.Controllers.Add(controllerModel);

        // Act
        convention.Apply(appModel);

        // Assert
        Assert.Single(controllerModel.Selectors);
        Assert.Equal("health", controllerModel.Selectors[0].AttributeRouteModel?.Template);
    }

    [Theory]
    [InlineData(typeof(AuthController), "api/[controller]", "api/v1/[controller]")]
    [InlineData(typeof(BranchController), "api/admin/branches", "api/v1/admin/branches")]
    [InlineData(typeof(CashVarianceController), "api/cash/variances", "api/v1/cash/variances")]
    [InlineData(typeof(CashManagementController), "api/cash", "api/v1/cash")]
    [InlineData(typeof(PurchaseOrdersController), "api/purchase-orders", "api/v1/purchase-orders")]
    [InlineData(typeof(PasswordRecoveryController), "api/auth/recovery", "api/v1/auth/recovery")]
    public void Apply_GeneratesV1Routes_ForControllers(Type controllerType, string legacyPrefix, string expectedV1)
    {
        // Arrange
        var convention = new ApiVersioningRouteConvention("v1");
        var appModel = new ApplicationModel();

        var controllerModel = new ControllerModel(controllerType.GetTypeInfo(), new List<object>())
        {
            ControllerName = controllerType.Name.Replace("Controller", "")
        };

        // Extract route attributes on the controller type
        var routeAttributes = controllerType.GetCustomAttributes<RouteAttribute>();
        foreach (var attr in routeAttributes)
        {
            controllerModel.Selectors.Add(new SelectorModel
            {
                AttributeRouteModel = new AttributeRouteModel(attr)
            });
        }
        appModel.Controllers.Add(controllerModel);

        // Act
        convention.Apply(appModel);

        // Assert
        var templates = controllerModel.Selectors
            .Select(s => s.AttributeRouteModel?.Template)
            .ToList();

        Assert.Contains(legacyPrefix, templates);
        Assert.Contains(expectedV1, templates);
    }
}
