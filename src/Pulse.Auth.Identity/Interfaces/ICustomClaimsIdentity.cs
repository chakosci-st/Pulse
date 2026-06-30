using System.Security.Principal;

namespace Pulse.Core.Interfaces
{
    public interface ICustomClaimsIdentity : IIdentity
    {
        string EmployeeId { get; }
        string Email { get; }
        string FirstName { get; }
        string LastName { get; }
        string STJobFunctionDescription { get; }
        string Department { get; }
        string Division { get; }
        string Photo { get; }
        string ManagerUsername { get; }
        string ManagerUserId { get; }
        string ManagerFirstName { get; }
        string ManagerLastName { get; }
        string ManagerEmail { get; }
        string ADGroups { get; }
        string CostCenter { get; }
        string CostCenterDescription { get; }
        string OICIsActive { get; }
        string OICUsername { get; }
        string OICUserId { get; }
        string OICFirstName { get; }
        string OICLastName { get; }
        string OICEmail { get; }
    }
}
