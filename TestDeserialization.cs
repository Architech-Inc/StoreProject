using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Test;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
}

public class PagedResult<T>
{
    public PagedResult() { }

    public PagedResult(IEnumerable<T> items, int totalCount, int page, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        Page = page;
        PageSize = pageSize;
    }

    public IEnumerable<T> Items { get; set; } = Array.Empty<T>();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public class Category { public int CategoryId { get; set; } public string Name { get; set; } = """"; }
public class EmployeeDto { public Guid EmployeeId { get; set; } public string FirstName { get; set; } = """"; }

var jsonCategory = @"{""success"":true,""message"":""Success"",""data"":[{""categoryId"":1,""name"":""Test""}]}";
var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
var apiResponseCat = JsonSerializer.Deserialize<ApiResponse<List<Category>>>(jsonCategory, options);
Console.WriteLine(""Categories: "" + apiResponseCat?.Data?.Count);

var jsonPaged = @"{""success"":true,""message"":""Success"",""data"":{""items"":[{""employeeId"":""e4ed1796-4741-11f1-814d-c858c0c6a8bc"",""firstName"":""Alice""}],""totalCount"":4,""page"":1,""pageSize"":20}}";
var apiResponseEmp = JsonSerializer.Deserialize<ApiResponse<PagedResult<EmployeeDto>>>(jsonPaged, options);
var count = 0;
foreach(var i in apiResponseEmp?.Data?.Items ?? Array.Empty<EmployeeDto>()) count++;
Console.WriteLine(""Employees: "" + count);
