using Store.Models.DTOs.Common;
using Store.Models.DTOs.Employees;
using System.Collections.Generic;

namespace Store.Models.DTOs.Users;

public class User360Dto
{
    public UserDto Profile { get; set; } = null!;
    public EmployeeDto? LinkedEmployee { get; set; }
    public IReadOnlyCollection<AuditLogDto> RecentActivity { get; set; } = new List<AuditLogDto>();
    public IReadOnlyCollection<ContactChangeRequestDto> PendingContactChanges { get; set; } = new List<ContactChangeRequestDto>();
    public IReadOnlyCollection<Store.Models.DTOs.Auth.UserSessionDto> ActiveSessions { get; set; } = new List<Store.Models.DTOs.Auth.UserSessionDto>();
}
