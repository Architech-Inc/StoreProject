using Store.Models.DTOs.Users;

namespace Store.Models.DTOs.Employees;

public class Employee360Dto
{
    public EmployeeDto Profile { get; set; } = new();
    
    public List<string> Phones { get; set; } = new();
    public List<string> Emails { get; set; } = new();
    
    public List<UserDto> Users { get; set; } = new();
}
