using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Store.API.Infrastructure.Conventions;

/// <summary>
/// Application model convention that ensures dual-routing for REST endpoints:
/// every controller with a route starting with "api/" is also exposed under "api/v1/...".
/// This provides standard "/api/v1/..." URL versioning while preserving backward
/// compatibility for unversioned "/api/..." clients.
/// </summary>
public class ApiVersioningRouteConvention : IApplicationModelConvention
{
    private readonly string _versionPrefix;

    public ApiVersioningRouteConvention(string versionPrefix = "v1")
    {
        _versionPrefix = versionPrefix.Trim('/');
    }

    public void Apply(ApplicationModel application)
    {
        foreach (var controller in application.Controllers)
        {
            var selectorsToAdd = new List<SelectorModel>();

            foreach (var selector in controller.Selectors)
            {
                var template = selector.AttributeRouteModel?.Template;
                if (string.IsNullOrWhiteSpace(template))
                    continue;

                // Match routes starting with "api/" or exact "api"
                if (template.Equals("api", StringComparison.OrdinalIgnoreCase) ||
                    template.StartsWith("api/", StringComparison.OrdinalIgnoreCase))
                {
                    // If already versioned with this prefix or api/v{...}, don't duplicate
                    if (template.StartsWith($"api/{_versionPrefix}/", StringComparison.OrdinalIgnoreCase) ||
                        template.Equals($"api/{_versionPrefix}", StringComparison.OrdinalIgnoreCase) ||
                        template.StartsWith("api/v{", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    string remaining = template.Length == 3 ? string.Empty : template.Substring(4);
                    string versionedTemplate = string.IsNullOrEmpty(remaining)
                        ? $"api/{_versionPrefix}"
                        : $"api/{_versionPrefix}/{remaining}";

                    var newSelector = new SelectorModel(selector)
                    {
                        AttributeRouteModel = new AttributeRouteModel(selector.AttributeRouteModel!)
                        {
                            Template = versionedTemplate
                        }
                    };

                    selectorsToAdd.Add(newSelector);
                }
            }

            foreach (var newSelector in selectorsToAdd)
            {
                controller.Selectors.Add(newSelector);
            }
        }
    }
}
